#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="BusinessException"/> instances.
/// </summary>
public static class BusinessExceptionValidation
{
    /// <summary>
    /// Validates that a <see cref="BusinessException"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this BusinessException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate ErrorCode
        if (string.IsNullOrWhiteSpace(value.ErrorCode))
        {
            problems.Add("ErrorCode must be a non-empty string.");
        }

        // Validate HttpStatusCode
        if (value.HttpStatusCode is < 400 or > 599)
        {
            problems.Add("HttpStatusCode must be between 400 and 599 (inclusive).");
        }

        // Validate Message (inherited from Exception)
        if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message must be a non-empty string.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a <see cref="BusinessException"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this BusinessException? value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that a <see cref="BusinessException"/> instance is in a valid state,
    /// throwing an <see cref="ArgumentException"/> with a detailed message if not.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this BusinessException? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"BusinessException is invalid. Problems:\n- {
                    string.Join("\n- ", problems)
                }");
        }
    }
}