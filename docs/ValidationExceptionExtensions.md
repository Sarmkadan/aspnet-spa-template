# ValidationExceptionExtensions

The `ValidationExceptionExtensions` class provides a set of static helper methods designed to simplify the creation, inspection, and manipulation of `ValidationException` instances within the ASP.NET SPA Template. These extensions facilitate the aggregation of validation errors from multiple sources, the retrieval of formatted error messages, and the programmatic verification of specific field failures, ensuring consistent error handling patterns across the application.

## API

### `WithError`
Creates a new instance of `ValidationException` populated with a specific field error.
*   **Parameters**: Takes the field name (`string`) and the error message (`string`).
*   **Returns**: A new `ValidationException` instance containing the specified error.
*   **Throws**: Does not throw; returns a constructed exception object ready to be thrown or inspected.

### `AddError`
Appends a single validation error to an existing `ValidationException` instance.
*   **Parameters**: The target `ValidationException` instance, the field name (`string`), and the error message (`string`).
*   **Returns**: `void`.
*   **Throws**: Throws `ArgumentNullException` if the exception instance is null.

### `HasErrorFor`
Determines whether a specific field contains any validation errors within the exception.
*   **Parameters**: The `ValidationException` instance and the field name (`string`) to check.
*   **Returns**: `true` if the field has one or more associated errors; otherwise, `false`.
*   **Throws**: Throws `ArgumentNullException` if the exception instance is null.

### `GetErrorMessages`
Retrieves a concatenated string of all error messages contained within the exception.
*   **Parameters**: The `ValidationException` instance.
*   **Returns**: A `string` containing all error messages, typically separated by delimiters. If no errors exist, returns an empty string.
*   **Throws**: Throws `ArgumentNullException` if the exception instance is null.

### `GetAllErrors`
Extracts the complete dictionary of validation errors from the exception.
*   **Parameters**: The `ValidationException` instance.
*   **Returns**: An `IReadOnlyDictionary<string, IReadOnlyList<string>>` where keys are field names and values are lists of error messages for that field.
*   **Throws**: Throws `ArgumentNullException` if the exception instance is null.

### `HasErrors`
Checks if the exception contains any validation errors at all.
*   **Parameters**: The `ValidationException` instance.
*   **Returns**: `true` if the internal error collection is not empty; otherwise, `false`.
*   **Throws**: Throws `ArgumentNullException` if the exception instance is null.

### `MergeErrors`
Combines errors from a source dictionary into the target `ValidationException`.
*   **Parameters**: The target `ValidationException` instance and a source `IReadOnlyDictionary<string, IReadOnlyList<string>>` containing errors to merge.
*   **Returns**: `void`. Existing errors for a field are preserved, and new errors are appended.
*   **Throws**: Throws `ArgumentNullException` if the exception instance or the source dictionary is null.

## Usage

### Creating and Populating an Exception
This example demonstrates how to instantiate a `ValidationException` using the fluent `WithError` method and subsequently add additional errors using `AddError` before throwing it.

```csharp
try 
{
    if (string.IsNullOrEmpty(model.Email))
    {
        var ex = ValidationException.WithError("Email", "Email address is required.");
        ex.AddError("Password", "Password must be at least 8 characters.");
        
        throw ex;
    }
}
catch (ValidationException ex)
{
    if (ex.HasErrorFor("Email"))
    {
        // Handle specific field logic
        Logger.LogWarning(ex.GetErrorMessages());
    }
    throw;
}
```

### Merging Errors from Multiple Sources
In scenarios where validation occurs across different layers or services, `MergeErrors` allows aggregating all issues into a single exception response.

```csharp
var validationEx = new ValidationException();
var externalErrors = new Dictionary<string, IReadOnlyList<string>>
{
    { "PhoneNumber", new List<string> { "Invalid format", "Country code missing" } }
};

// Merge external validation results into the main exception
validationEx.MergeErrors(externalErrors);
validationEx.AddError("Username", "Username already taken");

if (validationEx.HasErrors())
{
    var allErrors = validationEx.GetAllErrors();
    // Return aggregated errors to the client
    return BadRequest(allErrors);
}
```

## Notes

*   **Null Safety**: All instance extension methods (`AddError`, `HasErrorFor`, `GetErrorMessages`, `GetAllErrors`, `HasErrors`, `MergeErrors`) strictly validate that the `ValidationException` instance passed as the first argument is not null. Attempting to call these on a null reference will result in an `ArgumentNullException`.
*   **Immutability of Return Values**: The `GetAllErrors` method returns an `IReadOnlyDictionary`. While the dictionary interface itself prevents modification via the returned reference, the underlying lists within the `ValidationException` may still be mutable if accessed via other means. Callers should treat the returned data as a snapshot.
*   **Thread Safety**: This class consists entirely of static methods operating on provided instances. It does not maintain any internal static state. However, the `ValidationException` instances themselves are not inherently thread-safe for concurrent modification. If multiple threads attempt to call `AddError` or `MergeErrors` on the *same* exception instance simultaneously, external synchronization is required to prevent race conditions.
*   **Error Aggregation**: The `MergeErrors` method appends errors to existing fields rather than overwriting them. If a field already contains errors in the target exception, new errors from the source dictionary are added to the list for that field.
