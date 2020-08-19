#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Base exception for all application-specific exceptions in the AspNetSpaTemplate project.
/// All custom exceptions should inherit from this class.
/// </summary>
public abstract class AspNetSpaTemplateException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetSpaTemplateException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    protected AspNetSpaTemplateException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="AspNetSpaTemplateException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    protected AspNetSpaTemplateException(string message, Exception innerException) : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when there are configuration-related issues.
/// </summary>
public sealed class ConfigurationException : AspNetSpaTemplateException
{
    /// <summary>
    /// The configuration key that caused the issue.
    /// </summary>
    public string? ConfigurationKey { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public ConfigurationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="configurationKey">The configuration key that caused the issue.</param>
    /// <param name="message">The exception message.</param>
    public ConfigurationException(string configurationKey, string message) : base(message)
    {
        ConfigurationKey = configurationKey;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationException"/> class.
    /// </summary>
    /// <param name="configurationKey">The configuration key that caused the issue.</param>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConfigurationException(string configurationKey, string message, Exception innerException)
        : base(message, innerException)
    {
        ConfigurationKey = configurationKey;
    }
}