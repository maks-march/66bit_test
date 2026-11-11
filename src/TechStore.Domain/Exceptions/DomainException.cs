namespace TechStore.Domain.Exceptions;

/// <summary>
/// Базовое доменное исключение. Маппится в ProblemDetails по <see cref="Code"/>.
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message)
        : base(message)
    {
        Code = code;
    }
}
