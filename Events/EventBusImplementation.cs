#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// In-process event bus implementation.
/// Suitable for single-server deployments. For distributed systems, use a message broker.
/// Maintains subscribers in memory and dispatches events sequentially.
/// </summary>
public class EventBusImplementation : IEventBus
{
    /// <summary>
    /// Maximum number of invocation attempts (initial attempt plus retries) made per handler before
    /// the event is routed to the dead-letter sink.
    /// </summary>
    private const int MaxAttempts = 3;

    /// <summary>
    /// Base delay used for the exponential backoff applied between retry attempts.
    /// </summary>
    private static readonly TimeSpan BaseRetryDelay = TimeSpan.FromMilliseconds(100);

    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly object _subscriberLock = new();
    private readonly ILogger<EventBusImplementation> _logger;
    private readonly IDeadLetterSink _deadLetterSink;

    /// <summary>
    /// Initializes a new event bus with a default logging-based dead-letter sink.
    /// </summary>
    /// <param name="logger">Logger used for diagnostic and error logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public EventBusImplementation(ILogger<EventBusImplementation> logger)
        : this(logger, new LoggingDeadLetterSink(NullLogger<LoggingDeadLetterSink>.Instance))
    {
    }

    /// <summary>
    /// Initializes a new event bus with the given logger and dead-letter sink.
    /// </summary>
    /// <param name="logger">Logger used for diagnostic and error logging.</param>
    /// <param name="deadLetterSink">Sink that receives events whose handlers exhaust their retry budget.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> or <paramref name="deadLetterSink"/> is <c>null</c>.</exception>
    public EventBusImplementation(ILogger<EventBusImplementation> logger, IDeadLetterSink deadLetterSink)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(deadLetterSink);

        _logger = logger;
        _deadLetterSink = deadLetterSink;
    }

    /// <summary>
    /// Gets the logger instance used by this event bus.
    /// </summary>
    internal ILogger<EventBusImplementation> Logger => _logger;

    /// <summary>
    /// Gets the subscriber lock object.
    /// </summary>
    internal object SubscriberLock => _subscriberLock;

    /// <summary>
    /// Gets the subscribers dictionary.
    /// </summary>
    internal IReadOnlyDictionary<Type, List<Delegate>> Subscribers => _subscribers;

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
    {
        if (handler is null)
            throw new ArgumentNullException(nameof(handler));

        lock (_subscriberLock)
        {
            var eventType = typeof(TEvent);
            if (!_subscribers.ContainsKey(eventType))
                _subscribers[eventType] = new List<Delegate>();

            _subscribers[eventType].Add(handler);
            _logger.LogInformation($"Subscribed handler to {eventType.Name}");
        }
    }

    public void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
    {
        lock (_subscriberLock)
        {
            var eventType = typeof(TEvent);
            if (_subscribers.TryGetValue(eventType, out var handlers))
            {
                handlers.Remove(handler);
                if (handlers.Count == 0)
                    _subscribers.Remove(eventType);
            }
        }
    }

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent
    {
        if (@event is null)
            throw new ArgumentNullException(nameof(@event));

        var eventType = typeof(TEvent);
        _logger.LogInformation($"Publishing event: {eventType.Name} (ID: {@event.EventId})");

        List<Delegate>? handlers;
        lock (_subscriberLock)
        {
            if (!_subscribers.TryGetValue(eventType, out handlers))
                handlers = new List<Delegate>();
            else
                handlers = new List<Delegate>(handlers); // Copy to avoid locking during execution
        }

        // Call handlers sequentially. Each handler gets its own retry budget; one handler
        // exhausting its retries and being dead-lettered never prevents the remaining
        // handlers from receiving the event.
        foreach (var handler in handlers)
        {
            if (handler is Func<TEvent, Task> typedHandler)
            {
                await InvokeHandlerWithRetryAsync(typedHandler, @event, eventType);
            }
        }

        _logger.LogInformation($"Event published: {eventType.Name} (handled by {handlers.Count} subscribers)");
    }

    /// <summary>
    /// Invokes a single handler with retry (exponential backoff with jitter, up to <see cref="MaxAttempts"/>
    /// attempts). If every attempt fails, all captured exceptions are aggregated and the event is routed to
    /// the configured <see cref="IDeadLetterSink"/> instead of propagating the failure to the caller.
    /// </summary>
    /// <typeparam name="TEvent">The event type, constrained to <see cref="DomainEvent"/>.</typeparam>
    /// <param name="handler">The subscriber delegate to invoke.</param>
    /// <param name="event">The event payload passed to the handler.</param>
    /// <param name="eventType">The runtime type of <typeparamref name="TEvent"/>, used for logging.</param>
    private async Task InvokeHandlerWithRetryAsync<TEvent>(Func<TEvent, Task> handler, TEvent @event, Type eventType)
        where TEvent : DomainEvent
    {
        var attemptExceptions = new List<Exception>(MaxAttempts);

        for (var attempt = 1; attempt <= MaxAttempts; attempt++)
        {
            try
            {
                await handler(@event);
                return;
            }
            catch (Exception ex)
            {
                attemptExceptions.Add(ex);

                if (attempt == MaxAttempts)
                    break;

                _logger.LogWarning(
                    ex,
                    $"Handler attempt {attempt}/{MaxAttempts} failed for {eventType.Name} (ID: {@event.EventId}); retrying");

                await Task.Delay(ComputeBackoffDelay(attempt));
            }
        }

        var aggregate = new AggregateException(
            $"Handler for {eventType.Name} failed after {MaxAttempts} attempts",
            attemptExceptions);

        _logger.LogError(aggregate, $"Error in event handler for {eventType.Name}");

        await _deadLetterSink.SendAsync(@event, aggregate);
    }

    /// <summary>
    /// Computes the delay before the next retry attempt using exponential backoff with full jitter.
    /// </summary>
    /// <param name="attempt">The 1-based index of the attempt that just failed.</param>
    /// <returns>The delay to wait before the next attempt.</returns>
    private static TimeSpan ComputeBackoffDelay(int attempt)
    {
        var exponentialMs = BaseRetryDelay.TotalMilliseconds * Math.Pow(2, attempt - 1);
        var jitteredMs = Random.Shared.NextDouble() * exponentialMs;
        return TimeSpan.FromMilliseconds(jitteredMs);
    }

    public async Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events) where TEvent : DomainEvent
    {
        // Publish all events sequentially (not in parallel)
        foreach (var @event in events)
        {
            await PublishAsync(@event);
        }
    }

    /// <summary>
    /// Gets number of subscribers for an event type.
    /// Useful for diagnostics and monitoring.
    /// </summary>
    public int GetSubscriberCount<TEvent>() where TEvent : DomainEvent
    {
        lock (_subscriberLock)
        {
            var eventType = typeof(TEvent);
            return _subscribers.TryGetValue(eventType, out var handlers) ? handlers.Count : 0;
        }
    }

    /// <summary>
    /// Clears all subscribers (testing/cleanup only).
    /// </summary>
    public void Clear()
    {
        lock (_subscriberLock)
        {
            _subscribers.Clear();
        }
    }
}

/// <summary>
/// Extension methods for easy event publishing.
/// </summary>
public static class EventBusExtensions
{
    /// <summary>
    /// Publishes product created event.
    /// </summary>
    public static async Task PublishProductCreatedAsync(this IEventBus eventBus, int productId, string productName, decimal price)
    {
        await eventBus.PublishAsync(new ProductCreatedEvent
        {
            ProductId = productId,
            ProductName = productName,
            Price = price,
            AggregateType = "Product"
        });
    }

    /// <summary>
    /// Publishes order placed event.
    /// </summary>
    public static async Task PublishOrderPlacedAsync(this IEventBus eventBus, int orderId, int userId, decimal totalAmount, int itemCount)
    {
        await eventBus.PublishAsync(new OrderPlacedEvent
        {
            OrderId = orderId,
            UserId = userId,
            TotalAmount = totalAmount,
            ItemCount = itemCount,
            AggregateType = "Order"
        });
    }

    /// <summary>
    /// Publishes user registered event.
    /// </summary>
    public static async Task PublishUserRegisteredAsync(this IEventBus eventBus, int userId, string email, string fullName)
    {
        await eventBus.PublishAsync(new UserRegisteredEvent
        {
            UserId = userId,
            Email = email,
            FullName = fullName,
            AggregateType = "User"
        });
    }

    /// <summary>
    /// Publishes custom event with dynamic data.
    /// </summary>
    public static async Task PublishCustomEventAsync(this IEventBus eventBus, string eventName, Dictionary<string, object> data)
    {
        await eventBus.PublishAsync(new CustomEvent
        {
            EventName = eventName,
            Data = data,
            AggregateType = "Custom"
        });
    }
}
