using System;
using System.Text.Json;

namespace AspNetSpaTemplate.Benchmarks;

/// <summary>
/// Provides JSON serialization helpers for the <see cref="ProductServiceBenchmarks"/> class.
/// </summary>
public static class ProductServiceBenchmarksJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Converts the <see cref="ProductServiceBenchmarks"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ProductServiceBenchmarks"/> instance to convert.</param>
    /// <param name="indented">Whether to format the JSON string with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="ProductServiceBenchmarks"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is null.</exception>
    public static string ToJson(this ProductServiceBenchmarks value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = indented ? _jsonSerializerOptions with { WriteIndented = true } : _jsonSerializerOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ProductServiceBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="ProductServiceBenchmarks"/> instance, or null if the JSON string is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static ProductServiceBenchmarks? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            return JsonSerializer.Deserialize<ProductServiceBenchmarks>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ProductServiceBenchmarks"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="ProductServiceBenchmarks"/> instance, or null if the JSON string is invalid.</param>
    /// <returns>True if the JSON string was successfully deserialized; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is null or empty.</exception>
    public static bool TryFromJson(string json, out ProductServiceBenchmarks? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<ProductServiceBenchmarks>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}