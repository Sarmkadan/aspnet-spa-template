#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Provides validation helpers for <see cref="EventBusImplementationExtensions"/> extension methods.
/// Validates that the extension methods on <see cref="EventBusImplementation"/> are available and functional.
/// </summary>
public static class EventBusImplementationExtensionsValidation
{
    /// <summary>
    /// Validates that the extension methods on <see cref="EventBusImplementation"/> are available and functional.
    /// </summary>
    /// <param name="eventBus">The event bus implementation to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> is null.</exception>
    public static IReadOnlyList<string> ValidateEventBusExtensions(this EventBusImplementation eventBus)
    {
        ArgumentNullException.ThrowIfNull(eventBus);

        var problems = new List<string>();

        // Validate GetSubscriberCountLock method availability
        try
        {
            _ = eventBus.GetSubscriberCountLock();
        }
        catch (Exception ex)
        {
            problems.Add($"GetSubscriberCountLock method failed: {ex.Message}");
        }

        // Validate GetAllSubscriberCounts method availability
        try
        {
            _ = eventBus.GetAllSubscriberCounts();
        }
        catch (Exception ex)
        {
            problems.Add($"GetAllSubscriberCounts method failed: {ex.Message}");
        }

        // Validate Subscribe method availability
        try
        {
            eventBus.Subscribe<CustomEvent>(new List<Func<CustomEvent, Task>>());
        }
        catch (Exception ex)
        {
            problems.Add($"Subscribe method failed: {ex.Message}");
        }

        // Validate TryPublishAsync method availability
        try
        {
            _ = eventBus.TryPublishAsync(new CustomEvent());
        }
        catch (Exception ex)
        {
            problems.Add($"TryPublishAsync method failed: {ex.Message}");
        }

        // Validate PublishBatchAsync method availability
        try
        {
            _ = eventBus.PublishBatchAsync(new List<CustomEvent>());
        }
        catch (Exception ex)
        {
            problems.Add($"PublishBatchAsync method failed: {ex.Message}");
        }

        // Validate ClearSubscribers method availability
        try
        {
            eventBus.ClearSubscribers<CustomEvent>();
        }
        catch (Exception ex)
        {
            problems.Add($"ClearSubscribers method failed: {ex.Message}");
        }

        // Validate PublishWithDelayAsync methods availability
        try
        {
            _ = eventBus.PublishWithDelayAsync(new CustomEvent(), 100);
        }
        catch (Exception ex)
        {
            problems.Add($"PublishWithDelayAsync(int) method failed: {ex.Message}");
        }

        try
        {
            _ = eventBus.PublishWithDelayAsync(new CustomEvent(), TimeSpan.FromMilliseconds(100));
        }
        catch (Exception ex)
        {
            problems.Add($"PublishWithDelayAsync(TimeSpan) method failed: {ex.Message}");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="EventBusImplementation"/> instance has valid extension methods.
    /// </summary>
    /// <param name="eventBus">The event bus implementation to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> is null.</exception>
    public static bool AreEventBusExtensionsValid(this EventBusImplementation eventBus)
        => eventBus.ValidateEventBusExtensions().Count == 0;

    /// <summary>
    /// Ensures that the specified <see cref="EventBusImplementation"/> instance has valid extension methods.
    /// </summary>
    /// <param name="eventBus">The event bus implementation to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="eventBus"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when validation fails, containing the list of problems.</exception>
    public static void EnsureEventBusExtensionsAreValid(this EventBusImplementation eventBus)
    {
        ArgumentNullException.ThrowIfNull(eventBus);

        var problems = eventBus.ValidateEventBusExtensions();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"EventBusImplementationExtensions validation failed:{Environment.NewLine}- {
                    string.Join(Environment.NewLine + "- ", problems)
                }");
        }
    }
}
