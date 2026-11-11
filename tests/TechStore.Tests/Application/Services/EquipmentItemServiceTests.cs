using Microsoft.Extensions.Options;
using Moq;
using TechStore.Application.Abstractions;
using TechStore.Application.Common;
using TechStore.Application.EquipmentItems;
using TechStore.Domain.Common;
using TechStore.Domain.Entities;
using TechStore.Domain.Interfaces;

namespace TechStore.Tests.Application.Services;

public sealed class EquipmentItemServiceTests
{
    private sealed class Harness
    {
        public Mock<IEquipmentItemRepository> Items { get; } = new();
        public Mock<IEquipmentTypeRepository> Types { get; } = new();
        public Mock<IManufacturerRepository> Makers { get; } = new();
        public Mock<ISupplyCountryRepository> Countries { get; } = new();
        public Mock<IUnitOfWork> Uow { get; } = new();
        public Mock<IFileStorage> Files { get; } = new();
        public Mock<IInventoryNotifier> Notifier { get; } = new();
        public FileValidationOptions FileOptions { get; } = new();

        public EquipmentItemService Sut()
            => new(
                Items.Object,
                Types.Object,
                Makers.Object,
                Countries.Object,
                Uow.Object,
                new EquipmentItemCreateValidator(),
                new EquipmentItemUpdateValidator(),
                new EquipmentItemQueryValidator(),
                Mock.Of<ICurrentUser>(u => u.Name == "tester"),
                Notifier.Object,
                Files.Object,
                Options.Create(FileOptions));
    }

    private static void GivenReferences(Harness h)
    {
        h.Types.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.WithId(EquipmentType.Create("Телефон", null), 1));
        h.Makers.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.WithId(Manufacturer.Create("Sony", null), 2));
        h.Uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
    }

    [Fact]
    public async Task Create_NewCountry_AddsCountryAndFlushesForId()
    {
        var h = new Harness();
        GivenReferences(h);
        h.Countries.Setup(r => r.FindByNameAsync("Япония", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SupplyCountry?)null);
        // Эмулируем EF: при сохранении новой стране присваивается Id.
        h.Countries.Setup(r => r.AddAsync(It.IsAny<SupplyCountry>(), It.IsAny<CancellationToken>()))
            .Callback<SupplyCountry, CancellationToken>((c, _) => TestHelpers.WithId(c, 9));

        var result = await h.Sut().CreateAsync(new EquipmentItemCreateDto(1, 2, "WH-1000XM5", "Япония", 34_990m));

        Assert.True(result.IsSuccess);
        Assert.Equal("Япония", result.Value.SupplyCountry.Name);
        h.Countries.Verify(r => r.AddAsync(
            It.Is<SupplyCountry>(c => c.Name == "Япония"), It.IsAny<CancellationToken>()), Times.Once);
        h.Items.Verify(r => r.AddAsync(It.IsAny<EquipmentItem>(), It.IsAny<CancellationToken>()), Times.Once);
        // Флаш страны для Id + сохранение позиции.
        h.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Create_ExistingCountry_SavesOnce()
    {
        var h = new Harness();
        GivenReferences(h);
        h.Countries.Setup(r => r.FindByNameAsync("Китай", It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.WithId(SupplyCountry.Create("Китай"), 9));

        var result = await h.Sut().CreateAsync(new EquipmentItemCreateDto(1, 2, "Redmi", "Китай", 10_000m));

        Assert.True(result.IsSuccess);
        h.Countries.Verify(r => r.AddAsync(It.IsAny<SupplyCountry>(), It.IsAny<CancellationToken>()), Times.Never);
        h.Uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Create_MissingType_ReturnsNotFound()
    {
        var h = new Harness();
        h.Types.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((EquipmentType?)null);

        var result = await h.Sut().CreateAsync(new EquipmentItemCreateDto(1, 2, "Redmi", "Китай", 10_000m));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error.Code);
        h.Items.Verify(r => r.AddAsync(It.IsAny<EquipmentItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetPhoto_UnsupportedExtension_ReturnsPhotoInvalid()
    {
        var h = new Harness();
        h.Items.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.WithId(EquipmentItem.Create(1, 1, 1, "M", 100m, null), 5));

        using var stream = new MemoryStream([0xFF, 0xD8, 0xFF, 0xE0]);
        var result = await h.Sut().SetPhotoAsync(5, stream, "evil.exe");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.PhotoInvalid, result.Error.Code);
        h.Files.Verify(f => f.SaveAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SetPhoto_TooLarge_ReturnsPhotoTooLarge()
    {
        var h = new Harness();
        h.FileOptions.MaxBytes = 10;
        h.Items.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.WithId(EquipmentItem.Create(1, 1, 1, "M", 100m, null), 5));

        using var stream = new MemoryStream(new byte[20]);
        var result = await h.Sut().SetPhotoAsync(5, stream, "photo.jpg");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.PhotoTooLarge, result.Error.Code);
    }

    [Fact]
    public async Task SetPhoto_BadSignature_ReturnsPhotoInvalid()
    {
        var h = new Harness();
        h.Items.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestHelpers.WithId(EquipmentItem.Create(1, 1, 1, "M", 100m, null), 5));

        using var stream = new MemoryStream([0x25, 0x50, 0x44, 0x46]); // %PDF
        var result = await h.Sut().SetPhotoAsync(5, stream, "photo.jpg");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.PhotoInvalid, result.Error.Code);
    }

    [Fact]
    public async Task Delete_WithPhoto_RemovesItemAndDeletesFile()
    {
        var h = new Harness();
        var item = TestHelpers.WithId(EquipmentItem.Create(1, 1, 1, "M", 100m, null), 5);
        item.SetPhoto("2026/11/x.png", "tester");
        h.Items.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(item);
        h.Uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await h.Sut().DeleteAsync(5);

        Assert.True(result.IsSuccess);
        h.Items.Verify(r => r.Remove(item), Times.Once);
        h.Files.Verify(f => f.DeleteAsync("2026/11/x.png", It.IsAny<CancellationToken>()), Times.Once);
    }
}
