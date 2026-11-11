using TechStore.Infrastructure.Seed;

namespace TechStore.Tests.Infrastructure;

/// <summary>
/// Проверяет, что фото для сидера реально вшиты в сборку.
/// Без БД: сам SeedAsync гоняется только на живой базе.
/// </summary>
public sealed class SeedPhotosTests
{
    [Theory]
    [InlineData("phone")]
    [InlineData("tv")]
    [InlineData("laptop")]
    [InlineData("fridge")]
    [InlineData("headphones")]
    public void Photo_EmbeddedAndNonEmpty(string name)
    {
        var resource = $"TechStore.Infrastructure.Seed.Photos.{name}.jpg";

        using var stream = typeof(DbSeeder).Assembly.GetManifestResourceStream(resource);

        Assert.NotNull(stream);
        Assert.True(stream.Length > 0);
    }
}
