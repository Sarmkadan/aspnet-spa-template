#nullable enable
using System.Globalization;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Validation helpers for <see cref="MemoryCacheServiceTests"/> instances.
/// </summary>
public static class MemoryCacheServiceTestsValidation
{
    /// <summary>
    /// Validates that a <see cref="MemoryCacheServiceTests"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <returns>A list of validation problems; empty if valid.</returns>
    public static IReadOnlyList<string> Validate(this MemoryCacheServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = new List<string>();

        if (value.Id < 0)
        {
            problems.Add($"Id must be a non-negative number, but was {value.Id}");
        }

        if (string.IsNullOrWhiteSpace(value.Name))
        {
            problems.Add("Name is null, empty, or whitespace");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether the specified <see cref="MemoryCacheServiceTests"/> instance is valid.
    /// </summary>
    /// <param name="value">The instance to check.</param>
    /// <returns><see langword="true"/> if valid; otherwise, <see langword="false"/>.</returns>
    public static bool IsValid(this MemoryCacheServiceTests? value)
    {
        return Validate(value).Count == 0;
    }

    /// <summary>
    /// Ensures that the specified <see cref="MemoryCacheServiceTests"/> instance is in a valid state.
    /// </summary>
    /// <param name="value">The instance to validate.</param>
    /// <exception cref="ArgumentException">Thrown if the instance is not valid.</exception>
    public static void EnsureValid(this MemoryCacheServiceTests? value)
    {
        ArgumentNullException.ThrowIfNull(value);

        var problems = Validate(value);
        if (problems.Count == 0)
        {
            return;
        }

        throw new ArgumentException(
            $"MemoryCacheServiceTests instance is not valid. Problems: {string.Join(", ", problems)}");
    }
}