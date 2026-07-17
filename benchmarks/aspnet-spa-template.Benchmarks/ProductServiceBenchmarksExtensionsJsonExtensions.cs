using AspNetSpaTemplate.Benchmarks;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Provides JSON serialization and deserialization extensions for the <see cref="ProductServiceBenchmarksExtensions"/> class.
/// </summary>
public static class ProductServiceBenchmarksExtensionsJsonExtensions
{
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>
    /// Serializes the <see cref="ProductServiceBenchmarksExtensions"/> instance to a JSON string.
    /// </summary>
    /// <param name="value">The <see cref="ProductServiceBenchmarksExtensions"/> instance to serialize.</param>
    /// <param name="indented">Whether to format the JSON string with indentation.</param>
    /// <returns>A JSON string representation of the <see cref="ProductServiceBenchmarksExtensions"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="value"/> is <see langword="null"/>.</exception>
    public static string ToJson(this ProductServiceBenchmarksExtensions value, bool indented = false)
    {
        ArgumentNullException.ThrowIfNull(value);
        var options = indented
            ? _jsonSerializerOptions with { WriteIndented = true }
            : _jsonSerializerOptions;
        return JsonSerializer.Serialize(value, options);
    }

    /// <summary>
    /// Deserializes a JSON string to a <see cref="ProductServiceBenchmarksExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>A <see cref="ProductServiceBenchmarksExtensions"/> instance, or <see langword="null"/> if the JSON string is invalid.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static ProductServiceBenchmarksExtensions? FromJson(string json)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            return JsonSerializer.Deserialize<ProductServiceBenchmarksExtensions>(json, _jsonSerializerOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to deserialize a JSON string to a <see cref="ProductServiceBenchmarksExtensions"/> instance.
    /// </summary>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <param name="value">The deserialized <see cref="ProductServiceBenchmarksExtensions"/> instance, or <see langword="null"/> if the JSON string is invalid.</param>
    /// <returns>True if the JSON string was successfully deserialized; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="json"/> is <see langword="null"/> or empty.</exception>
    public static bool TryFromJson(string json, out ProductServiceBenchmarksExtensions? value)
    {
        ArgumentException.ThrowIfNullOrEmpty(json);
        try
        {
            value = JsonSerializer.Deserialize<ProductServiceBenchmarksExtensions>(json, _jsonSerializerOptions);
            return true;
        }
        catch (JsonException)
        {
            value = null;
            return false;
        }
    }
}
