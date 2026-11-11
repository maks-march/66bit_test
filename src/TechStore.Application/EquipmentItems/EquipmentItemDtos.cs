using TechStore.Domain.Entities;

namespace TechStore.Application.EquipmentItems;

public sealed record ReferenceDto(long Id, string Name);

/// <remarks>
/// PhotoPath — относительный путь из IFileStorage; 
/// в абсолютный URL его превращает Api.
/// </remarks>
public sealed record EquipmentItemDto(
    long Id,
    ReferenceDto EquipmentType,
    ReferenceDto Manufacturer,
    ReferenceDto SupplyCountry,
    string Model,
    decimal Price,
    string? PhotoPath,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? UpdatedBy)
{
    /// <summary>Для сущностей с загруженными навигациями.</summary>
    public static EquipmentItemDto From(EquipmentItem item)
        => From(item, item.EquipmentType.Name, item.Manufacturer.Name, item.SupplyCountry.Name);

    /// <summary>Для create/update: имена подставляются явно.</summary>
    public static EquipmentItemDto From(EquipmentItem item, string typeName, string manufacturerName, string countryName)
        => new(item.Id,
            new ReferenceDto(item.EquipmentTypeId, typeName),
            new ReferenceDto(item.ManufacturerId, manufacturerName),
            new ReferenceDto(item.SupplyCountryId, countryName),
            item.Model, item.Price, item.PhotoPath,
            item.CreatedAt, item.UpdatedAt, item.UpdatedBy);
}

/// <summary>Общая форма записи (create/update различаются только валидатором и сценарием).</summary>
public interface IEquipmentItemWrite
{
    long EquipmentTypeId { get; }
    long ManufacturerId { get; }
    string? Model { get; }
    string? SupplyCountryName { get; }
    decimal Price { get; }
}

/// <summary>Страна передаётся именем: несуществующая создастся автоматически (upsert).</summary>
public sealed record EquipmentItemCreateDto(
    long EquipmentTypeId, long ManufacturerId, string? Model, string? SupplyCountryName, decimal Price)
    : IEquipmentItemWrite;

public sealed record EquipmentItemUpdateDto(
    long EquipmentTypeId, long ManufacturerId, string? Model, string? SupplyCountryName, decimal Price)
    : IEquipmentItemWrite;

/// <summary>Фасеты для фильтров на фронте.</summary>
public sealed record FilterOptionsDto(
    IReadOnlyList<long> AvailableTypeIds, IReadOnlyList<long> AvailableManufacturerIds);
