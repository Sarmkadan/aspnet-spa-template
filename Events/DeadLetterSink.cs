#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using Microsoft.Extensions.Logging;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Sink that receives events which have exhausted their handler retry budget.
/// Implementations decide how to persist or surface these events (logging, storage, alerting, etc.).
/// </summary>
public interface IDeadLetterSink
{
    /// <summary>
    /// Routes an event that could not be handled successfully after all retry attempts were exhausted.
    /// </summary>
    /// <typeparam name="TEvent">The event type, constrained to <see cref="DomainEvent"/>.</typeparam>
    /// <param name="event">The event payload that failed handling.</param>
    /// <param name="exception">The exception (or aggregate of exceptions) describing why handling failed.</param>
    /// <param name="cancellationToken">Token used to cancel the sink operation.</param>
    /// <returns>A task representing the asynchronous sink operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> or <paramref name="exception"/> is <c>null</c>.</exception>
    Task SendAsync<TEvent>(TEvent @event, Exception exception, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;
}

/// <summary>
/// Default <see cref="IDeadLetterSink"/> implementation that records dead-lettered events as structured error logs.
/// Suitable as a baseline for single-server deployments; production systems may replace it with a durable sink
/// (database table, queue, alerting pipeline) by registering a different <see cref="IDeadLetterSink"/> implementation.
/// </summary>
public class LoggingDeadLetterSink : IDeadLetterSink
{
    private readonly ILogger<LoggingDeadLetterSink> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="LoggingDeadLetterSink"/>.
    /// </summary>
    /// <param name="logger">Logger used to record dead-lettered events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public LoggingDeadLetterSink(ILogger<LoggingDeadLetterSink> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Logs the dead-lettered event at <see cref="LogLevel.Error"/>, including its type, identifier and payload.
    /// </summary>
    /// <typeparam name="TEvent">The event type, constrained to <see cref="DomainEvent"/>.</typeparam>
    /// <param name="event">The event payload that failed handling.</param>
    /// <param name="exception">The exception (or aggregate of exceptions) describing why handling failed.</param>
    /// <param name="cancellationToken">Token used to cancel the sink operation.</param>
    /// <returns>A completed task; logging is synchronous.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="event"/> or <paramref name="exception"/> is <c>null</c>.</exception>
    public Task SendAsync<TEvent>(TEvent @event, Exception exception, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        ArgumentNullException.ThrowIfNull(exception);

        _logger.LogError(
            exception,
            "Dead-lettered event {EventType} (ID: {EventId}, Aggregate: {AggregateType}/{AggregateId}): {@Event}",
            typeof(TEvent).Name,
            @event.EventId,
            @event.AggregateType,
            @event.AggregateId,
            @event);

        return Task.CompletedTask;
    }
}
