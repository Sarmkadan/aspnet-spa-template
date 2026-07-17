#nullable enable

using System.Globalization;
using AspNetSpaTemplate.Models;
using FluentAssertions;
using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Extension methods for <see cref="ProductServiceTests"/> that provide additional test utilities
/// and assertions for product service testing scenarios.
/// </summary>
public static class ProductServiceTestsExtensions
{
    /// <summary>
    /// Creates a sample product for testing purposes.
    /// </summary>
    /// <param name="name">The product name. Cannot be null or empty.</param>
    /// <param name="price">The product price. Must be positive. If null, defaults to 99.99.</param>
    /// <param name="category">The product category. If null, defaults to Electronics.</param>
    /// <returns>A configured <see cref="Product"/> instance ready for testing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="price"/> is less than or equal to 0.</exception>
    public static Product CreateTestProduct(
        string name,
        decimal? price = null,
        ProductCategory? category = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(price ?? 99.99m, 0);

        return new Product
        {
            Id = 1,
            Name = name,
            Description = "A test product for unit testing",
            Price = price ?? 99.99m,
            StockQuantity = 100,
            IsAvailable = true,
            Category = category ?? ProductCategory.Electronics,
            Sku = "TEST-001",
            Rating = 4.5m
        };
    }

    /// <summary>
    /// Creates a sample product with price validation errors for negative price testing.
    /// </summary>
    /// <param name="name">The product name. Cannot be null or empty.</param>
    /// <returns>A product with negative price for validation testing.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="name"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="name"/> is empty.</exception>
    public static Product CreateInvalidPriceProduct(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        return new Product
        {
            Id = 1,
            Name = name,
            Price = -10.00m,
            StockQuantity = 100,
            IsAvailable = true,
            Category = ProductCategory.Electronics,
            Sku = "TEST-002"
        };
    }

    /// <summary>
    /// Asserts that a product matches the expected values with culture-invariant comparison.
    /// </summary>
    /// <param name="product">The product to assert.</param>
    /// <param name="expectedName">Expected product name.</param>
    /// <param name="expectedPrice">Expected product price.</param>
    /// <param name="expectedStock">Expected stock quantity.</param>
    /// <exception cref="ArgumentNullException"><paramref name="product"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="expectedName"/> is null or empty.</exception>
    public static void ShouldMatchExpectedValues(
        this Product product,
        string expectedName,
        decimal expectedPrice,
        int expectedStock)
    {
        ArgumentNullException.ThrowIfNull(product);
        ArgumentException.ThrowIfNullOrEmpty(expectedName);

        product.Should().NotBeNull();
        product.Name.Should().Be(expectedName, "Product name should match expected value");
        product.Price.Should().BeApproximately(expectedPrice, 0.01m, "Product price should match expected value");
        product.StockQuantity.Should().Be(expectedStock, "Product stock quantity should match expected value");
        product.IsAvailable.Should().BeTrue("Product should be available by default");
    }

    /// <summary>
    /// Creates a collection of products for pagination testing.
    /// </summary>
    /// <param name="count">Number of products to create. Must be positive.</param>
    /// <param name="category">Optional category to assign to all products.</param>
    /// <returns>An <see cref="IReadOnlyList{T}"/> of products ready for pagination testing.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="count"/> is less than or equal to 0.</exception>
    public static IReadOnlyList<Product> CreateProductCollection(
        int count,
        ProductCategory? category = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(count, 0);

        var products = new List<Product>(count);
        for (var i = 0; i < count; i++)
        {
            products.Add(CreateTestProduct(
                name: $"Product {i + 1}",
                price: 10.00m * (i + 1),
                category: category ?? ProductCategory.Electronics
            ));
        }

        return products.AsReadOnly();
    }
}
