#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using Microsoft.Extensions.Logging;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Service for review-related business logic.
/// </summary>
public sealed class ReviewService
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly ProductRepository _productRepository;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(IRepository<Review> reviewRepository, ProductRepository productRepository, ILogger<ReviewService> logger)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<Review?> GetReviewByIdAsync(int id)
    {
        _logger.LogDebug("Getting review by ID: {ReviewId}", id);

        var review = await _reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            _logger.LogWarning("Review not found: {ReviewId}", id);
            throw new NotFoundException("Review", id);
        }

        _logger.LogInformation("Retrieved review: {ReviewId} - Product: {ProductId}, Rating: {Rating}/5", review.Id, review.ProductId, review.Rating);
        return review;
    }

    public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
    {
        _logger.LogDebug("Getting approved reviews for product: {ProductId}", productId);
        var reviews = await _reviewRepository.FindAsync(r => r.ProductId == productId && r.IsApproved);
        var count = reviews.Count();
        _logger.LogInformation("Retrieved {ReviewCount} approved reviews for product {ProductId}", count, productId);
        return reviews.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task<IEnumerable<Review>> GetUserReviewsAsync(int userId)
    {
        _logger.LogDebug("Getting reviews for user: {UserId}", userId);
        var reviews = await _reviewRepository.FindAsync(r => r.UserId == userId);
        var count = reviews.Count();
        _logger.LogInformation("Retrieved {ReviewCount} reviews for user {UserId}", count, userId);
        return reviews.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task<Review> CreateReviewAsync(int productId, int userId, int rating, string title, string content, bool isVerifiedPurchase = false)
    {
        _logger.LogInformation("Creating review for product {ProductId} by user {UserId}: Rating={Rating}/5, Title={Title}", productId, userId, rating, title);

        ValidateReview(rating, title, content);

        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
        {
            _logger.LogWarning("Product not found for review creation: {ProductId}", productId);
            throw new NotFoundException("Product", productId);
        }

        // Check if user already reviewed this product
        var existingReview = await _reviewRepository.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);
        if (existingReview is not null)
        {
            _logger.LogWarning("User {UserId} already reviewed product {ProductId}", userId, productId);
            throw new BusinessException("User has already reviewed this product", "DUPLICATE_REVIEW");
        }

        var review = new Review
        {
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Title = title,
            Content = content,
            IsVerifiedPurchase = isVerifiedPurchase,
            IsApproved = true,
            CreatedAt = DateTime.UtcNow
        };

        try
        {
            _reviewRepository.Add(review);
            await _reviewRepository.SaveChangesAsync();

            // Update product rating
            await UpdateProductRatingAsync(productId);

            _logger.LogInformation("Review created successfully: {ReviewId} - Product: {ProductId}, Rating: {Rating}/5", review.Id, productId, rating);
            return review;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogError(ex, "Failed to create review for product {ProductId} by user {UserId}", productId, userId);
            throw;
        }
    }

    public async Task<Review> UpdateReviewAsync(int id, int rating, string title, string content)
    {
        _logger.LogInformation("Updating review {ReviewId}", id);

        var review = await _reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            _logger.LogWarning("Review not found for update: {ReviewId}", id);
            throw new NotFoundException("Review", id);
        }

        if (!review.CanBeEdited())
        {
            _logger.LogWarning("Review edit expired (older than 30 days): {ReviewId}", id);
            throw new BusinessException("Reviews can only be edited within 30 days of creation", "REVIEW_EDIT_EXPIRED");
        }

        ValidateReview(rating, title, content);

        review.UpdateReview(rating, title, content);
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(review.ProductId);

        _logger.LogInformation("Review updated successfully: {ReviewId}", review.Id);
        return review;
    }

    public async Task DeleteReviewAsync(int id)
    {
        _logger.LogInformation("Deleting review: {ReviewId}", id);

        var review = await _reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            _logger.LogWarning("Review not found for deletion: {ReviewId}", id);
            throw new NotFoundException("Review", id);
        }

        var productId = review.ProductId;
        _logger.LogDebug("Removing review {ReviewId} for product {ProductId}", id, productId);

        _reviewRepository.Remove(review);
        await _reviewRepository.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(productId);

        _logger.LogInformation("Review deleted successfully: {ReviewId}", review.Id);
    }

    public async Task ApproveReviewAsync(int id)
    {
        _logger.LogInformation("Approving review: {ReviewId}", id);

        var review = await _reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            _logger.LogWarning("Review not found for approval: {ReviewId}", id);
            throw new NotFoundException("Review", id);
        }

        review.Approve();
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        await UpdateProductRatingAsync(review.ProductId);

        _logger.LogInformation("Review approved: {ReviewId}", review.Id);
    }

    public async Task RejectReviewAsync(int id)
    {
        _logger.LogInformation("Rejecting review: {ReviewId}", id);

        var review = await _reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            _logger.LogWarning("Review not found for rejection: {ReviewId}", id);
            throw new NotFoundException("Review", id);
        }

        review.Reject();
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        await UpdateProductRatingAsync(review.ProductId);

        _logger.LogInformation("Review rejected: {ReviewId}", review.Id);
    }

    public async Task MarkAsHelpfulAsync(int id)
    {
        _logger.LogDebug("Marking review as helpful: {ReviewId}", id);

        var review = await _reviewRepository.GetByIdAsync(id);
        if (review is null)
        {
            _logger.LogWarning("Review not found for helpful mark: {ReviewId}", id);
            throw new NotFoundException("Review", id);
        }

        review.MarkAsHelpful();
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        _logger.LogDebug("Review marked as helpful: {ReviewId} (HelpfulCount: {HelpfulCount})", review.Id, review.HelpfulCount);
    }

    /// <summary>
    /// Returns a rating summary for the specified product, including average rating,
    /// total review count, and a per‑star breakdown.
    /// </summary>
    public async Task<RatingSummary> GetRatingSummaryAsync(int productId)
    {
        _logger.LogDebug("Getting rating summary for product: {ProductId}", productId);

        var reviews = await _reviewRepository.FindAsync(r => r.ProductId == productId && r.IsApproved);
        var reviewList = reviews.ToList();

        var count = reviewList.Count;
        var average = count == 0 ? 0m : (decimal)reviewList.Average(r => r.Rating);

        var starCounts = new Dictionary<int, int>();
        for (int i = 1; i <= 5; i++)
        {
            starCounts[i] = 0;
        }

        foreach (var rev in reviewList)
        {
            if (rev.Rating >= 1 && rev.Rating <= 5)
            {
                starCounts[rev.Rating]++;
            }
        }

        var summary = new RatingSummary
        {
            AverageRating = average,
            ReviewCount = count,
            StarCounts = starCounts
        };

        _logger.LogInformation("Rating summary for product {ProductId}: Avg={AverageRating}, Count={ReviewCount}", productId, average, count);
        return summary;
    }

    private async Task UpdateProductRatingAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product is null)
            return;

        var reviews = await _reviewRepository.FindAsync(r => r.ProductId == productId && r.IsApproved);
        var reviewList = reviews.ToList();

        if (reviewList.Count == 0)
        {
            product.UpdateRating(0, 0);
        }
        else
        {
            var averageRating = (decimal)reviewList.Average(r => r.Rating);
            product.UpdateRating(averageRating, reviewList.Count);
        }

        _productRepository.Update(product);
        await _productRepository.SaveChangesAsync();
    }

    private void ValidateReview(int rating, string title, string content)
    {
        if (rating < 1 || rating > 5)
            throw new ValidationException("Rating", "Rating must be between 1 and 5");

        if (string.IsNullOrWhiteSpace(title) || title.Length < 5)
            throw new ValidationException("Title", "Title must be at least 5 characters");

        if (string.IsNullOrWhiteSpace(content) || content.Length < 10)
            throw new ValidationException("Content", "Review content must be at least 10 characters");
    }
}
