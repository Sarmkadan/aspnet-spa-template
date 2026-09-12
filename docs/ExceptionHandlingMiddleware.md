# ExceptionHandlingMiddleware

Middleware component that catches exceptions raised by later components in the ASP.NET Core request pipeline and returns standardized JSON error responses with an HTTP status code and request trace identifier.

## API

### `public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)`

Constructor that initializes the exception handling middleware with the next request delegate and a logger.

- **Parameters**:
  - `next` – The `RequestDelegate` representing the next middleware in the pipeline.
  - `logger` – The logger used to record handled and unhandled exceptions.
- **Exceptions**:
  - Throws `ArgumentNullException` if `next` or `logger` is `null`.

---

### `public async Task InvokeAsync(HttpContext context)`

Invokes the next middleware component and converts any exception it throws into a standardized `ErrorResponse` written as JSON.

- **Parameters**:
  - `context` – The `HttpContext` for the current HTTP request.
- **Return value**: A `Task` representing the asynchronous operation.
- **Behavior**:
  - Calls the next middleware and allows the request to continue normally when no exception is thrown.
  - Maps `NotFoundException` to HTTP `404 Not Found` with error code `NOT_FOUND` and logs the event at information level.
  - Maps `ValidationException` to HTTP `400 Bad Request`, includes its validation errors, and logs the event at warning level.
  - Maps `BusinessException` to its configured HTTP status code, uses its error code or `BUSINESS_ERROR` when no code is provided, and logs the event at warning level.
  - Maps all other exceptions to HTTP `500 Internal Server Error` with the generic message `An unexpected error occurred` and error code `INTERNAL_SERVER_ERROR`, and logs the exception at error level.
  - Sets the response content type to `application/json`, assigns `HttpContext.TraceIdentifier` to the response `TraceId`, and writes the response asynchronously.

## Usage

### Basic Setup in ASP.NET Core Pipeline

```csharp
// In Program.cs
var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();
app.Run();
```

Register the middleware early enough in the pipeline to catch exceptions from the components that follow it.

## Notes

- **Standardized responses**: Every handled exception is serialized as an `ErrorResponse`, giving clients a consistent error payload.
- **Trace identifiers**: Each error response includes the current request trace identifier so client-visible failures can be correlated with server logs.
- **Information disclosure**: Unexpected exception details are logged but are not returned to the client.
- **Response state**: The middleware writes an error response after an exception is caught. It does not check whether the HTTP response has already started.
