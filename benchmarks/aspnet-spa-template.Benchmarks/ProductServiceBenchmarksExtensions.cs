using AspNetSpaTemplate.Benchmarks;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Provides extension methods for the <see cref="ProductServiceBenchmarks"/> class.
/// </summary>
public static class ProductServiceBenchmarksExtensions
{
    /// <summary>
    /// Retrieves all products and returns the average price.
    /// </summary>
    /// <param name="benchmarks">The <see cref="ProductServiceBenchmarks"/> instance.</param>
    /// <returns>The average price of all products.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
    public static async Task<decimal> GetAverageProductPriceAsync(this ProductServiceBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        var products = await benchmarks.GetAllProducts();
        return products.Average(p => p.Price);
    }

    /// <summary>
    /// Retrieves a product by ID and returns its details as a string.
    /// </summary>
    /// <param name="benchmarks">The <see cref="ProductServiceBenchmarks"/> instance.</param>
    /// <param name="id">The ID of the product to retrieve.</param>
    /// <returns>A string representation of the product details.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="id"/> is less than or equal to 0.</exception>
    public static async Task<string> GetProductDetailsAsStringAsync(this ProductServiceBenchmarks benchmarks, int id)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        if (id <= 0) throw new ArgumentException("ID must be greater than 0", nameof(id));
        var product = await benchmarks.GetProductById();
        return product is not null ? $"ID: {product.Id}, Name: {product.Name}, Price: {product.Price}" : "Product not found";
    }

    /// <summary>
    /// Retrieves the featured products and returns the count of products with a price greater than the specified threshold.
    /// </summary>
    /// <param name="benchmarks">The <see cref="ProductServiceBenchmarks"/> instance.</param>
    /// <param name="threshold">The price threshold.</param>
    /// <returns>The count of featured products with a price greater than the threshold.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
    public static async Task<int> GetFeaturedProductsCountAboveThresholdAsync(this ProductServiceBenchmarks benchmarks, decimal threshold)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        var featuredProducts = await benchmarks.GetFeaturedProducts();
        return featuredProducts.Count(p => p.Price > threshold);
    }

    /// <summary>
    /// Retrieves all products and returns the product with the highest price.
    /// </summary>
    /// <param name="benchmarks">The <see cref="ProductServiceBenchmarks"/> instance.</param>
    /// <returns>The product with the highest price, or null if no products are found.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="benchmarks"/> is null.</exception>
    public static async Task<AspNetSpaTemplate.DTOs.ProductResponse?> GetProductWithHighestPriceAsync(this ProductServiceBenchmarks benchmarks)
    {
        ArgumentNullException.ThrowIfNull(benchmarks);
        var products = await benchmarks.GetAllProducts();
        return products.OrderByDescending(p => p.Price).FirstOrDefault();
    }
}
