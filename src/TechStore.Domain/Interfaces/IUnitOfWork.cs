using TechStore.Domain.Entities;

namespace TechStore.Domain.Interfaces;

/// <summary>
/// Единица работы: доступ к репозиториям и атомарное сохранение.
/// </summary>
public interface IUnitOfWork
{
    IRepository<T> Repository<T>() where T : AuditableEntity;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
