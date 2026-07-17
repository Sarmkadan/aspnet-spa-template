#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Provides validation helpers for <see cref="ReviewServiceTests"/> instances.
/// </summary>
public static class ReviewServiceTestsValidation
{
    private static readonly FieldInfo? _mockReviewRepositoryField = typeof(ReviewServiceTests).GetField("_mockReviewRepository", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? _mockProductRepositoryField = typeof(ReviewServiceTests).GetField("_mockProductRepository", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? _reviewServiceField = typeof(ReviewServiceTests).GetField("_reviewService", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MethodInfo? _getReviewByIdValidMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.GetReviewByIdAsync_WithValidId_ReturnsReview));
    private static readonly MethodInfo? _getReviewByIdInvalidMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.GetReviewByIdAsync_WithInvalidId_ThrowsNotFoundException));
    private static readonly MethodInfo? _createReviewValidMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.CreateReviewAsync_WithValidRequest_CreatesReview));
    private static readonly MethodInfo? _createReviewInvalidRatingMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.CreateReviewAsync_WithInvalidRating_ThrowsValidationException));
    private static readonly MethodInfo? _createReviewShortTitleMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.CreateReviewAsync_WithShortTitle_ThrowsValidationException));
    private static readonly MethodInfo? _createReviewShortContentMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.CreateReviewAsync_WithShortContent_ThrowsValidationException));
    private static readonly MethodInfo? _createReviewNonExistentProductMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.CreateReviewAsync_WithNonExistentProduct_ThrowsNotFoundException));
    private static readonly MethodInfo? _createReviewDuplicateMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.CreateReviewAsync_WithDuplicateReview_ThrowsBusinessException));
    private static readonly MethodInfo? _getProductReviewsMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.GetProductReviewsAsync_ReturnsApprovedReviews));
    private static readonly MethodInfo? _getUserReviewsMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.GetUserReviewsAsync_ReturnsUserReviews));
    private static readonly MethodInfo? _approveReviewMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.ApproveReviewAsync_WithValidId_ApprovesReview));
    private static readonly MethodInfo? _deleteReviewMethod = typeof(ReviewServiceTests).GetMethod(nameof(ReviewServiceTests.DeleteReviewAsync_WithValidId_DeletesReview));

    /// <summary>
    /// Validates the specified <see cref="ReviewServiceTests"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this ReviewServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate field dependencies using reflection
        ValidateField(value, _mockReviewRepositoryField, "_mockReviewRepository", problems);
        ValidateField(value, _mockProductRepositoryField, "_mockProductRepository", problems);
        ValidateField(value, _reviewServiceField, "_reviewService", problems);

        // Validate public methods
        ValidateMethod(_getReviewByIdValidMethod, nameof(ReviewServiceTests.GetReviewByIdAsync_WithValidId_ReturnsReview), problems);
        ValidateMethod(_getReviewByIdInvalidMethod, nameof(ReviewServiceTests.GetReviewByIdAsync_WithInvalidId_ThrowsNotFoundException), problems);
        ValidateMethod(_createReviewValidMethod, nameof(ReviewServiceTests.CreateReviewAsync_WithValidRequest_CreatesReview), problems);
        ValidateMethod(_createReviewInvalidRatingMethod, nameof(ReviewServiceTests.CreateReviewAsync_WithInvalidRating_ThrowsValidationException), problems);
        ValidateMethod(_createReviewShortTitleMethod, nameof(ReviewServiceTests.CreateReviewAsync_WithShortTitle_ThrowsValidationException), problems);
        ValidateMethod(_createReviewShortContentMethod, nameof(ReviewServiceTests.CreateReviewAsync_WithShortContent_ThrowsValidationException), problems);
        ValidateMethod(_createReviewNonExistentProductMethod, nameof(ReviewServiceTests.CreateReviewAsync_WithNonExistentProduct_ThrowsNotFoundException), problems);
        ValidateMethod(_createReviewDuplicateMethod, nameof(ReviewServiceTests.CreateReviewAsync_WithDuplicateReview_ThrowsBusinessException), problems);
        ValidateMethod(_getProductReviewsMethod, nameof(ReviewServiceTests.GetProductReviewsAsync_ReturnsApprovedReviews), problems);
        ValidateMethod(_getUserReviewsMethod, nameof(ReviewServiceTests.GetUserReviewsAsync_ReturnsUserReviews), problems);
        ValidateMethod(_approveReviewMethod, nameof(ReviewServiceTests.ApproveReviewAsync_WithValidId_ApprovesReview), problems);
        ValidateMethod(_deleteReviewMethod, nameof(ReviewServiceTests.DeleteReviewAsync_WithValidId_DeletesReview), problems);

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="ReviewServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if the instance is valid; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this ReviewServiceTests value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="ReviewServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid, containing the validation problems.</exception>
    public static void EnsureValid(this ReviewServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ReviewServiceTests instance is not valid:{Environment.NewLine}{string.Join(Environment.NewLine, problems)}");
    }

    private static void ValidateField(ReviewServiceTests instance, FieldInfo? field, string fieldName, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        ArgumentNullException.ThrowIfNull(problems);

        if (field is null)
        {
            problems.Add($"Field '{fieldName}' is not found on ReviewServiceTests.");
            return;
        }

        var fieldValue = field.GetValue(instance);
        if (fieldValue is null)
        {
            problems.Add($"Field '{fieldName}' is null.");
        }
    }

    private static void ValidateMethod(MethodInfo? method, string methodName, List<string> problems)
    {
        ArgumentNullException.ThrowIfNull(problems);
        ArgumentException.ThrowIfNullOrEmpty(methodName);

        if (method is null)
        {
            problems.Add($"Method '{methodName}' is not found.");
        }
        else if (method.DeclaringType != typeof(ReviewServiceTests))
        {
            problems.Add($"Method '{methodName}' is not declared on ReviewServiceTests.");
        }
    }
}