namespace TechStore.Infrastructure.Files;

/// <summary>
/// Пути файлового хранилища. Та же секция "FileStorage", что и FileValidationOptions
/// из Application (каждый класс биндит свои свойства).
/// </summary>
public sealed class FileStorageOptions
{
    public const string SectionName = "FileStorage";

    /// <summary>Абсолютный путь (в compose — /data/uploads) или относительный от ContentRoot.</summary>
    public string RootPath { get; set; } = "uploads";

    public string PublicPrefix { get; set; } = "/uploads";
}
