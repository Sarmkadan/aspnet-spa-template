# EventBusImplementationExtensions

Provides extension methods for a lightweight in-process event bus implementation. This class enables type-based subscription management, synchronous and asynchronous event publishing, delayed publishing, and diagnostics for subscriber counts. It is designed to be used with a concrete event bus that stores subscriptions in a thread-safe manner.

## API

### Subscribe<TEvent>
```csharp
public static void Subscribe<TEvent>(this IEventBus eventBus, Func<TEvent, Task> handler)
```
Subscribes a handler to a specific event type. If a handler for the same event type is already registered, it is replaced.

**Parameters:**
- `eventBus`: The event bus instance to extend.
- `handler`: An asynchronous delegate that processes the event.

**Exceptions:**
- `ArgumentNullException` if `eventBus` or `handler` is null.

---

### TryPublishAsync<TEvent>
```csharp
public static async Task<bool> TryPublishAsync<TEvent>(this IEventBus eventBus, TEvent eventData)
```
Attempts to publish an event to its registered handler. Returns `true` if a handler was found and invoked successfully; returns `false` if no handler is registered for the event type.

**Parameters:**
- `eventBus`: The event bus instance.
- `eventData`: The event payload.

**Returns:**
- `true` if the event was handled; `false` otherwise.

**Exceptions:**
- `ArgumentNullException` if `eventBus` is null.

---

### PublishBatchAsync
```csharp
public static async Task PublishBatchAsync(this IEventBus eventBus, params object[] events)
```
Publishes multiple events sequentially. Each event is dispatched to its corresponding handler if one exists. Events without a registered handler are silently skipped.

**Parameters:**
- `eventBus`: The event bus instance.
- `events`: One or more event objects.

**Exceptions:**
- `ArgumentNullException` if `eventBus` or `events` is null.

---

### GetAllSubscriberCounts
```csharp
public static IReadOnlyDictionary<Type, int> GetAllSubscriberCounts(this IEventBus eventBus)
```
Returns a snapshot of the number of subscribers per event type. The count is 1 for types with a registered handler and 0 for types that have been cleared or never registered.

**Parameters:**
- `eventBus`: The event bus instance.

**Returns:**
- A read-only dictionary mapping event types to their subscriber counts.

**Exceptions:**
- `ArgumentNullException` if `eventBus` is null.

---

### ClearSubscribers<TEvent>
```csharp
public static void ClearSubscribers<TEvent>(this IEventBus eventBus)
```
Removes the registered handler for the specified event type. Subsequent calls to `TryPublishAsync<TEvent>` will return `false` until a new handler is subscribed.

**Parameters:**
- `eventBus`: The event bus instance.

**Exceptions:**
- `ArgumentNullException` if `eventBus` is null.

---

### PublishWithDelayAsync<TEvent>
```csharp
public static async Task PublishWithDelayAsync<TEvent>(this IEventBus eventBus, TEvent eventData, TimeSpan delay)
```
Publishes an event after a specified delay. The delay is implemented using `Task.Delay`. If no handler is registered, the method completes without error after the delay.

**Parameters:**
- `eventBus`: The event bus instance.
- `eventData`: The event data.
- `delay`: The time to wait before publishing.

**Exceptions:**
- `ArgumentNullException` if `eventBus` is null.
- `ArgumentOutOfRangeException` if `delay` is negative.

---

### GetSubscriberCountLock
```csharp
public static object GetSubscriberCountLock(this IEventBus eventBus)
```
Returns the lock object used internally to synchronize access to subscriber counts. This can be used by consumers to perform thread-safe operations that depend on consistent subscriber state.

**Parameters:**
- `eventBus`: The event bus instance.

**Returns:**
- The synchronization object.

**Exceptions:**
- `ArgumentNullException` if `eventBus` is null.

## Usage

### Example 1: Basic Publish and Subscribe
```csharp
var eventBus = new InMemoryEventBus();

// Subscribe to an event
eventBus.Subscribe<OrderPlacedEvent>(async order =>
{
    await SendConfirmationEmailAsync(order);
});

// Publish an event
var handled = await eventBus.TryPublishAsync(new OrderPlacedEvent { OrderId = 42 });
Console.WriteLine($"Event handled: {handled}"); // True
```

### Example 2: Delayed Publishing and Diagnostics
```csharp
var eventBus = new InMemoryEventBus();

eventBus.Subscribe<ReminderEvent>(async reminder =>
{
    await SendReminderAsync(reminder);
});

// Schedule a reminder 30 minutes from now
await eventBus.PublishWithDelayAsync(
    new ReminderEvent { UserId = 5 },
    TimeSpan.FromMinutes(30)
);

// Check subscriber counts
var counts = eventBus.GetAllSubscriberCounts();
Console.WriteLine($"ReminderEvent subscribers: {counts[typeof(ReminderEvent)]}");

// Clear subscribers when shutting down
eventBus.ClearSubscribers<ReminderEvent>();
```

## Notes

- **Thread Safety:** Subscriber registration and clearing are synchronized using the lock object returned by `GetSubscriberCountLock`. Publishing operations read subscriber state without holding this lock, which may result in a handler being invoked after `ClearSubscribers` has been called if the clear occurs during a publish.
- **Handler Replacement:** Subscribing a new handler for an event type overwrites the previous handler. There is no support for multiple handlers per event type.
- **Null Events:** `PublishBatchAsync` throws if the `events` array itself is null, but individual null elements within the array are passed to handlers without validation.
- **Delayed Publishing:** `PublishWithDelayAsync` does not cancel the delay if the subscriber is cleared during the wait period. The event will still be published after the delay, but will be silently ignored if no handler exists at that time.
- **Subscriber Counts:** `GetAllSubscriberCounts` returns a snapshot; the dictionary is not updated dynamically as subscribers are added or removed.
