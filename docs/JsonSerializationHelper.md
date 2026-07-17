# JsonSerializationHelper

Provides static helper methods for JSON (de)serialization using `System.Text.Json`. The class centralizes common serialization scenarios, offering both synchronous and asynchronous APIs, pretty‑printed output, and safe deserialization that avoids throwing exceptions.

## API

### `public static string Serialize<T>(T value)`
- **Purpose**: Serializes an instance of type `T` to a JSON string using the default serializer options.
- **Parameters**: 
  - `value`: The object to serialize. May be `null`; serializing `null` yields the JSON literal `null`.
- **Return value**: A JSON‑encoded string representing `value`. Returns `null` if `value` is `null` and the serializer is configured to output `null` for null values (the default behavior).
- **Exceptions**: 
  - `ArgumentException` if `T` is not supported by the serializer (e.g., certain pointer types). 
  - `InvalidOperationException` if a circular reference is encountered and the default options do not preserve references.
  - `NotSupportedException` for unsupported types such as `DynamicObject` without a custom converter.

### `public static string SerializePretty<T>(T value)`
- **Purpose**: Serializes an instance of type `T` to a formatted (indented) JSON string.
- **Parameters**: 
  - `value`: The object to serialize. May be `null`.
- **Return value**: A pretty‑printed JSON string. Formatting follows the options returned by `GetPrettyOptions`.
- **Exceptions**: Same as `Serialize<T>`.

### `public static T? Deserialize<T>(string json)`
- **Purpose**: Deserializes a JSON string into an instance of type `T`.
- **Parameters**: 
  - `json`: The JSON input. Must be valid JSON; passing `null` results in an `ArgumentNullException`.
- **Return value**: An object of type `T` populated from `json`, or `default(T?)` if the JSON represents `null` and `T` is a reference type or nullable value type.
- **Exceptions**: 
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if the JSON is malformed or cannot be mapped to `T`.

### `public static T? DeserializeSafe<T>(string json)`
- **Purpose**: Attempts to deserialize a JSON string into `T` without throwing exceptions on failure.
- **Parameters**: 
  - `json`: The JSON input. May be `null` or invalid.
- **Return value**: The deserialized object if successful; otherwise `default(T?)`.
- **Exceptions**: None. All errors are caught internally and result in the default return value.

### `public static async Task<T?> DeserializeAsync<T>(Stream utf8Json)`
- **Purpose**: Asynchronously deserializes UTF‑8 encoded JSON from a stream into an instance of type `T`.
- **Parameters**: 
  - `utf8Json`: A readable stream containing JSON data. Must not be `null`.
- **Return value**: A `Task` that completes with the deserialized object, or `default(T?)` if the stream represents `null` JSON and `T` permits null.
- **Exceptions**: 
  - `ArgumentNullException` if `utf8Json` is `null`.
  - `JsonException` if the stream contains invalid JSON or cannot be deserialized to `T`.
  - `ObjectDisposedException` if the stream is closed before reading completes.

### `public static T? ConvertObject<T>(object input)`
- **Purpose**: Converts an arbitrary object to type `T` by serializing it to JSON and then deserializing that JSON into `T`. Useful for deep copying or type conversion when direct casting is not possible.
- **Parameters**: 
  - `input`: The source object. May be `null`.
- **Return value**: An instance of `T` populated from the JSON representation of `input`, or `default(T?)` if `input` is `null` and `T` allows null.
- **Exceptions**: 
  - `ArgumentNullException` if `input` is `null` and `T` does not accept null (the method will still return `default(T?)` but may throw during serialization depending on serializer configuration).
  - `JsonException` if serialization or deserialization fails.

### `public static JsonElement? ParseJsonElement(string json)`
- **Purpose**: Parses a JSON string into a `JsonElement` without binding to a specific CLR type.
- **Parameters**: 
  - `json`: The JSON input. Must be valid JSON; passing `null` results in an `ArgumentNullException`.
- **Return value**: A `JsonElement` representing the JSON payload, or `null` if `json` is `null`.
- **Exceptions**: 
  - `ArgumentNullException` if `json` is `null`.
  - `JsonException` if the JSON is malformed.

### `public static JsonSerializerOptions GetDefaultOptions()`
- **Purpose**: Returns a `JsonSerializerOptions` instance configured with the library’s default settings (camelCase property names, ignoring null values, etc.).
- **Parameters**: None.
- **Return value**: A new `JsonSerializerOptions` object. The options are immutable after creation, making them safe for concurrent use.
- **Exceptions**: None.

### `public static JsonSerializerOptions GetPrettyOptions()`
- **Purpose**: Returns a `JsonSerializerOptions` instance identical to the defaults but with `WriteIndented = true` for pretty‑printed output.
- **Parameters**: None.
- **Return value**: A new `JsonSerializerOptions` object with indentation enabled.
- **Exceptions**: None.

### `public static bool IsValidJson(string json)`
- **Purpose**: Determines whether a string contains valid JSON.
- **Parameters**: 
  - `json`: The string to test. May be `null`.
- **Return value**: `true` if `json` is parsable as JSON; otherwise `false`.
- **Exceptions**: None. Invalid input simply yields `false`.

## Usage

### Example 1: Serializing and deserializing a model
```csharp
public class WeatherForecast
{
    public DateTime Date { get; set; }
    public int TemperatureC { get; set; }
    public string? Summary { get; set; }
}

// Serialize an instance
var forecast = new WeatherForecast
{
    Date = DateTime.UtcNow,
    TemperatureC = 25,
    Summary = "Warm"
};
string json = JsonSerializationHelper.Serialize(forecast);
// json => {"date":"2025-11-02T12:34:56.789Z","temperatureC":25,"summary":"Warm"}

// Deserialize back to the object
WeatherForecast? parsed = JsonSerializationHelper.Deserialize<WeatherForecast>(json);
if (parsed is not null)
{
    Console.WriteLine($"{parsed.Date}: {parsed.TemperatureC}°C ({parsed.Summary})");
}
```

### Example 2: Safe deserialization of user‑provided input
```csharp
string? userInput = GetJsonFromRequest(); // may be null or malformed

// Attempt to deserialize without risking exceptions
var result = JsonSerializationHelper.DeserializeSafe<MyDto>(userInput);

if (result is null)
{
    // Handle invalid or missing payload
    Log.Warning("Received invalid JSON payload.");
    return BadRequest("Invalid JSON.");
}

// Proceed with the validated object
ProcessDto(result);
```

## Notes

- All methods are **static** and contain no mutable state; they are safe to call from multiple threads concurrently.
- The `GetDefaultOptions` and `GetPrettyOptions` methods return newly created `JsonSerializerOptions` instances each call. Because options are immutable after construction, sharing them across calls does not introduce thread‑safety concerns, but callers may cache the returned options if they wish to avoid allocation overhead.
- `Serialize`/`SerializePretty` and the deserializing methods rely on the default `System.Text.Json` behavior: property names are converted to camelCase, null values are ignored, and unsupported types trigger exceptions. Custom converters or altered policies must be applied manually if different behavior is required.
- `DeserializeSafe` catches **any** exception thrown during deserialization and returns the default value for `T`. This includes `JsonException`, `ArgumentNullException`, and unexpected errors such as `OutOfMemoryException`; therefore, it should be used only when silencing all failure modes is acceptable.
- `ConvertObject<T>` performs a full serialization‑deserialization round‑trip. Consequently, it is more expensive than a direct cast and may lose type‑specific information (e.g., object references, subclass details) if the default serializer configuration does not preserve them.
- `IsValidJson` merely attempts to parse the string with the default options; it does not validate any schema or enforce additional constraints. An empty string (`""`) is considered invalid JSON. 

---
