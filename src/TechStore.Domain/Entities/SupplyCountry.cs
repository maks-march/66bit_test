using TechStore.Domain.Common;

namespace TechStore.Domain.Entities;

/// <summary>
/// Страна поставки.
/// </summary>
public sealed class SupplyCountry : AuditableEntity
{
    public const int NameMaxLength = 200;

    public string Name { get; private set; } = null!;

    public ICollection<EquipmentItem> EquipmentItems { get; } = new List<EquipmentItem>();

    private SupplyCountry()
    {
    }

    private SupplyCountry(string name, string? createdBy)
    {
        Name = name;
        MarkCreated(createdBy);
    }

    public static SupplyCountry Create(string? name, string? createdBy = null)
        => new(Guard.Required(name, "Страна поставки", NameMaxLength), createdBy);
}
