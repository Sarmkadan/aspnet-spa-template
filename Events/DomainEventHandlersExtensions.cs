#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using AspNetSpaTemplate.Caching;
using Microsoft.Extensions.Logging;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Extension methods for <see cref="DomainEventHandlers"/> that provide fluent APIs
/// for common event handling patterns and batch operations.
/// </summary>
public static class DomainEventHandlersExtensions
{
    /// <summary>
    /// Logger field name in <see cref="DomainEventHandlers"/>.
    /// </summary>
    private const string LoggerFieldName = "_logger";

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

        await handlers.ProcessEventsAsync(createdEvents, handlers.OnProductCreated);
        await handlers.ProcessEventsAsync(updatedEvents, handlers.OnProductUpdated);
        await handlers.ProcessEventsAsync(deletedEvents, handlers.OnProductDeleted);

        handlers.LogBatchSummary(
            createdEvents.Count,
            updatedEvents.Count,
            deletedEvents.Count,
            "product batch: {CreatedCount} created, {UpdatedCount} updated, {DeletedCount} deleted");
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

        await handlers.ProcessEventsAsync(placedEvents, handlers.OnOrderPlaced);
        await handlers.ProcessEventsAsync(completedEvents, handlers.OnOrderCompleted);
        await handlers.ProcessEventsAsync(cancelledEvents, handlers.OnOrderCancelled);

        handlers.LogBatchSummary(
            placedEvents.Count,
            completedEvents.Count,
            cancelledEvents.Count,
            "order batch: {PlacedCount} placed, {CompletedCount} completed, {CancelledCount} cancelled");
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

        await handlers.ProcessEventsAsync(registeredEvents, handlers.OnUserRegistered);

        handlers.LogBatchSummary(
            registeredEvents.Count,
            0,
            0,
            "user batch: {RegisteredCount} registered");
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

        await handlers.ProcessEventsAsync(submittedEvents, handlers.OnReviewSubmitted);

        handlers.LogBatchSummary(
            submittedEvents.Count,
            0,
            0,
            "review batch: {SubmittedCount} submitted");
    }

    /// <summary>
    /// Helper method to process a collection of events with a specific handler.
    /// </summary>
    /// <typeparam name="TEvent">The event type.</typeparam>
    /// <param name="events">The events to process.</param>
    /// <param name="handler">The handler action.</param>
    /// <returns>A task representing the operation.</returns>
    private static async Task ProcessEventsAsync<TEvent>(
        this DomainEventHandlers handlers,
        IReadOnlyList<TEvent> events,
        Func<TEvent, Task> handler)
        where TEvent : DomainEvent
    {
        foreach (var @event in events)
        {
            await handler(@event);
        }
    }

    /// <summary>
    /// Helper method to log batch processing summary.
    /// </summary>
    /// <param name="count1">First count value.</param>
    /// <param name="count2">Second count value.</param>
    /// <param name="count3">Third count value.</param>
    /// <param name="messageFormat">The log message format with named placeholders.</param>
    private static void LogBatchSummary(
        this DomainEventHandlers handlers,
        int count1,
        int count2,
        int count3,
        string messageFormat)
    {
        if (handlers.GetLogger() is { } logger)
        {
            logger.LogInformation(
                messageFormat,
                count1,
                count2,
                count3);
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
            LoggerFieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        return loggerField?.GetValue(handlers) as ILogger<DomainEventHandlers>;
    }
}