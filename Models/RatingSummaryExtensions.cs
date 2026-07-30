#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using AspNetSpaTemplate.Models;

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Extension methods for <see cref="RatingSummary"/>.
/// </summary>
public static class RatingSummaryExtensions
{
    /// <summary>
    /// Calculates the percentage of each star rating (1‑5) based on the total number of reviews.
    /// </summary>
    /// <param name="summary">The rating summary.</param>
    /// <returns>
    /// A dictionary where the key is the star value (1‑5) and the value is the percentage (0‑100).
    /// If there are no reviews, all percentages are 0.
    /// </returns>
    public static IDictionary<int, double> StarPercentages(this RatingSummary summary)
    {
        var percentages = new Dictionary<int, double>(5);
        // Ensure we have a dictionary for star counts; if null, treat as empty.
        var starCounts = summary.StarCounts ?? new Dictionary<int, int>();

        // Total number of reviews; fallback to 0 if null.
        var total = summary.ReviewCount > 0 ? summary.ReviewCount : 0;

        for (int star = 1; star <= 5; star++)
        {
            int count = starCounts.TryGetValue(star, out var c) ? c : 0;
            double percent = total == 0 ? 0 : (double)count / total * 100;
            percentages[star] = Math.Round(percent, 2, MidpointRounding.AwayFromZero);
        }

        return percentages;
    }

    /// <summary>
    /// Determines whether the rating summary contains at least the specified number of reviews.
    /// </summary>
    /// <param name="summary">The rating summary.</param>
    /// <param name="min">The minimum number of reviews required.</param>
    /// <returns>True if <see cref="RatingSummary.ReviewCount"/> is greater than or equal to <paramref name="min"/>.</returns>
    public static bool HasEnoughReviews(this RatingSummary summary, int min) =>
        summary.ReviewCount >= min;

    /// <summary>
    /// Returns the average rating formatted as a string with one decimal place.
    /// If there are no reviews, returns <c>"N/A"</c>.
    /// </summary>
    /// <param name="summary">The rating summary.</param>
    /// <returns>A formatted string representation of the average rating.</returns>
    public static string FormattedAverage(this RatingSummary summary)
    {
        if (summary.ReviewCount == 0)
            return "N/A";

        // Use invariant culture to ensure a consistent decimal separator.
        return summary.AverageRating.ToString("0.0", CultureInfo.InvariantCulture);
    }
}
