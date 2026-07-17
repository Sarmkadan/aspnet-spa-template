#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// ====================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides validation extension methods for <see cref="ExternalApiException"/>.
/// </summary>
public static class ExternalApiExceptionValidation
{
    /// <summary>
    /// Validates the <see cref="ExternalApiException"/> and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The external API exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ExternalApiException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Endpoint
        if (string.IsNullOrWhiteSpace(value.Endpoint))
        {
            problems.Add("Endpoint is null or empty.");
        }

        // Validate StatusCode (should be a valid HTTP status code if present)
        if (value.StatusCode.HasValue)
        {
            if (value.StatusCode < 100 || value.StatusCode > 599)
            {
                problems.Add("StatusCode is out of valid HTTP status code range (100-599).");
            }
        }

        // Validate Method
        if (string.IsNullOrWhiteSpace(value.Method))
        {
            problems.Add("Method is null or empty.");
        }
        else
        {
            // Basic HTTP method validation
            var validMethods = new[] { "GET", "POST", "PUT", "PATCH", "DELETE", "HEAD", "OPTIONS" };
            if (!validMethods.Contains(value.Method, StringComparer.OrdinalIgnoreCase))
            {
                problems.Add("Method is not a valid HTTP method (GET, POST, PUT, PATCH, DELETE, HEAD, OPTIONS).");
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="ExternalApiException"/> is valid.
    /// </summary>
    /// <param name="value">The external API exception to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ExternalApiException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="ExternalApiException"/> is valid, throwing an <see cref="ArgumentException"/>
    /// with the list of validation problems if it is not.
    /// </summary>
    /// <param name="value">The external API exception to ensure is valid.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid,
    /// containing the list of validation problems in the message.</exception>
    public static void EnsureValid(this ExternalApiException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ExternalApiException is not valid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}