// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Models;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public class ProductModelTests
{
    [Fact]
    public void IsInStock_WhenAvailableAndHasStock_ReturnsTrue()
    {
        // Arrange
        var product = new Product { IsAvailable = true, StockQuantity = 5 };

        // Act
        var result = product.IsInStock();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsInStock_WhenProductIsNotAvailable_ReturnsFalse()
    {
        // Arrange
        var product = new Product { IsAvailable = false, StockQuantity = 10 };

        // Act
        var result = product.IsInStock();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ReduceStock_WhenQuantityIsValid_DecreasesStockAndSetsUpdatedAt()
    {
        // Arrange
        var product = new Product { IsAvailable = true, StockQuantity = 10 };

        // Act
        product.ReduceStock(3);

        // Assert
        product.StockQuantity.Should().Be(7);
        product.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void ReduceStock_WhenQuantityExceedsAvailableStock_ThrowsInvalidOperationException()
    {
        // Arrange
        var product = new Product { IsAvailable = true, StockQuantity = 2 };

        // Act
        var act = () => product.ReduceStock(5);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot reduce stock*");
    }
}
