namespace TechStore.Web.Services;

/// <summary>
/// Ошибка API: ProblemDetails (code + detail) или HttpValidationProblemDetails (errors по полям).
/// Сетевые сбои сюда не попадают — там летит HttpRequestException.
/// </summary>
public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public string? Code { get; }
    public Dictionary<string, string[]>? FieldErrors { get; }

    public ApiException(int statusCode, string message, string? code = null, Dictionary<string, string[]>? fieldErrors = null)
        : base(message)
    {
        StatusCode = statusCode;
        Code = code;
        FieldErrors = fieldErrors;
    }
}
