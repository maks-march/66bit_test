namespace TechStore.Domain.Exceptions;

/// <summary>Бизнес-конфликт. Маппится в 409.</summary>
public sealed class ConflictException : DomainException
{
    public IReadOnlyDictionary<string, object>? Details { get; }

    public ConflictException(string code, string message, IReadOnlyDictionary<string, object>? details = null)
        : base(code, message)
    {
        Details = details;
    }
}
