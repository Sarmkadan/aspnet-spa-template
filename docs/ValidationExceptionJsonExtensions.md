# ValidationExceptionJsonExtensions

Provides JSON serialization and deserialization helpers for `ValidationException` to facilitate consistent error handling and transmission between layers in ASP.NET applications.

## API

### `ToJson`
Serializes a `ValidationException` into a JSON string representation. The resulting JSON includes the exception message and a structured dictionary of validation errors.

- **Parameters**
  - `exception` (`ValidationException`): The exception to serialize. Must not be `null`.
- **Return value**
  - `string`: A JSON string containing the serialized exception.
- **Exceptions**
  - Throws `ArgumentNullException` if `exception` is `null`.

### `FromJson`
Deserializes a JSON string back into a `ValidationException` instance. The JSON must conform to the structure produced by `ToJson`.

- **Parameters**
  - `json` (`string`): The JSON string to deserialize. Must not be `null`.
- **Return value**
  - `ValidationException?`: The deserialized exception, or `null` if the JSON is invalid or empty.
- **Exceptions**
  - Throws `JsonException` if the JSON is malformed or does not match the expected schema.

### `TryFromJson`
Attempts to deserialize a JSON string into a `ValidationException` without throwing exceptions. Returns a boolean indicating success and outputs the result via an `out` parameter.

- **Parameters**
  - `json` (`string`): The JSON string to attempt to deserialize. Must not be `null`.
  - `result` (`out ValidationException?`): Receives the deserialized exception if successful.
- **Return value**
  - `bool`: `true` if deserialization succeeded; otherwise, `false`.
- **Exceptions**
  - None.

### `Message`
Gets the error message associated with the exception. This is a property of the `ValidationException` type.

- **Return value**
  - `string?`: The error message, or `null` if not set.

### `Errors`
Gets a dictionary of validation errors where keys are field names and values are lists of error messages. This is a property of the `ValidationException` type.

- **Return value**
  - `Dictionary<string, List<string>>?`: The validation errors, or `null` if no errors are present.

### `ToException`
Converts the current instance into a `ValidationException`. This is a method of the `ValidationException` type.

- **Return value**
  - `ValidationException`: A new exception instance with the same message and errors.

## Usage

```csharp
// Example 1: Serializing a ValidationException to JSON
var validationException = new ValidationException(
    "Invalid order data",
    new Dictionary<string, List<string>>
    {
        { "Quantity", new List<string> { "Quantity must be greater than zero." } },
        { "ProductId", new List<string> { "ProductId is required." } }
    });

string json = ValidationExceptionJsonExtensions.ToJson(validationException);

// Example 2: Deserializing JSON back to a ValidationException
string jsonPayload = """
    {
        "Message": "Invalid order data",
        "Errors": {
            "Quantity": ["Quantity must be greater than zero."],
            "ProductId": ["ProductId is required."]
        }
    }
    """;

if (ValidationExceptionJsonExtensions.TryFromJson(jsonPayload, out var deserializedException))
{
    // Use deserializedException
}
```

## Notes

- The `ToJson` method requires a non-null `ValidationException`; passing `null` will throw an `ArgumentNullException`.
- The `FromJson` method may throw `JsonException` if the input JSON is malformed or does not conform to the expected schema.
- The `TryFromJson` method is the preferred way to deserialize when the validity of the input JSON is uncertain, as it avoids exceptions for invalid input.
- This type is stateless and thread-safe; its methods do not maintain any shared mutable state.
