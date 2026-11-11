using TechStore.Domain.Entities;
using TechStore.Domain.Interfaces;

namespace TechStore.Application.Abstractions;

public interface ISupplyCountryRepository : IRepository<SupplyCountry>
{
    /// <summary>Поиск страны по имени (case-insensitive) для upsert.</summary>
    Task<SupplyCountry?> FindByNameAsync(string name, CancellationToken ct = default);

    Task<IReadOnlyList<SupplyCountry>> AutocompleteAsync(string? search, int take, CancellationToken ct = default);
}
