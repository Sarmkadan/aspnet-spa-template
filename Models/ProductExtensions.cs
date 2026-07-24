#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Extension methods for <see cref="Product"/>.
/// </summary>
public static class ProductExtensions
{
    /// <summary>
    /// Determines whether the product is in stock based on its quantity and availability flag.
    /// </summary>
    /// <param name="product">The product instance.</param>
    /// <returns><c>true</c> if the product has a positive stock quantity and is marked as available; otherwise, <c>false</c>.</returns>
    public static bool IsInStock(this Product product)
    {
        if (product is null) throw new ArgumentNullException(nameof(product));
        return product.StockQuantity > 0 && product.IsAvailable;
    }

    /// <summary>
    /// Builds a display name for the product that includes its name, optional SKU, and category display name.
    /// </summary>
    /// <param name="product">The product instance.</param>
    /// <returns>A human‑readable product name.</returns>
    public static string DisplayName(this Product product)
    {
        if (product is null) throw new ArgumentNullException(nameof(product));

        var parts = new System.Collections.Generic.List<string>();

        // Base name
        parts.Add(product.Name);

        // Optional SKU
        if (!string.IsNullOrWhiteSpace(product.Sku))
        {
            parts.Add($"({product.Sku})");
        }

        // Category display name
        if (Enum.IsDefined(typeof(ProductCategory), product.Category))
        {
            parts.Add(product.Category.ToDisplayName());
        }

        return string.Join(" ", parts);
    }

    /// <summary>
    /// Calculates the price after applying a percentage discount without modifying the product.
    /// </summary>
    /// <param name="product">The product instance.</param>
    /// <param name="percent">The discount percentage (0–100).</param>
    /// <returns>The discounted price.</returns>
    public static decimal ApplyDiscount(this Product product, decimal percent)
    {
        if (product is null) throw new ArgumentNullException(nameof(product));
        if (percent < 0m || percent > 100m)
            throw new ArgumentOutOfRangeException(nameof(percent), "Discount percent must be between 0 and 100.");

        var discountFactor = 1m - (percent / 100m);
        return product.Price * discountFactor;
    }
}
