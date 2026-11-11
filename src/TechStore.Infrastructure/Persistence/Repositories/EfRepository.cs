using Microsoft.EntityFrameworkCore;
using TechStore.Domain.Entities;
using TechStore.Domain.Interfaces;

namespace TechStore.Infrastructure.Persistence.Repositories;

/// <summary>Базовый CRUD поверх DbSet. Спец-репозитории наследуют и добавляют запросы.</summary>
public abstract class EfRepository<T> : IRepository<T>
    where T : AuditableEntity
{
    protected readonly AppDbContext Db;
    protected DbSet<T> Set => Db.Set<T>();

    protected EfRepository(AppDbContext db)
    {
        Db = db;
    }

    public virtual Task<T?> GetByIdAsync(long id, CancellationToken ct = default)
        => Set.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task AddAsync(T entity, CancellationToken ct = default)
    {
        await Set.AddAsync(entity, ct);
    }

    public void Remove(T entity)
    {
        Set.Remove(entity);
    }
}
