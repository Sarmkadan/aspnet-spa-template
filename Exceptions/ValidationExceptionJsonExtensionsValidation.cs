#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Globalization;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides validation helpers for <see cref="ValidationException"/> instances in the context
/// of JSON serialization/deserialization. This class contains validation methods that complement
/// the functionality provided by <see cref="ValidationExceptionJsonExtensions"/>.
/// </summary>
public static class ValidationExceptionJsonExtensionsValidation
{
    /// <summary>
    /// Validates the <see cref="ValidationException"/> instance for problems that may occur during JSON serialization/deserialization.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <returns>A list of human-readable validation problems, or an empty list if valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Message - ensure it's safe for JSON serialization
        if (value.Message is null)
        {
            problems.Add("Message cannot be null.");
        }
        else if (string.IsNullOrWhiteSpace(value.Message))
        {
            problems.Add("Message cannot be empty or whitespace.");
        }

        // Validate Errors dictionary - ensure it's safe for JSON serialization
        if (value.Errors is null)
        {
            problems.Add("Errors dictionary cannot be null.");
        }
        else
        {
            // Check for null keys - would cause issues in JSON
            foreach (var key in value.Errors.Keys)
            {
                if (key is null)
                {
                    problems.Add("Errors dictionary contains a null key.");
                    break;
                }

                if (string.IsNullOrWhiteSpace(key))
                {
                    problems.Add("Errors dictionary contains an empty or whitespace key.");
                    break;
                }
            }

            // Check for null or empty error lists - would cause issues in JSON
            foreach (var kvp in value.Errors)
            {
                if (kvp.Value is null)
                {
                    problems.Add($"Errors dictionary contains a null error list for key '{kvp.Key}'.");
                    break;
                }

                if (kvp.Value.Count == 0)
                {
                    problems.Add($"Errors dictionary contains an empty error list for key '{kvp.Key}'.");
                }

                // Check for null or empty error messages - would cause issues in JSON
                foreach (var error in kvp.Value)
                {
                    if (error is null)
                    {
                        problems.Add($"Errors dictionary contains a null error message for key '{kvp.Key}'.");
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(error))
                    {
                        problems.Add($"Errors dictionary contains an empty or whitespace error message for key '{kvp.Key}'.");
                        break;
                    }
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="ValidationException"/> instance is valid for JSON operations.
    /// </summary>
    /// <param name="value">The exception to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static bool IsValid(ValidationException value)
    {
        return !Validate(value).Any();
    }

    /// <summary>
    /// Ensures that the <see cref="ValidationException"/> instance is valid for JSON operations.
    /// </summary>
    /// <param name="value">The exception to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid. The exception message contains all validation problems.</exception>
    public static void EnsureValid(ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"ValidationException is not valid for JSON operations. Problems: {string.Join(" ", problems)}");
    }
}
