# ThemeService

The `ThemeService` class provides a centralized mechanism for managing a user's colour scheme preference (e.g., light or dark mode) within an ASP.NET Single Page Application. It exposes asynchronous methods to retrieve, set, and clear the scheme, as well as synchronous properties to inspect the current scheme and the last time it was updated. The service is intended to be registered as a scoped or transient dependency and typically persists the scheme through an underlying data store (e.g., database, user profile).

## API

### `public ThemeService()`

Initializes a new instance of the `ThemeService` class. No parameters are required.

---

### `public async Task<ColourScheme> GetSchemeAsync()`

Asynchronously retrieves the current colour scheme.

- **Returns**: A `Task<ColourScheme>` that resolves to the current `ColourScheme` value. If no scheme has been set, the returned value may be `null` or a default value depending on the implementation.
- **Throws**: `InvalidOperationException` if the underlying data store is unavailable or the operation fails.

---

### `public async Task SetSchemeAsync(ColourScheme scheme)`

Asynchronously sets the colour scheme to the specified value.

- **Parameters**:
  - `scheme` (`ColourScheme`): The colour scheme to apply. Must not be `null`.
- **Returns**: A `Task` representing the asynchronous operation.
- **Throws**:
  - `ArgumentNullException` if `scheme` is `null`.
  - `InvalidOperationException` if the underlying data store cannot be updated.

---

### `public async Task ClearSchemeAsync()`

Asynchronously clears the current colour scheme, effectively resetting it to an unset or default state.

- **Returns**: A `Task` representing the asynchronous operation.
- **Throws**: `InvalidOperationException` if the underlying data store cannot be modified.

---

### `public ColourScheme Scheme { get; }`

Gets the current colour scheme. This property reflects the last successfully retrieved or set scheme. It may be `null` if no scheme has been loaded or after a call to `ClearSchemeAsync`.

- **Value**: A `ColourScheme` instance, or `null`.

---

### `public DateTime UpdatedAt { get; }`

Gets the timestamp of the last time the colour scheme was modified (set or cleared). The value is `DateTime.MinValue` if the scheme has never been updated.

- **Value**: A `DateTime` in UTC (or local, depending on implementation).

## Usage

### Example 1: Retrieving and setting a colour scheme in a controller

```csharp
[ApiController]
[Route("api/theme")]
public class ThemeController : ControllerBase
{
    private readonly ThemeService _themeService;

    public ThemeController(ThemeService themeService)
    {
        _themeService = themeService;
    }

    [HttpGet]
    public async Task<ActionResult<ColourScheme>> GetScheme()
    {
        var scheme = await _themeService.GetSchemeAsync();
        return Ok(scheme);
    }

    [HttpPut]
    public async Task<IActionResult> SetScheme([FromBody] ColourScheme scheme)
    {
        await _themeService.SetSchemeAsync(scheme);
        return Ok();
    }
}
```

### Example 2: Clearing the scheme and inspecting state

```csharp
public async Task ResetAndLogAsync(ThemeService themeService)
{
    await themeService.ClearSchemeAsync();

    var scheme = themeService.Scheme;          // likely null
    var lastUpdated = themeService.UpdatedAt;  // timestamp of the clear operation

    Console.WriteLine($"Scheme after clear: {scheme?.Name ?? "none"}");
    Console.WriteLine($"Last updated: {lastUpdated}");
}
```

## Notes

- **Thread safety**: The `ThemeService` is not guaranteed to be thread-safe. Concurrent calls to `GetSchemeAsync`, `SetSchemeAsync`, or `ClearSchemeAsync` from multiple threads may result in race conditions or inconsistent state. If shared across requests, consider registering the service as scoped (per-request) to avoid concurrency issues.
- **Null scheme**: The `Scheme` property can be `null` if no scheme has been loaded or after a clear operation. Always check for `null` before accessing its members.
- **Default `UpdatedAt`**: When no scheme has ever been set or cleared, `UpdatedAt` returns `DateTime.MinValue`. This value can be used to detect whether the service has been initialized.
- **Persistence**: The underlying implementation is responsible for persisting the scheme (e.g., in a database, session, or user claims). The service itself does not cache the scheme beyond the current instance lifetime unless explicitly implemented.
