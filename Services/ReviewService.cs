// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Service for review-related business logic.
/// </summary>
public class ReviewService
{
    private readonly IRepository<Review> _reviewRepository;
    private readonly ProductRepository _productRepository;

    public ReviewService(IRepository<Review> reviewRepository, ProductRepository productRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
    }

    public async Task<Review?> GetReviewByIdAsync(int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
            throw new NotFoundException("Review", id);

        return review;
    }

    public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId)
    {
        var reviews = await _reviewRepository.FindAsync(r => r.ProductId == productId && r.IsApproved);
        return reviews.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task<IEnumerable<Review>> GetUserReviewsAsync(int userId)
    {
        var reviews = await _reviewRepository.FindAsync(r => r.UserId == userId);
        return reviews.OrderByDescending(r => r.CreatedAt).ToList();
    }

    public async Task<Review> CreateReviewAsync(int productId, int userId, int rating, string title, string content, bool isVerifiedPurchase = false)
    {
        ValidateReview(rating, title, content);

        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
            throw new NotFoundException("Product", productId);

        // Check if user already reviewed this product
        var existingReview = await _reviewRepository.FirstOrDefaultAsync(r => r.ProductId == productId && r.UserId == userId);
        if (existingReview != null)
            throw new BusinessException("User has already reviewed this product", "DUPLICATE_REVIEW");

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

        _reviewRepository.Add(review);
        await _reviewRepository.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(productId);

        return review;
    }

    public async Task<Review> UpdateReviewAsync(int id, int rating, string title, string content)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
            throw new NotFoundException("Review", id);

        if (!review.CanBeEdited())
            throw new BusinessException("Reviews can only be edited within 30 days of creation", "REVIEW_EDIT_EXPIRED");

        ValidateReview(rating, title, content);

        review.UpdateReview(rating, title, content);
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(review.ProductId);

        return review;
    }

    public async Task DeleteReviewAsync(int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
            throw new NotFoundException("Review", id);

        var productId = review.ProductId;
        _reviewRepository.Remove(review);
        await _reviewRepository.SaveChangesAsync();

        // Update product rating
        await UpdateProductRatingAsync(productId);
    }

    public async Task ApproveReviewAsync(int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
            throw new NotFoundException("Review", id);

        review.Approve();
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        await UpdateProductRatingAsync(review.ProductId);
    }

    public async Task RejectReviewAsync(int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
            throw new NotFoundException("Review", id);

        review.Reject();
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();

        await UpdateProductRatingAsync(review.ProductId);
    }

    public async Task MarkAsHelpfulAsync(int id)
    {
        var review = await _reviewRepository.GetByIdAsync(id);
        if (review == null)
            throw new NotFoundException("Review", id);

        review.MarkAsHelpful();
        _reviewRepository.Update(review);
        await _reviewRepository.SaveChangesAsync();
    }

    private async Task UpdateProductRatingAsync(int productId)
    {
        var product = await _productRepository.GetByIdAsync(productId);
        if (product == null)
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
