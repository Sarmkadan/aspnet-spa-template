#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides validation extension methods for <see cref="ValidationException"/>.
/// </summary>
public static class ValidationExceptionValidation
{
    /// <summary>
    /// Validates the <see cref="ValidationException"/> and returns a list of human-readable validation problems.
    /// </summary>
    /// <param name="value">The validation exception to validate.</param>
    /// <returns>A read-only list of validation problems; empty if valid.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        // Validate Errors dictionary
        if (value.Errors == null)
        {
            problems.Add("Errors dictionary is null.");
            return problems.AsReadOnly();
        }

        if (value.Errors.Count == 0)
        {
            problems.Add("Errors dictionary is empty.");
        }

        foreach (var kvp in value.Errors)
        {
            if (string.IsNullOrWhiteSpace(kvp.Key))
            {
                problems.Add($"Field name is null or whitespace in Errors dictionary.");
            }

            if (kvp.Value == null)
            {
                problems.Add($"Error list for field '{kvp.Key}' is null.");
                continue;
            }

            if (kvp.Value.Count == 0)
            {
                problems.Add($"Error list for field '{kvp.Key}' is empty.");
            }

            foreach (var error in kvp.Value)
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    problems.Add($"Error message for field '{kvp.Key}' is null or whitespace.");
                }
            }
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the <see cref="ValidationException"/> is valid.
    /// </summary>
    /// <param name="value">The validation exception to check.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value.Validate().Count == 0;
    }

    /// <summary>
    /// Ensures that the <see cref="ValidationException"/> is valid, throwing an <see cref="ArgumentException"/>
    /// with the list of validation problems if it is not.
    /// </summary>
    /// <param name="value">The validation exception to ensure is valid.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not valid,
    /// containing the list of validation problems in the message.</exception>
    public static void EnsureValid(this ValidationException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = value.Validate();
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ValidationException is not valid. Problems: {string.Join(" ", problems)}",
                nameof(value));
        }
    }
}