#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AspNetSpaTemplate.Exceptions
{
    /// <summary>
    /// Provides System.Text.Json serialization extensions for methods in <see cref="ConfigurationExceptionExtensions"/>.
    /// This class enables round-trip serialization of configuration exception data through JSON.
    /// </summary>
    public static class ConfigurationExceptionExtensionsJsonExtensions
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
        /// Converts a <see cref="ConfigurationExceptionExtensions"/> method result to its JSON representation.
        /// This serializes the configuration exception data that would be returned by extension methods.
        /// </summary>
        /// <param name="value">The configuration exception to serialize.</param>
        /// <param name="indented">Whether to format the JSON with indentation.</param>
        /// <returns>The JSON string representation of the configuration exception.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public static string ToJson(this ConfigurationException value, bool indented = false)
        {
            ArgumentNullException.ThrowIfNull(value);

            return JsonSerializer.Serialize(value, indented ? _jsonIndentedOptions : _jsonOptions);
        }

        /// <summary>
        /// Deserializes a <see cref="ConfigurationException"/> from JSON.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>The deserialized configuration exception, or null if the JSON is null or empty.</returns>
        /// <exception cref="JsonException">Thrown when the JSON is invalid.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="json"/> is null.</exception>
        public static ConfigurationException? FromJson(string json)
        {
            ArgumentNullException.ThrowIfNull(json);

            return JsonSerializer.Deserialize<ConfigurationException>(json, _jsonOptions);
        }

        /// <summary>
        /// Attempts to deserialize a <see cref="ConfigurationException"/> from JSON.
        /// </summary>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <param name="value">The deserialized configuration exception, or null if deserialization fails.</param>
        /// <returns>True if deserialization succeeded; otherwise, false.</returns>
        public static bool TryFromJson(string json, out ConfigurationException? value)
        {
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
}