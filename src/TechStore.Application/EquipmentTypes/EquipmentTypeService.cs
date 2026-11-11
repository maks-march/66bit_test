using FluentValidation;
using TechStore.Application.Abstractions;
using TechStore.Application.Common;
using TechStore.Domain.Common;
using TechStore.Domain.Entities;
using TechStore.Domain.Exceptions;
using TechStore.Domain.Interfaces;

namespace TechStore.Application.EquipmentTypes;

public sealed class EquipmentTypeService : IEquipmentTypeService
{
    private const string EntityName = "Тип техники";

    private readonly IEquipmentTypeRepository _types;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<EquipmentTypeCreateDto> _createValidator;
    private readonly IValidator<EquipmentTypeUpdateDto> _updateValidator;
    private readonly IValidator<PagedQuery> _queryValidator;
    private readonly ICurrentUser _user;
    private readonly IInventoryNotifier _notifier;

    public EquipmentTypeService(
        IEquipmentTypeRepository types,
        IUnitOfWork uow,
        IValidator<EquipmentTypeCreateDto> createValidator,
        IValidator<EquipmentTypeUpdateDto> updateValidator,
        IValidator<PagedQuery> queryValidator,
        ICurrentUser user,
        IInventoryNotifier notifier)
    {
        _types = types;
        _uow = uow;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _queryValidator = queryValidator;
        _user = user;
        _notifier = notifier;
    }

    public async Task<Result<PagedResult<EquipmentTypeDto>>> ListAsync(PagedQuery query, CancellationToken ct = default)
    {
        var validation = await _queryValidator.ValidateAsync(query, ct);
        if (!validation.IsValid)
            return Result<PagedResult<EquipmentTypeDto>>.Failure(validation.ToError());

        var (items, total) = await _types.SearchAsync(query.Search?.Trim(), query.Page, query.PageSize, ct);
        return Result<PagedResult<EquipmentTypeDto>>.Success(
            new PagedResult<EquipmentTypeDto>(items.Select(EquipmentTypeDto.From).ToList(), total, query.Page, query.PageSize));
    }

    public async Task<Result<EquipmentTypeDto>> GetAsync(long id, CancellationToken ct = default)
    {
        var entity = await _types.GetByIdAsync(id, ct);
        return entity is null
            ? Result<EquipmentTypeDto>.Failure(Error.NotFound(EntityName, id))
            : Result<EquipmentTypeDto>.Success(EquipmentTypeDto.From(entity));
    }

    public async Task<Result<EquipmentTypeDto>> CreateAsync(EquipmentTypeCreateDto dto, CancellationToken ct = default)
    {
        var validation = await _createValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<EquipmentTypeDto>.Failure(validation.ToError());

        var name = dto.Name!.Trim();
        if (await _types.ExistsByNameAsync(name, null, ct))
            return Result<EquipmentTypeDto>.Failure(
                Error.Business(ErrorCodes.NameDuplicate, $"Тип техники «{name}» уже существует."));

        EquipmentType entity;
        try
        {
            entity = EquipmentType.Create(dto.Name, dto.Description, _user.Name);
        }
        catch (DomainException ex)
        {
            return Result<EquipmentTypeDto>.Failure(Error.FromDomainException(ex));
        }

        await _types.AddAsync(entity, ct);

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result<EquipmentTypeDto>.Failure(saved.Error);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.EquipmentType, ChangeAction.Created, entity.Id), ct);
        return Result<EquipmentTypeDto>.Success(EquipmentTypeDto.From(entity));
    }

    public async Task<Result<EquipmentTypeDto>> UpdateAsync(long id, EquipmentTypeUpdateDto dto, CancellationToken ct = default)
    {
        var validation = await _updateValidator.ValidateAsync(dto, ct);
        if (!validation.IsValid)
            return Result<EquipmentTypeDto>.Failure(validation.ToError());

        var entity = await _types.GetByIdAsync(id, ct);
        if (entity is null)
            return Result<EquipmentTypeDto>.Failure(Error.NotFound(EntityName, id));

        var name = dto.Name!.Trim();
        if (await _types.ExistsByNameAsync(name, id, ct))
            return Result<EquipmentTypeDto>.Failure(
                Error.Business(ErrorCodes.NameDuplicate, $"Тип техники «{name}» уже существует."));

        try
        {
            entity.Update(dto.Name, dto.Description, _user.Name);
        }
        catch (DomainException ex)
        {
            return Result<EquipmentTypeDto>.Failure(Error.FromDomainException(ex));
        }

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result<EquipmentTypeDto>.Failure(saved.Error);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.EquipmentType, ChangeAction.Updated, entity.Id), ct);
        return Result<EquipmentTypeDto>.Success(EquipmentTypeDto.From(entity));
    }

    public async Task<Result> DeleteAsync(long id, CancellationToken ct = default)
    {
        var entity = await _types.GetByIdAsync(id, ct);
        if (entity is null)
            return Result.Failure(Error.NotFound(EntityName, id));

        var usedCount = await _types.CountItemsAsync(id, ct);
        if (usedCount > 0)
            return Result.Failure(Error.Business(
                ErrorCodes.ReferenceInUse,
                $"Нельзя удалить тип техники: используется в позициях ({usedCount}).",
                new Dictionary<string, object> { ["count"] = usedCount }));

        _types.Remove(entity);

        var saved = await _uow.TrySaveAsync(ct);
        if (!saved.IsSuccess)
            return Result.Failure(saved.Error);

        await _notifier.NotifyAsync(new InventoryChange(InventoryEntityKind.EquipmentType, ChangeAction.Deleted, id), ct);
        return Result.Success();
    }
}
