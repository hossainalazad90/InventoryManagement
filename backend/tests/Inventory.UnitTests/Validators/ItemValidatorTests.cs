using FluentAssertions;
using Inventory.Application.Items.DTOs;
using Inventory.Application.Items.Validators;
using Xunit;

namespace Inventory.UnitTests.Validators;

public class ItemValidatorTests
{
    private readonly CreateItemDtoValidator _validator = new();

    [Fact]
    public void Validate_ValidItem_ShouldPass()
    {
        var dto = new CreateItemDto("ITEM-100", "Bearing Assembly", 1, 1, 10m, true);
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Item Code is required.")]
    [InlineData("   ", "Item Code is required.")]
    public void Validate_EmptyItemCode_ShouldFail(string code, string expectedMsg)
    {
        var dto = new CreateItemDto(code, "Bearing Assembly", 1, 1, 10m, true);
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedMsg);
    }

    [Fact]
    public void Validate_NegativeReorderLevel_ShouldFail()
    {
        var dto = new CreateItemDto("ITEM-100", "Bearing Assembly", 1, 1, -5m, true);
        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Reorder Level cannot be negative.");
    }
}
