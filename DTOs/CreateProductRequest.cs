// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Data transfer object for creating a new product.
/// </summary>
public class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ProductCategory Category { get; set; }
    public string? ImageUrl { get; set; }
    public string? Sku { get; set; }
}

/// <summary>
/// Data transfer object for updating a product.
/// </summary>
public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ProductCategory Category { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsAvailable { get; set; }
}

/// <summary>
/// Data transfer object for product response.
/// </summary>
public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Category { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Sku { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsAvailable { get; set; }
    public bool IsFeatured { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for product list response with pagination.
/// </summary>
public class ProductListResponse
{
    public List<ProductResponse> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
