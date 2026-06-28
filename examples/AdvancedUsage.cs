// Advanced usage example for ProductService
using AspNetSpaTemplate.Services;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Constants;

public class AdvancedUsage
{
    private readonly ProductService _productService;

    public AdvancedUsage(ProductService productService)
    {
        _productService = productService;
    }

    public async Task RunExampleAsync()
    {
        // 1. Creating a product with validation handling
        var newProductRequest = new CreateProductRequest
        {
            Name = "New Advanced Product",
            Price = 99.99m,
            StockQuantity = 50,
            Category = ProductCategory.Electronics
        };

        try
        {
            var createdProduct = await _productService.CreateProductAsync(newProductRequest);
            Console.WriteLine($"Product created with ID: {createdProduct.Id}");
        }
        catch (ValidationException ex)
        {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unexpected error: {ex.Message}");
        }
    }
}
