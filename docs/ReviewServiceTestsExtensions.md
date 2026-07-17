# ReviewServiceTestsExtensions

Extension methods for testing `ReviewService` functionality. These methods provide fluent assertions and test data builders for verifying review-related operations, including property validation, business rule enforcement, and service behavior under various conditions.

## API

### `public static void ShouldHaveReviewProperties(Review review)`

Verifies that a `Review` instance has all expected properties populated with non-default values.

- **Parameters**
  - `review`: The `Review` instance to validate.
- **Throws**
  - `ArgumentNullException`: If `review` is `null`.
  - `AssertionException`: If any property is `null`, empty, or has a default value.

### `public static void ShouldMatchReview(Review actual, Review expected)`

Asserts that two `Review` instances are equivalent by comparing all properties.

- **Parameters**
  - `actual`: The review instance produced by the test.
  - `expected`: The review instance representing the expected state.
- **Throws**
  - `ArgumentNullException`: If either parameter is `null`.
  - `AssertionException`: If any property differs between the two instances.

### `public static void ShouldContainExactly(IEnumerable<Review> reviews, int expectedCount)`

Asserts that a collection of reviews contains exactly the specified number of items.

- **Parameters**
  - `reviews`: The collection of reviews to inspect.
  - `expectedCount`: The exact number of reviews expected.
- **Throws**
  - `ArgumentNullException`: If `reviews` is `null`.
  - `AssertionException`: If the collection count does not match `expectedCount`.

### `public static void ShouldContainRating(IEnumerable<Review> reviews, int expectedRating)`

Asserts that a collection of reviews contains at least one review with the specified rating.

- **Parameters**
  - `reviews`: The collection of reviews to inspect.
  - `expectedRating`: The rating value to search for (e.g., 5 for five-star reviews).
- **Throws**
  - `ArgumentNullException`: If `reviews` is `null`.
  - `AssertionException`: If no review in the collection has the specified rating.

### `public static void ShouldContainUserReviews(IEnumerable<Review> reviews, User user)`

Asserts that a collection of reviews contains at least one review authored by the specified user.

- **Parameters**
  - `reviews`: The collection of reviews to inspect.
  - `user`: The user whose reviews are expected.
- **Throws**
  - `ArgumentNullException`: If either parameter is `null`.
  - `AssertionException`: If no review in the collection is authored by `user`.

### `public static void ShouldContainOnlyApprovedReviews(IEnumerable<Review> reviews)`

Asserts that every review in the collection has an `IsApproved` status of `true`.

- **Parameters**
  - `reviews`: The collection of reviews to inspect.
- **Throws**
  - `ArgumentNullException`: If `reviews` is `null`.
  - `AssertionException`: If any review in the collection is not approved.

### `public static void ShouldContainOnlyProductReviews(IEnumerable<Review> reviews)`

Asserts that every review in the collection is of type `ProductReview`.

- **Parameters**
  - `reviews`: The collection of reviews to inspect.
- **Throws**
  - `ArgumentNullException`: If `reviews` is `null`.
  - `AssertionException`: If any review in the collection is not a `ProductReview`.

### `public static void ShouldBeEmpty(IEnumerable<Review> reviews)`

Asserts that a collection of reviews is empty.

- **Parameters**
  - `reviews`: The collection of reviews to inspect.
- **Throws**
  - `ArgumentNullException`: If `reviews` is `null`.
  - `AssertionException`: If the collection contains any items.

### `public static Review CreateTestReview(Product product = null, User user = null, int? rating = null, string comment = null, bool isApproved = false)`

Creates a `Review` instance populated with test data. Parameters default to sensible test values if not specified.

- **Parameters**
  - `product`: The product to associate with the review. Defaults to a test product if `null`.
  - `user`: The user who authored the review. Defaults to a test user if `null`.
  - `rating`: The numeric rating (e.g., 1–5). Defaults to `5` if `null`.
  - `comment`: The review text. Defaults to a sample comment if `null`.
  - `isApproved`: Whether the review is approved. Defaults to `false`.
- **Returns**
  - A new `Review` instance with the specified or default values.

### `public static Product CreateTestProduct(string name = "Test Product", decimal price = 10.00m)`

Creates a `Product` instance populated with test data.

- **Parameters**
  - `name`: The product name. Defaults to `"Test Product"` if `null`.
  - `price`: The product price. Defaults to `10.00` if `null`.
- **Returns**
  - A new `Product` instance with the specified or default values.

### `public static void ShouldHaveValidationErrors(Review review, params Expression<Func<Review, object>>[] propertyExpressors)`

Asserts that a `Review` instance has validation errors for the specified properties.

- **Parameters**
  - `review`: The review instance to validate.
  - `propertyExpressors`: Expressions identifying the properties expected to have validation errors.
- **Throws**
  - `ArgumentNullException`: If `review` or `propertyExpressors` is `null`.
  - `AssertionException`: If any specified property does not have a validation error.

### `public static void ShouldHaveBusinessExceptionMessage(Action action, string expectedMessage)`

Asserts that a business exception with the specified message is thrown when executing an action.

- **Parameters**
  - `action`: The action expected to throw a business exception.
  - `expectedMessage`: The exact error message expected in the exception.
- **Throws**
  - `ArgumentNullException`: If `action` is `null`.
  - `AssertionException`: If no exception is thrown or the message does not match `expectedMessage`.

## Usage

```csharp
// Example 1: Validating review properties after creation
var product = ReviewServiceTestsExtensions.CreateTestProduct("Wireless Headphones", 99.99m);
var user = new User { Id = 1, Name = "Test User" };
var review = new Review
{
    Product = product,
    User = user,
    Rating = 5,
    Comment = "Excellent sound quality!",
    IsApproved = true
};

ReviewServiceTestsExtensions.ShouldHaveReviewProperties(review);

// Example 2: Testing service behavior with multiple reviews
var reviews = new List<Review>
{
    ReviewServiceTestsExtensions.CreateTestReview(product, user, 5, "Great!", true),
    ReviewServiceTestsExtensions.CreateTestReview(product, user, 4, "Good.", true)
};

ReviewServiceTestsExtensions.ShouldContainExactly(reviews, 2);
ReviewServiceTestsExtensions.ShouldContainRating(reviews, 5);
ReviewServiceTestsExtensions.ShouldContainOnlyApprovedReviews(reviews);
```

## Notes

- All methods are thread-safe as they operate on immutable inputs or create new instances without shared state.
- Edge cases such as `null` collections or invalid ratings (outside typical 1–5 range) are handled by throwing exceptions rather than silently succeeding.
- Default test values (e.g., rating of `5`, product price of `10.00`) are chosen to minimize test brittleness while remaining realistic.
- Methods like `ShouldHaveValidationErrors` and `ShouldHaveBusinessExceptionMessage` assume the use of a validation framework (e.g., FluentValidation) and a custom exception hierarchy for business rule violations.
