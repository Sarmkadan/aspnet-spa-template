#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System;
using System.Collections.Generic;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides validation helpers for parameters used with <see cref="ValidationExceptionExtensions"/> extension methods.
/// </summary>
public static class ValidationExceptionExtensionsValidation
{
    /// <summary>
    /// Validates field name and error message parameters for use with <see cref="ValidationExceptionExtensions"/> methods.
    /// </summary>
    /// <param name="fieldName">The field name to validate.</param>
    /// <param name="errorMessage">The error message to validate.</param>
    /// <returns>A list of validation problems; empty if both parameters are valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fieldName"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="fieldName"/> is empty or whitespace.</exception>
    public static IReadOnlyList<string> ValidateParameters(string fieldName, string errorMessage)
    {
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        return Array.Empty<string>();
    }

    /// <summary>
    /// Determines whether field name and error message parameters are valid for use with <see cref="ValidationExceptionExtensions"/> methods.
    /// </summary>
    /// <param name="fieldName">The field name to check.</param>
    /// <param name="errorMessage">The error message to check.</param>
    /// <returns>True if both parameters are valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="fieldName"/> is null.</exception>
    /// <exception cref="ArgumentException"><paramref name="fieldName"/> is empty or whitespace.</exception>
    public static bool AreParametersValid(string fieldName, string errorMessage)
    {
        try
        {
            ValidateParameters(fieldName, errorMessage);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Ensures that field name and error message parameters are valid for use with <see cref="ValidationExceptionExtensions"/> methods,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="fieldName">The field name to validate.</param>
    /// <param name="errorMessage">The error message to validate.</param>
    /// <exception cref="ArgumentException">Thrown when parameters are not valid.</exception>
    public static void EnsureParametersValid(string fieldName, string errorMessage)
    {
        try
        {
            ValidateParameters(fieldName, errorMessage);
        }
        catch (Exception ex)
        {
            throw new ArgumentException(
                "Field name and error message parameters are not valid for ValidationExceptionExtensions methods.",
                nameof(fieldName),
                ex);
        }
    }

    /// <summary>
    /// Validates an exception instance for use with <see cref="ValidationExceptionExtensions"/> methods.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <returns>A list of validation problems; empty if the exception is valid.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static IReadOnlyList<string> ValidateException(ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var problems = new List<string>();

        if (exception.Errors is null)
        {
            problems.Add("Exception.Errors dictionary cannot be null.");
        }
        else if (exception.Errors.Count == 0)
        {
            problems.Add("Exception.Errors dictionary cannot be empty when using ValidationExceptionExtensions methods.");
        }

        return problems.AsReadOnly();
    }

    /// <summary>
    /// Determines whether an exception instance is valid for use with <see cref="ValidationExceptionExtensions"/> methods.
    /// </summary>
    /// <param name="exception">The exception to check.</param>
    /// <returns>True if the exception is valid; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    public static bool IsExceptionValid(ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);
        return ValidateException(exception).Count == 0;
    }

    /// <summary>
    /// Ensures that an exception instance is valid for use with <see cref="ValidationExceptionExtensions"/> methods,
    /// throwing an <see cref="ArgumentException"/> if not.
    /// </summary>
    /// <param name="exception">The exception to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the exception is not valid.</exception>
    public static void EnsureExceptionValid(ValidationException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var problems = ValidateException(exception);
        if (problems.Count > 0)
        {
            throw new ArgumentException(
                $"ValidationException is not valid for ValidationExceptionExtensions methods. Problems:\n- {
string.Join("\n- ", problems)}");
        }
    }
}