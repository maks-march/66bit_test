using TechStore.Application.Abstractions;
using TechStore.Application.Common;

namespace TechStore.Application.SupplyCountries;

public sealed class SupplyCountryService : ISupplyCountryService
{
    private readonly ISupplyCountryRepository _countries;

    public SupplyCountryService(ISupplyCountryRepository countries)
    {
        _countries = countries;
    }

    public async Task<Result<IReadOnlyList<SupplyCountryDto>>> AutocompleteAsync(
        string? search, int take = 10, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);

        var items = await _countries.AutocompleteAsync(
            string.IsNullOrWhiteSpace(search) ? null : search.Trim(), take, ct);

        return Result<IReadOnlyList<SupplyCountryDto>>.Success(items.Select(SupplyCountryDto.From).ToList());
    }
}
