using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using TechStore.Application.Abstractions;

namespace TechStore.Infrastructure.Files;

/// <summary>Файлы на диске: {Root}/{yyyy}/{MM}/{guid}.{ext}. В БД хранится относительный путь.</summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;

    public LocalFileStorage(IOptions<FileStorageOptions> options, IHostEnvironment environment)
    {
        var root = options.Value.RootPath;
        _rootPath = Path.IsPathRooted(root) ? root : Path.Combine(environment.ContentRootPath, root);
    }

    public async Task<string> SaveAsync(Stream content, string fileName, CancellationToken ct = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var now = DateTime.UtcNow;
        var relativeDir = Path.Combine(now.ToString("yyyy"), now.ToString("MM"));
        var directory = Path.Combine(_rootPath, relativeDir);
        Directory.CreateDirectory(directory);

        var name = $"{Guid.NewGuid():N}{extension}";
        await using var output = new FileStream(
            Path.Combine(directory, name), FileMode.CreateNew, FileAccess.Write, FileShare.None,
            bufferSize: 81920, useAsync: true);

        if (content.CanSeek)
            content.Position = 0;

        await content.CopyToAsync(output, ct);

        return Path.Combine(relativeDir, name).Replace(Path.DirectorySeparatorChar, '/');
    }

    public Task DeleteAsync(string relativePath, CancellationToken ct = default)
    {
        // Защита от path traversal: удаляем только внутри RootPath.
        var root = Path.GetFullPath(_rootPath) + Path.DirectorySeparatorChar;
        var full = Path.GetFullPath(Path.Combine(_rootPath, relativePath));

        if (!full.StartsWith(root, StringComparison.Ordinal))
            return Task.CompletedTask;

        if (File.Exists(full))
            File.Delete(full);

        return Task.CompletedTask;
    }
}
