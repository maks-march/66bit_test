using TechStore.Domain.Entities;

namespace TechStore.Application.SupplyCountries;

public sealed record SupplyCountryDto(long Id, string Name)
{
    public static SupplyCountryDto From(SupplyCountry entity)
        => new(entity.Id, entity.Name);
}
