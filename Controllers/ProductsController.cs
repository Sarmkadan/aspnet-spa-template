#nullable enable
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
public sealed class ProductsController : ApiControllerBase
{
    private readonly ProductService _productService;

    public ProductsController(ProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Retrieves a single product by its unique identifier.</summary>
    /// <param name="id">The product ID.</param>
    /// <response code="200">Returns the requested product.</response>
    /// <response code="404">Product with the specified ID was not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SuccessResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return ApiSuccess(product);
    }

    /// <summary>Retrieves a paginated list of available products.</summary>
    /// <param name="pageNumber">Page number (1-based).</param>
    /// <param name="pageSize">Number of items per page.</param>
    [HttpGet]
    [ProducesResponseType(typeof(SuccessResponse<ProductListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var products = await _productService.GetAllProductsAsync(pageNumber, pageSize);
        return ApiSuccess(products);
    }

    /// <summary>Retrieves products filtered by category with pagination.</summary>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(SuccessResponse<ProductListResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategory(ProductCategory category, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var products = await _productService.GetProductsByCategoryAsync(category, pageNumber, pageSize);
        return ApiSuccess(products);
    }

    /// <summary>Returns featured products (limited to <paramref name="limit"/>).</summary>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(SuccessResponse<List<ProductResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeaturedProducts([FromQuery] int limit = 10)
    {
        var products = await _productService.GetFeaturedProductsAsync(limit);
        return ApiSuccess(products);
    }

    /// <summary>Returns top-rated products sorted by average rating.</summary>
    [HttpGet("top-rated")]
    [ProducesResponseType(typeof(SuccessResponse<List<ProductResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopRatedProducts([FromQuery] int limit = 10)
    {
        var products = await _productService.GetTopRatedProductsAsync(limit);
        return ApiSuccess(products);
    }

    /// <summary>Searches products by name or description.</summary>
    /// <param name="searchTerm">Search query string.</param>
    [HttpGet("search")]
    [ProducesResponseType(typeof(SuccessResponse<List<ProductResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
    {
        var products = await _productService.SearchProductsAsync(searchTerm);
        return ApiSuccess(products);
    }

    /// <summary>Creates a new product.</summary>
    /// <response code="201">Product created successfully.</response>
    /// <response code="400">Validation error in the request body.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SuccessResponse<ProductResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(request);
        return ApiSuccess(product, "Product created successfully", StatusCodes.Status201Created);
    }

    /// <summary>Updates an existing product.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SuccessResponse<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productService.UpdateProductAsync(id, request);
        return ApiSuccess(product, "Product updated successfully");
    }

    /// <summary>Toggles product availability.</summary>
    [HttpPatch("{id:int}/availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetAvailability(int id, [FromBody] Dictionary<string, bool> request)
    {
        if (!request.TryGetValue("isAvailable", out var isAvailable))
            return ApiError("isAvailable field is required", "INVALID_REQUEST", StatusCodes.Status400BadRequest);

        await _productService.SetProductAvailabilityAsync(id, isAvailable);
        return NoContent();
    }

    /// <summary>Toggles product featured status.</summary>
    [HttpPatch("{id:int}/featured")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetFeatured(int id, [FromBody] Dictionary<string, bool> request)
    {
        if (!request.TryGetValue("isFeatured", out var isFeatured))
            return ApiError("isFeatured field is required", "INVALID_REQUEST", StatusCodes.Status400BadRequest);

        await _productService.SetProductFeaturedAsync(id, isFeatured);
        return NoContent();
    }

    /// <summary>Permanently deletes a product.</summary>
    /// <response code="204">Product deleted successfully.</response>
    /// <response code="404">Product not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }
}
