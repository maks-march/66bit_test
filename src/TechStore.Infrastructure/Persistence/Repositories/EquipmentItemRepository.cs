using Microsoft.EntityFrameworkCore;
using TechStore.Application.Abstractions;
using TechStore.Application.EquipmentItems;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Persistence.Repositories;

public sealed class EquipmentItemRepository : EfRepository<EquipmentItem>, IEquipmentItemRepository
{
    public EquipmentItemRepository(AppDbContext db)
        : base(db)
    {
    }

    /// <summary>Трекаемая сущность со справочниками — для мутаций в сервисе.</summary>
    public override Task<EquipmentItem?> GetByIdAsync(long id, CancellationToken ct = default)
        => Set
            .Include(x => x.EquipmentType)
            .Include(x => x.Manufacturer)
            .Include(x => x.SupplyCountry)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<(IReadOnlyList<EquipmentItem> Items, int TotalCount)> SearchAsync(
        EquipmentItemQuery query, CancellationToken ct = default)
    {        
        var total = await ApplyFilters(Set.AsNoTracking(), query, ignoreType: false, ignoreManufacturer: false)
            .CountAsync(ct);

        var items = await ApplySorting(
                ApplyFilters(WithReferences(), query, ignoreType: false, ignoreManufacturer: false), query)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public async Task<FilterOptionsDto> GetFilterOptionIdsAsync(EquipmentItemQuery query, CancellationToken ct = default)
    {
        // Фасеты: типы при всех фильтрах кроме типа, производители — кроме производителя.
        // Include не нужны: Where по навигациям EF разворачивает в JOIN сам, а выбираем скаляры.
        var baseQuery = Set.AsNoTracking();

        var typeIds = await ApplyFilters(baseQuery, query, ignoreType: true, ignoreManufacturer: false)
            .Select(x => x.EquipmentTypeId)
            .Distinct()
            .ToListAsync(ct);

        var manufacturerIds = await ApplyFilters(baseQuery, query, ignoreType: false, ignoreManufacturer: true)
            .Select(x => x.ManufacturerId)
            .Distinct()
            .ToListAsync(ct);

        return new FilterOptionsDto(typeIds, manufacturerIds);
    }

    private IQueryable<EquipmentItem> WithReferences()
        => Set.AsNoTracking()
            .Include(x => x.EquipmentType)
            .Include(x => x.Manufacturer)
            .Include(x => x.SupplyCountry);

    private static IQueryable<EquipmentItem> ApplyFilters(
        IQueryable<EquipmentItem> source, EquipmentItemQuery query, bool ignoreType, bool ignoreManufacturer)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = LikePattern.Contains(query.Search.Trim());
            source = source.Where(x =>
                EF.Functions.ILike(x.Model, pattern) ||
                EF.Functions.ILike(x.EquipmentType.Name, pattern) ||
                EF.Functions.ILike(x.Manufacturer.Name, pattern));
        }

        if (!ignoreType && query.EquipmentTypeId.HasValue)
            source = source.Where(x => x.EquipmentTypeId == query.EquipmentTypeId.Value);

        if (!ignoreManufacturer && query.ManufacturerId.HasValue)
            source = source.Where(x => x.ManufacturerId == query.ManufacturerId.Value);

        if (query.PriceFrom.HasValue)
            source = source.Where(x => x.Price >= query.PriceFrom.Value);

        if (query.PriceTo.HasValue)
            source = source.Where(x => x.Price <= query.PriceTo.Value);

        return source;
    }

    private static IOrderedQueryable<EquipmentItem> ApplySorting(IQueryable<EquipmentItem> source, EquipmentItemQuery query)
    {
        var ordered = query.SortBy.ToLowerInvariant() switch
        {
            EquipmentItemSortFields.Price => query.SortDesc
                ? source.OrderByDescending(x => x.Price)
                : source.OrderBy(x => x.Price),
            EquipmentItemSortFields.Model => query.SortDesc
                ? source.OrderByDescending(x => x.Model)
                : source.OrderBy(x => x.Model),
            EquipmentItemSortFields.CreatedAt => query.SortDesc
                ? source.OrderByDescending(x => x.CreatedAt)
                : source.OrderBy(x => x.CreatedAt),
            _ => query.SortDesc
                ? source.OrderByDescending(x => x.UpdatedAt)
                : source.OrderBy(x => x.UpdatedAt),
        };

        return ordered.ThenBy(x => x.Id);
    }
}
