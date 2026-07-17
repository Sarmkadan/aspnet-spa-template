using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides System.Text.Json serialization and deserialization extension methods for <see cref="AspNetSpaTemplateException"/>.
/// </summary>
public static class AspNetSpaTemplateExceptionJsonExtensionsJsonExtensions
{
	private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false,
		DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
		ReferenceHandler = ReferenceHandler.IgnoreCycles,
	};

	/// <summary>
	/// Serializes the <see cref="AspNetSpaTemplateException"/> to a JSON string.
	/// </summary>
	/// <param name="value">The exception to serialize.</param>
	/// <param name="indented">Whether to format the JSON with indentation for readability.</param>
	/// <returns>The JSON string representation of the exception.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
	public static string ToJson(this AspNetSpaTemplateException value, bool indented = false)
	{
		ArgumentNullException.ThrowIfNull(value);

		return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions) { WriteIndented = true } : _jsonOptions);
	}

	/// <summary>
	/// Deserializes a JSON string to an <see cref="AspNetSpaTemplateException"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <returns>
	/// The deserialized <see cref="AspNetSpaTemplateException"/> instance, or <c>null</c> if <paramref name="json"/> is <c>null</c>,
	/// empty, or consists only of whitespace.
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
	/// <exception cref="JsonException">Thrown when the JSON is syntactically invalid.</exception>
	public static AspNetSpaTemplateException? FromJson(string json)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			return null;
		}

		try
		{
			return JsonSerializer.Deserialize<AspNetSpaTemplateException>(json, _jsonOptions);
		}
		catch (JsonException)
		{
			return null;
		}
	}

	/// <summary>
	/// Attempts to deserialize a JSON string to an <see cref="AspNetSpaTemplateException"/> instance.
	/// </summary>
	/// <param name="json">The JSON string to deserialize.</param>
	/// <param name="value">
	/// When this method returns <c>true</c>, contains the deserialized <see cref="AspNetSpaTemplateException"/>;
	/// otherwise, <c>null</c>.
	/// </param>
	/// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
	public static bool TryFromJson(string json, out AspNetSpaTemplateException? value)
	{
		ArgumentNullException.ThrowIfNull(json);

		if (string.IsNullOrWhiteSpace(json))
		{
			value = null;
			return false;
		}

		try
		{
			value = JsonSerializer.Deserialize<AspNetSpaTemplateException>(json, _jsonOptions);
			return true;
		}
		catch (JsonException)
		{
			value = null;
			return false;
		}
	}
}