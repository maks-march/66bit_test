namespace TechStore.Application.Abstractions;

/// <summary>
/// Проверка magic bytes картинки. Защита от переименованных .exe.
/// Позицию потока не меняет (возвращает на место).
/// </summary>
public static class ImageSignature
{
    public static bool MatchesExtension(Stream content, string normalizedExtension)
    {
        if (!content.CanRead)
            return false;

        Span<byte> header = stackalloc byte[12];
        var start = content.CanSeek ? content.Position : 0;

        var read = 0;
        while (read < header.Length)
        {
            var n = content.Read(header.Slice(read));
            if (n == 0)
                break;
            read += n;
        }

        if (content.CanSeek)
            content.Position = start;

        return normalizedExtension switch
        {
            ".jpg" or ".jpeg" => HasPrefix(header, read, 0xFF, 0xD8, 0xFF),
            ".png" => HasPrefix(header, read, 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A),
            ".webp" => read >= 12
                && header[0] == (byte)'R' && header[1] == (byte)'I' && header[2] == (byte)'F' && header[3] == (byte)'F'
                && header[8] == (byte)'W' && header[9] == (byte)'E' && header[10] == (byte)'B' && header[11] == (byte)'P',
            _ => false,
        };
    }

    private static bool HasPrefix(Span<byte> header, int read, params byte[] prefix)
    {
        if (read < prefix.Length)
            return false;

        for (var i = 0; i < prefix.Length; i++)
        {
            if (header[i] != prefix[i])
                return false;
        }

        return true;
    }
}
