// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.RegularExpressions;
using AspNetSpaTemplate.Exceptions;

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Helper class for common validation scenarios.
/// Centralizes validation logic to ensure consistent error handling across the application.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates that a value is not null, throws ValidationException if it is.
    /// Used at method entry points to fail fast on invalid input.
    /// </summary>
    public static void NotNull(object? value, string fieldName)
    {
        if (value == null)
            throw new ValidationException(fieldName, $"{fieldName} cannot be null");
    }

    /// <summary>
    /// Validates that a string is not null or empty.
    /// </summary>
    public static void NotNullOrEmpty(string? value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ValidationException(fieldName, $"{fieldName} cannot be empty");
    }

    /// <summary>
    /// Validates that a number is within specified range (inclusive).
    /// </summary>
    public static void InRange(decimal value, decimal min, decimal max, string fieldName)
    {
        if (value < min || value > max)
            throw new ValidationException(fieldName, $"{fieldName} must be between {min} and {max}");
    }

    /// <summary>
    /// Validates that a number is within specified range (inclusive).
    /// </summary>
    public static void InRange(int value, int min, int max, string fieldName)
    {
        if (value < min || value > max)
            throw new ValidationException(fieldName, $"{fieldName} must be between {min} and {max}");
    }

    /// <summary>
    /// Validates that a string length is within specified bounds.
    /// </summary>
    public static void LengthBetween(string value, int minLength, int maxLength, string fieldName)
    {
        NotNullOrEmpty(value, fieldName);
        if (value.Length < minLength || value.Length > maxLength)
            throw new ValidationException(fieldName, $"{fieldName} must be between {minLength} and {maxLength} characters");
    }

    /// <summary>
    /// Validates that a string matches a regular expression pattern.
    /// Used for complex string validation like phone numbers, postal codes.
    /// </summary>
    public static void MatchesPattern(string value, string pattern, string fieldName, string? customMessage = null)
    {
        NotNullOrEmpty(value, fieldName);
        if (!Regex.IsMatch(value, pattern))
            throw new ValidationException(fieldName, customMessage ?? $"{fieldName} format is invalid");
    }

    /// <summary>
    /// Validates that an email address format is correct.
    /// </summary>
    public static void ValidEmail(string email, string fieldName = "Email")
    {
        NotNullOrEmpty(email, fieldName);
        if (!email.IsValidEmail())
            throw new ValidationException(fieldName, "Invalid email address format");
    }

    /// <summary>
    /// Validates that a phone number contains only digits (after removing common separators).
    /// </summary>
    public static void ValidPhoneNumber(string? phoneNumber, string fieldName = "PhoneNumber")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return; // Phone number is optional

        var digits = Regex.Replace(phoneNumber, @"[\D]", "");
        if (digits.Length < 10 || digits.Length > 15)
            throw new ValidationException(fieldName, "Phone number must be between 10 and 15 digits");
    }

    /// <summary>
    /// Validates that a collection contains at least one item.
    /// </summary>
    public static void NotEmpty<T>(IEnumerable<T>? collection, string fieldName)
    {
        if (collection == null || !collection.Any())
            throw new ValidationException(fieldName, $"{fieldName} cannot be empty");
    }

    /// <summary>
    /// Validates that a collection has at most maxItems items.
    /// Prevents DOS-style attacks with extremely large payloads.
    /// </summary>
    public static void MaxItems<T>(IEnumerable<T>? collection, int maxItems, string fieldName)
    {
        if (collection != null && collection.Count() > maxItems)
            throw new ValidationException(fieldName, $"{fieldName} cannot contain more than {maxItems} items");
    }

    /// <summary>
    /// Validates multiple fields and collects all errors for batch reporting.
    /// Returns collection of validation errors instead of throwing.
    /// </summary>
    public static Dictionary<string, string> ValidateAndCollectErrors(
        Func<Dictionary<string, string>> validationFunc)
    {
        var errors = new Dictionary<string, string>();
        try
        {
            validationFunc();
        }
        catch (ValidationException ex)
        {
            errors[ex.Field] = ex.Message;
        }
        return errors;
    }

    /// <summary>
    /// Validates that two values are equal (case-sensitive for strings).
    /// Used for password confirmation, matching fields.
    /// </summary>
    public static void Equal<T>(T value1, T value2, string fieldName) where T : notnull
    {
        if (!value1.Equals(value2))
            throw new ValidationException(fieldName, "Values do not match");
    }
}
