# NotFoundExceptionExtensionsValidation

`NotFoundExceptionExtensionsValidation` is a static utility class that provides validation methods for `NotFoundException` messages, resources, and related properties. It is designed to ensure that exception messages and resources conform to expected formats before they are used in application code, helping to prevent runtime failures and improve error handling reliability.

## API

### `ValidateMessage`

Validates the `Message` property of a `NotFoundException`.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – A list of validation errors. Empty if the message is valid.
- **Throws**: Does not throw exceptions; returns an empty list for valid messages.

### `IsMessageValid`

Checks whether the `Message` property of a `NotFoundException` is valid.

- **Parameters**: None
- **Return value**: `bool` – `true` if the message is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `EnsureMessageValid`

Validates the `Message` property and throws an `ArgumentException` if it is invalid.

- **Parameters**: None
- **Return value**: None
- **Throws**: `ArgumentException` if the message is invalid.

### `ValidateResource`

Validates the `Resource` property of a `NotFoundException`.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – A list of validation errors. Empty if the resource is valid.
- **Throws**: Does not throw exceptions; returns an empty list for valid resources.

### `IsResourceValid`

Checks whether the `Resource` property of a `NotFoundException` is valid.

- **Parameters**: None
- **Return value**: `bool` – `true` if the resource is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `EnsureResourceValid`

Validates the `Resource` property and throws an `ArgumentException` if it is invalid.

- **Parameters**: None
- **Return value**: None
- **Throws**: `ArgumentException` if the resource is invalid.

### `ValidateFormattedResource`

Validates the `FormattedResource` property of a `NotFoundException`.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – A list of validation errors. Empty if the formatted resource is valid.
- **Throws**: Does not throw exceptions; returns an empty list for valid formatted resources.

### `IsFormattedResourceValid`

Checks whether the `FormattedResource` property of a `NotFoundException` is valid.

- **Parameters**: None
- **Return value**: `bool` – `true` if the formatted resource is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `EnsureFormattedResourceValid`

Validates the `FormattedResource` property and throws an `ArgumentException` if it is invalid.

- **Parameters**: None
- **Return value**: None
- **Throws**: `ArgumentException` if the formatted resource is invalid.

### `ValidateWithInner`

Validates the `WithInner` property of a `NotFoundException`.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – A list of validation errors. Empty if the inner exception flag is valid.
- **Throws**: Does not throw exceptions; returns an empty list for valid values.

### `IsWithInnerValid`

Checks whether the `WithInner` property of a `NotFoundException` is valid.

- **Parameters**: None
- **Return value**: `bool` – `true` if the inner exception flag is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `EnsureWithInnerValid`

Validates the `WithInner` property and throws an `ArgumentException` if it is invalid.

- **Parameters**: None
- **Return value**: None
- **Throws**: `ArgumentException` if the inner exception flag is invalid.

### `ValidateGeneric`

Validates the `Generic` property of a `NotFoundException`.

- **Parameters**: None
- **Return value**: `IReadOnlyList<string>` – A list of validation errors. Empty if the generic flag is valid.
- **Throws**: Does not throw exceptions; returns an empty list for valid values.

### `IsGenericValid`

Checks whether the `Generic` property of a `NotFoundException` is valid.

- **Parameters**: None
- **Return value**: `bool` – `true` if the generic flag is valid; otherwise, `false`.
- **Throws**: Does not throw exceptions.

### `EnsureGenericValid`

Validates the `Generic` property and throws an `ArgumentException` if it is invalid.

- **Parameters**: None
- **Return value**: None
- **Throws**: `ArgumentException` if the generic flag is invalid.

## Usage

```csharp
// Example 1: Validating a NotFoundException before throwing
var exception = new NotFoundException("User not found");
if (!NotFoundExceptionExtensionsValidation.IsMessageValid(exception))
{
    throw new ArgumentException("Invalid exception message.");
}

// Example 2: Ensuring a resource is valid before use
var resourceException = new NotFoundException("Product not found", "ProductService");
NotFoundExceptionExtensionsValidation.EnsureResourceValid(resourceException);
```

## Notes

- All validation methods are thread-safe and stateless. They operate solely on the provided `NotFoundException` instance and do not maintain any internal state.
- The validation logic assumes that `NotFoundException` properties are immutable after construction. If properties are modified post-construction, validation results may become stale.
- The `Ensure*` methods will throw `ArgumentException` with a message describing the first validation failure encountered. The exact message format is implementation-defined.
- Empty or `null` values for `Message`, `Resource`, or `FormattedResource` are considered invalid unless explicitly allowed by the underlying `NotFoundException` constructor.
