#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Models;

/// <summary>
/// Fluent extension methods for <see cref="Review"/>.
/// </summary>
public static class ReviewFluentExtensions
{
    /// <summary>
    /// Determines whether the review is positive (rating 4 or 5).
    /// </summary>
    /// <param name="review">The review to check.</param>
    /// <returns>true if the rating is 4 or 5; otherwise, false.</returns>
    public static bool IsPositive(this Review review) =>
        review.Rating >= 4;

    /// <summary>
    /// Gets the string representation of the review's rating using star characters.
    /// </summary>
    /// <param name="review">The review to get the star string for.</param>
    /// <returns>A string of five characters representing the rating (e.g., "★★★☆☆").</returns>
    public static string StarString(this Review review) =>
        review.Rating switch
        {
            1 => "★☆☆☆☆",
            2 => "★★☆☆☆",
            3 => "★★★☆☆",
            4 => "★★★★☆",
            5 => "★★★★★",
            _ => "No rating"
        };

    /// <summary>
    /// Determines whether the review was created within the specified time span.
    /// </summary>
    /// <param name="review">The review to check.</param>
    /// <param name="timeSpan">The time span to check against (e.g., TimeSpan.FromDays(30)).</param>
    /// <returns>true if the review was created within the time span; otherwise, false.</returns>
    public static bool IsRecent(this Review review, TimeSpan timeSpan) =>
        DateTime.UtcNow - review.CreatedAt <= timeSpan;
}