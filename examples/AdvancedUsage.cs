// Advanced usage example for ProductService 2
using AspNetSpaTemplate.Services;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Constants;

/// <summary>
/// Provides advanced usage examples for the ProductService demonstrating error handling,
/// validation scenarios, and integration patterns with ASP.NET Core dependency injection.
/// </summary>
public class AdvancedUsage
{
	private readonly ProductService _productService;

	/// <summary>
	/// Initializes a new instance of the AdvancedUsage class with ProductService dependency injection.
	/// </summary>
	/// <param name="productService">The ProductService instance to use for product operations.</param>
	public AdvancedUsage(ProductService productService)
	{
		_productService = productService;
	}

	/// <summary>
	/// Demonstrates advanced usage patterns for ProductService including validation handling,
	/// error scenarios, and asynchronous operations.
	/// </summary>
	/// <returns>A Task representing the asynchronous operation.</returns>
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