#nullable enable

using System;
using System.Collections.Generic;
using AspNetSpaTemplate.Exceptions;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Provides validation helpers for <see cref="ProductServiceTests"/> instances.
/// Includes methods to validate, check validity, and ensure validity of <see cref="ProductServiceTests"/> objects.
/// </summary>
public static class ProductServiceTestsValidation
{
    private static readonly string[] _emptyArray = Array.Empty<string>();

    /// <summary>
    /// Validates an instance of <see cref="ProductServiceTests"/> and returns a list of validation problems.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if the instance is valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    public static IReadOnlyList<string> Validate(this ProductServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        // ProductServiceTests is a test fixture class with injected dependencies
        // No properties to validate beyond null check
        // This provides the interface as requested by the task

        return _emptyArray;
    }

    /// <summary>
    /// Determines whether an instance of <see cref="ProductServiceTests"/> is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns>True if the instance is valid; otherwise, false.</returns>
    public static bool IsValid(this ProductServiceTests value)
    {
        try
        {
            Validate(value);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that an instance of <see cref="ProductServiceTests"/> is valid,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the instance is not valid.</exception>
    public static void EnsureValid(this ProductServiceTests value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ProductServiceTests instance is not valid. Problems:\n- {
                    string.Join(
                        "\n- ",
                        problems
                    )
                }");
        }
    }
}