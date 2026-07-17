# ErrorResponse

The `ErrorResponse` class provides a standardized structure for representing error responses in the ASP.NET SPA template. It is typically returned by API endpoints when an operation fails, carrying a human-readable message, an optional error code, a dictionary of field-level validation errors, a trace identifier for correlation, an HTTP status code, and a timestamp. This ensures consistent error reporting across the application and simplifies client-side error handling.

## API

### Properties

- **`Message`** (`string`)  
  A human-readable description of the error. This property is always set to a non-null value after construction.

- **`ErrorCode`** (`string?`)  
  An optional application-specific error code (e.g., `"VALIDATION_ERROR"`, `"NOT_FOUND"`). May be `null` if no code is applicable.

- **`Errors`** (`Dictionary<string, List<string>>?`)  
  An optional dictionary mapping field names to lists of validation error messages. Useful for reporting model validation failures. Can be `null` when no field-level errors exist.

- **`TraceId`** (`string?`)  
  An optional trace identifier (e.g., from `System.Diagnostics.Activity.Current?.Id`) used to correlate the error with server-side logs. May be `null`.

- **`StatusCode`** (`int`)  
  The HTTP status code associated with the error (e.g., 400, 404, 500). This value is typically set during construction and should match the response status code.

- **`Timestamp`** (`DateTime`)  
  The UTC date and time when the error response was created. Defaults to `DateTime.UtcNow` if not explicitly set.

### Constructors

- **`ErrorResponse()`**  
  Initializes a new instance of `ErrorResponse` with default values. `Message` is set to an empty string, `StatusCode` to 0, and `Timestamp` to `DateTime.UtcNow`. All other properties are `null`.

- **`ErrorResponse(...)`** (multiple overloads)  
  Additional overloaded constructors that accept various combinations of the above properties, allowing the caller to set initial values conveniently. The exact parameter signatures are not enumerated here; refer to the source code for details.

## Usage

### Example 1: Creating an error response with validation errors

```csharp
var errors = new Dictionary<string, List<string>>
{
    ["Email"] = new List<string> { "The Email field is required." },
    ["Password"] = new List<string> { "Password must be at least 8 characters." }
};

var response = new ErrorResponse
{
    Message = "One or more validation errors occurred.",
    ErrorCode = "VALIDATION_ERROR",
    Errors = errors,
    StatusCode = 400,
    TraceId = Activity.Current?.Id,
    Timestamp = DateTime.UtcNow
};
```

### Example 2: Returning an error response from an API controller

```csharp
[HttpGet("{id}")]
public IActionResult GetProduct(int id)
{
    var product = _repository.Find(id);
    if (product == null)
    {
        var error = new ErrorResponse
        {
            Message = $"Product with ID {id} not found.",
            ErrorCode = "NOT_FOUND",
            StatusCode = 404,
            TraceId = HttpContext.TraceIdentifier
        };
        return NotFound(error);
    }
    return Ok(product);
}
```

## Notes

- **Nullability**: All reference-type properties (`ErrorCode`, `Errors`, `TraceId`) are nullable. Consumers should check for `null` before accessing their values, especially `Errors` which may be `null` even when `Message` is set.
- **Empty dictionary**: If `Errors` is set to an empty dictionary, it is semantically equivalent to `null` – no field-level errors are reported. The choice between `null` and an empty dictionary depends on serialization preferences.
- **Thread safety**: Instances of `ErrorResponse` are not thread-safe. Properties are mutable and intended to be set during construction or shortly thereafter. Once the response is returned from an API endpoint, it should not be modified concurrently.
- **Timestamp**: The `Timestamp` property is set to `DateTime.UtcNow` by default. If the system clock is adjusted or the instance is created in a non-UTC context, the value may not represent true UTC. It is recommended to always set it explicitly or rely on the default only when the server clock is accurate.
