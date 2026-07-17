#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Integration;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Validation helpers for <see cref="DomainEventHandlers"/>.
/// </summary>
public static class DomainEventHandlersValidation
{
    /// <summary>
    /// Validates the supplied <see cref="DomainEventHandlers"/> instance.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>
    /// A read-only list of human-readable problem descriptions.
    /// The list is empty when the instance is considered valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static IReadOnlyList<string> Validate(this DomainEventHandlers value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Use reflection to inspect the three constructor-injected fields.
        // They are private readonly fields named _cacheService, _notificationService and _logger.
        // If any of them are null, the handler cannot function correctly.
        var type = typeof(DomainEventHandlers);
        var fields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

        var cacheField = fields.FirstOrDefault(static f => f.Name == "_cacheService");
        var notificationField = fields.FirstOrDefault(static f => f.Name == "_notificationService");
        var loggerField = fields.FirstOrDefault(static f => f.Name == "_logger");

        if (cacheField is null)
            problems.Add("Missing expected field '_cacheService'.");
        else if (cacheField.GetValue(value) is null)
            problems.Add("Cache service is not initialized.");

        if (notificationField is null)
            problems.Add("Missing expected field '_notificationService'.");
        else if (notificationField.GetValue(value) is null)
            problems.Add("Notification service is not initialized.");

        if (loggerField is null)
            problems.Add("Missing expected field '_logger'.");
        else if (loggerField.GetValue(value) is null)
            problems.Add("Logger is not initialized.");

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the supplied <see cref="DomainEventHandlers"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to test.</param>
    /// <returns><see langword="true"/> when no validation problems are reported; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    public static bool IsValid(this DomainEventHandlers value) =>
        !value.Validate().Any();

    /// <summary>
    /// Ensures that the supplied <see cref="DomainEventHandlers"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// Thrown when one or more validation problems are detected. The exception message contains a semicolon-separated list of problems.
    /// </exception>
    public static void EnsureValid(this DomainEventHandlers value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
            throw new ArgumentException(string.Join("; ", problems), nameof(value));
    }
}