using TechStore.Application.Common;

namespace TechStore.Application.EquipmentTypes;

public interface IEquipmentTypeService
{
    Task<Result<PagedResult<EquipmentTypeDto>>> ListAsync(PagedQuery query, CancellationToken ct = default);

    Task<Result<EquipmentTypeDto>> GetAsync(long id, CancellationToken ct = default);

    Task<Result<EquipmentTypeDto>> CreateAsync(EquipmentTypeCreateDto dto, CancellationToken ct = default);

    Task<Result<EquipmentTypeDto>> UpdateAsync(long id, EquipmentTypeUpdateDto dto, CancellationToken ct = default);

    /// <summary>Удаление при связанных позициях возвращает 409-ошибку REFERENCE_IN_USE.</summary>
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
