using TechStore.Application.EquipmentItems;

namespace TechStore.Tests.Application.Validators;

public sealed class EquipmentItemValidatorTests
{
    private readonly EquipmentItemQueryValidator _query = new();
    private readonly EquipmentItemCreateValidator _create = new();

    [Fact]
    public async Task Query_PriceToLessThanPriceFrom_FailsOnPriceTo()
    {
        var result = await _query.ValidateAsync(new EquipmentItemQuery { PriceFrom = 100, PriceTo = 50 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, f => f.PropertyName == nameof(EquipmentItemQuery.PriceTo));
    }

    [Theory]
    [InlineData("nonsense")]
    [InlineData("")]
    public async Task Query_BadSortBy_Fails(string sortBy)
    {
        var result = await _query.ValidateAsync(new EquipmentItemQuery { SortBy = sortBy });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, f => f.PropertyName == nameof(EquipmentItemQuery.SortBy));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Query_BadPage_Fails(int page)
    {
        var result = await _query.ValidateAsync(new EquipmentItemQuery { Page = page });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, f => f.PropertyName == nameof(EquipmentItemQuery.Page));
    }

    [Fact]
    public async Task Create_EmptyWrite_FailsOnAllFields()
    {
        var result = await _create.ValidateAsync(new EquipmentItemCreateDto(0, 0, " ", " ", 0));

        Assert.False(result.IsValid);

        var props = result.Errors.Select(f => f.PropertyName).ToHashSet();
        Assert.Contains(nameof(EquipmentItemCreateDto.EquipmentTypeId), props);
        Assert.Contains(nameof(EquipmentItemCreateDto.ManufacturerId), props);
        Assert.Contains(nameof(EquipmentItemCreateDto.Model), props);
        Assert.Contains(nameof(EquipmentItemCreateDto.SupplyCountryName), props);
        Assert.Contains(nameof(EquipmentItemCreateDto.Price), props);
    }

    [Fact]
    public async Task Create_Valid_Passes()
    {
        var result = await _create.ValidateAsync(new EquipmentItemCreateDto(1, 2, "iPhone", "Китай", 100));

        Assert.True(result.IsValid);
    }
}
