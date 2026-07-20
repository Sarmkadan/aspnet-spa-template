#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Represents a summary of ratings for a product, including average rating,
/// total review count, and a breakdown of how many reviews fall into each star rating.
/// </summary>
public sealed class RatingSummary
{
    /// <summary>
    /// The average rating (0‑5) across all approved reviews for the product.
    /// </summary>
    public decimal AverageRating { get; set; }

    /// <summary>
    /// Total number of approved reviews for the product.
    /// </summary>
    public int ReviewCount { get; set; }

    /// <summary>
    /// Dictionary where the key is the star rating (1‑5) and the value is the count of reviews with that rating.
    /// </summary>
    public Dictionary<int, int> StarCounts { get; set; } = new Dictionary<int, int>();
}
