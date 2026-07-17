# ReviewService

The `ReviewService` encapsulates all business logic for managing product reviews within the application. It provides a complete set of asynchronous operations for creating, reading, updating, deleting, moderating (approving/rejecting), and tracking helpfulness of reviews. The service enforces domain rules such as preventing duplicate reviews from the same user on the same product and ensuring that only authorized moderators can change a review's approval status.

## API

### `GetReviewByIdAsync(int reviewId)`

- **Purpose**: Retrieves a single review by its unique identifier.
- **Parameters**: `reviewId` – the integer ID of the review.
- **Returns**: `Task<Review?>` – the matching review, or `null` if no review with that ID exists.
- **Throws**: `ArgumentException` if `reviewId` is less than or equal to zero.

### `GetProductReviewsAsync(int productId)`

- **Purpose**: Returns all reviews for a given product.
- **Parameters**: `productId` – the integer ID of the product.
- **Returns**: `Task<IEnumerable<Review>>` – a collection of reviews for the product (may be empty).
- **Throws**: `ArgumentException` if `productId` is less than or equal to zero.

### `GetUserReviewsAsync(string userId)`

- **Purpose**: Returns all reviews submitted by a specific user.
- **Parameters**: `userId` – the string identifier of the user (e.g., GUID or username).
- **Returns**: `Task<IEnumerable<Review>>` – a collection of reviews authored by the user (may be empty).
- **Throws**: `ArgumentException` if `userId` is null or empty.

### `CreateReviewAsync(Review review)`

- **Purpose**: Creates a new review after validating business rules.
- **Parameters**: `review` – a `Review` object containing the review content, rating, product ID, and user ID. The `Id` property should be left unset (the service will assign it).
- **Returns**: `Task<Review>` – the newly created review, including its generated ID and timestamp.
- **Throws**:
  - `ArgumentNullException` if `review` is null.
  - `InvalidOperationException` if the user has already submitted a review for the same product.
  - `ValidationException` if required fields (e.g., rating, text) are missing or invalid.

### `UpdateReviewAsync(Review review)`

- **Purpose**: Updates an existing review. Only the author (or an admin) should be allowed to modify a review; the service enforces this rule.
- **Parameters**: `review` – a `Review` object with the updated fields. The `Id` must match an existing review.
- **Returns**: `Task<Review>` – the updated review after persistence.
- **Throws**:
  - `ArgumentNullException` if `review` is null.
  - `KeyNotFoundException` if no review with the given `Id` exists.
  - `InvalidOperationException` if the review has already been approved and the update is not allowed, or if the user is not the original author.

### `DeleteReviewAsync(int reviewId)`

- **Purpose**: Deletes a review by its ID.
- **Parameters**: `reviewId` – the integer ID of the review to delete.
- **Returns**: `Task` – completes when the review is removed from the data store.
- **Throws**:
  - `ArgumentException` if `reviewId` is less than or equal to zero.
  - `KeyNotFoundException` if no review with that ID exists.

### `ApproveReviewAsync(int reviewId)`

- **Purpose**: Marks a pending review as approved (moderation action).
- **Parameters**: `reviewId` – the integer ID of the review to approve.
- **Returns**: `Task` – completes when the review status is updated.
- **Throws**:
  - `ArgumentException` if `reviewId` is invalid.
  - `KeyNotFoundException` if the review does not exist.
  - `InvalidOperationException` if the review is already approved or rejected.

### `RejectReviewAsync(int reviewId)`

- **Purpose**: Marks a pending review as rejected (moderation action).
- **Parameters**: `reviewId` – the integer ID of the review to reject.
- **Returns**: `Task` – completes when the review status is updated.
- **Throws**:
  - `ArgumentException` if `reviewId` is invalid.
  - `KeyNotFoundException` if the review does not exist.
  - `InvalidOperationException` if the review is already approved or rejected.

### `MarkAsHelpfulAsync(int reviewId, string userId)`

- **Purpose**: Records that a user found a review helpful. Prevents the same user from marking the same review as helpful more than once.
- **Parameters**:
  - `reviewId` – the integer ID of the review.
  - `userId` – the string identifier of the user marking it helpful.
- **Returns**: `Task` – completes when the helpfulness count is incremented.
- **Throws**:
  - `ArgumentException` if `reviewId` is invalid or `userId` is null/empty.
  - `KeyNotFoundException` if the review does not exist.
  - `InvalidOperationException` if the user has already marked this review as helpful.

## Usage

### Example 1: Creating a review and then approving it

```csharp
public async Task<Review> SubmitAndApproveReview(ReviewService reviewService, string userId, int productId)
{
    var newReview = new Review
    {
        ProductId = productId,
        UserId = userId,
        Rating = 5,
        Text = "Excellent product, highly recommended!"
    };

    Review created = await reviewService.CreateReviewAsync(newReview);
    Console.WriteLine($"Review created with ID {created.Id}");

    // Moderator approves the review
    await reviewService.ApproveReviewAsync(created.Id);
    Console.WriteLine("Review approved.");

    return created;
}
```

### Example 2: Fetching product reviews and marking one as helpful

```csharp
public async Task MarkFirstReviewHelpful(ReviewService reviewService, int productId, string currentUserId)
{
    var reviews = await reviewService.GetProductReviewsAsync(productId);
    var firstReview = reviews.FirstOrDefault();

    if (firstReview != null)
    {
        await reviewService.MarkAsHelpfulAsync(firstReview.Id, currentUserId);
        Console.WriteLine($"Marked review {firstReview.Id} as helpful.");
    }
    else
    {
        Console.WriteLine("No reviews found for this product.");
    }
}
```

## Notes

- **Duplicate reviews**: `CreateReviewAsync` throws `InvalidOperationException` if the same user attempts to review the same product more than once. This check is performed before insertion.
- **Moderation state**: `ApproveReviewAsync` and `RejectReviewAsync` can only be called on reviews that are currently in a "pending" state. Calling them on already moderated reviews will throw an exception.
- **Helpfulness uniqueness**: `MarkAsHelpfulAsync` enforces a one‑vote‑per‑user‑per‑review rule. A second call with the same `userId` and `reviewId` will throw.
- **Concurrent updates**: The service does not provide built‑in locking or optimistic concurrency control. In high‑contention scenarios, consider wrapping calls in a database transaction or using a retry mechanism.
- **Thread safety**: Instances of `ReviewService` are typically registered as scoped services (one per HTTP request). They are not designed for concurrent use across multiple threads within the same scope. If shared across threads, external synchronization (e.g., a `SemaphoreSlim`) is required.
