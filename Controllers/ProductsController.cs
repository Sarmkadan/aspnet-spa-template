// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// API controller for product management.
/// </summary>
public class ProductsController : ApiControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("{id:int}")]
    [ProduceResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return ApiSuccess(product);
    }

    [HttpGet]
    [ProduceResponseType(typeof(ProductListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var products = await _productService.GetAllProductsAsync(pageNumber, pageSize);
        return ApiSuccess(products);
    }

    [HttpGet("category/{category}")]
    [ProduceResponseType(typeof(ProductListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategory(ProductCategory category, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var products = await _productService.GetProductsByCategoryAsync(category, pageNumber, pageSize);
        return ApiSuccess(products);
    }

    [HttpGet("featured")]
    [ProduceResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeaturedProducts([FromQuery] int limit = 10)
    {
        var products = await _productService.GetFeaturedProductsAsync(limit);
        return ApiSuccess(products);
    }

    [HttpGet("top-rated")]
    [ProduceResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopRatedProducts([FromQuery] int limit = 10)
    {
        var products = await _productService.GetTopRatedProductsAsync(limit);
        return ApiSuccess(products);
    }

    [HttpGet("search")]
    [ProduceResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
    {
        var products = await _productService.SearchProductsAsync(searchTerm);
        return ApiSuccess(products);
    }

    [HttpPost]
    [ProduceResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(request);
        return ApiSuccess(product, "Product created successfully", StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    [ProduceResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productService.UpdateProductAsync(id, request);
        return ApiSuccess(product, "Product updated successfully");
    }

    [HttpPatch("{id:int}/availability")]
    [ProduceResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetAvailability(int id, [FromBody] Dictionary<string, bool> request)
    {
        if (!request.TryGetValue("isAvailable", out var isAvailable))
            return ApiError("isAvailable field is required", "INVALID_REQUEST", StatusCodes.Status400BadRequest);

        await _productService.SetProductAvailabilityAsync(id, isAvailable);
        return NoContent();
    }

    [HttpPatch("{id:int}/featured")]
    [ProduceResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetFeatured(int id, [FromBody] Dictionary<string, bool> request)
    {
        if (!request.TryGetValue("isFeatured", out var isFeatured))
            return ApiError("isFeatured field is required", "INVALID_REQUEST", StatusCodes.Status400BadRequest);

        await _productService.SetProductFeaturedAsync(id, isFeatured);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProduceResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}
