# DependencyInjectionExtensions
The `DependencyInjectionExtensions` class provides a set of extension methods for configuring services and middleware in an ASP.NET Core application. These methods allow developers to easily add various features to their application, such as core services, structured logging, caching, event bus, background tasks, integration, health checks, CORS, API versioning, and response compression.

## API
* `public static IServiceCollection AddCoreServices(this IServiceCollection services)`: Adds core services to the service collection. **Parameters:** `services` - the service collection to add services to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddStructuredLogging(this IServiceCollection services)`: Adds structured logging to the service collection. **Parameters:** `services` - the service collection to add logging to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddCachingOnly(this IServiceCollection services)`: Adds caching to the service collection. **Parameters:** `services` - the service collection to add caching to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddEventBusOnly(this IServiceCollection services)`: Adds event bus to the service collection. **Parameters:** `services` - the service collection to add event bus to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddBackgroundTasksOnly(this IServiceCollection services)`: Adds background tasks to the service collection. **Parameters:** `services` - the service collection to add background tasks to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddIntegrationOnly(this IServiceCollection services)`: Adds integration to the service collection. **Parameters:** `services` - the service collection to add integration to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddHealthChecks(this IServiceCollection services)`: Adds health checks to the service collection. **Parameters:** `services` - the service collection to add health checks to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddDevelopmentCors(this IServiceCollection services)`: Adds CORS for development environments to the service collection. **Parameters:** `services` - the service collection to add CORS to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddProductionCors(this IServiceCollection services)`: Adds CORS for production environments to the service collection. **Parameters:** `services` - the service collection to add CORS to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddApiVersioning(this IServiceCollection services)`: Adds API versioning to the service collection. **Parameters:** `services` - the service collection to add API versioning to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IServiceCollection AddResponseCompression(this IServiceCollection services)`: Adds response compression to the service collection. **Parameters:** `services` - the service collection to add response compression to. **Return Value:** The updated service collection. **Throws:** No exceptions are thrown.
* `public static IApplicationBuilder UseAllMiddleware(this IApplicationBuilder builder)`: Adds all middleware to the application builder. **Parameters:** `builder` - the application builder to add middleware to. **Return Value:** The updated application builder. **Throws:** No exceptions are thrown.
* `public interface IServiceMarker`: A marker interface for services.
* `public class ServiceMarker : IServiceMarker`: A class implementing the `IServiceMarker` interface.

## Usage
The following examples demonstrate how to use the `DependencyInjectionExtensions` class:
```csharp
// Example 1: Adding core services and structured logging
public void ConfigureServices(IServiceCollection services)
{
    services.AddCoreServices();
    services.AddStructuredLogging();
}

// Example 2: Adding caching, event bus, and background tasks
public void ConfigureServices(IServiceCollection services)
{
    services.AddCachingOnly();
    services.AddEventBusOnly();
    services.AddBackgroundTasksOnly();
}
```

## Notes
When using the `DependencyInjectionExtensions` class, note that the order of adding services and middleware can be important. For example, adding CORS after adding API versioning may not work as expected. Additionally, some services and middleware may have dependencies on other services or middleware, so the order of addition can affect the overall behavior of the application. The `DependencyInjectionExtensions` class is designed to be thread-safe, but the underlying services and middleware may not be. Therefore, it is recommended to use the `DependencyInjectionExtensions` class in a thread-safe manner, such as during application startup.
