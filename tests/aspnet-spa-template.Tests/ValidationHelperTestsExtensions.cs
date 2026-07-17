#nullable enable

using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Extension methods for <see cref="ValidationHelperTests"/> that provide additional validation scenarios
/// and helper methods for working with ValidationHelper in test scenarios.
/// </summary>
public static class ValidationHelperTestsExtensions
{
    /// <summary>
    /// Validates that a value is not null and returns the value for fluent assertions.
    /// </summary>
    /// <typeparam name="T">Type of the value to validate.</typeparam>
    /// <param name="value">The value to validate.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated value for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the value is null.</exception>
    public static T NotNull<T>(this T? value, string fieldName) where T : class
    {
        ValidationHelper.NotNull(value, fieldName);
        return value;
    }

    /// <summary>
    /// Validates that a string is not null or empty and returns the string for fluent assertions.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated string for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the string is null or empty.</exception>
    public static string NotNullOrEmpty(this string? value, string fieldName)
    {
        ValidationHelper.NotNullOrEmpty(value, fieldName);
        return value!;
    }

    /// <summary>
    /// Validates that a number is within specified range (inclusive) and returns the value for fluent assertions.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">Minimum allowed value.</param>
    /// <param name="max">Maximum allowed value.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated value for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the value is out of range.</exception>
    public static decimal InRange(this decimal value, decimal min, decimal max, string fieldName)
    {
        ValidationHelper.InRange(value, min, max, fieldName);
        return value;
    }

    /// <summary>
    /// Validates that a number is within specified range (inclusive) and returns the value for fluent assertions.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">Minimum allowed value.</param>
    /// <param name="max">Maximum allowed value.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated value for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the value is out of range.</exception>
    public static int InRange(this int value, int min, int max, string fieldName)
    {
        ValidationHelper.InRange(value, min, max, fieldName);
        return value;
    }

    /// <summary>
    /// Validates that a string length is within specified bounds and returns the string for fluent assertions.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="minLength">Minimum allowed length.</param>
    /// <param name="maxLength">Maximum allowed length.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated string for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the string length is out of bounds.</exception>
    public static string LengthBetween(this string value, int minLength, int maxLength, string fieldName)
    {
        ValidationHelper.LengthBetween(value, minLength, maxLength, fieldName);
        return value;
    }

    /// <summary>
    /// Validates that a string matches a regular expression pattern and returns the string for fluent assertions.
    /// </summary>
    /// <param name="value">The string to validate.</param>
    /// <param name="pattern">Regular expression pattern to match.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <param name="customMessage">Optional custom error message.</param>
    /// <returns>The validated string for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the string doesn't match the pattern.</exception>
    public static string MatchesPattern(this string value, string pattern, string fieldName, string? customMessage = null)
    {
        ValidationHelper.MatchesPattern(value, pattern, fieldName, customMessage);
        return value;
    }

    /// <summary>
    /// Validates that an email address format is correct and returns the email for fluent assertions.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated email address for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the email format is invalid.</exception>
    public static string ValidEmail(this string email, string fieldName = "Email")
    {
        ValidationHelper.ValidEmail(email, fieldName);
        return email;
    }

    /// <summary>
    /// Validates that a phone number contains only digits (after removing common separators) and returns the phone number for fluent assertions.
    /// </summary>
    /// <param name="phoneNumber">The phone number to validate.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated phone number for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the phone number is invalid.</exception>
    public static string ValidPhoneNumber(this string phoneNumber, string fieldName = "PhoneNumber")
    {
        ValidationHelper.ValidPhoneNumber(phoneNumber, fieldName);
        return phoneNumber;
    }

    /// <summary>
    /// Validates that a collection contains at least one item and returns the collection for fluent assertions.
    /// </summary>
    /// <typeparam name="T">Type of items in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated collection for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the collection is null or empty.</exception>
    public static IEnumerable<T> NotEmpty<T>(this IEnumerable<T>? collection, string fieldName)
    {
        ValidationHelper.NotEmpty(collection, fieldName);
        return collection!;
    }

    /// <summary>
    /// Validates that a collection has at most maxItems items and returns the collection for fluent assertions.
    /// </summary>
    /// <typeparam name="T">Type of items in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="maxItems">Maximum allowed number of items.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated collection for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the collection has too many items.</exception>
    public static IEnumerable<T> MaxItems<T>(this IEnumerable<T>? collection, int maxItems, string fieldName)
    {
        ValidationHelper.MaxItems(collection, maxItems, fieldName);
        return collection!;
    }

    /// <summary>
    /// Validates that two values are equal (case-sensitive for strings) and returns the first value for fluent assertions.
    /// </summary>
    /// <typeparam name="T">Type of values being compared.</typeparam>
    /// <param name="value1">First value to compare.</param>
    /// <param name="value2">Second value to compare.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The first value for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the values are not equal.</exception>
    public static T Equal<T>(this T value1, T value2, string fieldName) where T : notnull
    {
        ValidationHelper.Equal(value1, value2, fieldName);
        return value1;
    }

    /// <summary>
    /// Validates that a collection contains exactly the expected number of items and returns the collection for fluent assertions.
    /// </summary>
    /// <typeparam name="T">Type of items in the collection.</typeparam>
    /// <param name="collection">The collection to validate.</param>
    /// <param name="expectedCount">Expected number of items.</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated collection for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the collection doesn't have the expected count.</exception>
    public static IEnumerable<T> CountEquals<T>(this IEnumerable<T>? collection, int expectedCount, string fieldName)
    {
        ArgumentNullException.ThrowIfNull(collection);
        if (collection.Count() != expectedCount)
        {
            throw new ValidationException(fieldName, $"{fieldName} must contain exactly {expectedCount} items");
        }
        return collection;
    }

    /// <summary>
    /// Validates that a value is greater than a minimum value and returns the value for fluent assertions.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="min">Minimum allowed value (exclusive).</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated value for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the value is not greater than min.</exception>
    public static decimal GreaterThan(this decimal value, decimal min, string fieldName)
    {
        if (value <= min)
        {
            throw new ValidationException(fieldName, $"{fieldName} must be greater than {min}");
        }
        return value;
    }

    /// <summary>
    /// Validates that a value is less than a maximum value and returns the value for fluent assertions.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="max">Maximum allowed value (exclusive).</param>
    /// <param name="fieldName">Name of the field being validated.</param>
    /// <returns>The validated value for chaining.</returns>
    /// <exception cref="ValidationException">Thrown when the value is not less than max.</exception>
    public static decimal LessThan(this decimal value, decimal max, string fieldName)
    {
        if (value >= max)
        {
            throw new ValidationException(fieldName, $"{fieldName} must be less than {max}");
        }
        return value;
    }
}