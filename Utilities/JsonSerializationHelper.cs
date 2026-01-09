// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Helper class for consistent JSON serialization/deserialization.
/// Centralizes JSON configuration to ensure uniformity across the API.
/// </summary>
public static class JsonSerializationHelper
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = new List<JsonConverter>
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private static readonly JsonSerializerOptions PrettyOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = new List<JsonConverter>
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    /// <summary>
    /// Serializes object to JSON string using standard application settings.
    /// </summary>
    public static string Serialize<T>(T obj) where T : notnull
    {
        return JsonSerializer.Serialize(obj, DefaultOptions);
    }

    /// <summary>
    /// Serializes object to JSON string with pretty formatting (indented).
    /// Used for logging and debugging, not for API responses.
    /// </summary>
    public static string SerializePretty<T>(T obj) where T : notnull
    {
        return JsonSerializer.Serialize(obj, PrettyOptions);
    }

    /// <summary>
    /// Deserializes JSON string to typed object.
    /// Throws JsonException if format is invalid.
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Safely deserializes JSON, returning null on parse failure (no exception).
    /// Used when lenient parsing is acceptable.
    /// </summary>
    public static T? DeserializeSafe<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<T>(json, DefaultOptions);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Deserializes JSON from Stream (useful for request bodies).
    /// </summary>
    public static async Task<T?> DeserializeAsync<T>(Stream stream)
    {
        if (stream == null || stream.Length == 0)
            return default;

        try
        {
            return await JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to deserialize JSON from stream: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Converts object to another type by serializing and deserializing.
    /// Useful for deep cloning or type conversion maintaining structure.
    /// </summary>
    public static T? ConvertObject<T>(object source) where T : class
    {
        if (source == null)
            return null;

        var json = JsonSerializer.Serialize(source, DefaultOptions);
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }

    /// <summary>
    /// Parses JSON string to JsonElement for flexible querying without typed model.
    /// </summary>
    public static JsonElement? ParseJsonElement(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonDocument.Parse(json).RootElement;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Gets all JSON serializer options as configured for this application.
    /// Used when you need to pass options to custom JSON handlers.
    /// </summary>
    public static JsonSerializerOptions GetDefaultOptions() => DefaultOptions;

    /// <summary>
    /// Gets pretty formatting JSON serializer options.
    /// </summary>
    public static JsonSerializerOptions GetPrettyOptions() => PrettyOptions;

    /// <summary>
    /// Validates that a string contains valid JSON without deserializing to specific type.
    /// </summary>
    public static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
