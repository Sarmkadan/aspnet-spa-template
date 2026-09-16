# ReviewFluentExtensions

ReviewFluentExtensions provides fluent helper methods for classifying a review by rating, formatting its rating as stars, and determining whether it was created within a specified time span.

## API

### IsPositive
```csharp
public static bool IsPositive(this Review review)
```
Determines whether a review has a positive rating. Ratings of 4 or 5 are considered positive.
- **Parameters**
  - `review`: The review to check.
- **Return value**
  `true` when `review.Rating` is greater than or equal to 4; otherwise `false`.
- **Exceptions**
  - `NullReferenceException` if `review` is `null`.

### StarString
```csharp
public static string StarString(this Review review)
```
Formats the review rating as a five-character string containing filled and empty stars.
- **Parameters**
  - `review`: The review whose rating will be formatted.
- **Return value**
  `"★☆☆☆☆"` through `"★★★★★"` for ratings 1 through 5. Any other rating returns `"No rating"`.
- **Exceptions**
  - `NullReferenceException` if `review` is `null`.

### IsRecent
```csharp
public static bool IsRecent(this Review review, TimeSpan timeSpan)
```
Determines whether the review was created within the supplied time span relative to the current UTC time.
- **Parameters**
  - `review`: The review to check.
  - `timeSpan`: The maximum elapsed time since the review was created.
- **Return value**
  `true` when the difference between `DateTime.UtcNow` and `review.CreatedAt` is less than or equal to `timeSpan`; otherwise `false`.
- **Exceptions**
  - `NullReferenceException` if `review` is `null`.

## Usage

### Example 1: Displaying a rating and classifying the review
```csharp
var review = new Review
{
    Rating = 5,
    Title = "Excellent product"
};

Console.WriteLine(review.StarString()); // "★★★★★"

if (review.IsPositive())
{
    Console.WriteLine("This is a positive review.");
}
```

### Example 2: Checking whether a review is recent
```csharp
var review = new Review
{
    Rating = 4,
    CreatedAt = DateTime.UtcNow.AddDays(-7)
};

bool createdThisMonth = review.IsRecent(TimeSpan.FromDays(30));
Console.WriteLine(createdThisMonth); // true
```

## Notes
- `IsPositive` treats every rating greater than or equal to 4 as positive; it does not validate that the rating is within the usual 1-to-5 range.
- `StarString` returns `"No rating"` for values outside the range 1 through 5.
- `IsRecent` uses `DateTime.UtcNow`, so `CreatedAt` should represent a UTC timestamp for a meaningful comparison.
- A review with a future `CreatedAt` value may be considered recent because its elapsed time is negative and therefore less than a positive `timeSpan`.
- The extension methods do not modify the `Review` instance.
