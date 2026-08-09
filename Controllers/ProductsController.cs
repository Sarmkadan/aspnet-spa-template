#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ===================================

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
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A <see cref="PagedResult{T}"/> envelope of <see cref="ProductResponse"/>.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        // Explicit validation for pagination bounds to reject invalid values with 400 responses
        if (pageNumber <= 0)
        {
            return ApiError("Page number must be greater than 0.", "VALIDATION_ERROR", StatusCodes.Status400BadRequest);
        }

        if (pageSize <= 0)
        {
            return ApiError("Page size must be greater than 0.", "VALIDATION_ERROR", StatusCodes.Status400BadRequest);
        }

        // Clamp pageSize to a reasonable maximum to prevent excessive database load
        const int maxPageSize = 100;
        if (pageSize > maxPageSize)
        {
            pageSize = maxPageSize;
        }

        var products = await _productService.GetAllProductsAsync(pageNumber, pageSize);
        var page = PagedResult<ProductResponse>.Create(products.Products, products.PageNumber, products.PageSize, products.TotalCount);
        return ApiSuccess(page);
    }

    /// <summary>
    /// Gets products belonging to a category, paginated. The category name is matched
    /// case-insensitively; an unrecognized category yields a 200 response with an
    /// empty <see cref="PagedResult{T}.Items"/> collection rather than a 404 or 400.
    /// </summary>
    /// <param name="category">The category name (case-insensitive). Must be a valid product category name.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A <see cref="PagedResult{T}"/> envelope of <see cref="ProductResponse"/>.</returns>
    [HttpGet("category/{category}")]
    [ProducesResponseType(typeof(PagedResult<ProductResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProductsByCategory(
        string category,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        ArgumentException.ThrowIfNullOrEmpty(category);

        // Validate category parameter for security and format issues
        // Category must be a valid product category name (alphanumeric with spaces and hyphens)
        // Reject path traversal sequences, SQL metacharacters, and overly long strings
        if (category.Length > 100)
        {
            return ApiError("Category name is too long.", "INVALID_CATEGORY", StatusCodes.Status400BadRequest);
        }

        // Reject path traversal sequences
        if (category.Contains("../") || category.Contains("..\\") || category.StartsWith("../") || category.StartsWith("..\\"))
        {
            return ApiError("Invalid category name.", "INVALID_CATEGORY", StatusCodes.Status400BadRequest);
        }

        // Reject common SQL injection patterns and special characters that aren't allowed in category names
        var invalidChars = new[] { "'", "\"", ";", "--", "/*", "*/", "xp_", "exec", "union", "select", "insert", "update", "delete", "drop", "alter", "create", "truncate" };
        if (invalidChars.Any(invalidChar => category.Contains(invalidChar, StringComparison.OrdinalIgnoreCase)))
        {
            return ApiError("Invalid category name.", "INVALID_CATEGORY", StatusCodes.Status400BadRequest);
        }

        // Reject control characters and non-printable characters
        if (category.Any(c => char.IsControl(c) || char.GetUnicodeCategory(c) == System.Globalization.UnicodeCategory.Format))
        {
            return ApiError("Invalid category name.", "INVALID_CATEGORY", StatusCodes.Status400BadRequest);
        }

        // Explicit validation for pagination bounds to reject invalid values with 400 responses
        if (pageNumber <= 0)
        {
            return ApiError("Page number must be greater than 0.", "VALIDATION_ERROR", StatusCodes.Status400BadRequest);
        }

        if (pageSize <= 0)
        {
            return ApiError("Page size must be greater than 0.", "VALIDATION_ERROR", StatusCodes.Status400BadRequest);
        }

        // Clamp pageSize to a reasonable maximum to prevent excessive database load
        const int maxPageSize = 100;
        if (pageSize > maxPageSize)
        {
            pageSize = maxPageSize;
        }

        if (!Enum.TryParse<ProductCategory>(category, ignoreCase: true, out var parsedCategory))
        {
            // Return empty result for unrecognized category with validated pagination parameters
            return ApiSuccess(PagedResult<ProductResponse>.Empty(pageNumber, pageSize));
        }

        var products = await _productService.GetProductsByCategoryAsync(parsedCategory, pageNumber, pageSize);
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

  /// <summary>
  /// Searches products by name and description using a free-text query. The query is case-insensitive and
  /// supports partial matching. Empty or whitespace-only queries return an empty result set with a 200 OK response.
  /// </summary>
  /// <param name="query">The search query term (max 100 characters).</param>
  /// <param name="category">Optional category filter.</param>
  /// <param name="minPrice">Optional minimum price filter.</param>
  /// <param name="maxPrice">Optional maximum price filter.</param>
  /// <returns>A list of matching products.</returns>
  [HttpGet("search")]
  [ProducesResponseType(typeof(List<ProductResponse>), StatusCodes.Status200OK)]
  [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
  public async Task<IActionResult> SearchProducts(
    [FromQuery] string query,
    [FromQuery] ProductCategory? category,
    [FromQuery] decimal? minPrice,
    [FromQuery] decimal? maxPrice)
  {
    ArgumentException.ThrowIfNullOrEmpty(query);

    // Enforce maximum query length to prevent potential regex/LIKE-based resource exhaustion
    // and protect against overly long queries that could cause performance issues
    if (query.Length > AppConstants.Validation.MaxSearchQueryLength)
    {
      return ApiError(
        $"Search query exceeds maximum length of {AppConstants.Validation.MaxSearchQueryLength} characters.",
        "QUERY_TOO_LONG",
        StatusCodes.Status400BadRequest);
    }

    // Sanitize query to prevent SQL injection and special character attacks
    // Remove or escape common SQL injection patterns and special regex characters
    var sanitizedQuery = SanitizeSearchQuery(query);

    // Validate sanitized query is not empty after sanitization
    if (string.IsNullOrWhiteSpace(sanitizedQuery))
    {
      return ApiError("Search query contains only invalid characters after sanitization.", "INVALID_SEARCH_QUERY", StatusCodes.Status400BadRequest);
    }

    var products = await _productService.SearchProductsAsync(sanitizedQuery, category, minPrice, maxPrice);
    return ApiSuccess(products);
  }

  /// <summary>
  /// Sanitizes a search query to remove potentially dangerous characters and patterns.
  /// </summary>
  /// <param name="query">The raw search query.</param>
  /// <returns>The sanitized search query.</returns>
  private static string SanitizeSearchQuery(string query)
  {
    if (string.IsNullOrEmpty(query))
      return query;

    // Remove common SQL injection patterns
    var dangerousPatterns = new[] { "'", "\"", ";", "--", "/*", "*/", "xp_", "exec", "union", "select", "insert", "update", "delete", "drop", "alter", "create", "truncate", "\x00", "\x1a", "\n", "\r", "\t" };

    var sanitized = query;
    foreach (var pattern in dangerousPatterns)
    {
      sanitized = sanitized.Replace(pattern, string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    // Remove control characters and non-printable characters
    sanitized = new string(sanitized.Where(c => !char.IsControl(c) && char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.Format).ToArray());

    // Trim whitespace and ensure minimum length
    sanitized = sanitized.Trim();

    return sanitized;
  }


    [HttpPost]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var product = await _productService.CreateProductAsync(request);
        return ApiSuccess(product, "Product created successfully", StatusCodes.Status201Created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var product = await _productService.UpdateProductAsync(id, request);
        return ApiSuccess(product, "Product updated successfully");
    }

    [HttpPatch("{id:int}/availability")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetAvailability(int id, [FromBody] Dictionary<string, bool> request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!request.TryGetValue("isAvailable", out var isAvailable))
            return ApiError("isAvailable field is required", "INVALID_REQUEST", StatusCodes.Status400BadRequest);

        await _productService.SetProductAvailabilityAsync(id, isAvailable);
        return NoContent();
    }

    [HttpPatch("{id:int}/featured")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SetFeatured(int id, [FromBody] Dictionary<string, bool> request)
    {
        ArgumentNullException.ThrowIfNull(request);

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
        ArgumentNullException.ThrowIfNull(request);

        var response = await _productService.UpdatePricesAsync(request);
        return ApiSuccess(response, "Bulk price update completed");
    }
}