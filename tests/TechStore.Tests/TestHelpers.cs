using TechStore.Domain.Entities;

namespace TechStore.Tests;

/// <summary>
/// Id генерирует БД, поэтому в unit-тестах проставляем его рефлексией.
/// </summary>
internal static class TestHelpers
{
    public static T WithId<T>(T entity, long id)
        where T : AuditableEntity
    {
        typeof(AuditableEntity)
            .GetProperty(nameof(AuditableEntity.Id))!
            .GetSetMethod(nonPublic: true)!
            .Invoke(entity, [id]);

        return entity;
    }
}
