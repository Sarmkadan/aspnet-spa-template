#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using Microsoft.Extensions.Logging;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Service for product-related business logic, including querying, creation, and management.
/// </summary>
public sealed class ProductService
{
    private readonly ProductRepository _productRepository;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ProductService"/>.
    /// </summary>
    /// <param name="productRepository">The product repository.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">Thrown when productRepository or logger is null.</exception>
    public ProductService(ProductRepository productRepository, ILogger<ProductService> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product response, or throws NotFoundException.</returns>
    public async Task<ProductResponse?> GetProductByIdAsync(int id)
    {
        _logger.LogDebug("Getting product by ID: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            throw new NotFoundException("Product", id);
        }

        _logger.LogInformation("Retrieved product: {ProductId} - {ProductName}", product.Id, product.Name);
        return MapToResponse(product);
    }

    /// <summary>
    /// Gets all products with pagination.
    /// </summary>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A paginated list response.</returns>
    public async Task<ProductListResponse> GetAllProductsAsync(int pageNumber = 1, int pageSize = 10)
    {
        _logger.LogDebug("Getting all available products: page={PageNumber}, pageSize={PageSize}", pageNumber, pageSize);

        pageSize = Math.Clamp(pageSize, AppConstants.Pagination.MinPageSize, AppConstants.Pagination.MaxPageSize);

        var totalCount = await _productRepository.CountAsync(p => p.IsAvailable);
        var products = await _productRepository.GetPagedAsync(pageNumber, pageSize, p => p.IsAvailable);

        _logger.LogInformation("Retrieved {ProductCount} products (total: {TotalCount})", products.Count(), totalCount);

        return new ProductListResponse
        {
            Products = products.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets products by category with pagination.
    /// </summary>
    /// <param name="category">The product category.</param>
    /// <param name="pageNumber">The page number.</param>
    /// <param name="pageSize">The page size.</param>
    /// <returns>A paginated list response.</returns>
    public async Task<ProductListResponse> GetProductsByCategoryAsync(ProductCategory category, int pageNumber = 1, int pageSize = 10)
    {
        var products = await _productRepository.GetPagedByCategoryAsync(category, pageNumber, pageSize);
        var totalCount = await _productRepository.CountAsync(p => p.Category == category && p.IsAvailable);

        return new ProductListResponse
        {
            Products = products.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Gets featured products.
    /// </summary>
    /// <param name="limit">The limit of products to return.</param>
    /// <returns>A list of featured products.</returns>
    public async Task<List<ProductResponse>> GetFeaturedProductsAsync(int limit = 10)
    {
        var products = await _productRepository.GetFeaturedProductsAsync(limit);
        return products.Select(MapToResponse).ToList();
    }

    /// <summary>
    /// Gets top-rated products.
    /// </summary>
    /// <param name="limit">The limit of products to return.</param>
    /// <returns>A list of top-rated products.</returns>
    public async Task<List<ProductResponse>> GetTopRatedProductsAsync(int limit = 10)
    {
        var products = await _productRepository.GetTopRatedAsync(limit);
        return products.Select(MapToResponse).ToList();
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
        if (string.IsNullOrWhiteSpace(query))
            return new List<ProductResponse>();

        var products = await _productRepository.SearchAsync(query, category, minPrice, maxPrice);
        return products.Select(MapToResponse).ToList();
    }

    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="request">The create product request.</param>
    /// <returns>The created product response.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<ProductResponse> CreateProductAsync(CreateProductRequest request)
    {
        _logger.LogInformation("Creating new product: {ProductName}", request?.Name ?? "Unknown");

        if (request is null)
        {
            _logger.LogWarning("CreateProductAsync called with null request");
            throw new ArgumentNullException(nameof(request));
        }

        ValidateProductRequest(request);

        var product = BuildProductEntity(request);

        return await SaveProductAsync(product, request.Name);
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
        _logger.LogInformation("Updating product: {ProductId}", id);

        if (request is null)
        {
            _logger.LogWarning("UpdateProductAsync called with null request for product {ProductId}", id);
            throw new ArgumentNullException(nameof(request));
        }

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Product not found for update: {ProductId}", id);
            throw new NotFoundException("Product", id);
        }

        try
        {
            UpdateProductEntity(product, request);
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product updated successfully: {ProductId} - {ProductName}", product.Id, product.Name);
            return MapToResponse(product);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to update product: {ProductId}", id);
            throw new BusinessException("Failed to update product due to database error", "PRODUCT_UPDATE_FAILED", 500).WithData(ex);
        }
    }

    /// <summary>
    /// Sets product availability.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="isAvailable">The availability status.</param>
    /// <exception cref="NotFoundException">Thrown when product with specified ID is not found.</exception>
    public async Task SetProductAvailabilityAsync(int id, bool isAvailable)
    {
        _logger.LogInformation("Setting product availability: {ProductId} - Available={IsAvailable}", id, isAvailable);

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Product not found for availability update: {ProductId}", id);
            throw new NotFoundException("Product", id);
        }

        try
        {
            product.SetAvailability(isAvailable);
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product availability updated: {ProductId} - Available={IsAvailable}", product.Id, isAvailable);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to update product availability: {ProductId}", id);
            throw new BusinessException("Failed to update product availability", "PRODUCT_AVAILABILITY_UPDATE_FAILED", 500).WithData(ex);
        }
    }

    /// <summary>
    /// Sets product featured status.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="isFeatured">The featured status.</param>
    /// <exception cref="NotFoundException">Thrown when product with specified ID is not found.</exception>
    public async Task SetProductFeaturedAsync(int id, bool isFeatured)
    {
        _logger.LogInformation("Setting product featured status: {ProductId} - Featured={IsFeatured}", id, isFeatured);

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Product not found for featured update: {ProductId}", id);
            throw new NotFoundException("Product", id);
        }

        try
        {
            product.SetFeatured(isFeatured);
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product featured status updated: {ProductId} - Featured={IsFeatured}", product.Id, isFeatured);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to update product featured status: {ProductId}", id);
            throw new BusinessException("Failed to update product featured status", "PRODUCT_FEATURED_UPDATE_FAILED", 500).WithData(ex);
        }
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <exception cref="NotFoundException">Thrown when product with specified ID is not found.</exception>
    public async Task DeleteProductAsync(int id)
    {
        _logger.LogInformation("Deleting product: {ProductId}", id);

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
        {
            _logger.LogWarning("Product not found for deletion: {ProductId}", id);
            throw new NotFoundException("Product", id);
        }

        try
        {
            _logger.LogDebug("Removing product from repository: {ProductId} - {ProductName}", product.Id, product.Name);
            _productRepository.Remove(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product deleted successfully: {ProductId} - {ProductName}", product.Id, product.Name);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to delete product: {ProductId}", id);
            throw new BusinessException("Failed to delete product", "PRODUCT_DELETION_FAILED", 500).WithData(ex);
        }
    }

    private void ValidateProductRequest(CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("Name", "Product name is required");

        if (request.Price < AppConstants.Product.MinPrice || request.Price > AppConstants.Product.MaxPrice)
            throw new ValidationException("Price", $"Price must be between {AppConstants.Product.MinPrice} and {AppConstants.Product.MaxPrice}");

        if (request.StockQuantity < AppConstants.Product.MinStock)
            throw new ValidationException("StockQuantity", $"Stock must be at least {AppConstants.Product.MinStock}");
    }

    private ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            Category = product.Category.ToDisplayName(),
            ImageUrl = product.ImageUrl,
            Sku = product.Sku,
            Rating = product.Rating,
            ReviewCount = product.ReviewCount,
            IsAvailable = product.IsAvailable,
            IsFeatured = product.IsFeatured,
            CreatedAt = product.CreatedAt
        };
    }

    private Product BuildProductEntity(CreateProductRequest request) =>
        new Product
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            Category = request.Category,
            ImageUrl = request.ImageUrl,
            Sku = request.Sku,
            IsAvailable = true,
            CreatedAt = DateTime.UtcNow
        };

    private async Task<ProductResponse> SaveProductAsync(Product product, string productName)
    {
        try
        {
            _productRepository.Add(product);
            await _productRepository.SaveChangesAsync();

            _logger.LogInformation("Product created successfully: {ProductId} - {ProductName}", product.Id, productName);
            return MapToResponse(product);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create product: {ProductName}", productName);
            throw new BusinessException("Failed to create product due to database error", "PRODUCT_CREATION_FAILED", 500).WithData(ex);
        }
    }

    private void UpdateProductEntity(Product product, UpdateProductRequest request)
    {
        product.UpdateDetails(request.Name, request.Description, request.Price, request.Category, request.ImageUrl);
        product.SetAvailability(request.IsAvailable);
    }

    /// <summary>
    /// Updates prices for multiple products in a single operation.
    /// </summary>
    /// <param name="request">The bulk price update request.</param>
    /// <returns>A response with per-item results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null.</exception>
    public async Task<UpdateProductPriceResponse> UpdatePricesAsync(UpdateProductPriceRequest request)
    {
        _logger.LogInformation("Starting bulk price update for {ProductCount} products", request?.PriceUpdates?.Count ?? 0);

        if (request is null)
        {
            _logger.LogWarning("UpdatePricesAsync called with null request");
            throw new ArgumentNullException(nameof(request));
        }

        if (request.PriceUpdates is null || request.PriceUpdates.Count == 0)
        {
            _logger.LogWarning("UpdatePricesAsync called with empty price updates list");
            throw new ValidationException("PriceUpdates", "Price updates list cannot be empty");
        }

        if (request.PriceUpdates.Count > 1000)
        {
            _logger.LogWarning("UpdatePricesAsync called with too many products: {Count}", request.PriceUpdates.Count);
            throw new ValidationException("PriceUpdates", "Cannot update more than 1000 products in one request");
        }

        var response = new UpdateProductPriceResponse
        {
            TotalProcessed = request.PriceUpdates.Count,
            Results = new List<ProductPriceUpdateResult>()
        };

        var processedCount = 0;
        var successCount = 0;
        var failureCount = 0;

        foreach (var update in request.PriceUpdates)
        {
            var result = new ProductPriceUpdateResult
            {
                ProductId = update.ProductId,
                NewPrice = update.NewPrice
            };

            try
            {
                if (update.NewPrice < AppConstants.Product.MinPrice || update.NewPrice > AppConstants.Product.MaxPrice)
                {
                    result.Success = false;
                    result.ErrorMessage = $"Price must be between {AppConstants.Product.MinPrice} and {AppConstants.Product.MaxPrice}";
                    result.ErrorCode = "INVALID_PRICE_RANGE";
                    failureCount++;
                    response.Results.Add(result);
                    continue;
                }

                var product = await _productRepository.GetByIdAsync(update.ProductId);
                if (product is null)
                {
                    result.Success = false;
                    result.ErrorMessage = "Product not found";
                    result.ErrorCode = "PRODUCT_NOT_FOUND";
                    failureCount++;
                    response.Results.Add(result);
                    continue;
                }

                product.UpdateDetails(product.Name, product.Description, update.NewPrice, product.Category, product.ImageUrl);
                _productRepository.Update(product);
                await _productRepository.SaveChangesAsync();

                result.Success = true;
                result.ProductName = product.Name;
                successCount++;
                _logger.LogInformation("Price updated successfully for product {ProductId}: {OldPrice} -> {NewPrice}",
                    product.Id, product.Price, update.NewPrice);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Failed to update price for product {ProductId}", update.ProductId);
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ErrorCode = "PRICE_UPDATE_FAILED";
                failureCount++;
            }

            response.Results.Add(result);
            processedCount++;
        }

        response.SuccessCount = successCount;
        response.FailureCount = failureCount;

        _logger.LogInformation("Bulk price update completed: {SuccessCount} succeeded, {FailureCount} failed out of {TotalProcessed} total",
            successCount, failureCount, processedCount);

        return response;
    }
}