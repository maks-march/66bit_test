using TechStore.Domain.Common;

namespace TechStore.Domain.Entities;

/// <summary>Тип техники (справочник).</summary>
public sealed class EquipmentType : AuditableEntity
{
    public const int NameMaxLength = 200;
    public const int DescriptionMaxLength = 2000;

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }

    public ICollection<EquipmentItem> EquipmentItems { get; } = new List<EquipmentItem>();

    private EquipmentType()
    {
    }

    private EquipmentType(string name, string? description, string? createdBy)
    {
        Name = name;
        Description = description;
        MarkCreated(createdBy);
    }

    public static EquipmentType Create(string? name, string? description, string? createdBy = null)
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
