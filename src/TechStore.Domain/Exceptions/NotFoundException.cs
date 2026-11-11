using TechStore.Domain.Common;

namespace TechStore.Domain.Exceptions;

/// <summary>Сущность не найдена. Маппится в 404.</summary>
public sealed class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object Key { get; }

    public NotFoundException(string entityName, object key)
        : base(ErrorCodes.NotFound, $"«{entityName}» с идентификатором {key} не найдена.")
    {
        EntityName = entityName;
        Key = key;
    }
}
