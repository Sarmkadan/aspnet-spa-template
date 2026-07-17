// Basic usage example for ProductService
using AspNetSpaTemplate.Services;
using AspNetSpaTemplate.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AspNetSpaTemplate.Data;

/// <summary>
/// Provides basic usage examples for the ProductService class.
/// </summary>
/// <remarks>
/// This example demonstrates how to instantiate and use ProductService
/// with dependency injection in an ASP.NET Core application context.
/// </remarks>
public class BasicUsage
{
    private readonly ProductService _productService;

    /// <summary>
    /// Initializes a new instance of the BasicUsage class.
    /// </summary>
    /// <param name="productRepository">The product repository instance.</param>
    /// <param name="logger">The logger instance for ProductService.</param>
    public BasicUsage(ProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productService = new ProductService(productRepository, logger);
    }

    /// <summary>
    /// Runs the basic usage examples for ProductService asynchronously.
    /// </summary>
    /// <returns>A Task representing the asynchronous operation.</returns>
    public async Task RunExampleAsync()
    {
        // 1. Get a product by ID
        var productId = 1;
        try
        {
            var product = await _productService.GetProductByIdAsync(productId);
            Console.WriteLine($"Found product: {product?.Name}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching product: {ex.Message}");
        }

        // 2. Get all products (paginated)
        var products = await _productService.GetAllProductsAsync(pageNumber: 1, pageSize: 5);
        Console.WriteLine($"Total products: {products.TotalCount}");
    }
}
