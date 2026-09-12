# AuthenticationMiddleware

Middleware component that authenticates incoming HTTP requests using a bearer token or API key, skips configured public endpoints, and makes authentication data available to downstream handlers.

## API

### `public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)`

Constructor that initializes the authentication middleware.

- **Parameters**:
  - `next` – The `RequestDelegate` representing the next middleware in the pipeline.
  - `logger` – The logger used to record authentication results and errors.
- **Exceptions**:
  - Throws `ArgumentNullException` if `next` or `logger` is `null`.

---

### `public async Task InvokeAsync(HttpContext context)`

Authenticates the current HTTP request and either continues the middleware pipeline or writes an error response.

- **Parameters**:
  - `context` – The `HttpContext` for the current HTTP request.
- **Return value**: A `Task` representing the asynchronous operation.
- **Behavior**:
  - Public endpoints bypass authentication and immediately invoke the next middleware.
  - The `Authorization` header is checked first for a case-insensitive `Bearer` scheme with a token. If no bearer token is found, the `api_key` query parameter is checked.
  - A missing token produces a `401 Unauthorized` JSON response with error code `UNAUTHORIZED`.
  - A token not found in the configured development API-key set produces a `401 Unauthorized` JSON response with error code `INVALID_TOKEN` and logs a warning.
  - A valid token is stored in `HttpContext.Items["AuthToken"]`. A deterministic, non-negative user ID derived from the token's SHA-256 hash is stored in `HttpContext.Items["UserId"]`, and the successful authentication is logged.
  - After successful authentication, the next middleware is invoked.
  - Any exception raised while processing authentication is logged and produces a `500 Internal Server Error` JSON response with error code `AUTH_ERROR`; the pipeline is not continued.

## Usage

### Basic Setup in ASP.NET Core Pipeline

```csharp
// In Program.cs or Startup.cs
var app = builder.Build();

app.UseMiddleware<AuthenticationMiddleware>();

app.MapControllers();
app.Run();
```

Authentication middleware should be registered before handlers that depend on `HttpContext.Items["UserId"]` or `HttpContext.Items["AuthToken"]`.

## Notes

- **Public endpoints**: Requests whose paths begin with `/health`, `/swagger`, `/api/users/login`, `/api/users/register`, or `/index.html` are exempt. Matching is case-insensitive and respects path-segment boundaries.
- **Token precedence**: A correctly formatted bearer token takes precedence over the `api_key` query parameter.
- **Development keys**: Token validation uses the middleware's in-memory API-key allowlist. Production deployments should validate signed JWTs or retrieve secrets from a secure store.
- **Query-string keys**: The `api_key` query parameter is intended only for testing because URLs can be logged or otherwise exposed. Prefer the `Authorization` header and enforce HTTPS.
- **User identity**: The derived user ID is stable across process restarts but is not a substitute for identity claims. Production JWT handling should obtain the user ID from validated claims.
