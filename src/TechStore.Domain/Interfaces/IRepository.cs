using TechStore.Domain.Entities;

namespace TechStore.Domain.Interfaces;

/// <summary>
/// Generic-репозиторий. <see cref="Query"/> отдаёт IQueryable (это LINQ, а не EF),
/// чтобы Application мог компоновать Specification-фильтры.
/// </summary>
public interface IRepository<T> where T : AuditableEntity
{
    IQueryable<T> Query();

    Task<T?> GetByIdAsync(long id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default);

    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    void Update(T entity);

    void Remove(T entity);
}
