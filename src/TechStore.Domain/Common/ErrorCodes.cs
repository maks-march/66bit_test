namespace TechStore.Domain.Common;

/// <summary>
/// Стабильные коды ошибок. Единый источник правды для Domain и Application:
/// фронт маппит их на поля формы и тексты.
/// </summary>
public static class ErrorCodes
{
    public const string Required = "REQUIRED";
    public const string TooLong = "TOO_LONG";
    public const string InvalidPrice = "INVALID_PRICE";
    public const string InvalidId = "INVALID_ID";

    public const string NotFound = "NOT_FOUND";
    public const string NameDuplicate = "NAME_DUPLICATE";
    public const string ReferenceInUse = "REFERENCE_IN_USE";
    public const string ValidationError = "VALIDATION";

    public const string PhotoInvalid = "PHOTO_INVALID";
    public const string PhotoTooLarge = "PHOTO_TOO_LARGE";
}
