using FluentAssertions;
using Inventory.Domain.Entities;
using Inventory.Domain.Enums;
using Xunit;

namespace Inventory.UnitTests.Domain;

public class StockMovementTests
{
    [Fact]
    public void CalculateSignedQuantity_Receive_ReturnsPositiveQuantity()
    {
        // Arrange
        var qty = 100m;

        // Act
        var signed = StockMovement.CalculateSignedQuantity(TransactionType.Receive, qty);

        // Assert
        signed.Should().Be(100m);
    }

    [Fact]
    public void CalculateSignedQuantity_Issue_ReturnsNegativeQuantity()
    {
        // Arrange
        var qty = 45.5m;

        // Act
        var signed = StockMovement.CalculateSignedQuantity(TransactionType.Issue, qty);

        // Assert
        signed.Should().Be(-45.5m);
    }

    [Fact]
    public void CalculateSignedQuantity_Return_ReturnsPositiveQuantity()
    {
        // Arrange
        var qty = 10m;

        // Act
        var signed = StockMovement.CalculateSignedQuantity(TransactionType.Return, qty);

        // Assert
        signed.Should().Be(10m);
    }
}
