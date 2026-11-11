using Moq;
using TechStore.Application.Abstractions;
using TechStore.Application.Common;
using TechStore.Application.Manufacturers;
using TechStore.Domain.Common;
using TechStore.Domain.Entities;
using TechStore.Domain.Interfaces;

namespace TechStore.Tests.Application.Services;

public sealed class ManufacturerServiceTests
{
    private readonly Mock<IManufacturerRepository> _repo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IInventoryNotifier> _notifier = new();

    private ManufacturerService CreateSut()
        => new(
            _repo.Object,
            _uow.Object,
            new ManufacturerCreateValidator(),
            new ManufacturerUpdateValidator(),
            new PagedQueryValidator<PagedQuery>(),
            Mock.Of<ICurrentUser>(u => u.Name == "tester"),
            _notifier.Object);

    [Fact]
    public async Task Create_DuplicateName_ReturnsNameConflict()
    {
        _repo.Setup(r => r.ExistsByNameAsync("Samsung", null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateSut().CreateAsync(new ManufacturerCreateDto("Samsung", null));

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NameDuplicate, result.Error.Code);
        _repo.Verify(r => r.AddAsync(It.IsAny<Manufacturer>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Delete_UsedByItems_ReturnsReferenceInUseWithCount()
    {
        _repo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Manufacturer.Create("Samsung", null));
        _repo.Setup(r => r.CountItemsAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var result = await CreateSut().DeleteAsync(7);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.ReferenceInUse, result.Error.Code);
        Assert.Equal(3, result.Error.Details!["count"]);
    }

    [Fact]
    public async Task Delete_Missing_ReturnsNotFound()
    {
        _repo.Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Manufacturer?)null);

        var result = await CreateSut().DeleteAsync(7);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NotFound, result.Error.Code);
    }

    [Fact]
    public async Task Create_Valid_SavesAndNotifies()
    {
        _repo.Setup(r => r.ExistsByNameAsync(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await CreateSut().CreateAsync(new ManufacturerCreateDto("Sony", "desc"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Sony", result.Value.Name);
        _repo.Verify(r => r.AddAsync(It.IsAny<Manufacturer>(), It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyAsync(
            It.Is<InventoryChange>(c => c.Kind == InventoryEntityKind.Manufacturer && c.Action == ChangeAction.Created),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
