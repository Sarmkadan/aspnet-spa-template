// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Events;

/// <summary>
/// In-process event bus implementation.
/// Suitable for single-server deployments. For distributed systems, use a message broker.
/// Maintains subscribers in memory and dispatches events sequentially.
/// </summary>
public class EventBusImplementation : IEventBus
{
    private readonly Dictionary<Type, List<Delegate>> _subscribers = new();
    private readonly object _subscriberLock = new();
    private readonly ILogger<EventBusImplementation> _logger;

    public EventBusImplementation(ILogger<EventBusImplementation> logger)
    {
        _logger = logger;
    }

    public void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent
    {
        if (handler == null)
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
        if (@event == null)
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

        // Call handlers sequentially
        foreach (var handler in handlers)
        {
            try
            {
                if (handler is Func<TEvent, Task> typedHandler)
                {
                    await typedHandler(@event);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in event handler for {eventType.Name}");
                // Continue with other handlers instead of failing
            }
        }

        _logger.LogInformation($"Event published: {eventType.Name} (handled by {handlers.Count} subscribers)");
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
