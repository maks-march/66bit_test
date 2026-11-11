using TechStore.Domain.Entities;

namespace TechStore.Domain.Interfaces;

/// <summary>
/// Generic-репозиторий для CRUD. Обновления — через трекинг загруженных сущностей
/// (load-then-mutate). Сложные запросы (поиск, фильтры, фасеты) — в специализированных интерфейсах слоя Application.
/// </summary>
public interface IRepository<T> where T : AuditableEntity
{
    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Remove(T entity);
}
