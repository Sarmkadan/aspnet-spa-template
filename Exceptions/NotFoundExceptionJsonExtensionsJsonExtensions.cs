#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="NotFoundException"/>.
/// </summary>
public static class NotFoundExceptionJsonExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Converts a <see cref="NotFoundException"/> to its JSON representation.
    /// </summary>
    /// <param name="value">The exception to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation.</param>
    /// <returns>The JSON string representation of the exception.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this NotFoundException value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        return JsonSerializer.Serialize(value, indented ? new JsonSerializerOptions(_jsonOptions)
        {
            WriteIndented = true
        } : _jsonOptions);
    }

    /// <summary>
    /// Deserializes a <see cref="NotFoundException"/> from JSON.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>The deserialized exception, or <see langword="null"/> if the JSON is <see langword="null"/>.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid and cannot be deserialized.</exception>
    public static NotFoundException? FromJson(string? json)
    {
        if (json is null)
        {
            return null;
        }

        return JsonSerializer.Deserialize<NotFoundException>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="NotFoundException"/> from JSON.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">Receives the deserialized exception if successful.</param>
    /// <returns><see langword="true"/> if deserialization succeeded; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out NotFoundException? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        try
        {
            value = JsonSerializer.Deserialize<NotFoundException>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}