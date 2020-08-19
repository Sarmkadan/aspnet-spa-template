#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Exception thrown when external API calls fail.
/// </summary>
public sealed class ExternalApiException : AspNetSpaTemplateException
{
	/// <summary>
	/// The API endpoint that failed.
	/// </summary>
	public string? Endpoint { get; }

	/// <summary>
	/// The HTTP status code returned by the API.
	/// </summary>
	public int? StatusCode { get; }

	/// <summary>
	/// The HTTP method used for the request.
	/// </summary>
	public string? Method { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="ExternalApiException"/> class.
	/// </summary>
	/// <param name="message">The exception message.</param>
	public ExternalApiException(string message) : base(message) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="ExternalApiException"/> class.
	/// </summary>
	/// <param name="endpoint">The API endpoint that failed.</param>
	/// <param name="message">The exception message.</param>
	public ExternalApiException(string endpoint, string message)
		: base($"API call to {endpoint} failed: {message}")
	{
		Endpoint = endpoint;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ExternalApiException"/> class.
	/// </summary>
	/// <param name="endpoint">The API endpoint that failed.</param>
	/// <param name="method">The HTTP method used.</param>
	/// <param name="statusCode">The HTTP status code.</param>
	/// <param name="message">The exception message.</param>
	public ExternalApiException(string endpoint, string method, int statusCode, string message)
		: base($"API call to {endpoint} ({method}) failed with status {statusCode}: {message}")
	{
		Endpoint = endpoint;
		Method = method;
		StatusCode = statusCode;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ExternalApiException"/> class.
	/// </summary>
	/// <param name="endpoint">The API endpoint that failed.</param>
	/// <param name="innerException">The inner exception.</param>
	public ExternalApiException(string endpoint, Exception innerException)
		: base($"API call to {endpoint} failed", innerException)
	{
		Endpoint = endpoint;
	}

	/// <summary>
	/// Creates an ExternalApiException with additional context.
	/// </summary>
	public ExternalApiException WithContext(string key, object value)
	{
		Data[key] = value;
		return this;
	}
}