#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Extension methods for <see cref="EventBusImplementation"/> providing additional functionality
/// for event bus operations including bulk operations, conditional publishing, and
/// subscriber management.
/// </summary>
public static class EventBusImplementationExtensions
{
    /// <summary>
    /// Subscribes multiple handlers for the same event type in a single call.
    /// Useful when multiple components need to react to the same event.
    /// </summary>
    /// <typeparam name="TEvent">The event type to subscribe to.</typeparam>
    /// <param name="eventBus">The event bus instance.</param>
    /// <param name="handlers">Collection of handler functions.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> or <paramref name="handlers"/> is null.</exception>
    public static void Subscribe<TEvent>(this EventBusImplementation eventBus, IEnumerable<Func<TEvent, Task>> handlers) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(handlers);

        foreach (var handler in handlers)
        {
            eventBus.Subscribe(handler);
        }
    }

    /// <summary>
    /// Publishes an event only if there are subscribers interested in it.
    /// Useful for optimization when you want to avoid creating events that nobody listens to.
    /// </summary>
    /// <typeparam name="TEvent">The event type to publish.</typeparam>
    /// <param name="eventBus">The event bus instance.</param>
    /// <param name="event">The event to publish.</param>
    /// <returns>True if the event was published, false if no subscribers were registered.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> or <paramref name="event"/> is null.</exception>
    public static async Task<bool> TryPublishAsync<TEvent>(this EventBusImplementation eventBus, TEvent @event) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(@event);

        var subscriberCount = eventBus.GetSubscriberCount<TEvent>();
        if (subscriberCount > 0)
        {
            await eventBus.PublishAsync(@event);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Publishes multiple events of different types in a single batch.
    /// Events are published sequentially in the order provided.
    /// </summary>
    /// <param name="eventBus">The event bus instance.</param>
    /// <param name="events">Collection of events to publish.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> or <paramref name="events"/> is null.</exception>
    public static async Task PublishBatchAsync(this EventBusImplementation eventBus, IEnumerable<DomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(events);

        foreach (var @event in events)
        {
            await eventBus.PublishAsync(@event);
        }
    }

    /// <summary>
    /// Gets all subscriber counts for all registered event types.
    /// Useful for diagnostics and monitoring of the entire event bus.
    /// </summary>
    /// <param name="eventBus">The event bus instance.</param>
    /// <returns>A dictionary mapping event types to their subscriber counts.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> is null.</exception>
    public static IReadOnlyDictionary<Type, int> GetAllSubscriberCounts(this EventBusImplementation eventBus)
    {
        ArgumentNullException.ThrowIfNull(eventBus);

        lock (eventBus.GetSubscriberCountLock())
        {
            var result = new Dictionary<Type, int>();

            // Use reflection to access the internal _subscribers dictionary
            var subscribersField = typeof(EventBusImplementation).GetField(
                "_subscribers",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (subscribersField?.GetValue(eventBus) is Dictionary<Type, List<Delegate>> subscribers)
            {
                foreach (var kvp in subscribers)
                {
                    result[kvp.Key] = kvp.Value.Count;
                }
            }

            return result;
        }
    }

    /// <summary>
    /// Unsubscribes all handlers for a specific event type.
    /// Useful for cleanup when a feature/module is being disabled.
    /// </summary>
    /// <typeparam name="TEvent">The event type to clear.</typeparam>
    /// <param name="eventBus">The event bus instance.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> is null.</exception>
    public static void ClearSubscribers<TEvent>(this EventBusImplementation eventBus) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(eventBus);

        lock (eventBus.GetSubscriberCountLock())
        {
            var subscribersField = typeof(EventBusImplementation).GetField(
                "_subscribers",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (subscribersField?.GetValue(eventBus) is Dictionary<Type, List<Delegate>> subscribers)
            {
                subscribers.Remove(typeof(TEvent));
            }
        }
    }

    /// <summary>
    /// Publishes an event with a delay using Task.Delay.
    /// Useful for implementing delayed processing or retry mechanisms.
    /// </summary>
    /// <typeparam name="TEvent">The event type to publish.</typeparam>
    /// <param name="eventBus">The event bus instance.</param>
    /// <param name="event">The event to publish.</param>
    /// <param name="delayMilliseconds">Delay in milliseconds before publishing.</param>
    /// <returns>A task representing the delayed publish operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> or <paramref name="event"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="delayMilliseconds"/> is negative.</exception>
    public static async Task PublishWithDelayAsync<TEvent>(this EventBusImplementation eventBus, TEvent @event, int delayMilliseconds) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(@event);

        if (delayMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(delayMilliseconds), "Delay must be non-negative.");
        }

        await Task.Delay(delayMilliseconds, CancellationToken.None);
        await eventBus.PublishAsync(@event);
    }

    /// <summary>
    /// Publishes an event with a delay using TimeSpan.
    /// Useful for implementing scheduled processing.
    /// </summary>
    /// <typeparam name="TEvent">The event type to publish.</typeparam>
    /// <param name="eventBus">The event bus instance.</param>
    /// <param name="event">The event to publish.</param>
    /// <param name="delay">Delay before publishing.</param>
    /// <returns>A task representing the delayed publish operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> or <paramref name="event"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="delay"/> is negative.</exception>
    public static async Task PublishWithDelayAsync<TEvent>(this EventBusImplementation eventBus, TEvent @event, TimeSpan delay) where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(eventBus);
        ArgumentNullException.ThrowIfNull(@event);

        if (delay < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(delay), "Delay must be non-negative.");
        }

        await Task.Delay(delay, CancellationToken.None);
        await eventBus.PublishAsync(@event);
    }

    /// <summary>
    /// Gets the lock object used for thread-safe operations on subscribers.
    /// Exposed for advanced scenarios where custom synchronization is needed.
    /// </summary>
    /// <param name="eventBus">The event bus instance.</param>
    /// <returns>The lock object used for subscriber synchronization.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> is null.</exception>
    public static object GetSubscriberCountLock(this EventBusImplementation eventBus)
    {
        ArgumentNullException.ThrowIfNull(eventBus);

        var lockField = typeof(EventBusImplementation).GetField(
            "_subscriberLock",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return lockField?.GetValue(eventBus) as object ?? throw new InvalidOperationException("Could not access subscriber lock.");
    }
}