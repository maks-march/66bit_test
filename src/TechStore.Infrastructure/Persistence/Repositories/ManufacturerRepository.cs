using Microsoft.EntityFrameworkCore;
using TechStore.Application.Abstractions;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Persistence.Repositories;

public sealed class ManufacturerRepository : EfRepository<Manufacturer>, IManufacturerRepository
{
    public ManufacturerRepository(AppDbContext db)
        : base(db)
    {
    }

    public Task<bool> ExistsByNameAsync(string name, long? excludeId = null, CancellationToken ct = default)
        // citext: сравнение == уже case-insensitive.
        => Set.AnyAsync(x => x.Name == name && (excludeId == null || x.Id != excludeId), ct);

    public async Task<(IReadOnlyList<Manufacturer> Items, int TotalCount)> SearchAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = LikePattern.Contains(search.Trim());
            query = query.Where(x =>
                EF.Functions.ILike(x.Name, pattern) ||
                (x.Description != null && EF.Functions.ILike(x.Description, pattern)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    public Task<int> CountItemsAsync(long manufacturerId, CancellationToken ct = default)
        => Db.EquipmentItems.CountAsync(x => x.ManufacturerId == manufacturerId, ct);
}
