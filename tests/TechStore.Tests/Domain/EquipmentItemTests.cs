using TechStore.Domain.Common;
using TechStore.Domain.Entities;
using TechStore.Domain.Exceptions;

namespace TechStore.Tests.Domain;

public sealed class EquipmentItemTests
{
    [Fact]
    public void Create_PriceZero_ThrowsInvalidPrice()
    {
        var ex = Assert.Throws<DomainException>(() =>
            EquipmentItem.Create(1, 1, 1, "iPhone", 0m, null));

        Assert.Equal(ErrorCodes.InvalidPrice, ex.Code);
    }

    [Fact]
    public void Create_PriceAboveMax_ThrowsInvalidPrice()
    {
        var ex = Assert.Throws<DomainException>(() =>
            EquipmentItem.Create(1, 1, 1, "iPhone", EquipmentItem.MaxPrice + 0.01m, null));

        Assert.Equal(ErrorCodes.InvalidPrice, ex.Code);
    }

    [Fact]
    public void Create_EmptyModel_ThrowsRequired()
    {
        var ex = Assert.Throws<DomainException>(() =>
            EquipmentItem.Create(1, 1, 1, "   ", 100m, null));

        Assert.Equal(ErrorCodes.Required, ex.Code);
    }

    [Fact]
    public void Create_Valid_NormalizesAndAudits()
    {
        var before = DateTime.UtcNow;

        var item = EquipmentItem.Create(1, 2, 3, "  Galaxy S24  ", 79990.999m, null, "tester");

        Assert.Equal("Galaxy S24", item.Model);
        Assert.Equal(79991m, item.Price);
        Assert.Equal("tester", item.UpdatedBy);
        Assert.True(item.CreatedAt >= before && item.CreatedAt <= DateTime.UtcNow);
        Assert.Equal(item.CreatedAt, item.UpdatedAt);
    }
}
