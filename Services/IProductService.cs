#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.DTOs;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Interface for product-related business logic, including querying, creation, and management.
/// </summary>
public interface IProductService
{
    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product response, or throws NotFoundException.</returns>
    Task<ProductResponse?> GetProductByIdAsync(int id);

    /// <summary>
    /// Gets all products with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size (1-100).</param>
    /// <returns>A paginated list response.</returns>
    Task<ProductListResponse> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Gets products by category with pagination.
    /// </summary>
    /// <param name="category">The product category.</param>
    /// <param name="pageNumber">The page number (1-based).</param>
    /// <param name="pageSize">The page size (1-100).</param>
    /// <returns>A paginated list response.</returns>
    Task<ProductListResponse> GetProductsByCategoryAsync(ProductCategory category, int pageNumber = 1, int pageSize = 10);

    /// <summary>
    /// Gets featured products.
    /// </summary>
    /// <param name="limit">The limit of products to return.</param>
    /// <returns>A list of featured products.</returns>
    Task<List<ProductResponse>> GetFeaturedProductsAsync(int limit = 10);

    /// <summary>
    /// Gets top-rated products.
    /// </summary>
    /// <param name="limit">The limit of products to return.</param>
    /// <returns>A list of top-rated products.</returns>
    Task<List<ProductResponse>> GetTopRatedProductsAsync(int limit = 10);

    /// <summary>
    /// Searches products by query, category, and price range.
    /// </summary>
    /// <param name="query">The search query term.</param>
    /// <param name="category">Optional category filter.</param>
    /// <param name="minPrice">Optional minimum price filter.</param>
    /// <param name="maxPrice">Optional maximum price filter.</param>
    /// <returns>A list of matching products.</returns>
    Task<List<ProductResponse>> SearchProductsAsync(
        string query,
        ProductCategory? category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null);

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The create product request.</param>
    /// <returns>The created product response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    Task<ProductResponse> CreateProductAsync(CreateProductRequest request);

    /// <summary>
    /// Updates a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="request">The update product request.</param>
    /// <returns>The updated product response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    Task<ProductResponse> UpdateProductAsync(int id, UpdateProductRequest request);

    /// <summary>
    /// Sets product availability.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="isAvailable">The availability status.</param>
    /// <exception cref="NotFoundException">Thrown when product with specified ID is not found.</exception>
    Task SetProductAvailabilityAsync(int id, bool isAvailable);

    /// <summary>
    /// Sets product featured status.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="isFeatured">The featured status.</param>
    /// <exception cref="NotFoundException">Thrown when product with specified ID is not found.</exception>
    Task SetProductFeaturedAsync(int id, bool isFeatured);

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <exception cref="NotFoundException">Thrown when product with specified ID is not found.</exception>
    Task DeleteProductAsync(int id);

    /// <summary>
    /// Updates prices for multiple products in a single operation.
    /// </summary>
    /// <param name="request">The bulk price update request.</param>
    /// <returns>A response with per-item results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    Task<UpdateProductPriceResponse> UpdatePricesAsync(UpdateProductPriceRequest request);
}