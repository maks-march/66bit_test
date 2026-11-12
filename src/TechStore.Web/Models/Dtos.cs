// Клиентские зеркала контрактов API (сериализация — System.Text.Json Web defaults:
// camelCase + регистронезависимость, поэтому имена свойств совпадают с серверными).

namespace TechStore.Web.Models;

public sealed record ReferenceDto(long Id, string Name);

public sealed record EquipmentItemModel(
    long Id,
    ReferenceDto EquipmentType,
    ReferenceDto Manufacturer,
    ReferenceDto SupplyCountry,
    string Model,
    decimal Price,
    string? PhotoUrl,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? UpdatedBy);

public sealed record ManufacturerModel(
    long Id, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt, string? UpdatedBy);

public sealed record EquipmentTypeModel(
    long Id, string Name, string? Description, DateTime CreatedAt, DateTime UpdatedAt, string? UpdatedBy);

public sealed record SupplyCountryModel(long Id, string Name);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>Фасеты фильтров: id, доступные при текущих условиях (остальные select'ы блокируем).</summary>
public sealed record FilterOptions(IReadOnlyList<long> AvailableTypeIds, IReadOnlyList<long> AvailableManufacturerIds);

/// <summary>Запись позиции (create/update отличаются только endpoint'ом).</summary>
public sealed record ItemWriteRequest(
    long EquipmentTypeId, long ManufacturerId, string? Model, string? SupplyCountryName, decimal Price);

/// <summary>Запись справочника (производитель/тип техники).</summary>
public sealed record DictionaryWriteRequest(string? Name, string? Description);

/// <summary>Факт изменения из SignalR (kind/action/entityId/at). Детали не шлются —
/// клиент перезапрашивает текущую страницу сам.</summary>
public sealed record InventoryChangeNotification(string Kind, string Action, long? EntityId, DateTime At);
