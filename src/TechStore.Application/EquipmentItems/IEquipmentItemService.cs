using TechStore.Application.Common;

namespace TechStore.Application.EquipmentItems;

public interface IEquipmentItemService
{
    Task<Result<PagedResult<EquipmentItemDto>>> SearchAsync(EquipmentItemQuery query, CancellationToken ct = default);

    Task<Result<FilterOptionsDto>> GetFilterOptionsAsync(EquipmentItemQuery query, CancellationToken ct = default);

    Task<Result<EquipmentItemDto>> GetAsync(long id, CancellationToken ct = default);

    Task<Result<EquipmentItemDto>> CreateAsync(EquipmentItemCreateDto dto, CancellationToken ct = default);

    Task<Result<EquipmentItemDto>> UpdateAsync(long id, EquipmentItemUpdateDto dto, CancellationToken ct = default);

    Task<Result> DeleteAsync(long id, CancellationToken ct = default);

    /// <param name="content">Поток файла (владелец — вызывающий, сервис его не освобождает).</param>
    Task<Result<EquipmentItemDto>> SetPhotoAsync(
        long id, Stream content, string fileName, CancellationToken ct = default);

    Task<Result<EquipmentItemDto>> ClearPhotoAsync(long id, CancellationToken ct = default);
}
