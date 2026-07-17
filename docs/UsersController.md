# UsersController

Manages user authentication, registration, profile operations, and administrative user lifecycle actions for the ASP.NET SPA application. Exposes endpoints that handle identity workflows including JWT-based login, self-service account management, and administrative user enumeration with support for soft deactivation and reactivation.

## API

### `public UsersController`

Default parameterless constructor. Initializes the controller with its injected dependencies (typically `UserManager`, `SignInManager`, `ITokenService`, and related services resolved through the ASP.NET Core DI container). Called by the framework during request activation.

---

### `public async Task<IActionResult> Register`

Registers a new user account. Accepts a registration model containing at minimum a username, email, and password. Validates input, creates the identity user, assigns default roles if configured, and returns a success response or a list of validation errors.

- **Parameters:** Request body deserialized into a `RegisterRequest` DTO (expected properties: `Username`, `Email`, `Password`, and optionally `ConfirmPassword`).
- **Returns:** `200 OK` with a confirmation message or JWT token on success. `400 BadRequest` with an `ErrorResponse` containing validation failures or duplicate-user errors.
- **Throws:** Does not throw to callers; all exceptions are caught and mapped to `500 Internal Server Error` with a generic error payload.

---

### `public async Task<IActionResult> Login`

Authenticates a user by credentials and issues a JWT bearer token. Checks whether the account is active and not locked out before generating the token.

**Parameters:** `LoginRequest` body with `UsernameOrEmail` and `Password` fields.
**Returns:** `200 OK` with a `LoginResponse` containing the access token, expiration, and refresh token if refresh-token rotation is enabled. `401 Unauthorized` when credentials are invalid or the account is deactivated/locked.
**Throws:** No unhandled exceptions; all failures are translated to `401` or `400` responses.

---

### `public async Task<IActionResult> GetUser`

Retrieves the currently authenticated user’s identity and basic profile information. Reads the user ID from the JWT claims principal.

**Parameters:** None (extracts user from `HttpContext.User`).
**Returns:** `200 OK` with a `UserDto` containing `Id`, `Username`, `Email`, and `Roles`. `401 Unauthorized` if the token is missing or invalid.
**Throws:** `InvalidOperationException` if the claims principal lacks a name identifier claim (should not occur with properly configured authentication middleware).

---

### `public async Task<IActionResult> GetProfile`

Returns the full profile of the authenticated user, including extended fields such as display name, bio, avatar URL, and timestamps.

**Parameters:** None (user resolved from `HttpContext.User`).
**Returns:** `200 OK` with a `UserProfileDto`. `404 Not Found` if the user record has been deleted between token issuance and the request.
**Throws:** No exceptions; missing user returns `404`.

---

### `public async Task<IActionResult> UpdateUser`

Updates mutable profile fields for the authenticated user. Accepts a partial update payload; only supplied fields are modified.

**Parameters:** `UpdateUserRequest` body with optional `DisplayName`, `Bio`, `AvatarUrl`, and `Email`.
**Returns:** `200 OK` with the updated `UserProfileDto`. `400 Bad Request` on validation failure or duplicate email. `401 Unauthorized` if not authenticated.
**Throws:** No exceptions; validation and persistence errors are caught and mapped to `400` responses.

---

### `public async Task<IActionResult> DeactivateUser`

Sets the authenticated user’s account as inactive. The user is not deleted; subsequent login attempts are rejected until reactivation.

**Parameters:** None (identity user from `HttpContext.User`).
**Returns:** `200 OK` with a confirmation message. `404 Not Found` if the user does not exist.
**Throws:** No exceptions; all errors are caught and returned as `400` or `500` status codes.

---

### `public async Task<IActionResult> ActivateUser`

Reactivates a previously deactivated account for the authenticated user. Only callable when the user is already authenticated (typically via a reactivation token flow or admin override).

**Parameters:** None (identity user from `HttpContext.User`).
**Returns:** `200 OK` on success. `400 Bad Request` if the account is already active or the user is not in a deactivated state.
**Throws:** No exceptions.

---

### `public async Task<IActionResult> GetAllUsers`

Administrative endpoint that returns a paginated list of all registered users. Requires an `Admin` or `Moderator` role claim.

**Parameters:** Query string `page` (int, default 1) and `pageSize` (int, default 20, max 100).
**Returns:** `200 OK` with a `PagedResult<UserListItemDto>` containing items, total count, and page metadata. `403 Forbidden` if the caller lacks the required role.
**Throws:** No exceptions; authorization failures return `403`.

---

### `public async Task<IActionResult> GetRecentlyActiveUsers`

Returns users who have been active within a configurable lookback window (default 7 days). Requires an administrative role.

**Parameters:** Optional query parameter `days` (int, default 7, clamped to 1–30).
**Returns:** `200 OK` with a `List<UserActivityDto>` ordered by last activity descending. `403 Forbidden` for unauthorized callers.
**Throws:** No exceptions.

---

## Usage

### Example 1: Register a new user and immediately retrieve their profile

```csharp
// Arrange
var client = _factory.CreateClient();
var registerPayload = new
{
    Username = "alice",
    Email = "alice@example.com",
    Password = "Str0ng!Pass",
    ConfirmPassword = "Str0ng!Pass"
};

// Act — register
var registerResponse = await client.PostAsJsonAsync("/api/users/register", registerPayload);
registerResponse.EnsureSuccessStatusCode();
var loginResult = await registerResponse.Content.ReadFromJsonAsync<LoginResponse>();

// Set the JWT for subsequent requests
client.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", loginResult.Token);

// Retrieve profile
var profileResponse = await client.GetAsync("/api/users/profile");
var profile = await profileResponse.Content.ReadFromJsonAsync<UserProfileDto>();

// Assert
Assert.Equal("testuser", profile.Username);
Assert.Equal("alice@example.com", profile.Email);
```

### Example 2: Admin fetches recently active users and deactivates a stale account

```csharp
var adminClient = _factory.CreateClient();
adminClient.DefaultRequestHeaders.Authorization =
    new AuthenticationHeaderValue("Bearer", _adminToken);

// Get users active in the last 7 days
var recentResponse = await adminClient.GetAsync("/api/users/recently-active?days=7");
var recentUsers = await recentResponse.Content.ReadFromJsonAsync<List<UserListItemDto>>();

// Find a user who hasn't been active in the window (simulated)
var staleUserId = recentUsers.FirstOrDefault(u => u.LastActive < DateTime.UtcNow.AddDays(-7))?.Id;
if (staleUserId != null)
{
    // Impersonate or use admin override to deactivate
    var deactivateResponse = await adminClient.PostAsync($"/api/users/{staleUserId}/deactivate", null);
    deactivateResponse.EnsureSuccessStatusCode();
}
```

---

## Notes

- **Thread safety:** The controller itself is stateless; all state resides in the underlying `UserManager` and database context, which are scoped per request. No shared mutable state exists between invocations, so concurrent requests are safe.
- **Edge cases:**
  - `Register` returns `400` when the username or email already exists; the error response distinguishes between duplicate username and duplicate email via the `Errors` array.
  - `Login` rejects deactivated users with a `401` response that does not disclose whether the account exists, preventing enumeration attacks.
  - `GetRecentlyActiveUsers` caps the `days` parameter at 30; values exceeding this are silently clamped to 30.
  - `DeactivateUser` and `ActivateUser` are idempotent for the authenticated user — calling deactivate on an already-inactive account returns `400`, not `200`.
  - `GetAllUsers` pagination uses 1-based indexing; requesting page 0 returns an empty set rather than throwing.
- **Authorization:** All endpoints except `Register` and `Login` require authentication. `GetAllUsers` and `GetRecentlyActiveUsers` additionally require an `Admin` or `Moderator` role. Authorization failures return `403` before any database queries execute.
- **Token revocation:** `Login` issues a new token but does not invalidate previous tokens unless refresh-token rotation is configured server-side. Clients should discard old tokens upon receiving a new one.
