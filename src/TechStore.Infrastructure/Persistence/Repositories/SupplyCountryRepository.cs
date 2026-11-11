using Microsoft.EntityFrameworkCore;
using TechStore.Application.Abstractions;
using TechStore.Domain.Entities;

namespace TechStore.Infrastructure.Persistence.Repositories;

public sealed class SupplyCountryRepository : EfRepository<SupplyCountry>, ISupplyCountryRepository
{
    public SupplyCountryRepository(AppDbContext db)
        : base(db)
    {
    }

    public Task<SupplyCountry?> FindByNameAsync(string name, CancellationToken ct = default)
        // citext: сравнение == уже case-insensitive (для upsert).
        => Set.FirstOrDefaultAsync(x => x.Name == name, ct);

    public async Task<IReadOnlyList<SupplyCountry>> AutocompleteAsync(
        string? search, int take, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = LikePattern.Contains(search.Trim());
            query = query.Where(x => EF.Functions.ILike(x.Name, pattern));
        }

        return await query
            .OrderBy(x => x.Name)
            .Take(take)
            .ToListAsync(ct);
    }
}
