// Basic usage example for ProductService
using AspNetSpaTemplate.Services;
using AspNetSpaTemplate.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using AspNetSpaTemplate.Data;

// Note: This example assumes you have an instance of AppDbContext 
// and ProductRepository set up, or are running within the ASP.NET Core context.

public class BasicUsage
{
    private readonly ProductService _productService;

    public BasicUsage(ProductRepository productRepository)
    {
        _productService = new ProductService(productRepository);
    }

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
