# ValidationExceptionJsonExtensionsJsonExtensions

Provides serialization and deserialization extensions for the `ValidationException` type, enabling conversion between `ValidationException` instances and their JSON string representations. This utility is primarily used for error payload transmission in API responses, particularly in scenarios where validation failures need to be serialized for client consumption or persisted for debugging.

## API

### `public static string ToJson(this ValidationException exception)`
Converts a `ValidationException` instance into its JSON string representation.

**Parameters:**
- `exception` (`ValidationException`): The exception instance to serialize. Must not be `null`.

**Returns:**
- (`string`): A JSON string containing the exception's `Message` and `Errors` properties.

**Throws:**
- `ArgumentNullException`: If `exception` is `null`.

---

### `public static ValidationException? FromJson(string json)`
Deserializes a JSON string into a `ValidationException` instance.

**Parameters:**
- `json` (`string`): The JSON string to deserialize. Must not be `null` or empty.

**Returns:**
- (`ValidationException?`): A `ValidationException` instance if deserialization succeeds; otherwise, `null`.

**Throws:**
- `ArgumentNullException`: If `json` is `null` or whitespace.
- `JsonException`: If the JSON is malformed or does not conform to the expected schema.

---

### `public static bool TryFromJson(string json, out ValidationException? exception)`
Attempts to deserialize a JSON string into a `ValidationException` instance without throwing exceptions on failure.

**Parameters:**
- `json` (`string`): The JSON string to deserialize. Must not be `null` or empty.
- `exception` (`out ValidationException?`): Output parameter containing the deserialized `ValidationException` if successful; otherwise, `null`.

**Returns:**
- (`bool`): `true` if deserialization succeeds; otherwise, `false`.

**Remarks:**
- Does not throw exceptions. Returns `false` for invalid JSON or schema mismatches.

---

### `public string? Message { get; }`
Gets the error message associated with the validation failure.

**Returns:**
- (`string?`): The exception message, or `null` if not set.

---

### `public Dictionary<string, List<string>>? Errors { get; }`
Gets a dictionary mapping field names to lists of validation errors for each field.

**Returns:**
- (`Dictionary<string, List<string>>?`): A dictionary of validation errors, or `null` if no errors are present.

---

### `public ValidationException ToException()`
Creates a new `ValidationException` instance from the current object's `Message` and `Errors` properties.

**Returns:**
- (`ValidationException`): A new `ValidationException` instance.

**Remarks:**
- Useful for rehydrating exceptions from deserialized data.

## Usage

### Example 1: Serializing a ValidationException for API Responses
