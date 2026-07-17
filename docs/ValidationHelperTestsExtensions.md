# ValidationHelperTestsExtensions

The `ValidationHelperTestsExtensions` class provides a set of static extension methods designed to simplify common validation checks in unit tests. Each method performs a single validation rule on its input and returns the input value if the check passes; otherwise it throws an appropriate exception (typically `ArgumentNullException`, `ArgumentException`, or `ArgumentOutOfRangeException`). These methods are intended to be used as fluent assertions or guard clauses within test code, reducing boilerplate and improving readability.

## API

### `NotNull<T>`
```csharp
public static T NotNull<T>(this T value)
```
- **Purpose**: Asserts that `value` is not `null`.
- **Parameters**: `value` – the object to validate.
- **Returns**: `value` if it is not `null`.
- **Throws**: `ArgumentNullException` if `value` is `null`.

### `NotNullOrEmpty`
```csharp
public static string NotNullOrEmpty(this string value)
```
- **Purpose**: Asserts that `value` is not `null` and not an empty string (`""`).
- **Parameters**: `value` – the string to validate.
- **Returns**: `value` if it is not `null` and not empty.
- **Throws**: `ArgumentNullException` if `value` is `null`; `ArgumentException` if `value` is empty.

### `InRange` (decimal)
```csharp
public static decimal InRange(this decimal value, decimal min, decimal max)
```
- **Purpose**: Asserts that `value` is within the inclusive range `[min, max]`.
- **Parameters**:
  - `value` – the decimal to validate.
  - `min` – the lower bound (inclusive).
  - `max` – the upper bound (inclusive).
- **Returns**: `value` if it is between `min` and `max` (inclusive).
- **Throws**: `ArgumentOutOfRangeException` if `value` is less than `min` or greater than `max`.

### `InRange` (int)
```csharp
public static int InRange(this int value, int min, int max)
```
- **Purpose**: Asserts that `value` is within the inclusive range `[min, max]`.
- **Parameters**:
  - `value` – the integer to validate.
  - `min` – the lower bound (inclusive).
  - `max` – the upper bound (inclusive).
- **Returns**: `value` if it is between `min` and `max` (inclusive).
- **Throws**: `ArgumentOutOfRangeException` if `value` is less than `min` or greater than `max`.

### `LengthBetween`
```csharp
public static string LengthBetween(this string value, int min, int max)
```
- **Purpose**: Asserts that the length of `value` is between `min` and `max` (inclusive).
- **Parameters**:
  - `value` – the string to validate.
  - `min` – the minimum allowed length (inclusive).
  - `max` – the maximum allowed length (inclusive).
- **Returns**: `value` if its length is within the specified range.
- **Throws**: `ArgumentNullException` if `value` is `null`; `ArgumentOutOfRangeException` if the length is less than `min` or greater than `max`.

### `MatchesPattern`
```csharp
public static string MatchesPattern(this string value, string pattern)
```
- **Purpose**: Asserts that `value` matches the specified regular expression `pattern`.
- **Parameters**:
  - `value` – the string to validate.
  - `pattern` – a regular expression pattern.
- **Returns**: `value` if it matches the pattern.
- **Throws**: `ArgumentNullException` if `value` or `pattern` is `null`; `ArgumentException` if `value` does not match the pattern.

### `ValidEmail`
```csharp
public static string ValidEmail(this string value)
```
- **Purpose**: Asserts that `value` is a valid email address according to a predefined regular expression.
- **Parameters**: `value` – the string to validate.
- **Returns**: `value` if it is a valid email.
- **Throws**: `ArgumentNullException` if `value` is `null`; `ArgumentException` if `value` is not a valid email.

### `ValidPhoneNumber`
```csharp
public static string ValidPhoneNumber(this string value)
```
- **Purpose**: Asserts that `value` is a valid phone number according to a predefined regular expression.
- **Parameters**: `value` – the string to validate.
- **Returns**: `value` if it is a valid phone number.
- **Throws**: `ArgumentNullException` if `value` is `null`; `ArgumentException` if `value` is not a valid phone number.

### `NotEmpty<T>`
```csharp
public static IEnumerable<T> NotEmpty<T>(this IEnumerable<T> value)
```
- **Purpose**: Asserts that the collection `value` is not `null` and contains at least one element.
- **Parameters**: `value` – the collection to validate.
- **Returns**: `value` if it is not `null` and not empty.
- **Throws**: `ArgumentNullException` if `value` is `null`; `ArgumentException` if the collection is empty.

### `MaxItems<T>`
```csharp
public static IEnumerable<T> MaxItems<T>(this IEnumerable<T> value, int max)
```
- **Purpose**: Asserts that the collection `value` contains at most `max` items.
- **Parameters**:
  - `value` – the collection to validate.
  - `max` – the maximum allowed number of items.
- **Returns**: `value` if its count is less than or equal to `max`.
- **Throws**: `ArgumentNullException` if `value` is `null`; `ArgumentOutOfRangeException` if the count exceeds `max`.

### `Equal<T>`
```csharp
public static T Equal<T>(this T value, T expected)
```
- **Purpose**: Asserts that `value` is equal to `expected` using the default equality comparer.
- **Parameters**:
  - `value` – the actual value.
  - `expected` – the expected value.
- **Returns**: `value` if it is equal to `expected`.
- **Throws**: `ArgumentException` if `value` is not equal to `expected`.

### `CountEquals<T>`
```csharp
public static IEnumerable<T> CountEquals<T>(this IEnumerable<T> value, int expectedCount)
```
- **Purpose**: Asserts that the collection `value` contains exactly `expectedCount` items.
- **Parameters**:
  - `value` – the collection to validate.
  - `expectedCount` – the exact number of items expected.
- **Returns**: `value` if its count equals `expectedCount`.
- **Throws**: `ArgumentNullException` if `value` is `null`; `ArgumentException` if the count does not equal `expectedCount`.

### `GreaterThan`
```csharp
public static decimal GreaterThan(this decimal value, decimal threshold)
```
- **Purpose**: Asserts that `value` is strictly greater than `threshold`.
- **Parameters**:
  - `value` – the decimal to validate.
  - `threshold` – the lower bound (exclusive).
- **Returns**: `value` if it is greater than `threshold`.
- **Throws**: `ArgumentOutOfRangeException` if `value` is less than or equal to `threshold`.

### `LessThan`
```csharp
public static decimal LessThan(this decimal value, decimal threshold)
```
- **Purpose**: Asserts that `value` is strictly less than `threshold`.
- **Parameters**:
  - `value` – the decimal to validate.
  - `threshold` – the upper bound (exclusive).
- **Returns**: `value` if it is less than `threshold`.
- **Throws**: `ArgumentOutOfRangeException` if `value` is greater than or equal to `threshold`.

## Usage

The following examples demonstrate typical usage of these extension methods in a unit test context (using xUnit).

### Example 1: Validating user input fields

```csharp
[Fact]
public void CreateUser_WithValidInput_ShouldSucceed()
{
    // Arrange
    var name = "John Doe".NotNullOrEmpty();
    var email = "john.doe@example.com".ValidEmail();
    var phone = "+1234567890".ValidPhoneNumber();

    // Act
    var user = new User(name, email, phone);

    // Assert
    Assert.NotNull(user);
}
```

### Example 2: Validating collection and numeric constraints

```csharp
[Fact]
public void Order_WithValidItems_ShouldBeProcessed()
{
    // Arrange
    var items = new List<OrderItem>
    {
        new OrderItem { Price = 9.99m },
        new OrderItem { Price = 19.99m }
    };

    // Validate collection is not empty and has exactly 2 items
    items.NotEmpty().CountEquals(2);

    // Validate each item's price is within range
    foreach (var item in items)
    {
        item.Price.InRange(0.01m, 1000.00m);
    }

    // Act
    var total = items.Sum(i => i.Price);

    // Assert
    total.GreaterThan(0).LessThan(2000);
}
```

## Notes

- All methods are designed for use in test code and throw exceptions on validation failure. They are not intended for production validation where more graceful error handling is required.
- When a method throws, the exception message typically includes the name of the validated parameter and a description of the failure. This aids in diagnosing test failures.
- **Edge cases**:
  - `NotNull<T>` works with nullable value types (e.g., `int?`) but will throw if the value is `null`.
  - `NotNullOrEmpty` treats whitespace-only strings as non-empty; use additional validation if whitespace should be rejected.
  - `InRange` and `LengthBetween` use inclusive bounds. Passing a value equal to `min` or `max` is considered valid.
  - `MatchesPattern` uses the .NET regular expression engine; the pattern is matched against the entire string (implicit `^` and `$` are not added automatically).
  - `ValidEmail` and `ValidPhoneNumber` rely on internal regex patterns that may not cover all international formats. Adjust patterns if broader validation is needed.
  - `NotEmpty<T>` and `CountEquals<T>` enumerate the collection to determine its count. Avoid passing infinite sequences.
- **Thread safety**: All methods are stateless and thread-safe. They do not modify any shared state and can be called concurrently from multiple threads without synchronization.
