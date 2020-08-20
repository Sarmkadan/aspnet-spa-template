#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Exception thrown when business logic constraints are violated.
/// </summary>
public sealed class BusinessException : AspNetSpaTemplateException
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

    /// <summary>
    /// Attaches additional contextual data (e.g. the original exception) to this instance
    /// and returns it, enabling fluent construction like:
    /// <c>throw new BusinessException(...).WithData(ex);</c>
    /// </summary>
    /// <param name="data">The data to attach (commonly the original caught exception).</param>
    /// <returns>This <see cref="BusinessException"/> instance.</returns>
    public BusinessException WithData(object data)
    {
        Data["Context"] = data;
        return this;
    }
}
