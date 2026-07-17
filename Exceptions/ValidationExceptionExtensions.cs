#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Generic;
using System.Linq;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="ValidationException"/> to simplify common validation scenarios.
/// </summary>
public static class ValidationExceptionExtensions
{
    /// <summary>
    /// Creates a new <see cref="ValidationException"/> with a single error message for a field.
    /// </summary>
    /// <param name="fieldName">The name of the field that failed validation.</param>
    /// <param name="errorMessage">The error message describing the validation failure.</param>
    /// <returns>A new <see cref="ValidationException"/> instance.</returns>
    /// <exception cref="ArgumentException"><paramref name="fieldName"/> is null or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is null or empty.</exception>
    public static ValidationException WithError(
        this string fieldName,
        string errorMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        return new ValidationException(fieldName, errorMessage);
    }

    /// <summary>
    /// Adds an error to the existing <see cref="ValidationException"/> for the specified field.
    /// </summary>
    /// <param name="exception">The <see cref="ValidationException"/> to add the error to.</param>
    /// <param name="fieldName">The name of the field that failed validation.</param>
    /// <param name="errorMessage">The error message describing the validation failure.</param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="fieldName"/> is null or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="errorMessage"/> is null or empty.</exception>
    public static void AddError(
        this ValidationException exception,
        string fieldName,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        exception.AddError(fieldName, errorMessage);
    }

    /// <summary>
    /// Determines whether the validation exception contains any errors for the specified field.
    /// </summary>
    /// <param name="exception">The <see cref="ValidationException"/> to check.</param>
    /// <param name="fieldName">The name of the field to check for errors.</param>
    /// <returns><see langword="true"/> if the field has validation errors; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="fieldName"/> is null or empty.</exception>
    public static bool HasErrorFor(
        this ValidationException exception,
        string fieldName)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(fieldName);

        return exception.Errors.TryGetValue(fieldName, out var errors) && errors.Count > 0;
    }

    /// <summary>
    /// Gets the error messages for the specified field as a concatenated string.
    /// </summary>
    /// <param name="exception">The <see cref="ValidationException"/> containing the errors.</param>
    /// <param name="fieldName">The name of the field to get error messages for.</param>
    /// <param name="separator">The separator to use between error messages. Defaults to <c>"; </c>.</param>
    /// <returns>A string containing all error messages for the field, separated by the specified separator.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="fieldName"/> is null or empty.</exception>
    /// <exception cref="ArgumentException"><paramref name="separator"/> is null or empty.</exception>
    public static string GetErrorMessages(
        this ValidationException exception,
        string fieldName,
        string separator = "; ")
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        ArgumentException.ThrowIfNullOrEmpty(separator);

        return exception.Errors.TryGetValue(fieldName, out var errors) && errors.Count > 0
            ? string.Join(separator, errors)
            : string.Empty;
    }

    /// <summary>
    /// Gets all error messages from the validation exception as a flattened dictionary.
    /// </summary>
    /// <param name="exception">The <see cref="ValidationException"/> containing the errors.</param>
    /// <returns>A dictionary mapping field names to their error messages.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static IReadOnlyDictionary<string, IReadOnlyList<string>> GetAllErrors(
        this ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception.Errors.ToDictionary(
            kvp => kvp.Key,
            kvp => (IReadOnlyList<string>)kvp.Value.AsReadOnly());
    }

    /// <summary>
    /// Determines whether the validation exception contains any errors.
    /// </summary>
    /// <param name="exception">The <see cref="ValidationException"/> to check.</param>
    /// <returns><see langword="true"/> if the exception contains any validation errors; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static bool HasErrors(this ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return exception.Errors.Count > 0;
    }

    /// <summary>
    /// Merges another validation exception's errors into this one.
    /// </summary>
    /// <param name="exception">The <see cref="ValidationException"/> to merge errors into.</param>
    /// <param name="other">The <see cref="ValidationException"/> whose errors should be merged.</param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="other"/> is null.</exception>
    public static void MergeErrors(
        this ValidationException exception,
        ValidationException other)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentNullException.ThrowIfNull(other);

        foreach (var kvp in other.Errors)
        {
            if (!exception.Errors.TryGetValue(kvp.Key, out var existingErrors))
            {
                exception.Errors[kvp.Key] = new List<string>(kvp.Value);
            }
            else
            {
                foreach (var error in kvp.Value)
                {
                    existingErrors.Add(error);
                }
            }
        }
    }
}