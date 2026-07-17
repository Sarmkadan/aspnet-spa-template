#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Collections.Generic;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Provides validation helpers for <see cref="EventBusImplementation"/>.
/// Validates the event bus state and configuration.
/// </summary>
public static class EventBusImplementationValidation
{
    /// <summary>
    /// Validates the <see cref="EventBusImplementation"/> instance.
    /// </summary>
    /// <param name="value">The event bus implementation to validate.</param>
    /// <returns>A list of human-readable validation problems; empty list if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this EventBusImplementation value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // EventBusImplementation has no configurable properties to validate
        // The validation is primarily about the instance being non-null and functional
        // which is already ensured by the constructor and method guards

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="EventBusImplementation"/> is valid.
    /// </summary>
    /// <param name="value">The event bus implementation to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this EventBusImplementation value)
    {
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="EventBusImplementation"/> is valid.
    /// </summary>
    /// <param name="value">The event bus implementation to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid.</exception>
    public static void EnsureValid(this EventBusImplementation value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"EventBusImplementation is not valid. Problems: {string.Join(", ", problems)}");
        }
    }
}