#nullable enable
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ErrorMappingHelper"/> class.
/// Tests exception mapping to HTTP status codes, error codes, user messages, retry info, log levels, error details, and error classification.
/// </summary>
public sealed class ErrorMappingHelperUnitTests
{
    #region MapToStatusCode Tests

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.MapToStatusCode(Exception)"/> returns 400 for ValidationException.
    /// </summary>
    [Fact]
    public void MapToStatusCode_WithValidationException_ReturnsBadRequest()
    {
        // Arrange
        var exception = new ValidationException("Test validation error");

        // Act
        var statusCode = ErrorMappingHelper.MapToStatusCode(exception);

        // Assert
        statusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.MapToStatusCode(Exception)"/> returns 404 for NotFoundException.
    /// </>
    [Fact]
    public void MapToStatusCode_WithNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var exception = new NotFoundException("Resource not found");

        // Act
        var statusCode = ErrorMappingHelper.MapToStatusCode(exception);

        // Assert
        statusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.MapToStatusCode(Exception)"/> returns 500 for unknown exceptions.
    /// </summary>
    [Fact]
    public void MapToStatusCode_WithUnknownException_ReturnsInternalServerError()
    {
        // Arrange
        var exception = new Exception("Unknown error");

        // Act
        var statusCode = ErrorMappingHelper.MapToStatusCode(exception);

        // Assert
        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    #endregion

    #region MapToErrorCode Tests

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.MapToErrorCode(Exception)"/> returns correct error code for BusinessException.
    /// </summary>
    [Fact]
    public void MapToErrorCode_WithBusinessException_ReturnsBusinessError()
    {
        // Arrange
        var exception = new BusinessException("Business rule violated");

        // Act
        var errorCode = ErrorMappingHelper.MapToErrorCode(exception);

        // Assert
        errorCode.Should().Be("BUSINESS_ERROR");
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.MapToErrorCode(Exception)"/> returns correct error code for TimeoutException.
    /// </summary>
    [Fact]
    public void MapToErrorCode_WithTimeoutException_ReturnsRequestTimeout()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");

        // Act
        var errorCode = ErrorMappingHelper.MapToErrorCode(exception);

        // Assert
        errorCode.Should().Be("REQUEST_TIMEOUT");
    }

    #endregion

    #region MapToUserMessage Tests

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.MapToUserMessage(Exception)"/> returns the original message for ValidationException.
    /// </summary>
    [Fact]
    public void MapToUserMessage_WithValidationException_ReturnsOriginalMessage()
    {
        // Arrange
        var customMessage = "Field X is required";
        var exception = new ValidationException(customMessage);

        // Act
        var userMessage = ErrorMappingHelper.MapToUserMessage(exception);

        // Assert
        userMessage.Should().Be(customMessage);
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.MapToUserMessage(Exception)"/> returns default message for NotFoundException.
    /// </summary>
    [Fact]
    public void MapToUserMessage_WithNotFoundException_ReturnsDefaultMessage()
    {
        // Arrange
        var exception = new NotFoundException("Specific not found message");

        // Act
        var userMessage = ErrorMappingHelper.MapToUserMessage(exception);

        // Assert
        userMessage.Should().Be("The requested resource was not found");
    }

    #endregion

    #region GetRetryInfo Tests

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.GetRetryInfo(Exception, int)"/> returns retryable=true for 504 Gateway Timeout.
    /// </summary>
    [Fact]
    public void GetRetryInfo_WithTimeoutException_ReturnsRetryableWithBackoff()
    {
        // Arrange
        var exception = new TimeoutException("Request timeout");
        int attempt = 1;

        // Act
        var (retryable, retryAfter) = ErrorMappingHelper.GetRetryInfo(exception, attempt);

        // Assert
        retryable.Should().BeTrue();
        retryAfter.Should().Be(2); // 2^1 = 2 seconds
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.GetRetryInfo(Exception, int)"/> returns retryable=false for ValidationException (4xx).
    /// </summary>
    [Fact]
    public void GetRetryInfo_WithValidationException_ReturnsNotRetryable()
    {
        // Arrange
        var exception = new ValidationException("Invalid input");
        int attempt = 1;

        // Act
        var (retryable, retryAfter) = ErrorMappingHelper.GetRetryInfo(exception, attempt);

        // Assert
        retryable.Should().BeFalse();
        retryAfter.Should().BeNull();
    }

    #endregion

    #region GetLogLevel Tests

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.GetLogLevel(Exception)"/> returns Warning for ValidationException.
    /// </summary>
    [Fact]
    public void GetLogLevel_WithValidationException_ReturnsWarning()
    {
        // Arrange
        var exception = new ValidationException("Validation failed");

        // Act
        var logLevel = ErrorMappingHelper.GetLogLevel(exception);

        // Assert
        logLevel.Should().Be(Microsoft.Extensions.Logging.LogLevel.Warning);
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.GetLogLevel(Exception)"/> returns Error for unknown exceptions.
    /// </summary>
    [Fact]
    public void GetLogLevel_WithUnknownException_ReturnsError()
    {
        // Arrange
        var exception = new Exception("Unexpected error");

        // Act
        var logLevel = ErrorMappingHelper.GetLogLevel(exception);

        // Assert
        logLevel.Should().Be(Microsoft.Extensions.Logging.LogLevel.Error);
    }

    #endregion

    #region ExtractErrorDetails Tests

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.ExtractErrorDetails(Exception)"/> populates basic fields correctly.
    /// </summary>
    [Fact]
    public void ExtractErrorDetails_WithSimpleException_PopulatesBasicFields()
    {
        // Arrange
        var exception = new InvalidOperationException("Test operation failed");

        // Act
        var details = ErrorMappingHelper.ExtractErrorDetails(exception);

        // Assert
        details.ExceptionType.Should().Be("InvalidOperationException");
        details.Message.Should().Be("Test operation failed");
        details.Timestamp.Should().BeCloseTo(DateTime.UtcNow, precision: TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.ExtractErrorDetails(Exception)"/> includes inner exception details.
    /// </summary>
    [Fact]
    public void ExtractErrorDetails_WithInnerException_PopulatesInnerFields()
    {
        // Arrange
        var innerException = new ArgumentNullException("param", "Parameter cannot be null");
        var exception = new InvalidOperationException("Operation failed", innerException);

        // Act
        var details = ErrorMappingHelper.ExtractErrorDetails(exception);

        // Assert
        details.InnerException.Should().Be("ArgumentNullException");
        details.InnerMessage.Should().Contain("Parameter cannot be null");
    }

    #endregion

    #region IsTransientError Tests

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.IsTransientError(Exception)"/> returns true for TimeoutException.
    /// </summary>
    [Fact]
    public void IsTransientError_WithTimeoutException_ReturnsTrue()
    {
        // Arrange
        var exception = new TimeoutException("Operation timed out");

        // Act
        var isTransient = ErrorMappingHelper.IsTransientError(exception);

        // Assert
        isTransient.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.IsTransientError(Exception)"/> returns false for ValidationException.
    /// </summary>
    [Fact]
    public void IsTransientError_WithValidationException_ReturnsFalse()
    {
        // Arrange
        var exception = new ValidationException("Validation failed");

        // Act
        var isTransient = ErrorMappingHelper.IsTransientError(exception);

        // Assert
        isTransient.Should().BeFalse();
    }

    #endregion

    #region IsCriticalError Tests

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.IsCriticalError(Exception)"/> returns true for OutOfMemoryException.
    /// </summary>
    [Fact]
    public void IsCriticalError_WithOutOfMemoryException_ReturnsTrue()
    {
        // Arrange
        var exception = new OutOfMemoryException();

        // Act
        var isCritical = ErrorMappingHelper.IsCriticalError(exception);

        // Assert
        isCritical.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.IsCriticalError(Exception)"/> returns false for regular exceptions.
    /// </summary>
    [Fact]
    public void IsCriticalError_WithRegularException_ReturnsFalse()
    {
        // Arrange
        var exception = new Exception("Regular error");

        // Act
        var isCritical = ErrorMappingHelper.IsCriticalError(exception);

        // Assert
        isCritical.Should().BeFalse();
    }

    #endregion

    #region Edge Cases and Null Handling

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.MapToStatusCode(Exception)"/> handles null exception gracefully.
    /// </summary>
    [Fact]
    public void MapToStatusCode_WithNullException_ReturnsInternalServerError()
    {
        // Arrange
        Exception exception = null!;

        // Act
        var statusCode = ErrorMappingHelper.MapToStatusCode(exception);

        // Assert
        statusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    /// <summary>
    /// Tests that <see cref="ErrorMappingHelper.MapToErrorCode(Exception)"/> handles null exception gracefully.
    /// </summary>
    [Fact]
    public void MapToErrorCode_WithNullException_ReturnsInternalServerError()
    {
        // Arrange
        Exception exception = null!;

        // Act
        var errorCode = ErrorMappingHelper.MapToErrorCode(exception);

        // Assert
        errorCode.Should().Be("INTERNAL_SERVER_ERROR");
    }

    #endregion
}