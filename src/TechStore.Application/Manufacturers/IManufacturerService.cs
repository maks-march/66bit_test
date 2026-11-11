using TechStore.Application.Common;

namespace TechStore.Application.Manufacturers;

public interface IManufacturerService
{
    Task<Result<PagedResult<ManufacturerDto>>> ListAsync(PagedQuery query, CancellationToken ct = default);

    Task<Result<ManufacturerDto>> GetAsync(long id, CancellationToken ct = default);

    Task<Result<ManufacturerDto>> CreateAsync(ManufacturerCreateDto dto, CancellationToken ct = default);

    Task<Result<ManufacturerDto>> UpdateAsync(long id, ManufacturerUpdateDto dto, CancellationToken ct = default);

    /// <summary>Удаление при связанных позициях возвращает 409-ошибку REFERENCE_IN_USE.</summary>
    Task<Result> DeleteAsync(long id, CancellationToken ct = default);
}
