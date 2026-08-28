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

## OrderStatsResponse

The `OrderStatsResponse` DTO (defined in `DTOs/OrderStatsResponse.cs`) aggregates order statistics for reporting and dashboard scenarios. It exposes the total number of orders along with how many were cancelled or refunded, the overall revenue, an optional revenue figure limited to a specific date range, and a per-status breakdown of order counts. Services populate this response so clients can display summary metrics without querying individual orders.

Example usage:

```csharp
using AspNetSpaTemplate.DTOs;

var stats = new OrderStatsResponse
{
    TotalOrders = 150,
    CancelledOrders = 12,
    RefundedOrders = 5,
    TotalRevenue = 24_980.75m,
    DateRangeRevenue = 3_450.20m,
    StatusCounts = new Dictionary<string, int>
    {
        ["Pending"] = 30,
        ["Shipped"] = 95,
        ["Delivered"] = 13,
        ["Cancelled"] = 12
    }
};

Console.WriteLine($"Orders: {stats.TotalOrders}, Revenue: {stats.TotalRevenue}");
Console.WriteLine($"Cancelled: {stats.CancelledOrders}, Refunded: {stats.RefundedOrders}");

if (stats.DateRangeRevenue.HasValue)
{
    Console.WriteLine($"Revenue in selected range: {stats.DateRangeRevenue.Value}");
}

foreach (var (status, count) in stats.StatusCounts)
{
    Console.WriteLine($"{status}: {count}");
}
```

## NotificationWorkerTests

The `NotificationWorkerTests` class (defined in `tests/aspnet-spa-template.Tests/BackgroundWorkers/NotificationWorkerTests.cs`) contains unit tests for the notification background worker. It verifies batched processing of queued notifications, resilience against individual failures and exceptions, cancellation support, metrics/status reporting, and cleanup of stale push subscriptions. Like other xUnit fixtures it receives its dependencies via constructor injection and implements `IDisposable` so resources are released after each test.

Example usage:

```csharp
// Dependencies (e.g. mocked services) are resolved through the constructor,
// mirroring how the test host activates the class:
var tests = new NotificationWorkerTests(/* mocked dependencies */);

try
{
    // Batched execution:
    await tests.ExecuteAsync_WithMoreThan100Notifications_ProcessesInBatchesOf100();

    // Resilience and cancellation:
    await tests.ExecuteAsync_WhenOneNotificationFails_ContinuesProcessingRemaining();
    await tests.ExecuteAsync_WithCancellationToken_StopsProcessing();
    await tests.ExecuteAsync_WhenExceptionThrown_HandlesGracefully();

    // Status and metrics:
    await tests.GetStatus_ReturnsCorrectTaskNameAndMetrics();
    await tests.ExecuteAsync_TracksMetricsCorrectly();

    // Queue and cleanup behavior:
    await tests.ExecuteAsync_WithEmptyQueue_LogsDebugMessage();
    await tests.CleanupStalePushSubscriptionsAsync_WithPurgeDaysZero_DoesNothing();
    await tests.CleanupStalePushSubscriptionsAsync_WithEmptyDatabase_HandlesGracefully();

    // Mixed workloads:
    await tests.ExecuteAsync_WithMixedNotificationTypes_ProcessesAllTypes();
    await tests.ExecuteAsync_CleansUpStalePushSubscriptions();
}
finally
{
    // Release any resources held by the test fixture:
    tests.Dispose();
}
```

## OrderControllerIdempotencyTests

The `OrderControllerIdempotencyTests` class (defined in `tests/aspnet-spa-template.Tests/OrderControllerIdempotencyTests.cs`) contains unit tests for the idempotency functionality in the OrdersController. It verifies that orders created with a valid idempotency key return cached responses when available, store responses in cache, and work normally when no or empty idempotency key is provided.

Example usage:

```csharp
// Dependencies (e.g. mocked services) are resolved through the constructor,
// mirroring how the test host activates the class:
var tests = new OrderControllerIdempotencyTests();

// Test idempotent order creation with cached response:
await tests.CreateOrder_WithValidIdempotencyKey_WhenResponseCached_ReturnsCachedResponse();

// Test storing response in cache for idempotent order creation:
await tests.CreateOrder_WithValidIdempotencyKey_StoresResponseInCache();

// Test normal order creation without idempotency key:
await tests.CreateOrder_WithoutIdempotencyKey_WorksNormally();

// Test normal order creation with empty idempotency key:
await tests.CreateOrder_WithEmptyIdempotencyKey_WorksNormally();
```

## MetricsRegistry

The `MetricsRegistry` (defined in `Services/MetricsRegistry.cs`) is a singleton service that collects and provides system and application metrics. It tracks request counts, memory usage, thread statistics, and process information, exposing them via properties and methods for monitoring and reporting.

Example usage:

```csharp
// Access the singleton instance
var registry = MetricsRegistry.Instance;

// Increment the request count (e.g., in a middleware or controller)
registry.IncrementRequestCount();

// Get total memory in MB
long totalMemory = registry.TotalMemoryMB;
Console.WriteLine($"Total Memory: {totalMemory} MB");

// Get working set memory
long workingSet = registry.WorkingSetMB;
Console.WriteLine($"Working Set: {workingSet} MB");

// Get active thread count
int activeThreads = registry.ActiveThreadCount;
Console.WriteLine($"Active Threads: {activeThreads}");

// Get thread pool completed work item count
long completedWorkItems = registry.ThreadPoolCompletedWorkItemCount;
Console.WriteLine($"Completed Work Items: {completedWorkItems}");

// Reset the request counter (if needed)
registry.ResetRequestCounter();

// Note: The registry implements IDisposable, so dispose when no longer needed (if not using the singleton for the app's lifetime)
// registry.Dispose();
```

## OrderServiceUnitTests

The OrderServiceUnitTests class contains unit tests for the OrderService class. It uses an in-memory database to test repository interactions and verifies order creation, retrieval, validation, and calculation logic.

Example usage:

```csharp
public OrderServiceUnitTests
public async Task InitializeAsync
public async Task DisposeAsync
public async Task CreateOrder_HappyPath_OrderCreatedSuccessfully
public async Task CreateOrder_NullRequest_ThrowsArgumentNullException
public async Task CreateOrder_InvalidUserId_ThrowsArgumentOutOfRangeException
public async Task CreateOrder_EmptyItemsList_ThrowsValidationException
public async Task CreateOrder_ProductNotFound_ThrowsNotFoundException
public async Task CreateOrder_InsufficientStock_ThrowsBusinessException
public async Task CreateOrder_TotalsCalculation_CalculatesCorrectly
public async Task GetOrderById_OrderExists_ReturnsOrderResponse
public async Task GetOrderById_OrderNotFound_ThrowsNotFoundException
public async Task GetOrderById_InvalidOrderId_ThrowsArgumentOutOfRangeException
public async Task CreateOrder_InvalidItemQuantity_ThrowsBusinessException
public async Task CreateOrder_SingleItem_OrderCreatedSuccessfully
public async Task CreateOrder_ItemDetailsMappedCorrectly
```