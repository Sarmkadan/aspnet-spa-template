# AspnetSpaTemplateOptions

Configuration options for the ASP.NET Core SPA template, controlling JWT authentication, request logging behavior, and environment-specific settings.

## API

### `JwtSecret`
Gets or sets the secret key used for signing JWT tokens. Must be a non-empty string with sufficient entropy (e.g., 32+ random bytes encoded in Base64) to ensure token security. Changing this value will invalidate all previously issued tokens.

### `JwtExpiration`
Gets or sets the expiration time in minutes for issued JWT tokens. Must be a positive integer. Defaults to `60` if not specified. Values below `5` are not recommended for production environments.

### `Environment`
Gets or sets the runtime environment name (e.g., `"Development"`, `"Staging"`, `"Production"`). Affects behavior such as request logging verbosity and error display. Must not be null or empty.

### `RequestLogging`
Gets or sets additional options for request logging behavior. See `RequestLoggingOptions` for configurable properties. Defaults to a new instance if not provided.

### `Enabled`
Gets or sets a value indicating whether the SPA template features are enabled. When `false`, disables SPA middleware, JWT authentication, and related services. Defaults to `true`.

### `VerbosityLevel`
Gets or sets the logging verbosity for diagnostic output. Accepts values such as `"Quiet"`, `"Normal"`, or `"Verbose"`. Affects console and file logging detail. Must not be null.

### `LogRequestHeaders`
Gets or sets a value indicating whether to include HTTP request headers in logs. Enabling this may expose sensitive data (e.g., `Authorization` headers). Defaults to `false`.

### `LogResponseHeaders`
Gets or sets a value indicating whether to include HTTP response headers in logs. Enabling this may expose sensitive data (e.g., `Set-Cookie`). Defaults to `false`.

### `LogRequestBody`
Gets or sets a value indicating whether to include the request body in logs. Enabling this may log sensitive user input. Defaults to `false`.

### `LogResponseBody`
Gets or sets a value indicating whether to include the response body in logs. Enabling this may log sensitive data (e.g., PII). Defaults to `false`.

### `SlowRequestThresholdMs`
Gets or sets the threshold in milliseconds for identifying slow requests. Requests exceeding this duration are logged with a warning. Must be a non-negative integer. Defaults to `2000`.

### `ExcludedPaths`
Gets or sets a list of path prefixes to exclude from request logging and middleware processing (e.g., `"/health"`, `"/api/status"`). Paths are case-sensitive and compared using `String.StartsWith`. Defaults to an empty list.
