namespace TechStore.Application.Abstractions;

/// <summary>
/// Хранилище файлов. Принимает уже проверенный поток
/// (расширение/размер/сигнатура проверяются сервисом до вызова).
/// </summary>
public interface IFileStorage
{
    /// <returns>Относительный путь вида "2026/11/&lt;guid&gt;.webp".</returns>
    Task<string> SaveAsync(Stream content, string fileName, CancellationToken cancellationToken = default);

    Task DeleteAsync(string relativePath, CancellationToken cancellationToken = default);
}
