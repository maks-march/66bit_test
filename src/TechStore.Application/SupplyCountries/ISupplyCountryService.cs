using TechStore.Application.Common;

namespace TechStore.Application.SupplyCountries;

/// <summary>
/// Только чтение для автокомплита. Создание стран — upsert внутри EquipmentItemService,
/// в одной транзакции с позицией.
/// </summary>
public interface ISupplyCountryService
{
    Task<Result<IReadOnlyList<SupplyCountryDto>>> AutocompleteAsync(
        string? search, int take = 10, CancellationToken ct = default);
}
