using TechStore.Domain.Common;

namespace TechStore.Application.Common;

/// <summary>
/// Бизнес-ошибка. Api маппит её в ProblemDetails: код определяет статус
/// (NOT_FOUND → 404, NAME_DUPLICATE/REFERENCE_IN_USE → 409,
/// VALIDATION/PHOTO_* → 400).
/// </summary>
public sealed record Error(
    string Code,
    string Message,
    IReadOnlyDictionary<string, string[]>? FieldErrors = null,
    IReadOnlyDictionary<string, object>? Details = null)
{
    public static Error Validation(IReadOnlyDictionary<string, string[]> fieldErrors)
        => new(ErrorCodes.ValidationError, "Проверьте правильность заполнения полей.", fieldErrors);

    public static Error NotFound(string entityName, object key)
        => new(ErrorCodes.NotFound, $"«{entityName}» с идентификатором {key} не найдена.");

    public static Error Business(string code, string message, IReadOnlyDictionary<string, object>? details = null)
        => new(code, message, null, details);

    public static Error FromDomainException(Domain.Exceptions.DomainException exception)
        => exception is Domain.Exceptions.ConflictException conflict
            ? Business(conflict.Code, conflict.Message, conflict.Details)
            : new Error(exception.Code, exception.Message);
}
