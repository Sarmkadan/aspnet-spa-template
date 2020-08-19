#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Exceptions;

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