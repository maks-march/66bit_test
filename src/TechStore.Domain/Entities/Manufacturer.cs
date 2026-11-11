using TechStore.Domain.Common;

namespace TechStore.Domain.Entities;

/// <summary>Производитель техники (справочник).</summary>
public sealed class Manufacturer : AuditableEntity
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public ICollection<EquipmentItem> EquipmentItems { get; } = new List<EquipmentItem>();

    private Manufacturer()
    {
    }

    private Manufacturer(string name, string? description, string? createdBy)
    {
        Name = name;
        Description = description;
        MarkCreated(createdBy);
    }

    public static Manufacturer Create(string? name, string? description, string? createdBy = null)
        => new(
            Guard.Required(name, "Наименование", NameMaxLength),
            Guard.Optional(description, "Описание", DescriptionMaxLength),
            createdBy);

    public void Update(string? name, string? description, string? updatedBy)
    {
        Name = Guard.Required(name, "Наименование", NameMaxLength);
        Description = Guard.Optional(description, "Описание", DescriptionMaxLength);
        Touch(updatedBy);
    }
}
