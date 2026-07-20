#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// API controller for review management and voting.
/// </summary>
public sealed class ReviewsController : ApiControllerBase
{
    private readonly ReviewService _reviewService;

    public ReviewsController(ReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>
    /// Votes a review as helpful.
    /// </summary>
    /// <param name="id">The ID of the review to vote as helpful.</param>
    /// <returns>Success response with the updated review.</returns>
    [HttpPost("{id:int}/vote/helpful")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VoteHelpful(int id)
    {
        await _reviewService.MarkAsHelpfulAsync(id);

        // Get the updated review to return
        var review = await _reviewService.GetReviewByIdAsync(id);
        return ApiSuccess(review, "Review marked as helpful successfully");
    }

    /// <summary>
    /// Gets a review by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ReviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReview(int id)
    {
        var review = await _reviewService.GetReviewByIdAsync(id);
        return ApiSuccess(review);
    }

    /// <summary>
    /// Gets all approved reviews for a product.
    /// </summary>
    [HttpGet("product/{productId:int}")]
    [ProducesResponseType(typeof(List<ReviewResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProductReviews(int productId)
    {
        var reviews = await _reviewService.GetProductReviewsAsync(productId);
        return ApiSuccess(reviews.ToList());
    }
}

/// <summary>
/// Response DTO for review data.
/// </summary>
public sealed class ReviewResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int HelpfulCount { get; set; }
    public bool IsVerifiedPurchase { get; set; }
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}