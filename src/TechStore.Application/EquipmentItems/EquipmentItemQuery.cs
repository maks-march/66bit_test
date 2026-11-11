using TechStore.Application.Common;

namespace TechStore.Application.EquipmentItems;

public sealed class EquipmentItemQuery : PagedQuery
{
    public long? EquipmentTypeId { get; set; }

    public long? ManufacturerId { get; set; }

    public decimal? PriceFrom { get; set; }

    public decimal? PriceTo { get; set; }

    public string SortBy { get; set; } = EquipmentItemSortFields.UpdatedAt;

    public bool SortDesc { get; set; } = true;
}

public static class EquipmentItemSortFields
{
    public const string Model = "model";
    public const string Price = "price";
    public const string CreatedAt = "createdAt";
    public const string UpdatedAt = "updatedAt";

    public static readonly string[] All = [Model, Price, CreatedAt, UpdatedAt];

    public static bool IsKnown(string? value)
        => value is not null && All.Contains(value, StringComparer.OrdinalIgnoreCase);
}
