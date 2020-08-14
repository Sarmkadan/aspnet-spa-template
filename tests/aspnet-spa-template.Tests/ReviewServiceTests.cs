#nullable enable
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class ReviewServiceTests
{
    private readonly Mock<IRepository<Review>> _mockReviewRepository;
    private readonly Mock<ProductRepository> _mockProductRepository;
    private readonly ReviewService _reviewService;

    public ReviewServiceTests()
    {
        _mockReviewRepository = new Mock<IRepository<Review>>();
        _mockProductRepository = new Mock<ProductRepository>();
        _reviewService = new ReviewService(_mockReviewRepository.Object, _mockProductRepository.Object);
    }

    [Fact]
    public async Task GetReviewByIdAsync_WithValidId_ReturnsReview()
    {
        // Arrange
        var reviewId = 1;
        var review = new Review { Id = reviewId, Rating = 5, Title = "Great product", Content = "Really love this item" };
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);

        // Act
        var result = await _reviewService.GetReviewByIdAsync(reviewId);

        // Assert
        result.Should().NotBeNull();
        result?.Rating.Should().Be(5);
    }

    [Fact]
    public async Task GetReviewByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var reviewId = 999;
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync((Review?)null);

        // Act
        var act = () => _reviewService.GetReviewByIdAsync(reviewId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateReviewAsync_WithValidRequest_CreatesReview()
    {
        // Arrange
        var productId = 1;
        var userId = 5;
        var product = new Product { Id = productId, Name = "Test Product" };
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _mockReviewRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<Review, bool>>())).ReturnsAsync((Review?)null);
        _mockReviewRepository.Setup(r => r.Add(It.IsAny<Review>()));
        _mockReviewRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Func<Review, bool>>())).ReturnsAsync(new List<Review>());
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        var result = await _reviewService.CreateReviewAsync(productId, userId, 5, "Great!", "This product is fantastic");

        // Assert
        result.Should().NotBeNull();
        result.Rating.Should().Be(5);
        _mockReviewRepository.Verify(r => r.Add(It.IsAny<Review>()), Times.Once);
    }

    [Fact]
    public async Task CreateReviewAsync_WithInvalidRating_ThrowsValidationException()
    {
        // Act
        var act = () => _reviewService.CreateReviewAsync(1, 5, 6, "Title", "Content is long enough");

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateReviewAsync_WithShortTitle_ThrowsValidationException()
    {
        // Act
        var act = () => _reviewService.CreateReviewAsync(1, 5, 5, "Bad", "Content is long enough");

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateReviewAsync_WithShortContent_ThrowsValidationException()
    {
        // Act
        var act = () => _reviewService.CreateReviewAsync(1, 5, 5, "Valid Title", "Short");

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateReviewAsync_WithNonExistentProduct_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 999;
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        // Act
        var act = () => _reviewService.CreateReviewAsync(productId, 5, 5, "Title Here", "Content here is good");

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateReviewAsync_WithDuplicateReview_ThrowsBusinessException()
    {
        // Arrange
        var productId = 1;
        var userId = 5;
        var product = new Product { Id = productId };
        var existingReview = new Review { Id = 1, ProductId = productId, UserId = userId };
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _mockReviewRepository.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<Review, bool>>())).ReturnsAsync(existingReview);

        // Act
        var act = () => _reviewService.CreateReviewAsync(productId, userId, 4, "Title Here", "Content here is good");

        // Assert
        await act.Should().ThrowAsync<BusinessException>();
    }

    [Fact]
    public async Task GetProductReviewsAsync_ReturnsApprovedReviews()
    {
        // Arrange
        var productId = 1;
        var reviews = new List<Review>
        {
            new Review { Id = 1, ProductId = productId, IsApproved = true, CreatedAt = DateTime.UtcNow },
            new Review { Id = 2, ProductId = productId, IsApproved = true, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        };
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Func<Review, bool>>())).ReturnsAsync(reviews);

        // Act
        var result = await _reviewService.GetProductReviewsAsync(productId);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserReviewsAsync_ReturnsUserReviews()
    {
        // Arrange
        var userId = 5;
        var reviews = new List<Review>
        {
            new Review { Id = 1, UserId = userId, CreatedAt = DateTime.UtcNow },
            new Review { Id = 2, UserId = userId, CreatedAt = DateTime.UtcNow.AddDays(-1) }
        };
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Func<Review, bool>>())).ReturnsAsync(reviews);

        // Act
        var result = await _reviewService.GetUserReviewsAsync(userId);

        // Assert
        result.Should().HaveCount(2);
    }

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
        _mockReviewRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Func<Review, bool>>())).ReturnsAsync(new List<Review> { review });
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _reviewService.ApproveReviewAsync(reviewId);

        // Assert
        _mockReviewRepository.Verify(r => r.Update(It.IsAny<Review>()), Times.Once);
    }

    [Fact]
    public async Task DeleteReviewAsync_WithValidId_DeletesReview()
    {
        // Arrange
        var reviewId = 1;
        var review = new Review { Id = reviewId, ProductId = 1 };
        var product = new Product { Id = 1 };
        _mockReviewRepository.Setup(r => r.GetByIdAsync(reviewId)).ReturnsAsync(review);
        _mockProductRepository.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(product);
        _mockReviewRepository.Setup(r => r.Remove(It.IsAny<Review>()));
        _mockReviewRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);
        _mockReviewRepository.Setup(r => r.FindAsync(It.IsAny<Func<Review, bool>>())).ReturnsAsync(new List<Review>());
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

        // Act
        await _reviewService.DeleteReviewAsync(reviewId);

        // Assert
        _mockReviewRepository.Verify(r => r.Remove(review), Times.Once);
    }
}
