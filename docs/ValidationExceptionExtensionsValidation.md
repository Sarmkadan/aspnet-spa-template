# ValidationExceptionExtensionsValidation

The `ValidationExceptionExtensionsValidation` class provides a centralized set of static utility methods for validating parameters and `ValidationException` instances within the ASP.NET SPA template architecture. It offers a consistent pattern for checking validity, retrieving detailed error messages, and enforcing validity constraints by throwing exceptions when necessary, ensuring robust error handling and input verification across the application.

## API

### ValidateParameters
```csharp
public static IReadOnlyList<string> ValidateParameters(...)
```
Analyzes the provided arguments to determine if they meet the required validation criteria for parameter sets. This method returns a read-only list of strings containing specific error messages describing each validation failure. If the parameters are valid, the returned list is empty. This method does not throw exceptions; it strictly reports validation state.

### AreParametersValid
```csharp
public static bool AreParametersValid(...)
```
Performs a boolean check to determine if the provided arguments satisfy all validation requirements. This method returns `true` if the parameters are valid and `false` if any validation rules are violated. It is optimized for scenarios where only the validity status is required without the overhead of generating error message strings.

### EnsureParametersValid
```csharp
public static void EnsureParametersValid(...)
```
Enforces validation rules on the provided arguments. If the parameters are valid, the method completes silently. If any validation failures are detected, this method throws a `ValidationException` containing the aggregated error details. This method is intended for use at the entry point of public APIs to guard against invalid input.

### ValidateException
```csharp
public static IReadOnlyList<string> ValidateException(ValidationException exception)
```
Inspects a specific `ValidationException` instance to verify its internal consistency and content integrity. It returns a read-only list of strings detailing any structural issues found within the exception object itself. If the exception instance is well-formed, the returned list is empty. This method does not throw exceptions.

### IsExceptionValid
```csharp
public static bool IsExceptionValid(ValidationException exception)
```
Evaluates whether the provided `ValidationException` instance is structurally valid. Returns `true` if the exception contains valid data and conforms to expected formats, or `false` if the exception object itself is malformed or incomplete.

### EnsureExceptionValid
```csharp
public static void EnsureExceptionValid(ValidationException exception)
```
Validates the structural integrity of the provided `ValidationException`. If the exception is valid, the method returns normally. If the exception instance is found to be invalid or corrupted, this method throws a new `ValidationException` indicating the structural failure. This ensures that only well-formed exception objects are propagated or logged.

## Usage

### Example 1: Guarding a Service Method
Use `EnsureParametersValid` at the start of a service method to automatically reject invalid inputs with a standardized exception.

```csharp
public void RegisterUser(string username, string email)
{
    // Throws ValidationException if username or email format is incorrect
    ValidationExceptionExtensionsValidation.EnsureParametersValid(username, email);

    // Proceed with business logic only if validation passes
    _userRepository.Create(new User { Username = username, Email = email });
}
```

### Example 2: Conditional Logic Based on Validity
Use `AreParametersValid` or `ValidateParameters` when custom error handling or alternative flows are required instead of immediate exception throwing.

```csharp
public IActionResult SubmitOrder(OrderDto order)
{
    var errors = ValidationExceptionExtensionsValidation.ValidateParameters(order);
    
    if (errors.Count > 0)
    {
        // Return a 400 Bad Request with specific error details
        return BadRequest(new { Errors = errors });
    }

    // Process the order
    _orderService.Process(order);
    return Ok();
}
```

## Notes

*   **Return Value Mutability**: The methods returning `IReadOnlyList<string>` provide a snapshot of validation errors. The returned list is read-only and should not be modified by the caller.
*   **Exception Safety**: `ValidateParameters` and `ValidateException` are safe to call in any context as they never throw; they strictly return data. Conversely, `EnsureParametersValid` and `EnsureExceptionValid` are designed specifically to interrupt flow via exceptions when validation fails.
*   **Thread Safety**: As this class consists entirely of static methods operating on provided arguments without maintaining internal mutable state, it is thread-safe and can be used concurrently across multiple requests.
*   **Null Handling**: While specific null behavior depends on the underlying validation rules implemented, passing `null` where an object is expected will typically result in a validation failure reported in the error list or triggered as an exception in `Ensure` methods.
