# IdempotencyResponse

The `IdempotencyResponse` class provides a standardized structure for representing idempotency key information in API responses. It is typically returned by endpoints that support idempotency, containing the generated idempotency key, creation timestamp, and a descriptive message.

## API

### Properties

- **`IdempotencyKey`** (`string`)  
  The unique idempotency key generated for the request. This property is initialized to an empty string and should be set to a non-empty value when a key is generated.

- **`CreatedAt`** (`DateTime`)  
  The date and time when the idempotency key was created. This property has no default value and should be explicitly set (typically to `DateTime.UtcNow`) when generating the response.

- **`Message`** (`string`)  
  A human-readable description of the idempotency response. Defaults to "Idempotency key generated".

### Constructors

- **`IdempotencyResponse()`**  
  Initializes a new instance of `IdempotencyResponse` with default values. `IdempotencyKey` is set to an empty string, `CreatedAt` to `DateTime.MinValue`, and `Message` to "Idempotency key generated".

## Usage

### Example 1: Creating an idempotency response

```csharp
var response = new IdempotencyResponse
{
    IdempotencyKey = Guid.NewGuid().ToString(),
    CreatedAt = DateTime.UtcNow,
    Message = "Idempotency key generated for request"
};
```

### Example 2: Returning an idempotency response from an API controller

```csharp
[HttpPost]
public IActionResult CreateResource([FromBody] CreateResourceRequest request)
{
    // Check if idempotency key exists in header
    if (Request.Headers.TryGetValue("Idempotency-Key", out var key))
    {
        // Process request with idempotency key
        var idempotencyKey = key.FirstOrDefault();
        
        // ... processing logic ...
        
        var response = new IdempotencyResponse
        {
            IdempotencyKey = idempotencyKey,
            CreatedAt = DateTime.UtcNow,
            Message = "Idempotency key processed"
        };
        
        return Ok(response);
    }
    
    // If no idempotency key, process normally
    // ...
    
    return Ok();
}
```

## Notes

- **Default values**: The `IdempotencyKey` and `Message` properties have default values assigned via property initializers, while `CreatedAt` must be explicitly set to avoid the default `DateTime.MinValue` value.
- **Immutability consideration**: While the class properties are mutable via setters, idempotency responses are typically treated as immutable after creation in API contexts.
- **Serialization**: When serialized to JSON, the property names will match the C# property names (PascalCase) unless configured otherwise in JSON serialization settings.