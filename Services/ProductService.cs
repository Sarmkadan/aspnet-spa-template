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

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Service for product-related business logic, including querying, creation, and management.
/// </summary>
public sealed class ProductService
{
    private readonly ProductRepository _productRepository;

    /// <summary>
    /// Initializes a new instance of <see cref="ProductService"/>.
    /// </summary>
    /// <param name="productRepository">The product repository.</param>
    /// <exception cref="ArgumentNullException">Thrown when productRepository is null.</exception>
    public ProductService(ProductRepository productRepository)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
    }

    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <returns>The product response, or throws NotFoundException.</returns>
    public async Task<ProductResponse?> GetProductByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException("Product", id);

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
        pageSize = Math.Min(pageSize, AppConstants.Pagination.MaxPageSize);
        pageSize = Math.Max(pageSize, AppConstants.Pagination.MinPageSize);

        var totalCount = await _productRepository.CountAsync(p => p.IsAvailable);
        var products = await _productRepository.GetPagedAsync(pageNumber, pageSize, p => p.IsAvailable);

        return new ProductListResponse
        {
            Products = products.Select(MapToResponse).ToList(),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (totalCount + pageSize - 1) / pageSize
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
            PageSize = pageSize,
            TotalPages = (totalCount + pageSize - 1) / pageSize
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
    /// Searches products by term.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    /// <returns>A list of matching products.</returns>
    public async Task<List<ProductResponse>> SearchProductsAsync(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new List<ProductResponse>();

        var products = await _productRepository.SearchAsync(searchTerm);
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
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        ValidateProductRequest(request);

        var product = new Product
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

        try
        {
            _productRepository.Add(product);
            await _productRepository.SaveChangesAsync();

            return MapToResponse(product);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            throw new BusinessException("Failed to create product due to database error", "PRODUCT_CREATION_FAILED", 500).WithData(ex);
        }
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
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException("Product", id);

        try
        {
            product.UpdateDetails(request.Name, request.Description, request.Price, request.Category, request.ImageUrl);
            product.SetAvailability(request.IsAvailable);

            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();

            return MapToResponse(product);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
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
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException("Product", id);

        try
        {
            product.SetAvailability(isAvailable);
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
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
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException("Product", id);

        try
        {
            product.SetFeatured(isFeatured);
            _productRepository.Update(product);
            await _productRepository.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
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
        var product = await _productRepository.GetByIdAsync(id);
        if (product is null)
            throw new NotFoundException("Product", id);

        try
        {
            _productRepository.Remove(product);
            await _productRepository.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
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
}
