using TechStore.Domain.Entities;

namespace TechStore.Application.Manufacturers;

public sealed record ManufacturerDto(
    long Id, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt, string? UpdatedBy)
{
    public static ManufacturerDto From(Manufacturer entity)
        => new(entity.Id, entity.Name, entity.Description, entity.CreatedAt, entity.UpdatedAt, entity.UpdatedBy);
}

/// <summary>Общая форма записи (create/update различаются только валидатором и сценарием).</summary>
public interface IManufacturerWrite
{
    string? Name { get; }
    string? Description { get; }
}

public sealed record ManufacturerCreateDto(string? Name, string? Description) : IManufacturerWrite;

public sealed record ManufacturerUpdateDto(string? Name, string? Description) : IManufacturerWrite;
