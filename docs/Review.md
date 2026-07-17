# Review
The `Review` type represents a user's review of a product, encapsulating details such as the review's content, rating, and creation date. It provides properties to access the review's metadata and relationships with the associated product and user, as well as methods to update the review's status and calculate display information.

## API
* `Id`: A unique identifier for the review.
* `ProductId`: The identifier of the product being reviewed.
* `UserId`: The identifier of the user who wrote the review.
* `Rating`: The rating given by the user, represented as an integer.
* `Title`: The title of the review.
* `Content`: The main content of the review.
* `HelpfulCount`: The number of times the review has been marked as helpful.
* `IsVerifiedPurchase`: Indicates whether the review is from a verified purchase.
* `IsApproved`: Indicates whether the review has been approved.
* `CreatedAt`: The date and time when the review was created.
* `UpdatedAt`: The date and time when the review was last updated, or null if it has not been updated.
* `Product`: The product being reviewed, or null if not loaded.
* `User`: The user who wrote the review, or null if not loaded.
* `IsValidRating`: Indicates whether the rating is within a valid range.
* `IsRecent`: Indicates whether the review is recent, based on its creation date.
* `MarkAsHelpful()`: Increments the helpful count for the review.
* `UpdateReview()`: Updates the review's content and title.
* `Approve()`: Sets the review's approval status to true.
* `Reject()`: Sets the review's approval status to false.
* `GetRatingDisplay()`: Returns a string representation of the review's rating for display purposes.

## Usage
```csharp
// Example 1: Creating and updating a review
var review = new Review
{
    ProductId = 1,
    UserId = 1,
    Rating = 5,
    Title = "Excellent product",
    Content = "This product exceeded my expectations."
};
review.UpdateReview();
review.MarkAsHelpful();

// Example 2: Displaying review information
var review = GetReviewFromDatabase(1);
Console.WriteLine($"Rating: {review.GetRatingDisplay()}");
Console.WriteLine($"Helpful count: {review.HelpfulCount}");
if (review.IsApproved)
{
    Console.WriteLine("Review is approved.");
}
```

## Notes
When using the `Review` type, consider the following edge cases:
* The `UpdatedAt` property may be null if the review has not been updated since its creation.
* The `Product` and `User` properties may be null if the associated entities are not loaded.
* The `IsValidRating` property checks whether the rating is within a valid range, but does not enforce this range when setting the `Rating` property.
* The `IsRecent` property is based on the review's creation date and may not reflect the review's relevance or usefulness.
* The `MarkAsHelpful`, `UpdateReview`, `Approve`, and `Reject` methods modify the review's state and should be used carefully to avoid inconsistent data.
* The `GetRatingDisplay` method returns a string representation of the review's rating and can be used for display purposes.
Thread-safety notes:
* The `Review` type is not thread-safe by default, and concurrent access to its properties and methods may result in inconsistent data.
* To ensure thread-safety, consider using synchronization mechanisms such as locks or concurrent collections when working with reviews in a multi-threaded environment.
