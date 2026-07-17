#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using AspNetSpaTemplate.Caching;
using System.Globalization;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Extension methods for <see cref="DomainEventHandlers"/> that provide fluent APIs
/// for common event handling patterns and batch operations.
/// </summary>
public static class DomainEventHandlersExtensions
{
    /// <summary>
    /// Processes a collection of product events in a single batch operation.
    /// Useful for bulk operations like importing products or syncing with external systems.
    /// </summary>
    /// <param name="handlers">The event handlers instance.</param>
    /// <param name="events">The collection of product events to process.</param>
    /// <returns>A task representing the batch operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlers"/> or <paramref name="events"/> is null.</exception>
    public static async Task HandleProductEventsAsync(
        this DomainEventHandlers handlers,
        IEnumerable<DomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(events);

        var createdEvents = new List<ProductCreatedEvent>();
        var updatedEvents = new List<ProductUpdatedEvent>();
        var deletedEvents = new List<ProductDeletedEvent>();

        foreach (var @event in events)
        {
            switch (@event)
            {
                case ProductCreatedEvent created:
                    createdEvents.Add(created);
                    break;
                case ProductUpdatedEvent updated:
                    updatedEvents.Add(updated);
                    break;
                case ProductDeletedEvent deleted:
                    deletedEvents.Add(deleted);
                    break;
            }
        }

        // Process created events
        foreach (var @event in createdEvents)
        {
            await handlers.OnProductCreated(@event);
        }

        // Process updated events
        foreach (var @event in updatedEvents)
        {
            await handlers.OnProductUpdated(@event);
        }

        // Process deleted events
        foreach (var @event in deletedEvents)
        {
            await handlers.OnProductDeleted(@event);
        }

        // Log batch summary
        if (handlers.GetLogger() is { } logger)
        {
            logger.LogInformation(
                "Processed product batch: {CreatedCount} created, {UpdatedCount} updated, {DeletedCount} deleted",
                createdEvents.Count,
                updatedEvents.Count,
                deletedEvents.Count);
        }
    }

    /// <summary>
    /// Processes a collection of order events in a single batch operation.
    /// Useful for reporting or analytics aggregation without triggering individual notifications.
    /// </summary>
    /// <param name="handlers">The event handlers instance.</param>
    /// <param name="events">The collection of order events to process.</param>
    /// <returns>A task representing the batch operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlers"/> or <paramref name="events"/> is null.</exception>
    public static async Task HandleOrderEventsAsync(
        this DomainEventHandlers handlers,
        IEnumerable<DomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(events);

        var placedEvents = new List<OrderPlacedEvent>();
        var completedEvents = new List<OrderCompletedEvent>();
        var cancelledEvents = new List<OrderCancelledEvent>();

        foreach (var @event in events)
        {
            switch (@event)
            {
                case OrderPlacedEvent placed:
                    placedEvents.Add(placed);
                    break;
                case OrderCompletedEvent completed:
                    completedEvents.Add(completed);
                    break;
                case OrderCancelledEvent cancelled:
                    cancelledEvents.Add(cancelled);
                    break;
            }
        }

        // Process placed events
        foreach (var @event in placedEvents)
        {
            await handlers.OnOrderPlaced(@event);
        }

        // Process completed events
        foreach (var @event in completedEvents)
        {
            await handlers.OnOrderCompleted(@event);
        }

        // Process cancelled events
        foreach (var @event in cancelledEvents)
        {
            await handlers.OnOrderCancelled(@event);
        }

        // Log batch summary
        if (handlers.GetLogger() is { } logger)
        {
            logger.LogInformation(
                "Processed order batch: {PlacedCount} placed, {CompletedCount} completed, {CancelledCount} cancelled",
                placedEvents.Count,
                completedEvents.Count,
                cancelledEvents.Count);
        }
    }

    /// <summary>
    /// Processes a collection of user events in a single batch operation.
    /// Useful for user import operations or batch registrations.
    /// </summary>
    /// <param name="handlers">The event handlers instance.</param>
    /// <param name="events">The collection of user events to process.</param>
    /// <returns>A task representing the batch operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlers"/> or <paramref name="events"/> is null.</exception>
    public static async Task HandleUserEventsAsync(
        this DomainEventHandlers handlers,
        IEnumerable<DomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(events);

        var registeredEvents = new List<UserRegisteredEvent>();

        foreach (var @event in events)
        {
            if (@event is UserRegisteredEvent registered)
            {
                registeredEvents.Add(registered);
            }
        }

        foreach (var @event in registeredEvents)
        {
            await handlers.OnUserRegistered(@event);
        }

        if (handlers.GetLogger() is { } logger)
        {
            logger.LogInformation(
                "Processed user batch: {RegisteredCount} registered",
                registeredEvents.Count);
        }
    }

    /// <summary>
    /// Processes a collection of review events in a single batch operation.
    /// Updates product ratings and clears relevant caches.
    /// </summary>
    /// <param name="handlers">The event handlers instance.</param>
    /// <param name="events">The collection of review events to process.</param>
    /// <returns>A task representing the batch operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="handlers"/> or <paramref name="events"/> is null.</exception>
    public static async Task HandleReviewEventsAsync(
        this DomainEventHandlers handlers,
        IEnumerable<DomainEvent> events)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        ArgumentNullException.ThrowIfNull(events);

        var submittedEvents = new List<ReviewSubmittedEvent>();

        foreach (var @event in events)
        {
            if (@event is ReviewSubmittedEvent submitted)
            {
                submittedEvents.Add(submitted);
            }
        }

        foreach (var @event in submittedEvents)
        {
            await handlers.OnReviewSubmitted(@event);
        }

        if (handlers.GetLogger() is { } logger)
        {
            logger.LogInformation(
                "Processed review batch: {SubmittedCount} submitted",
                submittedEvents.Count);
        }
    }

    /// <summary>
    /// Gets the logger instance from the handlers for use in extension methods.
    /// </summary>
    /// <param name="handlers">The event handlers instance.</param>
    /// <returns>The logger instance or null if not available.</returns>
    private static ILogger<DomainEventHandlers>? GetLogger(this DomainEventHandlers handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        // Use reflection to access the private logger field
        var loggerField = typeof(DomainEventHandlers).GetField(
            "_logger",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return loggerField?.GetValue(handlers) as ILogger<DomainEventHandlers>;
    }
}