using FluentValidation.Results;

namespace TechStore.Application.Common;

internal static class ValidationMapping
{
    /// <summary>
    /// Расширение для перевода ошибки валидации в ошибку Application <see cref="Error"/>
    /// </summary>
    public static Error ToError(this ValidationResult result)
    {
        var fieldErrors = result.Errors
            .GroupBy(f => string.IsNullOrWhiteSpace(f.PropertyName) ? "general" : f.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).Distinct().ToArray());

        return Error.Validation(fieldErrors);
    }
}
