## ReviewServiceUnitTests

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
