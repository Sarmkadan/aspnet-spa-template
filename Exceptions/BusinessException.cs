// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Exception thrown when business logic constraints are violated.
/// </summary>
public class BusinessException : Exception
{
    public string? ErrorCode { get; }
    public int HttpStatusCode { get; set; } = 400;

    public BusinessException(string message) : base(message) { }

    public BusinessException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public BusinessException(string message, string errorCode, int httpStatusCode)
        : base(message)
    {
        ErrorCode = errorCode;
        HttpStatusCode = httpStatusCode;
    }

    public BusinessException(string message, Exception innerException)
        : base(message, innerException) { }
}
