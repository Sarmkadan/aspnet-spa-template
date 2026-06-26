#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Data transfer object for creating a new product.
/// </summary>
public sealed class CreateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public ProductCategory Category { get; set; }
    public string? ImageUrl { get; set; }
    public string? Sku { get; set; }
}
