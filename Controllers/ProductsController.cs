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
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        return ApiSuccess(product);
    }

    /// <summary>
    /// Gets all available products, paginated.
    /// </summary>
    /// <param name="pagination">The pagination parameters.</param>
    /// <returns>A <see cref="PagedResult{T}"/> envelope of <see cref="ProductResponse"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pagination"/> is null.</exception>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts([FromQuery] PaginationRequest pagination)
    {
        ArgumentNullException.ThrowIfNull(pagination);

        var products = await _productService.GetAllProductsAsync(pagination.PageNumber, pagination.PageSize);
        var page = PagedResult<ProductResponse>.Create(products.Products, products.PageNumber, products.PageSize, products.TotalCount);
        return ApiSuccess(page);
    }

    /// <summary>
    /// Gets products belonging to a category, paginated. The category name is matched
    /// case-insensitively; an unrecognized category yields a 200 response with an
    /// empty <see cref="PagedResult{T}.Items"/> collection rather than a 404 or 400.
    /// </summary>
    /// <param name="category">The category name (case-insensitive).</param>
    /// <param name="pagination">The pagination parameters.</param>
    /// <returns>A <see cref="PagedResult{T}"/> envelope of <see cref="ProductResponse"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="category"/> is null or empty.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="pagination"/> is null.</exception>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductsByCategory(string category, [FromQuery] PaginationRequest pagination)
    {
        ArgumentException.ThrowIfNullOrEmpty(category);
        ArgumentNullException.ThrowIfNull(pagination);

        if (!Enum.TryParse<ProductCategory>(category, ignoreCase: true, out var parsedCategory))
            return ApiSuccess(PagedResult<ProductResponse>.Empty(pagination.PageNumber, pagination.PageSize));

        var products = await _productService.GetProductsByCategoryAsync(parsedCategory, pagination.PageNumber, pagination.PageSize);
        var page = PagedResult<ProductResponse>.Create(products.Products, products.PageNumber, products.PageSize, products.TotalCount);
        return ApiSuccess(page);
    }

    /// <summary>
    /// Gets a bounded set of featured products. This endpoint is unpaged: it returns a
    /// plain array capped at <paramref name="limit"/> items rather than a <see cref="PagedResult{T}"/>.
    /// </summary>
    /// <param name="limit">The maximum number of products to return.</param>
    /// <returns>A plain array of <see cref="ProductResponse"/>, not wrapped in a paging envelope.</returns>
    [HttpGet("featured")]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeaturedProducts([FromQuery] int limit = 10)
    {
        var products = await _productService.GetFeaturedProductsAsync(limit);
        return ApiSuccess(products);
    }

    [HttpGet("top-rated")]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTopRatedProducts([FromQuery] int limit = 10)
    {
        var products = await _productService.GetTopRatedProductsAsync(limit);
        return ApiSuccess(products);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchProducts(
        [FromQuery] string query,
        [FromQuery] ProductCategory? category,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice)
    {
        var products = await _productService.SearchProductsAsync(query, category, minPrice, maxPrice);
        return ApiSuccess(products);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var product = await _productService.CreateProductAsync(request);
        return ApiSuccess(product, "Product created successfully", StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var product = await _productService.UpdateProductAsync(id, request);
        return ApiSuccess(product, "Product updated successfully");
    }

    [HttpPatch("{id:int}/availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetAvailability(int id, [FromBody] Dictionary<string, bool> request)
    {
        if (!request.TryGetValue("isAvailable", out var isAvailable))
            return ApiError("isAvailable field is required", "INVALID_REQUEST", StatusCodes.Status400BadRequest);

        await _productService.SetProductAvailabilityAsync(id, isAvailable);
        return NoContent();
    }

    [HttpPatch("{id:int}/featured")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetFeatured(int id, [FromBody] Dictionary<string, bool> request)
    {
        if (!request.TryGetValue("isFeatured", out var isFeatured))
            return ApiError("isFeatured field is required", "INVALID_REQUEST", StatusCodes.Status400BadRequest);

        await _productService.SetProductFeaturedAsync(id, isFeatured);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id);
        return NoContent();
    }

    [HttpPost("prices/bulk-update")]
    [ProducesResponseType(typeof(UpdateProductPriceResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdatePrices([FromBody] UpdateProductPriceRequest request)
    {
        var response = await _productService.UpdatePricesAsync(request);
        return ApiSuccess(response, "Bulk price update completed");
    }
}