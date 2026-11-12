using TechStore.Application.EquipmentItems;

namespace TechStore.Api.Models;

/// <summary>
/// Позиция для API: относительный PhotoPath превращается в URL для клиента (/uploads/...).
/// Остальные DTO отдаются как есть.
/// </summary>
public sealed record EquipmentItemResponse(
    long Id,
    ReferenceDto EquipmentType,
    ReferenceDto Manufacturer,
    ReferenceDto SupplyCountry,
    string Model,
    decimal Price,
    string? PhotoUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? UpdatedBy)
{
    public static EquipmentItemResponse From(EquipmentItemDto dto, string publicPrefix)
        => new(
            dto.Id,
            dto.EquipmentType,
            dto.Manufacturer,
            dto.SupplyCountry,
            dto.Model,
            dto.Price,
            dto.PhotoPath is null ? null : $"{publicPrefix}/{dto.PhotoPath}",
            dto.CreatedAt,
            dto.UpdatedAt,
            dto.UpdatedBy);
}
