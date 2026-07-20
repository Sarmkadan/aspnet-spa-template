#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Request DTO for bulk price update operation.
/// Contains a list of products with their new prices.
/// </summary>
public sealed class UpdateProductPriceRequest
{
    /// <summary>
    /// List of product price updates.
    /// </summary>
    public required List<ProductPriceUpdate> PriceUpdates { get; set; }
}

/// <summary>
/// Individual product price update.
/// </summary>
public sealed class ProductPriceUpdate
{
    /// <summary>
    /// Product ID to update.
    /// </summary>
    public required int ProductId { get; set; }

    /// <summary>
    /// New price for the product.
    /// </summary>
    public required decimal NewPrice { get; set; }
}

/// <summary>
/// Response DTO for bulk price update operation.
/// Contains per-item results with success/failure status.
/// </summary>
public sealed class UpdateProductPriceResponse
{
    /// <summary>
    /// Total number of products processed.
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Number of successfully updated products.
    /// </summary>
    public int SuccessCount { get; set; }

    /// <summary>
    /// Number of failed updates.
    /// </summary>
    public int FailureCount { get; set; }

    /// <summary>
    /// List of update results for each product.
    /// </summary>
    public required List<ProductPriceUpdateResult> Results { get; set; }
}

/// <summary>
/// Individual result of a price update operation.
/// </summary>
public sealed class ProductPriceUpdateResult
{
    /// <summary>
    /// Product ID that was updated.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Product name (for reference).
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Success status of the update.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Updated price (if successful).
    /// </summary>
    public decimal? NewPrice { get; set; }

    /// <summary>
    /// Error message (if failed).
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Error code (if failed).
    /// </summary>
    public string? ErrorCode { get; set; }
}