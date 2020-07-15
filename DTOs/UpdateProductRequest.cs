// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Request DTO for updating product details.
/// Encapsulates product update fields with validation rules.
/// </summary>
public class UpdateProductRequest
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public string? ImageUrl { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; } = true;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("Product name is required");

        if (Name.Length > 200)
            throw new ArgumentException("Product name cannot exceed 200 characters");

        if (Price < 0)
            throw new ArgumentException("Price cannot be negative");

        if (Price > 1_000_000)
            throw new ArgumentException("Price exceeds maximum allowed value");

        if (StockQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative");
    }
}

/// <summary>
/// Response DTO for product details.
/// Used in all product-related API responses.
/// </summary>
public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = "";
    public string? ImageUrl { get; set; }
    public string? Sku { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TaxAmount => Price * 0.1m; // 10% tax (simplified)
    public decimal PriceWithTax => Price + TaxAmount;
}

/// <summary>
/// Response DTO for paginated product list.
/// </summary>
public class ProductListResponse
{
    public List<ProductResponse> Products { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

/// <summary>
/// Request DTO for searching products.
/// </summary>
public class ProductSearchRequest
{
    public string? SearchTerm { get; set; }
    public ProductCategory? Category { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? OnlyAvailable { get; set; } = true;
    public bool? OnlyFeatured { get; set; } = false;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public void Validate()
    {
        if (PageNumber < 1) PageNumber = 1;
        if (PageSize < 1) PageSize = 10;
        if (PageSize > 100) PageSize = 100;

        if (MinPrice.HasValue && MaxPrice.HasValue && MinPrice > MaxPrice)
            throw new ArgumentException("MinPrice cannot be greater than MaxPrice");
    }
}

/// <summary>
/// Request DTO for bulk product operations.
/// </summary>
public class BulkProductRequest
{
    public List<int> ProductIds { get; set; } = new();
    public BulkOperation Operation { get; set; }
    public Dictionary<string, object>? UpdateValues { get; set; }

    public void Validate()
    {
        if (ProductIds == null || ProductIds.Count == 0)
            throw new ArgumentException("ProductIds cannot be empty");

        if (ProductIds.Count > 1000)
            throw new ArgumentException("Cannot process more than 1000 products in one request");
    }
}

public enum BulkOperation
{
    SetAvailability,
    SetFeatured,
    UpdatePrice,
    UpdateCategory,
    Delete
}

/// <summary>
/// Response DTO for product statistics.
/// </summary>
public class ProductStatsResponse
{
    public int TotalProducts { get; set; }
    public int AvailableProducts { get; set; }
    public int FeaturedProducts { get; set; }
    public int OutOfStockProducts { get; set; }
    public decimal AveragePrice { get; set; }
    public decimal AverageRating { get; set; }
    public Dictionary<string, int> ProductsByCategory { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
