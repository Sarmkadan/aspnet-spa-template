#nullable enable
using System.Linq.Expressions;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Additional unit tests for the <see cref="ReviewService"/> class.
/// Tests cover update, rejection, helpful marking, rating summary, and recalculation scenarios.
/// Focuses on testing service behavior through mock interactions rather than complex filtering.
/// </summary>
public sealed class ReviewServiceUnitTests
{
    /// <summary>
    /// Mock repository for <see cref="Review"/> entities.
    /// </>
    private readonly Mock<IRepository<Review>> _mockReviewRepository;

    /// <summary>
    /// Mock repository for <see cref="Product"/> entities.
    /// </summary>
    private readonly Mock<ProductRepository> _mockProductRepository;

    /// <summary>
    /// Instance of the service being tested.
    /// </summary>
    private readonly ReviewService _reviewService;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReviewServiceUnitTests"/> class.
    /// Sets up mock repositories and creates the <see cref="ReviewService"/> instance for testing.
    /// </summary>
    public ReviewServiceUnitTests()
    {
        _mockReviewRepository = new Mock<IRepository<Review>>();
        _mockProductRepository = new Mock<ProductRepository>();
        _reviewService = new ReviewService(_mockReviewRepository.Object, _mockProductRepository.Object, NullLogger<ReviewService>.Instance);
    }

    #region UpdateReviewAsync Tests

    /// <summary>
    /// Tests that UpdateReviewAsync successfully updates a review with valid parameters.
    /// </summary>
    [Fact]
    public async Task UpdateReviewAsync_WithValidRequest_UpdatesReview()
    {
        // Arrange
        var reviewId = 1;
        var existingReview = new Review
        {
            Id = reviewId,
            Rating = 3,
            Title = "Old Title",
            Content = "Old content that is long enough",
            CreatedAt = DateTime.UtcNow.AddDays(-5) // Within 30 days
        };
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(existingReview);
        _mockReviewRepository.Setup(r => r.Update(It.IsAny<Review>()));
        _mockReviewRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Review, bool>>>()))
            .ReturnsAsync(new List<Review> { existingReview });
        _mockProductRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Product { Id = 1 });
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _reviewService.UpdateReviewAsync(reviewId, 5, "New Title", "New content that is also long enough for validation");

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(5);
        result.Title.Should().Be("New Title");
        result.Content.Should().Be("New content that is also long enough for validation");
        result.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(5));
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Once);
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _mockProductRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateReviewAsync throws NotFoundException when review ID is invalid.
    /// </summary>
    [Fact]
    public async Task UpdateReviewAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var reviewId = 999;
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);

        // Act
        Func<Task> act = () => _reviewService.UpdateReviewAsync(reviewId, 5, "Title", "Content is long enough");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _mockReviewRepository.Verify(r => r.GetByIdAsync(reviewId), Times.Once);
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Never);
    }

    /// <summary>
    /// Tests that UpdateReviewAsync throws BusinessException when review is older than 30 days.
    /// </summary>
    [Fact]
    public async Task UpdateReviewAsync_WithExpiredReview_ThrowsBusinessException()
    {
        // Arrange
        var reviewId = 1;
        var expiredReview = new Review
        {
            Id = reviewId,
            Rating = 3,
            Title = "Old Title",
            Content = "Old content that is long enough",
            CreatedAt = DateTime.UtcNow.AddDays(-35) // Older than 30 days
        };
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(expiredReview);

        // Act
        Func<Task> act = () => _reviewService.UpdateReviewAsync(reviewId, 5, "New Title", "New content that is long enough");

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
        _mockReviewRepository.Verify(r => r.GetByIdAsync(reviewId), Times.Once);
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Never);
    }

    /// <summary>
    /// Tests that UpdateReviewAsync throws ValidationException when rating is invalid.
    /// </summary>
    [Fact]
    public async Task UpdateReviewAsync_WithInvalidRating_ThrowsValidationException()
    {
        // Arrange
        var reviewId = 1;
        var existingReview = new Review
        {
            Id = reviewId,
            Rating = 3,
            Title = "Old Title",
            Content = "Old content that is long enough",
            CreatedAt = DateTime.UtcNow.AddDays(-5)
        };
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(existingReview);

        // Act
        Func<Task> act = () => _reviewService.UpdateReviewAsync(reviewId, 0, "Title", "Content is long enough");

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
        _mockReviewRepository.Verify(r => r.GetByIdAsync(reviewId), Times.Once);
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Never);
    }

    #endregion

    #region DeleteReviewAsync Tests

    /// <summary>
    /// Tests that DeleteReviewAsync successfully deletes a review with a valid ID.
    /// </summary>
    [Fact]
    public async Task DeleteReviewAsync_WithValidId_DeletesReview()
    {
        // Arrange
        var reviewId = 1;
        var reviewToDelete = new Review { Id = reviewId, ProductId = 1 };
        var product = new Product { Id = 1 };
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(reviewToDelete);
        _mockProductRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(product);
        _mockReviewRepository.Setup(r => r.Remove(It.IsAny<Review>()));
        _mockReviewRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Review, bool>>>())).ReturnsAsync(new List<Review>());
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _reviewService.DeleteReviewAsync(reviewId);

        // Assert
        _mockReviewRepository.Verify(r => r.Remove(reviewToDelete), Times.Once);
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _mockProductRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteReviewAsync throws NotFoundException when review ID is invalid.
    /// </summary>
    [Fact]
    public async Task DeleteReviewAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var reviewId = 999;
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);

        // Act
        Func<Task> act = () => _reviewService.DeleteReviewAsync(reviewId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _mockReviewRepository.Verify(r => r.GetByIdAsync(reviewId), Times.Once);
        _mockReviewRepository.Verify(r => r.Remove(It.IsAny<Review>()), Times.Never);
    }

    #endregion

    #region ApproveReviewAsync Tests

    /// <summary>
    /// Tests that ApproveReviewAsync successfully approves a review with a valid ID.
    /// </summary>
    [Fact]
    public async Task ApproveReviewAsync_WithValidId_ApprovesReview()
    {
        // Arrange
        var reviewId = 1;
        var review = new Review { Id = reviewId, IsApproved = false, ProductId = 1 };
        var product = new Product { Id = 1 };
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _mockProductRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(product);
        _mockReviewRepository.Setup(r => r.Update(It.IsAny<Review>()));
        _mockReviewRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Review, bool>>>())).ReturnsAsync(new List<Review> { review });
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _reviewService.ApproveReviewAsync(reviewId);

        // Assert
        review.IsApproved.Should().BeTrue();
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Once);
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _mockProductRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that ApproveReviewAsync throws NotFoundException when review ID is invalid.
    /// </summary>
    [Fact]
    public async Task ApproveReviewAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var reviewId = 999;
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);

        // Act
        Func<Task> act = () => _reviewService.ApproveReviewAsync(reviewId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _mockReviewRepository.Verify(r => r.GetByIdAsync(reviewId), Times.Once);
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Never);
    }

    #endregion

    #region RejectReviewAsync Tests

    /// <summary>
    /// Tests that RejectReviewAsync successfully rejects a review with a valid ID.
    /// </summary>
    [Fact]
    public async Task RejectReviewAsync_WithValidId_RejectsReview()
    {
        // Arrange
        var reviewId = 1;
        var review = new Review { Id = reviewId, IsApproved = true, ProductId = 1 };
        var product = new Product { Id = 1 };
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _mockProductRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(product);
        _mockReviewRepository.Setup(r => r.Update(It.IsAny<Review>()));
        _mockReviewRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Review, bool>>>())).ReturnsAsync(new List<Review> { review });
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _reviewService.RejectReviewAsync(reviewId);

        // Assert
        review.IsApproved.Should().BeFalse();
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Once);
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
        _mockProductRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that RejectReviewAsync throws NotFoundException when review ID is invalid.
    /// </summary>
    [Fact]
    public async Task RejectReviewAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var reviewId = 999;
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);

        // Act
        Func<Task> act = () => _reviewService.RejectReviewAsync(reviewId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _mockReviewRepository.Verify(r => r.GetByIdAsync(reviewId), Times.Once);
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Never);
    }

    #endregion

    #region MarkAsHelpfulAsync Tests

    /// <summary>
    /// Tests that MarkAsHelpfulAsync successfully increments the helpful count.
    /// </summary>
    [Fact]
    public async Task MarkAsHelpfulAsync_WithValidId_IncrementsHelpfulCount()
    {
        // Arrange
        var reviewId = 1;
        var review = new Review { Id = reviewId, HelpfulCount = 5 };
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _mockReviewRepository.Setup(r => r.Update(It.IsAny<Review>()));
        _mockReviewRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _reviewService.MarkAsHelpfulAsync(reviewId);

        // Assert
        review.HelpfulCount.Should().Be(6);
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Once);
        _mockReviewRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Tests that MarkAsHelpfulAsync throws NotFoundException when review ID is invalid.
    /// </summary>
    [Fact]
    public async Task MarkAsHelpfulAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var reviewId = 999;
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);

        // Act
        Func<Task> act = () => _reviewService.MarkAsHelpfulAsync(reviewId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
        _mockReviewRepository.Verify(r => r.GetByIdAsync(reviewId), Times.Once);
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Never);
    }

    #endregion

    #region GetRatingSummaryAsync Tests

    /// <summary>
    /// Tests that GetRatingSummaryAsync returns correct summary when there are approved reviews.
    /// </summary>
    [Fact]
    public async Task GetRatingSummaryAsync_WithApprovedReviews_ReturnsCorrectSummary()
    {
        // Arrange
        var productId = 1;
        var reviews = new List<Review>
        {
            new Review { Id = 1, ProductId = productId, Rating = 5, IsApproved = true },
            new Review { Id = 2, ProductId = productId, Rating = 5, IsApproved = true },
            new Review { Id = 3, ProductId = productId, Rating = 4, IsApproved = true },
            new Review { Id = 4, ProductId = productId, Rating = 3, IsApproved = true }
        };
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Review, bool>>>())).ReturnsAsync(reviews);

        // Act
        var result = await _reviewService.GetRatingSummaryAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.ReviewCount.Should().Be(4); // Only approved reviews
        result.AverageRating.Should().Be(4.25m); // (5+5+4+3)/4
        result.StarCounts[5].Should().Be(2);
        result.StarCounts[4].Should().Be(1);
        result.StarCounts[3].Should().Be(1);
        result.StarCounts[2].Should().Be(0);
        result.StarCounts[1].Should().Be(0);
    }

    /// <summary>
    /// Tests that GetRatingSummaryAsync returns zero values when there are no approved reviews.
    /// </summary>
    [Fact]
    public async Task GetRatingSummaryAsync_NoApprovedReviews_ReturnsZeroValues()
    {
        // Arrange
        var productId = 1;
        var reviews = new List<Review>();
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Review, bool>>>())).ReturnsAsync(reviews);

        // Act
        var result = await _reviewService.GetRatingSummaryAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result.ReviewCount.Should().Be(0);
        result.AverageRating.Should().Be(0m);
        result.StarCounts.Values.Should().AllBeEquivalentTo(0);
    }

    #endregion

    #region RecalculateAsync Tests

    /// <summary>
    /// Tests that RecalculateAsync updates ratings for all products and returns count of updated products.
    /// </summary>
    [Fact]
    public async Task RecalculateAsync_WithProductsAndReviews_UpdatesAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1" },
            new Product { Id = 2, Name = "Product 2" },
            new Product { Id = 3, Name = "Product 3" }
        };
        var reviews = new List<Review>();
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(products);
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Review, bool>>>())).ReturnsAsync(reviews);
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _reviewService.RecalculateAsync();

        // Assert
        result.Should().Be(3); // All 3 products should be updated
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Exactly(3));
        _mockProductRepository.Verify(r => r.SaveChangesAsync(), Times.Once); // Called once at the end
    }

    /// <summary>
    /// Tests that RecalculateAsync returns 0 when there are no products.
    /// </summary>
    [Fact]
    public async Task RecalculateAsync_NoProducts_ReturnsZero()
    {
        // Arrange
        _mockProductRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Product>());
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _reviewService.RecalculateAsync();

        // Assert
        result.Should().Be(0);
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
        _mockProductRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    #endregion
}