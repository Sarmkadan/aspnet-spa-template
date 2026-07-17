# ServiceConfiguration

Central static configuration facade for registering application services, HTTP clients, middleware, caching, authentication, and other cross-cutting concerns in ASP.NET Core applications built from the spa-template. Exposes typed settings and registration helpers to standardize bootstrapping across services and background tasks.

## API

### `public static IServiceCollection AddApplicationServices(IServiceCollection services)`

Registers core application services including logging, options validation, health checks, and background task consumers. Adds `IOptionsSnapshot<T>` registrations for strongly-typed settings consumed by controllers and hosted services.

- **services**: The `IServiceCollection` instance to which services are added.
- **Return value**: The same `IServiceCollection` for chaining.
- **Throws**: `ArgumentNullException` if `services` is `null`.

### `public static IServiceCollection AddHttpClients(IServiceCollection services)`

Registers named and typed HTTP clients with Polly resilience policies and base address configuration. Clients are configured to respect `ServiceConfiguration.ConnectionString` and `ServiceConfiguration.CommandTimeout`.

- **services**: The `IServiceCollection` instance to which HTTP clients are added.
- **Return value**: The same `IServiceCollection` for chaining.
- **Throws**: `ArgumentNullException` if `services` is `null`.

### `public static IApplicationBuilder UseApplicationMiddleware(IApplicationBuilder app)`

Configures the HTTP pipeline with standard middleware in the recommended order: exception handling, HTTPS redirection, static files, routing, health checks, and endpoint mapping.

- **app**: The `IApplicationBuilder` used to configure the pipeline.
- **Return value**: The same `IApplicationBuilder` for chaining.
- **Throws**: `ArgumentNullException` if `app` is `null`.

### `public static IServiceCollection ConfigureCaching(IServiceCollection services)`

Registers in-memory and distributed caching services based on `ServiceConfiguration.Caching.Provider`. Adds `IMemoryCache`, `IDistributedCache`, and typed cache wrappers with TTL values from `ServiceConfiguration.*CacheTtl`.

- **services**: The `IServiceCollection` instance to which cache services are added.
- **Return value**: The same `IServiceCollection` for chaining.
- **Throws**: `ArgumentNullException` if `services` is `null`; `InvalidOperationException` if `Caching.Provider` is neither `"Memory"` nor `"Redis"`.

### `public static void RegisterEventHandlers(IServiceCollection services)`

Scans the assembly for implementations of `IEventHandler<T>` and registers them as singletons. Handlers are invoked via `IEventBus` during domain events raised by repositories and services.

- **services**: The `IServiceCollection` instance used to register handlers.
- **Throws**: `ArgumentNullException` if `services` is `null`.

### `public TimeSpan ProductCacheTtl { get; }`

Gets the time-to-live for product-related cache entries. Defaults to 5 minutes.

### `public TimeSpan UserCacheTtl { get; }`

Gets the time-to-live for user-related cache entries. Defaults to 2 minutes.

### `public TimeSpan OrderCacheTtl { get; }`

Gets the time-to-live for order-related cache entries. Defaults to 1 minute.

### `public TimeSpan SearchResultsCacheTtl { get; }`

Gets the time-to-live for search result cache entries. Defaults to 30 seconds.

### `public TimeSpan SessionCacheTtl { get; }`

Gets the time-to-live for session-related cache entries. Defaults to 10 minutes.

### `public TimeSpan ConfigurationCacheTtl { get; }`

Gets the time-to-live for configuration cache entries. Defaults to 1 hour.

### `public DatabaseSettings Database { get; }`

Gets the strongly-typed database settings including connection string, command timeout, and retry configuration.

### `public CachingSettings Caching { get; }`

Gets the strongly-typed caching settings including provider and TTL overrides.

### `public AuthenticationSettings Authentication { get; }`

Gets the strongly-typed authentication settings including scheme names, token lifetimes, and issuer configuration.

### `public NotificationSettings Notifications { get; }`

Gets the strongly-typed notification settings including email templates, SMS providers, and webhook endpoints.

### `public string ConnectionString { get; }`

Gets the primary database connection string used by repositories and HTTP clients. Must be set before calling `AddApplicationServices`.

### `public int CommandTimeout { get; }`

Gets the default command timeout in seconds for database operations. Defaults to 30 seconds.

### `public bool EnableLogging { get; }`

Gets a value indicating whether detailed request and application logging is enabled.

### `public bool Enabled { get; }`

Gets a global feature flag controlling whether the service is active. When `false`, controllers return `503 Service Unavailable`.

### `public string Provider { get; }`

Gets the caching provider name (`"Memory"` or `"Redis"`). Must be set before calling `ConfigureCaching`.

## Usage

### Register services and middleware in `Program.cs`
