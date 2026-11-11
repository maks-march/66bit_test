using TechStore.Application.Abstractions;

namespace TechStore.Tests.Application.Files;

public sealed class ImageSignatureTests
{
    [Theory]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ".jpg", true)]
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ".jpeg", true)]
    [InlineData(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ".png", true)]
    [InlineData(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0, (byte)'W', (byte)'E', (byte)'B', (byte)'P' }, ".webp", true)]
    [InlineData(new byte[] { 0x25, 0x50, 0x44, 0x46 }, ".jpg", false)] // %PDF
    [InlineData(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ".png", false)]
    [InlineData(new byte[] { 0x00 }, ".webp", false)]
    public void MatchesExtension_ClassifiesHeaders(byte[] header, string extension, bool expected)
    {
        using var stream = new MemoryStream(header);

        Assert.Equal(expected, ImageSignature.MatchesExtension(stream, extension));
        Assert.Equal(0, stream.Position); // позицию потока не сдвигаем
    }
}
