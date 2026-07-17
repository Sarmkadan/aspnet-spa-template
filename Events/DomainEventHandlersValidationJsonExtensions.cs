#nullable enable

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetSpaTemplate.Events;

/// <summary>
/// Provides System.Text.Json serialization extensions for <see cref="DomainEventHandlers"/> related to validation.
/// Enables round-trip serialization of domain event handlers validation state for testing and debugging.
/// </summary>
public static class DomainEventHandlersValidationJsonExtensions
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
    /// Serializes the <see cref="DomainEventHandlers"/> validation state to a JSON string.
    /// </summary>
    /// <param name="value">The domain event handlers to serialize.</param>
    /// <param name="indented">Whether to format the JSON with indentation for readability.</param>
    /// <returns>A JSON string representation of the validation state.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
    public static string ToJson(this DomainEventHandlers value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);

        var options = indented ? _jsonIndentedOptions : _jsonOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a <see cref="DomainEventHandlers"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="DomainEventHandlers"/> instance, or null if the JSON is null or empty.</returns>
    /// <exception cref="JsonException">Thrown when the JSON is invalid or cannot be deserialized.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
    public static DomainEventHandlers? FromJson(string json)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonSerializer.Deserialize<DomainEventHandlers>(json, _jsonOptions);
    }

    /// <summary>
    /// Attempts to deserialize a <see cref="DomainEventHandlers"/> instance from a JSON string.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">
    /// When this method returns, contains the deserialized <see cref="DomainEventHandlers"/> instance if deserialization succeeds; otherwise, <see langword="null"/>.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if deserialization succeeds; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is <see langword="null"/>.</exception>
    public static bool TryFromJson(string json, out DomainEventHandlers? value)
    {
        ArgumentNullException.ThrowIfNull(json);

        if (string.IsNullOrWhiteSpace(json))
        {
            value = null;
            return true;
        }

        try
        {
            value = JsonSerializer.Deserialize<DomainEventHandlers>(json, _jsonOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}