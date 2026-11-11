namespace TechStore.Application.Abstractions;

/// <summary>
/// Правила приёмки фото. Секция конфигурации — "FileStorage" (биндится в Api;
/// там же Infrastructure читает RootPath/PublicPrefix в свои опции).
/// </summary>
public sealed class FileValidationOptions
{
    public const string SectionName = "FileStorage";

    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];

    public long MaxBytes { get; set; } = 5 * 1024 * 1024;
}
