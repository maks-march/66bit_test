using TechStore.Domain.Entities;

namespace TechStore.Application.EquipmentTypes;

public sealed record EquipmentTypeDto(
    long Id, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt, string? UpdatedBy)
{
    public static EquipmentTypeDto From(EquipmentType entity)
        => new(entity.Id, entity.Name, entity.Description, entity.CreatedAt, entity.UpdatedAt, entity.UpdatedBy);
}

/// <summary>Общая форма записи (create/update различаются только валидатором и сценарием).</summary>
public interface IEquipmentTypeWrite
{
    string? Name { get; }
    string? Description { get; }
}

public sealed record EquipmentTypeCreateDto(string? Name, string? Description) : IEquipmentTypeWrite;

public sealed record EquipmentTypeUpdateDto(string? Name, string? Description) : IEquipmentTypeWrite;
