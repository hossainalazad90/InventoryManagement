using FluentAssertions;
using Inventory.Application.StockTransactions.DTOs;
using Inventory.Application.StockTransactions.Validators;
using Inventory.Domain.Enums;
using Xunit;

namespace Inventory.UnitTests.Validators;

public class StockTransactionValidatorTests
{
    private readonly CreateStockTransactionValidator _validator = new();

    [Fact]
    public void Validate_ValidTransaction_ShouldPass()
    {
        var dto = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Receive,
            1,
            "Valid receipt",
            new List<CreateStockTransactionDetailDto>
            {
                new(1, 50m, 1, "Good condition", DateTime.UtcNow, true)
            }
        );

        var result = _validator.Validate(dto);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_EmptyDetails_ShouldFail()
    {
        var dto = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Receive,
            1,
            "Empty details",
            new List<CreateStockTransactionDetailDto>()
        );

        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "At least one detail row is required.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Validate_ZeroOrNegativeQuantity_ShouldFail(decimal qty)
    {
        var dto = new CreateStockTransactionDto(
            DateTime.UtcNow,
            TransactionType.Receive,
            1,
            "Invalid quantity",
            new List<CreateStockTransactionDetailDto>
            {
                new(1, qty, 1, "Test", DateTime.UtcNow, true)
            }
        );

        var result = _validator.Validate(dto);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "Quantity must be greater than zero.");
    }
}
