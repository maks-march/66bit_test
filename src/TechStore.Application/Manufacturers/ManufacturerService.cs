using FluentValidation;
using TechStore.Application.Abstractions;
using TechStore.Application.Common;
using TechStore.Domain.Common;
using TechStore.Domain.Entities;
using TechStore.Domain.Exceptions;
using TechStore.Domain.Interfaces;

namespace TechStore.Application.Manufacturers;

public sealed class ManufacturerService : IManufacturerService
{
    private const string EntityName = "Производитель";

    private readonly IManufacturerRepository _manufacturers;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<ManufacturerCreateDto> _createValidator;
    private readonly IValidator<ManufacturerUpdateDto> _updateValidator;
    private readonly IValidator<PagedQuery> _queryValidator;
    private readonly ICurrentUser _user;
    private readonly IInventoryNotifier _notifier;

    public ManufacturerService(
        IManufacturerRepository manufacturers,
        IUnitOfWork uow,
        IValidator<ManufacturerCreateDto> createValidator,
        IValidator<ManufacturerUpdateDto> updateValidator,
        IValidator<PagedQuery> queryValidator,
        ICurrentUser user,
        IInventoryNotifier notifier)
    {
        _manufacturers = manufacturers;
        _uow = uow;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _queryValidator = queryValidator;
        _user = user;
        _notifier = notifier;
    }

    public async Task<Result<PagedResult<ManufacturerDto>>> ListAsync(PagedQuery query, CancellationToken ct = default)
    {
        var validation = await _queryValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            return Result<PagedResult<ManufacturerDto>>.Failure(validation.ToError());

        var (items, total) = await _manufacturers.SearchAsync(query.Search?.Trim(), query.Page, query.PageSize, ct);
        return Result<PagedResult<ManufacturerDto>>.Success(
            new PagedResult<ManufacturerDto>(items.Select(ManufacturerDto.From).ToList(), total, query.Page, query.PageSize));
    }

    public async Task<Result<ManufacturerDto>> GetAsync(long id, CancellationToken ct = default)
    {
        var entity = await _manufacturers.GetByIdAsync(id, ct);
        return entity is null
            ? Result<ManufacturerDto>.Failure(Error.NotFound(EntityName, id))
            : Result<ManufacturerDto>.Success(ManufacturerDto.From(entity));
    }

    public async Task<Result<ManufacturerDto>> CreateAsync(ManufacturerCreateDto dto, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<ManufacturerDto>.Failure(validation.ToError());

        var name = dto.Name!.Trim();
        if (await _manufacturers.ExistsByNameAsync(name, null, ct))
            return Result<ManufacturerDto>.Failure(
                Error.Business(ErrorCodes.NameDuplicate, $"Производитель «{name}» уже существует."));

        Manufacturer entity;
        try
        {
            entity = Manufacturer.Create(dto.Name, dto.Description, _user.Name);
        }
        catch (DomainException ex)
        {
            return Result<ManufacturerDto>.Failure(Error.FromDomainException(ex));
        }

        await _manufacturers.AddAsync(entity, ct);

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result<ManufacturerDto>.Failure(saved.Error);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.Manufacturer, ChangeAction.Created, entity.Id), ct);
        return Result<ManufacturerDto>.Success(ManufacturerDto.From(entity));
    }

    public async Task<Result<ManufacturerDto>> UpdateAsync(long id, ManufacturerUpdateDto dto, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<ManufacturerDto>.Failure(validation.ToError());

        var entity = await _manufacturers.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<ManufacturerDto>.Failure(Error.NotFound(EntityName, id));

        var name = dto.Name!.Trim();
        if (await _manufacturers.ExistsByNameAsync(name, id, ct))
            return Result<ManufacturerDto>.Failure(
                Error.Business(ErrorCodes.NameDuplicate, $"Производитель «{name}» уже существует."));

        try
        {
            entity.Update(dto.Name, dto.Description, _user.Name);
        }
        catch (DomainException ex)
        {
            return Result<ManufacturerDto>.Failure(Error.FromDomainException(ex));
        }

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result<ManufacturerDto>.Failure(saved.Error);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.Manufacturer, ChangeAction.Updated, entity.Id), ct);
        return Result<ManufacturerDto>.Success(ManufacturerDto.From(entity));
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _manufacturers.GetByIdAsync(id, ct);
        if (entity is null)
            return Result.Failure(Error.NotFound(EntityName, id));

        var usedCount = await _manufacturers.CountItemsAsync(id, ct);
        if (usedCount > 0)
            return Result.Failure(Error.Business(
                ErrorCodes.ReferenceInUse,
                $"Нельзя удалить производителя: используется в позициях ({usedCount}).",
                new Dictionary<string, object> { ["count"] = usedCount }));

        _manufacturers.Remove(entity);

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result.Failure(saved.Error);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.Manufacturer, ChangeAction.Deleted, id), ct);
        return Result.Success();
    }
}
