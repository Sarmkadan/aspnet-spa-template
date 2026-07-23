#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.DTOs;
using Microsoft.Extensions.Logging;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Decorator for <see cref="ProductService"/> that adds caching for featured products.
/// Caches the featured list with a 60s absolute expiration and cache-stampede guard.
/// Invalidates cache on product create/update/delete that touches the IsFeatured flag.
/// </summary>
public sealed class CachedProductService : IProductService
{
    private readonly IProductService _decoratedService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedProductService> _logger;
    private const string FeaturedProductsCacheKeyPrefix = "featured_products:";

    /// <summary>
    /// Initializes a new instance of <see cref="CachedProductService"/>.
    /// </summary>
    /// <param name="decoratedService">The decorated product service.</param>
    /// <param name="cacheService">The cache service.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public CachedProductService(
        IProductService decoratedService,
        ICacheService cacheService,
        ILogger<CachedProductService> logger)
    {
        _decoratedService = decoratedService ?? throw new ArgumentNullException(nameof(decoratedService));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product response, or throws NotFoundException.</returns>
    public async Task<ProductResponse?> GetProductByIdAsync(int id)
    {
        return await _decoratedService.GetProductByIdAsync(id);
    }

    /// <summary>
    /// Gets all products with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size (1-100).</param>
    /// <returns>A paginated list response.</returns>
    public async Task<ProductListResponse> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        return await _decoratedService.GetAllProductsAsync(pageNumber, pageSize);
    }

    /// <summary>
    /// Gets products by category with pagination.
    /// </summary>
    /// <param name="category">The product category.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size (1-100).</param>
    /// <returns>A paginated list response.</returns>
    public async Task<ProductListResponse> GetProductsByCategoryAsync(
        ProductCategory category,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return await _decoratedService.GetProductsByCategoryAsync(category, pageNumber, pageSize);
    }

    /// <summary>
    /// Gets featured products.
    /// </summary>
    /// <param name="limit">The limit of products to return.</param>
    /// <returns>A list of featured products.</returns>
    public async Task<List<ProductResponse>> GetFeaturedProductsAsync(int limit = 10)
    {
        var cacheKey = $"{FeaturedProductsCacheKeyPrefix}{limit}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () => await _decoratedService.GetFeaturedProductsAsync(limit),
            TimeSpan.FromSeconds(60)
        );
    }

    /// <summary>
    /// Gets top-rated products.
    /// </summary>
    /// <param name="limit">The limit of products to return.</param>
    /// <returns>A list of top-rated products.</returns>
    public async Task<List<ProductResponse>> GetTopRatedProductsAsync(int limit = 10)
    {
        return await _decoratedService.GetTopRatedProductsAsync(limit);
    }

    /// <summary>
    /// Searches products by query, category, and price range.
    /// </summary>
    /// <param name="query">The search query term.</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="minPrice">Optional minimum price filter.</param>
    /// <param name="maxPrice">Optional maximum price filter.</param>
    /// <returns>A list of matching products.</returns>
    public async Task<List<ProductResponse>> SearchProductsAsync(
        string query,
        ProductCategory? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null)
    {
        return await _decoratedService.SearchProductsAsync(query, category, minPrice, maxPrice);
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The create product request.</param>
    /// <returns>The created product response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        var result = await _decoratedService.CreateProductAsync(request);

        // Invalidate featured products cache since new product might be featured
        await _cacheService.RemoveByPatternAsync($"{FeaturedProductsCacheKeyPrefix}*");
        _logger.LogDebug("Cache invalidated for featured products after product creation");

        return result;
    }

    /// <summary>
    /// Updates a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="request">The update product request.</param>
    /// <returns>The updated product response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest request)
    {
        var result = await _decoratedService.UpdateProductAsync(id, request);

        // Invalidate featured products cache since any update might affect featured status
        // The decorated service will handle the actual featured status changes
        await _cacheService.RemoveByPatternAsync($"{FeaturedProductsCacheKeyPrefix}*");
        _logger.LogDebug("Cache invalidated for featured products after product update");

        return result;
    }

    /// <summary>
    /// Sets product availability.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="isAvailable">The availability status.</param>
    /// <exception cref="NotFoundException">Thrown when product with specified ID is not found.</exception>
    public async Task SetProductAvailabilityAsync(int id, bool isAvailable)
    {
        await _decoratedService.SetProductAvailabilityAsync(id, isAvailable);

        // Invalidate featured products cache since availability affects featured products
        await _cacheService.RemoveByPatternAsync($"{FeaturedProductsCacheKeyPrefix}*");
        _logger.LogDebug("Cache invalidated for featured products after availability change");
    }

    /// <summary>
    /// Sets product featured status.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="isFeatured">The featured status.</param>
    /// <exception cref="NotFoundException">Thrown when product with specified ID is not found.</exception>
    public async Task SetProductFeaturedAsync(int id, bool isFeatured)
    {
        await _decoratedService.SetProductFeaturedAsync(id, isFeatured);

        // Invalidate featured products cache when featured status changes
        await _cacheService.RemoveByPatternAsync($"{FeaturedProductsCacheKeyPrefix}*");
        _logger.LogDebug("Cache invalidated for featured products after featured status change");
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <exception cref="NotFoundException">Thrown when product with specified ID is not found.</exception>
    public async Task DeleteProductAsync(int id)
    {
        await _decoratedService.DeleteProductAsync(id);

        // Invalidate featured products cache since product is deleted
        await _cacheService.RemoveByPatternAsync($"{FeaturedProductsCacheKeyPrefix}*");
        _logger.LogDebug("Cache invalidated for featured products after product deletion");
    }

    /// <summary>
    /// Updates prices for multiple products in a single operation.
    /// </summary>
    /// <param name="request">The bulk price update request.</param>
    /// <returns>A response with per-item results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<UpdateProductPriceResponse> UpdatePricesAsync(UpdateProductPriceRequest request)
    {
        var result = await _decoratedService.UpdatePricesAsync(request);

        // Invalidate featured products cache since prices might affect featured products
        await _cacheService.RemoveByPatternAsync($"{FeaturedProductsCacheKeyPrefix}*");
        _logger.LogDebug("Cache invalidated for featured products after bulk price update");

        return result;
    }
}