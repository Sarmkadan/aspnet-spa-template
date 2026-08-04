## CollectionExtensionsUnitTests

The CollectionExtensionsUnitTests class contains unit tests for the CollectionExtensions class. These tests cover various scenarios, including:
* Batch_WithCollection_ReturnsCorrectlySizedBatches: Tests that CollectionExtensions.Batch{T} correctly batches a collection.
* Batch_WithNullSource_ThrowsArgumentNullException: Tests that CollectionExtensions.Batch{T} throws ArgumentNullException for null source.
* Batch_WithNonPositiveBatchSize_ThrowsArgumentOutOfRangeException: Tests that CollectionExtensions.Batch{T} throws ArgumentOutOfRangeException for non-positive batch size.
* Batch_WithEmptyCollection_ReturnsEmptyBatches: Tests that CollectionExtensions.Batch{T} handles empty collection correctly.
* Batch_WithBatchSizeLargerThanCollection_ReturnsSingleBatch: Tests that CollectionExtensions.Batch{T} handles batch size larger than collection.
* IsNullOrEmpty_WithNullSource_ReturnsTrue: Tests that CollectionExtensions.IsNullOrEmpty{T} returns true for null collection.
* IsNullOrEmpty_WithEmptySource_ReturnsTrue: Tests that CollectionExtensions.IsNullOrEmpty{T} returns true for empty collection.
* IsNullOrEmpty_WithNonEmptySource_ReturnsFalse: Tests that CollectionExtensions.IsNullOrEmpty{T} returns false for non-empty collection.
* OrEmpty_WithNonNullSource_ReturnsOriginalCollection: Tests that CollectionExtensions.OrEmpty{T} returns original collection when not null.
* OrEmpty_WithNullSource_ReturnsEmptyCollection: Tests that CollectionExtensions.OrEmpty{T} returns empty collection when source is null.
* DistinctBy_WithKeySelector_ReturnsDistinctElements: Tests that CollectionExtensions.DistinctBy{T,TKey} returns distinct elements based on key selector.
* DistinctBy_WithNullSource_ThrowsArgumentNullException: Tests that CollectionExtensions.DistinctBy{T,TKey} throws ArgumentNullException for null source.
* DistinctBy_WithNullKeySelector_ThrowsArgumentNullException: Tests that CollectionExtensions.DistinctBy{T,TKey} throws ArgumentNullException for null key selector.
* Paginate_WithCollection_ReturnsCorrectPageAndTotal: Tests that CollectionExtensions.Paginate{T} correctly paginates a collection.
* Paginate_WithPageNumberLessThanOne_DefaultsToFirstPage: Tests that CollectionExtensions.Paginate{T} handles page number less than 1.
* Paginate_WithPageSizeLessThanOne_DefaultsToTen: Tests that CollectionExtensions.Paginate{T} handles page size less than 1.
* Paginate_WithNullSource_ThrowsArgumentNullException: Tests that CollectionExtensions.Paginate{T} throws ArgumentNullException for null source.
* ForEach_WithCollection_AppliesActionToEachItem: Tests that CollectionExtensions.ForEach{T} applies action to each item.
* ForEach_WithNullSource_ThrowsArgumentNullException: Tests that CollectionExtensions.ForEach{T} throws ArgumentNullException for null source.

## EventBusImplementationTests

The EventBusImplementationTests class provides comprehensive unit tests for the EventBusImplementation class, ensuring robust event handling and subscription management. These tests verify core functionalities such as publishing events, managing subscribers, and ensuring that multiple handlers are executed in the correct order, even when exceptions occur.

```csharp
// Example usage snippet based on EventBusImplementationTests
public async Task ExampleUsage()
{
    var eventBus = new EventBusImplementation(logger);

    // Subscribe a handler
    eventBus.Subscribe<MyEvent>(handler);

    // Publish an event
    await eventBus.PublishAsync(new MyEvent());

    // Unsubscribe a handler
    eventBus.Unsubscribe<MyEvent>(handler);

    // Clear all subscribers
    eventBus.Clear();
}
```
