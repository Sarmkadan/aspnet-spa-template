#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Globalization;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides extension methods for <see cref="BusinessException"/> to enhance error handling scenarios.
/// </summary>
public static class BusinessExceptionExtensions
{
    /// <summary>
    /// Creates a new <see cref="BusinessException"/> with the same error code and HTTP status code as the original,
    /// but with an updated error message that includes the original message as context.
    /// Useful for wrapping exceptions with additional context while preserving error codes.
    /// </summary>
    /// <param name="exception">The original business exception.</param>
    /// <param name="additionalContext">Additional context to include in the error message.</param>
    /// <returns>A new <see cref="BusinessException"/> instance with combined context.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static BusinessException WithContext(
        this BusinessException exception,
        string additionalContext)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(additionalContext);

        var newMessage = $"{additionalContext}: {exception.Message}";
        var errorCode = exception.ErrorCode;
        var httpStatusCode = exception.HttpStatusCode;

        var result = new BusinessException(newMessage, errorCode, httpStatusCode);
        return result.WithData(exception);
    }

    /// <summary>
    /// Creates a new <see cref="BusinessException"/> that represents a validation error with a standardized error code format.
    /// Useful for converting validation failures into business exceptions with consistent error codes.
    /// </summary>
    /// <param name="exception">The original business exception.</param>
    /// <param name="validationErrorCode">The validation error code prefix (e.g., "VALIDATION_").</param>
    /// <param name="fieldName">The name of the field that failed validation.</param>
    /// <param name="errorMessage">The specific validation error message.</param>
    /// <returns>A new <see cref="BusinessException"/> instance representing the validation error.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    public static BusinessException ToValidationError(
        this BusinessException exception,
        string validationErrorCode,
        string fieldName,
        string errorMessage)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(validationErrorCode);
        ArgumentException.ThrowIfNullOrEmpty(fieldName);
        ArgumentException.ThrowIfNullOrEmpty(errorMessage);

        var fullErrorCode = $"{validationErrorCode}{fieldName.ToUpperInvariant()}";
        var combinedMessage = $"Validation failed for '{fieldName}': {errorMessage}";

        return new BusinessException(combinedMessage, fullErrorCode, exception.HttpStatusCode)
            .WithData(exception);
    }

    /// <summary>
    /// Determines whether this exception represents a specific error code.
    /// Useful for conditional error handling based on error codes.
    /// </summary>
    /// <param name="exception">The business exception to check.</param>
    /// <param name="errorCode">The error code to match against.</param>
    /// <returns>True if the exception has the specified error code; otherwise, false.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="exception"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorCode"/> is null or empty.</exception>
    public static bool HasErrorCode(
        this BusinessException exception,
        string errorCode)
    {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrEmpty(errorCode);

        return string.Equals(
            exception.ErrorCode,
            errorCode,
            StringComparison.OrdinalIgnoreCase);
    }
}