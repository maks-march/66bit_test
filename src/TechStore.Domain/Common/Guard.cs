using TechStore.Domain.Exceptions;

namespace TechStore.Domain.Common;

/// <summary>
/// Доменные проверки инвариантов. Бросает <see cref="DomainException"/> с русскими сообщениями.
/// </summary>
public static class Guard
{
    /// <summary>Обязательная строка: непустая после Trim, опционально — с лимитом длины.</summary>
    public static string Required(string? value, string fieldName, int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(ErrorCodes.Required, $"Поле «{fieldName}» обязательно для заполнения.");

        var trimmed = value.Trim();

        if (maxLength.HasValue && trimmed.Length > maxLength.Value)
            throw new DomainException(
                ErrorCodes.TooLong,
                $"Поле «{fieldName}» не должно быть длиннее {maxLength} символов.");

        return trimmed;
    }

    /// <summary>Необязательная строка: null/пустая превращается в null, иначе Trim + лимит длины.</summary>
    public static string? Optional(string? value, string fieldName, int? maxLength = null)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        if (maxLength.HasValue && trimmed.Length > maxLength.Value)
            throw new DomainException(
                ErrorCodes.TooLong,
                $"Поле «{fieldName}» не должно быть длиннее {maxLength} символов.");

        return trimmed;
    }

    public static long PositiveId(long value, string fieldName)
    {
        if (value <= 0)
            throw new DomainException(ErrorCodes.InvalidId, $"Некорректный идентификатор в поле «{fieldName}».");

        return value;
    }
}
