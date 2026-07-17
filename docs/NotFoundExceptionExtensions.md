# NotFoundExceptionExtensions

Provides a set of static helper methods for creating, inspecting, and extracting data from `NotFoundException` instances. These extensions simplify working with resource‑not‑found scenarios by allowing concise construction of the exception, checking whether a caught exception corresponds to a specific resource, and retrieving the resource type or identifier embedded in the exception.

## API

### `public static NotFoundException ToNotFound(Exception innerException)`
Creates a `NotFoundException` that wraps the supplied `innerException`.  
- **Parameters**  
  - `innerException`: The exception that caused the not‑found condition.  
- **Return value**  
  - A new `NotFoundException` whose `InnerException` property is set to `innerException`.  
- **Exceptions**  
  - `ArgumentNullException` if `innerException` is `null`.

### `public static NotFoundException ToNotFound(string message)`
Creates a `NotFoundException` with the specified message.  
- **Parameters**  
  - `message`: The error message that describes the not‑found condition.  
- **Return value**  
  - A new `NotFoundException` initialized with `message`.  
- **Exceptions**  
  - `ArgumentNullException` if `message` is `null`.

### `public static NotFoundException ToNotFound(Type resourceType, object resourceId)`
Creates a `NotFoundException` that identifies the missing resource by its type and identifier.  
- **Parameters**  
  - `resourceType`: The CLR type of the resource that was not found.  
  - `resourceId`: The identifier of the resource that was not found.  
- **Return value**  
  - A new `NotFoundException` containing the supplied `resourceType` and `resourceId`.  
- **Exceptions**  
  - `ArgumentNullException` if `resourceType` is `null`.  
  - `ArgumentNullException` if `resourceId` is `null`.

### `public static NotFoundException ToNotFound(Type resourceType, object resourceId, string message)`
Creates a `NotFoundException` that identifies the missing resource and includes a custom message.  
- **Parameters**  
  - `resourceType`: The CLR type of the resource that was not found.  
  - `resourceId`: The identifier of the resource that was not found.  
  - `message`: A descriptive message for the exception.  
- **Return value**  
  - A new `NotFoundException` containing the resource information and `message`.  
- **Exceptions**  
  - `ArgumentNullException` if any of `resourceType`, `resourceId`, or `message` is `null`.

### `public static NotFoundException ToNotFound<T>(T resourceId)`
Creates a `NotFoundException` for a resource of type `T` using the supplied identifier.  
- **Type parameters**  
  - `T`: The type of the resource identifier.  
- **Parameters**  
  - `resourceId`: The identifier of the missing resource.  
- **Return value**  
  - A new `NotFoundException` where the resource type is inferred as `typeof(T)` and the identifier is `resourceId`.  
- **Exceptions**  
  - `ArgumentNullException` if `resourceId` is `null`.

### `public static bool IsNotFoundFor(Exception ex, Type resourceType)`
Determines whether the supplied exception is a `NotFoundException` that pertains to the given resource type.  
- **Parameters**  
  - `ex`: The exception to test.  
  - `resourceType`: The resource type to match against the exception’s data.  
- **Return value**  
  - `true` if `ex` is a `NotFoundException` whose embedded resource type equals `resourceType`; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if `ex` is `null`.  
  - `ArgumentNullException` if `resourceType` is `null`.

### `public static bool IsNotFoundFor(Exception ex, Type resourceType, object resourceId)`
Determines whether the supplied exception is a `NotFoundException` that pertains to the given resource type and identifier.  
- **Parameters**  
  - `ex`: The exception to test.  
  - `resourceType`: The expected resource type.  
  - `resourceId`: The expected resource identifier.  
- **Return value**  
  - `true` if `ex` is a `NotFoundException` whose resource type and identifier match the supplied values; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if any of `ex`, `resourceType`, or `resourceId` is `null`.

### `public static bool IsNotFoundFor<T>(Exception ex, T resourceId)`
Determines whether the supplied exception is a `NotFoundException` for a resource of type `T` with the given identifier.  
- **Type parameters**  
  - `T`: The type of the resource identifier.  
- **Parameters**  
  - `ex`: The exception to test.  
  - `resourceId`: The expected resource identifier.  
- **Return value**  
  - `true` if `ex` is a `NotFoundException` whose inferred resource type is `typeof(T)` and whose identifier equals `resourceId`; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if `ex` is `null`.  
  - `ArgumentNullException` if `resourceId` is `null`.

### `public static string GetResourceType(Exception ex)`
Extracts the resource type string from a `NotFoundException`.  
- **Parameters**  
  - `ex`: The exception from which to read the resource type.  
- **Return value**  
  - The resource type name if `ex` is a `NotFoundException`; otherwise `null`.  
- **Exceptions**  
  - `ArgumentNullException` if `ex` is `null`.

### `public static T GetResourceId<T>(Exception ex)`
Extracts the resource identifier of type `T` from a `NotFoundException`.  
- **Type parameters**  
  - `T`: The expected type of the resource identifier.  
- **Parameters**  
  - `ex`: The exception from which to read the identifier.  
- **Return value**  
  - The resource identifier cast to `T` if `ex` is a `NotFoundException` and the identifier is compatible with `T`; otherwise the default value for `T`.  
- **Exceptions**  
  - `ArgumentNullException` if `ex` is `null`.  
  - `InvalidCastException` if the identifier stored in the exception cannot be cast to `T`.

## Usage

```csharp
using AspNetSpaTemplate.Exceptions;

// Building a NotFoundException for a missing product with ID 42
var ex = NotFoundExceptionExtensions.ToNotFound(typeof(Product), 42);
throw ex;

// Checking whether a caught exception corresponds to a specific order
try
{
    // ... code that may throw NotFoundException ...
}
catch (Exception caught)
{
    if (NotFoundExceptionExtensions.IsNotFoundFor(caught, typeof(Order), orderId))
    {
        // Handle the missing order case
        Log.Warn($"Order {orderId} not found.");
    }
    else
    {
        // Rethrow or handle other exceptions
        throw;
    }
}
```

```csharp
using AspNetSpaTemplate.Exceptions;

// Generic helper to retrieve the ID of a missing entity
public TId GetMissingId<TId>(Exception ex)
{
    // Returns default(TId) if ex is not a NotFoundException or the ID cannot be cast
    return NotFoundExceptionExtensions.GetResourceId<TId>(ex);
}

// Example usage
var missingId = GetMissingId<int>(caughtException);
if (!EqualityComparer<int>.Default.Equals(missingId, 0))
{
    Console.WriteLine($"Missing entity ID: {missingId}");
}
```

## Notes

- All extension methods validate their arguments and throw `ArgumentNullException` for null inputs where a meaningful value is required.  
- The methods are pure and do not modify the supplied exception instance; they either create new instances or read existing data.  
- Because the methods rely only on the values of their parameters and do not access mutable static state, they are thread‑safe.  
- `GetResourceId<T>` returns the default value for `T` when the exception is not a `NotFoundException` or when the stored identifier cannot be cast to `T`; callers should verify the result against expected semantics if needed.  
- The generic `ToNotFound<T>` infers the resource type from `typeof(T)`. If a more specific type name is required, use the non‑generic overload that accepts a `System.Type` parameter.  
- These helpers are intended to work with the `NotFoundException` type defined in the same assembly; behavior with other exception types is undefined and will generally result in `null` or default return values.
