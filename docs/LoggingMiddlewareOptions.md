# LoggingMiddlewareOptions
The `LoggingMiddlewareOptions` class provides a set of options for configuring the logging behavior of the `RequestResponseLoggingMiddleware`. It allows developers to customize the level of logging detail, including the logging of request and response headers and bodies, as well as the threshold for slow requests. This class is used to fine-tune the logging functionality in ASP.NET Core applications.

## API
* `public bool Enabled`: Gets or sets a value indicating whether logging is enabled.
* `public string VerbosityLevel`: Gets or sets the verbosity level of the logging output.
* `public bool LogRequestHeaders`: Gets or sets a value indicating whether request headers should be logged.
* `public bool LogResponseHeaders`: Gets or sets a value indicating whether response headers should be logged.
* `public bool LogRequestBody`: Gets or sets a value indicating whether request bodies should be logged.
* `public bool LogResponseBody`: Gets or sets a value indicating whether response bodies should be logged.
* `public int SlowRequestThresholdMs`: Gets or sets the threshold in milliseconds for slow requests.
* `public List<string> ExcludedPaths`: Gets or sets a list of paths that should be excluded from logging.
* `public RequestResponseLoggingMiddleware`: This property is not documented as it seems to be an instance of the middleware itself, rather than a configuration option.
* `public async Task InvokeAsync`: This method is not documented as it seems to be an implementation detail of the middleware itself, rather than a configuration option.

## Usage
The following example demonstrates how to configure the logging middleware to log request and response headers and bodies:
```csharp
var options = new LoggingMiddlewareOptions
{
    Enabled = true,
    VerbosityLevel = "Debug",
    LogRequestHeaders = true,
    LogResponseHeaders = true,
    LogRequestBody = true,
    LogResponseBody = true,
    SlowRequestThresholdMs = 500,
    ExcludedPaths = new List<string> { "/healthcheck" }
};
```
Another example shows how to use the logging middleware in a ASP.NET Core application:
```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseMiddleware<RequestResponseLoggingMiddleware>(new LoggingMiddlewareOptions
    {
        Enabled = true,
        VerbosityLevel = "Information"
    });
}
```

## Notes
When using the `LoggingMiddlewareOptions` class, it is essential to consider the performance implications of logging request and response bodies, as this can result in significant overhead. Additionally, the `SlowRequestThresholdMs` property should be set carefully, as it can affect the accuracy of slow request detection. The `ExcludedPaths` property can be used to exclude sensitive paths from logging, such as those containing authentication tokens. The logging middleware is designed to be thread-safe, but it is still important to ensure that the `LoggingMiddlewareOptions` instance is properly synchronized when accessed from multiple threads.
