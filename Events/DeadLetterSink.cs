#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Receives events whose handlers have exhausted their retry budget.
/// Implementations decide what to do with undeliverable events (log, persist, alert, etc.).
/// </summary>
public interface IDeadLetterSink
{
    /// <summary>
    /// Handles an event that could not be delivered after all retry attempts failed.
    /// </summary>
    /// <typeparam name="TEvent">The event type, constrained to <see cref="DomainEvent"/>.</typeparam>
    /// <param name="event">The event that failed delivery.</param>
    /// <param name="exception">The exception describing the failure (typically an <see cref="AggregateException"/> of all attempt failures).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task SendAsync<TEvent>(TEvent @event, Exception exception, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent;
}

/// <summary>
/// Default <see cref="IDeadLetterSink"/> implementation that logs dead-lettered events.
/// </summary>
public class LoggingDeadLetterSink : IDeadLetterSink
{
    private readonly ILogger<LoggingDeadLetterSink> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingDeadLetterSink"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record dead-lettered events.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="logger"/> is <c>null</c>.</exception>
    public LoggingDeadLetterSink(ILogger<LoggingDeadLetterSink> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _logger = logger;
    }

    /// <summary>
    /// Logs the dead-lettered event and its failure at error level.
    /// </summary>
    /// <typeparam name="TEvent">The event type, constrained to <see cref="DomainEvent"/>.</typeparam>
    /// <param name="event">The event that failed delivery.</param>
    /// <param name="exception">The exception describing the failure.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A completed <see cref="Task"/>.</returns>
    public Task SendAsync<TEvent>(TEvent @event, Exception exception, CancellationToken cancellationToken = default)
        where TEvent : DomainEvent
    {
        _logger.LogError(
            exception,
            "Dead-lettered event: {EventType} (ID: {EventId})",
            typeof(TEvent).Name,
            @event.EventId);

        return Task.CompletedTask;
    }
}
