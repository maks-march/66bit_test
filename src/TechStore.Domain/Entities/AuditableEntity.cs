using TechStore.Domain.Common;

namespace TechStore.Domain.Entities;

/// <summary>
/// База всех сущностей.
/// </summary>
public abstract class AuditableEntity
{
    public const int UpdatedByMaxLength = 100;

    public long Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime UpdatedAt { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    protected AuditableEntity()
    {
    }

    protected void MarkCreated(string? createdBy)
    {
        var now = DateTime.UtcNow;
        CreatedAt = now;
        UpdatedAt = now;
        UpdatedBy = Guard.Optional(createdBy, "Автор", UpdatedByMaxLength);
    }

    protected void Touch(string? updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;

        var normalized = Guard.Optional(updatedBy, "Автор", UpdatedByMaxLength);
        if (normalized is not null)
            UpdatedBy = normalized;
    }
}
