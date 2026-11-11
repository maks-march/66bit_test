using Microsoft.EntityFrameworkCore;
using Npgsql;
using TechStore.Domain.Common;
using TechStore.Domain.Exceptions;
using TechStore.Domain.Interfaces;

namespace TechStore.Infrastructure.Persistence;

/// <summary>
/// Граница транзакции и трансляция ошибок БД в доменные.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public UnitOfWork(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        try
        {
            return await _db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new ConflictException(
                ErrorCodes.NameDuplicate,
                "Запись с таким именем уже существует. Обновите данные и повторите.");
        }
    }
}
