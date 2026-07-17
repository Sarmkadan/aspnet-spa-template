#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="ConfigurationException"/>.
/// </summary>
public static sealed class ConfigurationExceptionJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly JsonSerializerOptions _jsonIndentedOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts a <see cref="ConfigurationException"/> to its JSON representation.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>The JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is <c>null</c>.</exception>
    public static string ToJson(this ConfigurationException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        return JsonSerializer.Serialize(value, indented ? _jsonIndentedOptions : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a <see cref="ConfigurationException"/> from JSON.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>
    /// The deserialized <see cref="ConfigurationException"/> instance, or <c>null</c> if <paramref name="json"/>
    /// is <c>null</c>, empty, or consists only of whitespace.
    /// </returns>
    /// <exception cref="JsonException">Thrown when the JSON is syntactically invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static ConfigurationException? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        // Treat empty or whitespace‑only JSON as “no data”.
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<ConfigurationException>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="ConfigurationException"/> from JSON.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns <c>true</c>, contains the deserialized <see cref="ConfigurationException"/>;
    /// otherwise, <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if deserialization succeeded; otherwise, <c>false</c>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <c>null</c>.</exception>
    public static bool TryFromJson(string json, out ConfigurationException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        // Empty or whitespace JSON cannot be deserialized into a valid exception.
        if (string.IsNullOrWhiteSpace(json))
        {
            value = null;
            return false;
        }

        try
        {
            value = JsonSerializer.Deserialize<ConfigurationException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
