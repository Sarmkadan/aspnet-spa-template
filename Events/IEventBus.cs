// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Interface for publish-subscribe event bus.
/// Decouples event publishers from subscribers using observer pattern.
/// Enables loose coupling and reactive programming patterns.
/// </summary>
public interface IEventBus
{
    /// <summary>
    /// Subscribes a handler to an event type.
    /// Handler will be called whenever event is published.
    /// </summary>
    void Subscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent;

    /// <summary>
    /// Unsubscribes a handler from an event type.
    /// </summary>
    void Unsubscribe<TEvent>(Func<TEvent, Task> handler) where TEvent : DomainEvent;

    /// <summary>
    /// Publishes an event to all subscribers.
    /// Handlers are called sequentially (not in parallel).
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : DomainEvent;

    /// <summary>
    /// Publishes multiple events atomically.
    /// All events are published or none (transaction-like behavior).
    /// </summary>
    Task PublishManyAsync<TEvent>(IEnumerable<TEvent> events) where TEvent : DomainEvent;
}

/// <summary>
/// Base class for domain events.
/// All events should inherit from this to work with event bus.
/// </summary>
public abstract class DomainEvent
{
    public string EventId { get; set; } = Guid.NewGuid().ToString();
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int? AggregateId { get; set; }
    public string AggregateType { get; set; } = "";
}

/// <summary>
/// Event handlers for domain events.
/// These are strongly-typed handlers for specific events.
/// </summary>

/// <summary>
/// Fired when a product is created.
/// </summary>
public class ProductCreatedEvent : DomainEvent
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
}

/// <summary>
/// Fired when a product is updated.
/// </summary>
public class ProductUpdatedEvent : DomainEvent
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
    public decimal Price { get; set; }
}

/// <summary>
/// Fired when a product is deleted.
/// </summary>
public class ProductDeletedEvent : DomainEvent
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = "";
}

/// <summary>
/// Fired when an order is placed.
/// </summary>
public class OrderPlacedEvent : DomainEvent
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
}

/// <summary>
/// Fired when an order is completed.
/// </summary>
public class OrderCompletedEvent : DomainEvent
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public DateTime CompletedAt { get; set; }
}

/// <summary>
/// Fired when an order is cancelled.
/// </summary>
public class OrderCancelledEvent : DomainEvent
{
    public int OrderId { get; set; }
    public int UserId { get; set; }
    public string Reason { get; set; } = "";
}

/// <summary>
/// Fired when a user registers.
/// </summary>
public class UserRegisteredEvent : DomainEvent
{
    public int UserId { get; set; }
    public string Email { get; set; } = "";
    public string FullName { get; set; } = "";
}

/// <summary>
/// Fired when a review is submitted.
/// </summary>
public class ReviewSubmittedEvent : DomainEvent
{
    public int ReviewId { get; set; }
    public int ProductId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = "";
}

/// <summary>
/// Generic event for custom notifications.
/// </summary>
public class CustomEvent : DomainEvent
{
    public string EventName { get; set; } = "";
    public Dictionary<string, object> Data { get; set; } = new();
}
