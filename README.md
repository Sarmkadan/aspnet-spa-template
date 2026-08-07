// ## ReviewServiceUnitTests

The ReviewServiceUnitTests class contains unit tests for the ReviewService class.

These tests cover various scenarios, including:

* Updating a review with a valid request
* Updating a review with an invalid ID
* Updating a review with an expired review
* Updating a review with an invalid rating
* Deleting a review with a valid ID
* Deleting a review with an invalid ID
* Approving a review with a valid ID
* Approving a review with an invalid ID
* Rejecting a review with a valid ID
* Rejecting a review with an invalid ID
* Marking a review as helpful with a valid ID
* Marking a review as helpful with an invalid ID
* Getting the rating summary for approved reviews
* Getting the rating summary with no approved reviews
* Recalculating the rating summary with products and reviews
* Recalculating the rating summary with no products

Example usage:

```csharp
public ReviewServiceUnitTests
public async Task UpdateReviewAsync_WithValidRequest_UpdatesReview
public async Task UpdateReviewAsync_WithInvalidId_ThrowsNotFoundException
public async Task UpdateReviewAsync_WithExpiredReview_ThrowsBusinessException
public async Task UpdateReviewAsync_WithInvalidRating_ThrowsValidationException
public async Task DeleteReviewAsync_WithValidId_DeletesReview
public async Task DeleteReviewAsync_WithInvalidId_ThrowsNotFoundException
public async Task ApproveReviewAsync_WithValidId_ApprovesReview
public async Task ApproveReviewAsync_WithInvalidId_ThrowsNotFoundException
public async Task RejectReviewAsync_WithValidId_RejectsReview
public async Task RejectReviewAsync_WithInvalidId_ThrowsNotFoundException
public async Task MarkAsHelpfulAsync_WithValidId_IncrementsHelpfulCount
public async Task MarkAsHelpfulAsync_WithInvalidId_ThrowsNotFoundException
public async Task GetRatingSummaryAsync_WithApprovedReviews_ReturnsCorrectSummary
public async Task GetRatingSummaryAsync_NoApprovedReviews_ReturnsZeroValues
public async Task RecalculateAsync_WithProductsAndReviews_UpdatesAllProducts
public async Task RecalculateAsync_NoProducts_ReturnsZero
```

## PaginationRequestUnitTests

The `PaginationRequestUnitTests` class validates the behavior of the `PaginationRequest` DTO. It ensures that default values are set correctly and that paging, sorting, and search‑term properties enforce their minimum and maximum constraints.

Example usage:

```csharp
// Create a request with default values
var request = new PaginationRequest(); // Constructor_WithNoParameters_SetsDefaultValues

// PageNumber tests
request.PageNumber = 5;   // PageNumber_WithValidValue_SetsAndGetsCorrectly
request.PageNumber = 0;   // PageNumber_WithZero_EnforcesMinimumValue
request.PageNumber = -3;  // PageNumber_WithNegativeValue_EnforcesMinimumValue
request.PageNumber = 1000; // PageNumber_WithLargeValue_AcceptsLargeValue

// PageSize tests
request.PageSize = 20;    // PageSize_WithValidValue_SetsAndGetsCorrectly
request.PageSize = 0;     // PageSize_WithZero_EnforcesMinimumValue
request.PageSize = -5;    // PageSize_WithNegativeValue_EnforcesMinimumValue
request.PageSize = 150;   // PageSize_WithValueGreaterThan100_EnforcesMaximumValue
request.PageSize = 100;   // PageSize_WithValueOf100_AcceptsBoundaryValue
request.PageSize = 1;     // PageSize_WithValueOf1_AcceptsBoundaryValue

// SortBy tests
request.SortBy = "Name";          // SortBy_WithValidString_SetsCorrectly
request.SortBy = null;           // SortBy_WithNull_SetsToNull
request.SortBy = string.Empty;   // SortBy_WithEmptyString_SetsToEmptyString

// SortDescending tests
bool defaultSortDesc = request.SortDescending; // SortDescending_DefaultValue_IsFalse
request.SortDescending = true;   // SortDescending_WithTrueValue_SetsToTrue
request.SortDescending = false;  // SortDescending_WithFalseValue_SetsToFalse

// SearchTerm tests
request.SearchTerm = "apple";    // SearchTerm_WithValidString_SetsCorrectly
request.SearchTerm = null;       // SearchTerm_WithNull_SetsToNull
request.SearchTerm = string.Empty; // SearchTerm_WithEmptyString_SetsToEmptyString
```

## ReviewsController

The `ReviewsController` (defined in `Controllers/ReviewsController.cs`) exposes the HTTP API for product reviews. It lets clients retrieve a single review or the reviews belonging to a product, vote a review as helpful, and trigger a recalculation of product rating summaries. Reviews returned by these endpoints carry their rating, title, content, helpfulness count, and verification/approval status.

Example usage:

```csharp
// ReviewsController is activated by the ASP.NET Core framework, which
// resolves its constructor dependencies via dependency injection:
//
//     public ReviewsController(...)
//
// Once an instance exists (for example in a unit test), its actions can
// be awaited directly.

// A review as returned by GetReview / GetProductReviews exposes these
// properties:
var review = new Review
{
    Id = 1,
    ProductId = 42,
    UserId = 7,
    Rating = 5,
    Title = "Great product",
    Content = "Works exactly as described.",
    HelpfulCount = 3,
    IsVerifiedPurchase = true,
    IsApproved = true,
    CreatedAt = DateTime.UtcNow,
    UpdatedAt = null
};

// Fetch a single review by its id:
IActionResult getResult = await reviewsController.GetReview(review.Id);

// Fetch the reviews for a product:
IActionResult listResult = await reviewsController.GetProductReviews(review.ProductId);

// Register a "helpful" vote on a review:
IActionResult voteResult = await reviewsController.VoteHelpful(review.Id);

// Recalculate the product rating summaries:
IActionResult recalcResult = await reviewsController.RecalculateRatings();
```
