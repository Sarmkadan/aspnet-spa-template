#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Models;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="Product"/> model to verify its behavior and functionality.
/// </summary>
public sealed class ProductModelTests
{
    /// <summary>
    /// Tests that <see cref="Product.IsInStock()"/> returns true when a product is available and has positive stock quantity.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Product.IsInStock()"/> returns false when a product is not available, regardless of stock quantity.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Product.ReduceStock(int)"/> decreases the stock quantity and updates the UpdatedAt timestamp when the reduction is valid.
    /// </summary>
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

    /// <summary>
    /// Tests that <see cref="Product.ReduceStock(int)"/> throws an <see cref="InvalidOperationException"/> when attempting to reduce stock by a quantity that exceeds the available stock.
    /// </summary>
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
