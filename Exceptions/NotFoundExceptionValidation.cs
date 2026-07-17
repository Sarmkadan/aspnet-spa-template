#nullable enable
// =============================================================================
// Author: 
// =============================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Validation helpers for the <see cref="NotFoundException"/> type.
/// </summary>
public static class NotFoundExceptionValidation
{
    /// <summary>
    /// Validates the given <paramref name="value"/> and returns a list of human-readable problems.
    /// </summary>
    /// <param name="value">The <see cref="NotFoundException"/> to validate.</param>
    /// <returns>A list of human-readable problems.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this NotFoundException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (string.IsNullOrWhiteSpace(value.ResourceType))
        {
            problems.Add("Resource type is null or empty.");
        }

        if (value.ResourceId == null)
        {
            problems.Add("Resource ID is null.");
        }

        return problems;
    }

    /// <summary>
    /// Checks if the given <paramref name="value"/> is valid.
    /// </summary>
    /// <param name="value">The <see cref="NotFoundException"/> to check.</param>
    /// <returns>True if the <paramref name="value"/> is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static bool IsValid(this NotFoundException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures the given <paramref name="value"/> is valid, throwing an exception if it's not.
    /// </summary>
    /// <param name="value">The <see cref="NotFoundException"/> to ensure is valid.</param>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="value"/> is not valid.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static void EnsureValid(this NotFoundException value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);

        if (problems.Count > 0)
        {
            throw new ArgumentException($"The NotFoundException is not valid: {string.Join(", ", problems)}", nameof(value));
        }
    }
}
