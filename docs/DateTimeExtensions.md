# DateTimeExtensions

A set of utility extension methods for the `DateTime` and `DateTimeOffset` types, providing common date manipulation and formatting operations. These extensions simplify recurring tasks such as determining the start or end of a day, week, or month, calculating age, converting to ISO 8601 format, and evaluating temporal relationships.

## API

### `StartOfDay(DateTime date)`
Returns a new `DateTime` representing the start of the specified day (i.e., midnight with zero milliseconds).
- **Parameters**: `date` – The source date.
- **Returns**: A `DateTime` at 00:00:00.000 on the same day.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid range.

### `EndOfDay(DateTime date)`
Returns a new `DateTime` representing the end of the specified day (i.e., 23:59:59.999).
- **Parameters**: `date` – The source date.
- **Returns**: A `DateTime` at 23:59:59.999 on the same day.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid range.

### `StartOfWeek(DateTime date)`
Returns a new `DateTime` representing the start of the week containing the specified date (i.e., midnight on the first day of the week, where the first day is Monday).
- **Parameters**: `date` – The source date.
- **Returns**: A `DateTime` at 00:00:00.000 on the Monday of the current week.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid range.

### `EndOfWeek(DateTime date)`
Returns a new `DateTime` representing the end of the week containing the specified date (i.e., 23:59:59.999 on the last day of the week, where the last day is Sunday).
- **Parameters**: `date` – The source date.
- **Returns**: A `DateTime` at 23:59:59.999 on the Sunday of the current week.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid range.

### `StartOfMonth(DateTime date)`
Returns a new `DateTime` representing the start of the month containing the specified date (i.e., midnight on the first day of the month).
- **Parameters**: `date` – The source date.
- **Returns**: A `DateTime` at 00:00:00.000 on the first day of the month.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid range.

### `EndOfMonth(DateTime date)`
Returns a new `DateTime` representing the end of the month containing the specified date (i.e., 23:59:59.999 on the last day of the month).
- **Parameters**: `date` – The source date.
- **Returns**: A `DateTime` at 23:59:59.999 on the last day of the month.
- **Throws**: `ArgumentOutOfRangeException` if the resulting date is outside the valid range.

### `GetAge(DateTime birthDate)`
Calculates the age in years from the given birth date to the current date.
- **Parameters**: `birthDate` – The date of birth.
- **Returns**: The age in whole years.
- **Throws**: `ArgumentOutOfRangeException` if `birthDate` is in the future.

### `ToIso8601(DateTime date)`
Formats the date as an ISO 8601 string in the UTC timezone (e.g., `2024-05-20T14:30:00Z`).
- **Parameters**: `date` – The source date.
- **Returns**: An ISO 8601 formatted string in UTC.
- **Throws**: `ArgumentOutOfRangeException` if the resulting string cannot be produced.

### `ToRelativeTime(DateTime date)`
Converts the date into a human-readable relative time string (e.g., "2 hours ago", "in 3 days").
- **Parameters**: `date` – The source date.
- **Returns**: A localized relative time string.
- **Throws**: `ArgumentOutOfRangeException` if the resulting string cannot be produced.

### `IsBusinessHours(DateTime date)`
Determines whether the specified date falls within standard business hours (09:00 to 17:00, Monday through Friday).
- **Parameters**: `date` – The source date.
- **Returns**: `true` if the date is within business hours; otherwise, `false`.
- **Throws**: `ArgumentOutOfRangeException` if the resulting calculation is invalid.

### `IsPast(DateTime date)`
Determines whether the specified date is in the past relative to the current date and time.
- **Parameters**: `date` – The source date.
- **Returns**: `true` if the date is in the past; otherwise, `false`.

### `IsFuture(DateTime date)`
Determines whether the specified date is in the future relative to the current date and time.
- **Parameters**: `date` – The source date.
- **Returns**: `true` if the date is in the future; otherwise, `false`.

## Usage

```csharp
// Example 1: Calculate age and check if a date is in the past
var birthDate = new DateTime(1990, 5, 20);
var age = DateTimeExtensions.GetAge(birthDate);
var isPast = DateTimeExtensions.IsPast(birthDate);

Console.WriteLine($"Age: {age}");
Console.WriteLine($"Is in the past: {isPast}");

// Example 2: Determine business hours and format a date
var now = DateTime.UtcNow;
var startOfDay = DateTimeExtensions.StartOfDay(now);
var endOfWeek = DateTimeExtensions.EndOfWeek(now);
var isBusinessHours = DateTimeExtensions.IsBusinessHours(now);
var isoDate = DateTimeExtensions.ToIso8601(now);

Console.WriteLine($"Start of day: {startOfDay}");
Console.WriteLine($"End of week: {endOfWeek}");
Console.WriteLine($"Is business hours: {isBusinessHours}");
Console.WriteLine($"ISO 8601: {isoDate}");
```

## Notes

- All methods that accept a `DateTime` parameter operate on the `Kind` property of the input. If the input is `DateTimeKind.Unspecified`, the behavior is equivalent to `DateTimeKind.Local`.
- Methods that perform date arithmetic (e.g., `StartOfDay`, `EndOfWeek`) may throw `ArgumentOutOfRangeException` if the resulting date is outside the range representable by `DateTime`.
- `GetAge` assumes the current date is the system clock time; for deterministic testing, inject a `DateTimeProvider` or similar abstraction.
- `ToRelativeTime` produces strings based on the current system culture; ensure the desired culture is set before calling if localization is required.
- All methods are thread-safe and do not mutate shared state.
