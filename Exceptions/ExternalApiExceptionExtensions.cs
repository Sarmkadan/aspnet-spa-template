#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="ExternalApiException"/> to facilitate common operations
/// when handling external API failures.
/// </summary>
public static class ExternalApiExceptionExtensions
{
    /// <summary>
    /// Determines whether the API call resulted in a client error (4xx status code).
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception represents a client error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static bool IsClientError(this ExternalApiException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.StatusCode is >= 400 and < 500;
    }

    /// <summary>
    /// Determines whether the API call resulted in a server error (5xx status code).
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception represents a server error; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static bool IsServerError(this ExternalApiException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.StatusCode is >= 500 and < 600;
    }

    /// <summary>
    /// Formats the exception details into a standardized error message suitable for logging
    /// or user display. Includes endpoint, method, status code, and message.
    /// </summary>
    /// <param name="exception">The exception to format.</param>
    /// <returns>A formatted error message string containing the base message and additional context.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    public static string FormatForDisplay(this ExternalApiException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var details = new List<string>();
        if (!string.IsNullOrEmpty(exception.Endpoint))
            details.Add($"Endpoint: {exception.Endpoint}");
        if (!string.IsNullOrEmpty(exception.Method))
            details.Add($"Method: {exception.Method}");
        if (exception.StatusCode.HasValue)
            details.Add($"Status Code: {exception.StatusCode.Value}");

        return details.Count > 0
            ? $"{exception.Message} | {string.Join(" | ", details)}"
            : exception.Message;
    }

    /// <summary>
    /// Creates a new <see cref="ExternalApiException"/> with additional context about retry attempts.
    /// </summary>
    /// <param name="exception">The original exception.</param>
    /// <param name="attemptNumber">The retry attempt number (1-based).</param>
    /// <param name="maxAttempts">The maximum number of retry attempts configured.</param>
    /// <returns>A new exception with retry context added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="attemptNumber"/> or <paramref name="maxAttempts"/> is less than 1.</exception>
    public static ExternalApiException WithRetryContext(
        this ExternalApiException exception,
        int attemptNumber,
        int maxAttempts)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentOutOfRangeException.ThrowIfLessThan(attemptNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAttempts, 1);

        return exception.WithContext("RetryAttempt", attemptNumber)
            .WithContext("MaxRetries", maxAttempts)
            .WithContext("RetryExhausted", attemptNumber >= maxAttempts);
    }
}