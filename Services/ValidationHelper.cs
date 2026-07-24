#nullable enable

using AspNetSpaTemplate.Exceptions;

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Provides consistent validation helpers for service layer methods.
/// Ensures uniform exception handling across Product, Order, User, Theme, and Review services.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates that an argument is not null.
    /// </summary>
    /// <param name="argument">The argument to validate.</param>
    /// <param name="paramName">Name of the parameter for exception messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when argument is null.</exception>
    public static void ValidateNotNull(object? argument, string paramName)
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);
    }

    /// <summary>
    /// Validates that a string argument is not null, empty, or whitespace.
    /// </summary>
    /// <param name="argument">The string argument to validate.</param>
    /// <param name="paramName">Name of the parameter for exception messages.</param>
    /// <exception cref="ArgumentException">Thrown when argument is null, empty, or whitespace.</exception>
    public static void ValidateNotNullOrWhitespace(string? argument, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(argument, paramName);

        if (string.IsNullOrWhiteSpace(argument))
        {
            throw new ArgumentException("String cannot contain only whitespace.", paramName);
        }
    }

    /// <summary>
    /// Validates that an integer ID is positive (greater than 0).
    /// </summary>
    /// <param name="id">The ID to validate.</param>
    /// <param name="paramName">Name of the parameter for exception messages.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when ID is less than or equal to 0.</exception>
    public static void ValidatePositiveId(int id, string paramName)
    {
        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "ID must be greater than 0.");
        }
    }

    /// <summary>
    /// Validates that a nullable integer ID is positive (greater than 0).
    /// </summary>
    /// <param name="id">The nullable ID to validate.</param>
    /// <param name="paramName">Name of the parameter for exception messages.</param>
    /// <exception cref="ArgumentException">Thrown when ID is null or less than or equal to 0.</exception>
    public static void ValidatePositiveId(int? id, string paramName)
    {
        ArgumentException.ThrowIfNullOrEmpty(id?.ToString());

        if (id <= 0)
        {
            throw new ArgumentOutOfRangeException(paramName, "ID must be greater than 0.");
        }
    }

    /// <summary>
    /// Validates that a collection is not null or empty.
    /// </summary>
    /// <typeparam name="T">Type of collection items.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="paramName">Name of the parameter for exception messages.</param>
    /// <exception cref="ArgumentNullException">Thrown when collection is null.</exception>
    /// <exception cref="ValidationException">Thrown when collection is empty.</exception>
    public static void ValidateNotNullOrEmpty<T>(ICollection<T>? collection, string paramName)
    {
        ArgumentNullException.ThrowIfNull(collection, paramName);

        if (collection.Count == 0)
        {
            throw new ValidationException(paramName, "Collection cannot be empty.");
        }
    }

    /// <summary>
    /// Validates that a collection count does not exceed a maximum limit.
    /// </summary>
    /// <typeparam name="T">Type of collection items.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="paramName">Name of the parameter for exception messages.</param>
    /// <param name="maxCount">Maximum allowed count.</param>
    /// <exception cref="ValidationException">Thrown when collection count exceeds maximum.</exception>
    public static void ValidateMaxCount<T>(ICollection<T> collection, string paramName, int maxCount)
    {
        if (collection.Count > maxCount)
        {
            throw new ValidationException(paramName, $"Collection cannot contain more than {maxCount} items.");
        }
    }
}