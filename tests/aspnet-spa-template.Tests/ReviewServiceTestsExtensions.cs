#nullable enable

using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Extension methods for <see cref="ReviewServiceTests"/> that provide convenient assertions and helper methods
/// for testing review service functionality.
/// </summary>
public static class ReviewServiceTestsExtensions
{
    /// <summary>
    /// Asserts that a review has the expected properties.
    /// </summary>
    /// <param name="review">The review to assert against.</param>
    /// <param name="expectedRating">The expected rating value.</param>
    /// <param name="expectedTitle">The expected title.</param>
    /// <param name="expectedContent">The expected content.</param>
    /// <param name="expectedIsApproved">The expected approval status.</param>
    public static void ShouldHaveReviewProperties(
        this Review? review,
        int expectedRating,
        string expectedTitle,
        string expectedContent,
        bool expectedIsApproved = true)
    {
        ArgumentNullException.ThrowIfNull(review);

        review.Rating.Should().Be(expectedRating, "because the rating should match the expected value");
        review.Title.Should().Be(expectedTitle, "because the title should match the expected value");
        review.Content.Should().Be(expectedContent, "because the content should match the expected value");
        review.IsApproved.Should().Be(expectedIsApproved, "because the approval status should match the expected value");
    }

    /// <summary>
    /// Asserts that a review matches the expected review.
    /// </summary>
    /// <param name="actual">The actual review.</param>
    /// <param name="expected">The expected review.</param>
    public static void ShouldMatchReview(this Review? actual, Review expected)
    {
        ArgumentNullException.ThrowIfNull(actual);
        ArgumentNullException.ThrowIfNull(expected);

        actual.Id.Should().Be(expected.Id);
        actual.Rating.Should().Be(expected.Rating);
        actual.Title.Should().Be(expected.Title);
        actual.Content.Should().Be(expected.Content);
        actual.IsApproved.Should().Be(expected.IsApproved);
        actual.ProductId.Should().Be(expected.ProductId);
        actual.UserId.Should().Be(expected.UserId);
    }

    /// <summary>
    /// Asserts that a collection of reviews contains exactly the expected reviews.
    /// </summary>
    /// <param name="reviews">The collection of reviews.</param>
    /// <param name="expectedReviews">The expected reviews.</param>
    public static void ShouldContainExactly(this IEnumerable<Review> reviews, IEnumerable<Review> expectedReviews)
    {
        ArgumentNullException.ThrowIfNull(reviews);
        ArgumentNullException.ThrowIfNull(expectedReviews);

        reviews.Should().HaveCount(expectedReviews.Count(), "because the count should match the expected reviews count");

        foreach (var expectedReview in expectedReviews)
        {
            reviews.Should().ContainSingle(r => r.Id == expectedReview.Id,
                $"because there should be exactly one review with Id {expectedReview.Id}");
        }
    }

    /// <summary>
    /// Asserts that a collection of reviews contains a review with the specified rating.
    /// </summary>
    /// <param name="reviews">The collection of reviews.</param>
    /// <param name="expectedRating">The expected rating.</param>
    public static void ShouldContainRating(this IEnumerable<Review> reviews, int expectedRating)
    {
        ArgumentNullException.ThrowIfNull(reviews);

        reviews.Should().ContainSingle(r => r.Rating == expectedRating,
            $"because there should be exactly one review with rating {expectedRating}");
    }

    /// <summary>
    /// Asserts that a collection of reviews contains a review with the specified user ID.
    /// </summary>
    /// <param name="reviews">The collection of reviews.</param>
    /// <param name="expectedUserId">The expected user ID.</param>
    public static void ShouldContainUserReviews(this IEnumerable<Review> reviews, int expectedUserId)
    {
        ArgumentNullException.ThrowIfNull(reviews);

        reviews.Should().AllSatisfy(r => r.UserId.Should().Be(expectedUserId,
            $"because all reviews should belong to user {expectedUserId}"));
    }

    /// <summary>
    /// Asserts that a collection of reviews contains only approved reviews.
    /// </summary>
    /// <param name="reviews">The collection of reviews.</param>
    public static void ShouldContainOnlyApprovedReviews(this IEnumerable<Review> reviews)
    {
        ArgumentNullException.ThrowIfNull(reviews);

        reviews.Should().AllSatisfy(r => r.IsApproved.Should().BeTrue(
            "because all reviews in the collection should be approved"));
    }

    /// <summary>
    /// Asserts that a collection of reviews contains only reviews for the specified product.
    /// </summary>
    /// <param name="reviews">The collection of reviews.</param>
    /// <param name="expectedProductId">The expected product ID.</param>
    public static void ShouldContainOnlyProductReviews(this IEnumerable<Review> reviews, int expectedProductId)
    {
        ArgumentNullException.ThrowIfNull(reviews);

        reviews.Should().AllSatisfy(r => r.ProductId.Should().Be(expectedProductId,
            $"because all reviews should belong to product {expectedProductId}"));
    }

    /// <summary>
    /// Asserts that a collection of reviews is empty.
    /// </summary>
    /// <param name="reviews">The collection of reviews.</param>
    public static void ShouldBeEmpty(this IEnumerable<Review> reviews)
    {
        ArgumentNullException.ThrowIfNull(reviews);
        reviews.Should().BeEmpty("because the reviews collection should be empty");
    }

    /// <summary>
    /// Creates a test review with the specified properties.
    /// </summary>
    /// <param name="id">The review ID.</param>
    /// <param name="productId">The product ID.</param>
    /// <param name="userId">The user ID.</param>
    /// <param name="rating">The rating (1-5).</param>
    /// <param name="title">The review title.</param>
    /// <param name="content">The review content.</param>
    /// <param name="isApproved">Whether the review is approved.</param>
    /// <returns>A configured review instance.</returns>
    public static Review CreateTestReview(
        int id = 1,
        int productId = 1,
        int userId = 1,
        int rating = 5,
        string title = "Great product",
        string content = "This is an excellent product that I highly recommend",
        bool isApproved = true)
    {
        return new Review
        {
            Id = id,
            ProductId = productId,
            UserId = userId,
            Rating = rating,
            Title = title,
            Content = content,
            IsApproved = isApproved,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a test product with the specified properties.
    /// </summary>
    /// <param name="id">The product ID.</param>
    /// <param name="name">The product name.</param>
    /// <returns>A configured product instance.</returns>
    public static Product CreateTestProduct(int id = 1, string name = "Test Product")
    {
        return new Product
        {
            Id = id,
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Asserts that a validation exception has the expected validation errors.
    /// </summary>
    /// <param name="validationException">The validation exception.</param>
    /// <param name="expectedErrors">The expected error messages.</param>
    public static void ShouldHaveValidationErrors(this ValidationException validationException, params string[] expectedErrors)
    {
        ArgumentNullException.ThrowIfNull(validationException);
        ArgumentNullException.ThrowIfNull(expectedErrors);

        var allErrors = validationException.Errors.Values.SelectMany(v => v).ToList();
        allErrors.Should().HaveCount(expectedErrors.Length,
            "because the number of validation errors should match the expected count");

        foreach (var expectedError in expectedErrors)
        {
            allErrors.Should().ContainSingle(e => e.Contains(expectedError, StringComparison.OrdinalIgnoreCase),
                $"because validation errors should contain: {expectedError}");
        }
    }

    /// <summary>
    /// Asserts that a business exception has the expected error message.
    /// </summary>
    /// <param name="businessException">The business exception.</param>
    /// <param name="expectedMessage">The expected error message.</param>
    public static void ShouldHaveBusinessExceptionMessage(this BusinessException businessException, string expectedMessage)
    {
        ArgumentNullException.ThrowIfNull(businessException);
        ArgumentException.ThrowIfNullOrEmpty(expectedMessage);

        businessException.Message.Should().Be(expectedMessage,
            "because the business exception message should match the expected message");
    }
}