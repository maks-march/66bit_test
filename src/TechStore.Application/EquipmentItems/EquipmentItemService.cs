using FluentValidation;
using Microsoft.Extensions.Options;
using TechStore.Application.Abstractions;
using TechStore.Application.Common;
using TechStore.Domain.Common;
using TechStore.Domain.Entities;
using TechStore.Domain.Exceptions;
using TechStore.Domain.Interfaces;

namespace TechStore.Application.EquipmentItems;

public sealed class EquipmentItemService : IEquipmentItemService
{
    private const string EntityName = "Позиция";

    private readonly IEquipmentItemRepository _items;
    private readonly IEquipmentTypeRepository _types;
    private readonly IManufacturerRepository _manufacturers;
    private readonly ISupplyCountryRepository _countries;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<EquipmentItemCreateDto> _createValidator;
    private readonly IValidator<EquipmentItemUpdateDto> _updateValidator;
    private readonly IValidator<EquipmentItemQuery> _queryValidator;
    private readonly ICurrentUser _user;
    private readonly IInventoryNotifier _notifier;
    private readonly IFileStorage _files;
    private readonly FileValidationOptions _fileOptions;

    public EquipmentItemService(
        IEquipmentItemRepository items,
        IEquipmentTypeRepository types,
        IManufacturerRepository manufacturers,
        ISupplyCountryRepository countries,
        IUnitOfWork uow,
        IValidator<EquipmentItemCreateDto> createValidator,
        IValidator<EquipmentItemUpdateDto> updateValidator,
        IValidator<EquipmentItemQuery> queryValidator,
        ICurrentUser user,
        IInventoryNotifier notifier,
        IFileStorage files,
        IOptions<FileValidationOptions> fileOptions)
    {
        _items = items;
        _types = types;
        _manufacturers = manufacturers;
        _countries = countries;
        _uow = uow;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _queryValidator = queryValidator;
        _user = user;
        _notifier = notifier;
        _files = files;
        _fileOptions = fileOptions.Value;
    }

    public async Task<Result<PagedResult<EquipmentItemDto>>> SearchAsync(
        EquipmentItemQuery query, CancellationToken ct = default)
    {
        var validation = await _queryValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            return Result<PagedResult<EquipmentItemDto>>.Failure(validation.ToError());

        var (items, total) = await _items.SearchAsync(query, ct);
        return Result<PagedResult<EquipmentItemDto>>.Success(new PagedResult<EquipmentItemDto>(
            items.Select(EquipmentItemDto.From).ToList(), total, query.Page, query.PageSize));
    }

    public async Task<Result<FilterOptionsDto>> GetFilterOptionsAsync(
        EquipmentItemQuery query, CancellationToken ct = default)
    {
        var validation = await _queryValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            return Result<FilterOptionsDto>.Failure(validation.ToError());

        var options = await _items.GetFilterOptionIdsAsync(query, ct);
        return Result<FilterOptionsDto>.Success(options);
    }

    public async Task<Result<EquipmentItemDto>> GetAsync(long id, CancellationToken ct = default)
    {
        var item = await _items.GetByIdAsync(id, ct);
        return item is null
            ? Result<EquipmentItemDto>.Failure(Error.NotFound(EntityName, id))
            : Result<EquipmentItemDto>.Success(EquipmentItemDto.From(item));
    }

    public async Task<Result<EquipmentItemDto>> CreateAsync(EquipmentItemCreateDto dto, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<EquipmentItemDto>.Failure(validation.ToError());

        var refs = await LoadReferencesAsync(dto.EquipmentTypeId, dto.ManufacturerId, ct);
        if (!refs.IsSuccess)
            return Result<EquipmentItemDto>.Failure(refs.Error);
        var (type, manufacturer) = refs.Value;

        var country = await GetOrCreateCountryAsync(dto.SupplyCountryName, ct);
        if (!country.IsSuccess)
            return Result<EquipmentItemDto>.Failure(country.Error);

        EquipmentItem item;
        try
        {
            item = EquipmentItem.Create(
                type.Id, manufacturer.Id, country.Value.Id, dto.Model, dto.Price, photoPath: null, _user.Name);
        }
        catch (DomainException ex)
        {
            return Result<EquipmentItemDto>.Failure(Error.FromDomainException(ex));
        }

        await _items.AddAsync(item, ct);

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result<EquipmentItemDto>.Failure(saved.Error);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.EquipmentItem, ChangeAction.Created, item.Id), ct);

        // Навигации у новой сущности не загружены — подставляем уже известные имена без лишнего запроса.
        return Result<EquipmentItemDto>.Success(EquipmentItemDto.From(item, type.Name, manufacturer.Name, country.Value.Name));
    }

    public async Task<Result<EquipmentItemDto>> UpdateAsync(
        long id, EquipmentItemUpdateDto dto, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<EquipmentItemDto>.Failure(validation.ToError());

        var item = await _items.GetByIdAsync(id, ct);
        if (item is null)
            return Result<EquipmentItemDto>.Failure(Error.NotFound(EntityName, id));

        var refs = await LoadReferencesAsync(dto.EquipmentTypeId, dto.ManufacturerId, ct);
        if (!refs.IsSuccess)
            return Result<EquipmentItemDto>.Failure(refs.Error);
        var (type, manufacturer) = refs.Value;

        var country = await GetOrCreateCountryAsync(dto.SupplyCountryName, ct);
        if (!country.IsSuccess)
            return Result<EquipmentItemDto>.Failure(country.Error);

        try
        {
            item.Update(type.Id, manufacturer.Id, country.Value.Id, dto.Model, dto.Price, item.PhotoPath, _user.Name);
        }
        catch (DomainException ex)
        {
            return Result<EquipmentItemDto>.Failure(Error.FromDomainException(ex));
        }

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result<EquipmentItemDto>.Failure(saved.Error);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.EquipmentItem, ChangeAction.Updated, item.Id), ct);

        // После смены FK навигационные объекты протухли — имена берём из свежих сущностей.
        return Result<EquipmentItemDto>.Success(EquipmentItemDto.From(item, type.Name, manufacturer.Name, country.Value.Name));
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var item = await _items.GetByIdAsync(id, ct);
        if (item is null)
            return Result.Failure(Error.NotFound(EntityName, id));

        var photoPath = item.PhotoPath;
        _items.Remove(item);

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result.Failure(saved.Error);

        // Файл удаляем после успешного коммита.
        await DeleteFileAttemptAsync(photoPath, CancellationToken.None);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.EquipmentItem, ChangeAction.Deleted, id), ct);
        return Result.Success();
    }

    public async Task<Result<EquipmentItemDto>> SetPhotoAsync(
        long id, Stream content, string fileName, CancellationToken ct = default)
    {
        var item = await _items.GetByIdAsync(id, ct);
        if (item is null)
            return Result<EquipmentItemDto>.Failure(Error.NotFound(EntityName, id));

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!_fileOptions.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
            return Result<EquipmentItemDto>.Failure(new Error(
                ErrorCodes.PhotoInvalid,
                $"Недопустимый формат фото. Разрешены: {string.Join(", ", _fileOptions.AllowedExtensions)}."));

        // Копируем в память: нужен размер, проверка сигнатуры и перечитываемый поток для хранилища.
        using var buffer = new MemoryStream();
        await content.CopyToAsync(buffer, ct);

        if (buffer.Length == 0)
            return Result<EquipmentItemDto>.Failure(new Error(ErrorCodes.PhotoInvalid, "Файл фотографии пуст."));

        if (buffer.Length > _fileOptions.MaxBytes)
            return Result<EquipmentItemDto>.Failure(new Error(
                ErrorCodes.PhotoTooLarge,
                $"Фото слишком большое: максимум {_fileOptions.MaxBytes / 1024 / 1024} МБ."));

        buffer.Position = 0;
        if (!ImageSignature.MatchesExtension(buffer, extension))
            return Result<EquipmentItemDto>.Failure(new Error(
                ErrorCodes.PhotoInvalid, "Файл не похож на изображение указанного формата."));

        var oldPath = item.PhotoPath;
        string? uploadedPath = null;
        try
        {
            buffer.Position = 0;
            uploadedPath = await _files.SaveAsync(buffer, fileName, ct);
            item.SetPhoto(uploadedPath, _user.Name);

            var saved = await _uow.TrySaveAsync(ct);
            if (!saved.IsSuccess)
            {
                await DeleteFileAttemptAsync(uploadedPath, CancellationToken.None);
                return Result<EquipmentItemDto>.Failure(saved.Error);
            }
        }
        catch (Exception) when (uploadedPath is not null)
        {
            // БД не сохранилась — залитый файл удаляем.
            await DeleteFileAttemptAsync(uploadedPath, CancellationToken.None);
            throw;
        }

        if (oldPath is not null && oldPath != uploadedPath)
            await DeleteFileAttemptAsync(oldPath, CancellationToken.None);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.EquipmentItem, ChangeAction.Updated, item.Id), ct);
        return Result<EquipmentItemDto>.Success(EquipmentItemDto.From(item));
    }

    public async Task<Result<EquipmentItemDto>> ClearPhotoAsync(long id, CancellationToken ct = default)
    {
        var item = await _items.GetByIdAsync(id, ct);
        if (item is null)
            return Result<EquipmentItemDto>.Failure(Error.NotFound(EntityName, id));

        if (item.PhotoPath is null)
            return Result<EquipmentItemDto>.Success(EquipmentItemDto.From(item));

        var oldPath = item.PhotoPath;
        item.SetPhoto(null, _user.Name);

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result<EquipmentItemDto>.Failure(saved.Error);

        await DeleteFileAttemptAsync(oldPath, CancellationToken.None);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.EquipmentItem, ChangeAction.Updated, item.Id), ct);
        return Result<EquipmentItemDto>.Success(EquipmentItemDto.From(item));
    }

    private async Task<Result<(EquipmentType, Manufacturer)>> LoadReferencesAsync(
        long typeId, long manufacturerId, CancellationToken ct)
    {
        var type = await _types.GetByIdAsync(typeId, ct);
        if (type is null)
            return Result<(EquipmentType, Manufacturer)>.Failure(Error.NotFound("Тип техники", typeId));

        var manufacturer = await _manufacturers.GetByIdAsync(manufacturerId, ct);
        if (manufacturer is null)
            return Result<(EquipmentType, Manufacturer)>.Failure(Error.NotFound("Производитель", manufacturerId));

        return Result<(EquipmentType, Manufacturer)>.Success((type, manufacturer));
    }

    private async Task<Result<SupplyCountry>> GetOrCreateCountryAsync(string? name, CancellationToken ct)
    {
        var normalized = (name ?? string.Empty).Trim();

        var existing = await _countries.FindByNameAsync(normalized, ct);
        if (existing is not null)
            return Result<SupplyCountry>.Success(existing);

        try
        {
            var created = SupplyCountry.Create(normalized, _user.Name);
            await _countries.AddAsync(created, ct);
            // Флаш до создания позиции: EF присвоит Id, иначе FK останется 0.
            var saved = await _uow.TrySaveAsync(ct);
            if (!saved.IsSuccess)
                return Result<SupplyCountry>.Failure(saved.Error);

            return Result<SupplyCountry>.Success(created);
        }
        catch (DomainException ex)
        {
            return Result<SupplyCountry>.Failure(Error.FromDomainException(ex));
        }
    }

    private async Task DeleteFileAttemptAsync(string? path, CancellationToken ct)
    {
        if (path is null)
            return;

        try
        {
            await _files.DeleteAsync(path, ct);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Файл уже удалён вручную или хранилище временно недоступно
        }
    }
}
