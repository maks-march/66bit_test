using TechStore.Application.EquipmentItems;
using TechStore.Domain.Entities;
using TechStore.Domain.Interfaces;

namespace TechStore.Application.Abstractions;

/// <summary>
/// Репозиторий позиций. GetByIdAsync и SearchAsync возвращают сущности
/// с загруженными справочниками (тип, производитель, страна).
/// </summary>
public interface IEquipmentItemRepository : IRepository<EquipmentItem>
{
    Task<(IReadOnlyList<EquipmentItem> Items, int TotalCount)> SearchAsync(
        EquipmentItemQuery query, CancellationToken ct = default);

    /// <summary>
    /// Фасеты для сужающихся фильтров: типы, доступные при всех фильтрах кроме типа,
    /// и производители, доступные при всех фильтрах кроме производителя.
    /// </summary>
    Task<FilterOptionsDto> GetFilterOptionIdsAsync(
        EquipmentItemQuery query, CancellationToken ct = default);
}
