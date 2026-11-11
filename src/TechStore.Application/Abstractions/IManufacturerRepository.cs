using TechStore.Domain.Entities;
using TechStore.Domain.Interfaces;

namespace TechStore.Application.Abstractions;

public interface IManufacturerRepository : IRepository<Manufacturer>
{
    /// <summary>Проверка дубля имени (case-insensitive), исключая указанную запись.</summary>
    Task<bool> ExistsByNameAsync(string name, long? excludeId = null, CancellationToken ct = default);

    Task<(IReadOnlyList<Manufacturer> Items, int TotalCount)> SearchAsync(
        string? search, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Число позиций, ссылающихся на производителя (для 409 при удалении).</summary>
    Task<int> CountItemsAsync(long manufacturerId, CancellationToken ct = default);
}
