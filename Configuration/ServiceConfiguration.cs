// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.BackgroundWorkers;
using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Events;
using AspNetSpaTemplate.Integration;

namespace AspNetSpaTemplate.Configuration;

/// <summary>
/// Extension methods to configure application services.
/// Centralizes DI registration and configuration setup.
/// Separates infrastructure concerns from Program.cs for readability.
/// </summary>
public static class ServiceConfiguration
{
    /// <summary>
    /// Registers all application services in the DI container.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Caching
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Event Bus
        services.AddSingleton<IEventBus, EventBusImplementation>();
        services.AddSingleton<DomainEventHandlers>();

        // Integration
        services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();
        services.AddScoped<ExternalApiClient>();
        services.AddSingleton<NotificationService>();

        // Background Tasks
        services.AddBackgroundTaskScheduler();
        services.AddBackgroundTask<NotificationWorker>();
        services.AddBackgroundTask<CacheMaintenanceWorker>();

        // Health monitoring
        services.AddSingleton<ICacheHealthMonitor, DefaultCacheHealthMonitor>();

        return services;
    }

    /// <summary>
    /// Registers and configures HTTP clients for external APIs.
    /// </summary>
    public static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("ExternalApi", client =>
        {
            client.BaseAddress = new Uri("https://api.example.com");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "AspNetSpaTemplate/1.0");
        });

        return services;
    }

    /// <summary>
    /// Configures middleware pipeline for the application.
    /// Order matters: authentication -> rate limiting -> logging -> errors -> business logic.
    /// </summary>
    public static IApplicationBuilder UseApplicationMiddleware(this IApplicationBuilder app)
    {
        // Correlation ID should be first (tracks request through entire pipeline)
        app.UseMiddleware<Middleware.CorrelationIdMiddleware>();

        // Authentication must come before rate limiting
        app.UseMiddleware<Middleware.AuthenticationMiddleware>();

        // Rate limiting after authentication (using API key if present)
        app.UseMiddleware<Middleware.RateLimitingMiddleware>();

        // Error handling should wrap everything else
        // Already added in Program.cs via app.UseMiddleware<ExceptionHandlingMiddleware>()

        return app;
    }

    /// <summary>
    /// Configures caching policies for different data types.
    /// </summary>
    public static IServiceCollection ConfigureCaching(this IServiceCollection services)
    {
        // Cache configuration could be read from appsettings.json in production
        var cacheConfig = new CachePolicyConfiguration
        {
            ProductCacheTtl = TimeSpan.FromHours(1),
            UserCacheTtl = TimeSpan.FromMinutes(30),
            SearchResultsCacheTtl = TimeSpan.FromMinutes(10),
            SessionCacheTtl = TimeSpan.FromHours(24)
        };

        services.AddSingleton(cacheConfig);
        return services;
    }

    /// <summary>
    /// Registers event handlers in the event bus.
    /// Called after services are built.
    /// </summary>
    public static void RegisterEventHandlers(this IApplicationBuilder app)
    {
        var services = app.ApplicationServices;
        var eventBus = services.GetRequiredService<IEventBus>();
        var handlers = services.GetRequiredService<DomainEventHandlers>();

        // Subscribe handlers to events
        eventBus.Subscribe<ProductCreatedEvent>(handlers.OnProductCreated);
        eventBus.Subscribe<ProductUpdatedEvent>(handlers.OnProductUpdated);
        eventBus.Subscribe<ProductDeletedEvent>(handlers.OnProductDeleted);
        eventBus.Subscribe<OrderPlacedEvent>(handlers.OnOrderPlaced);
        eventBus.Subscribe<OrderCompletedEvent>(handlers.OnOrderCompleted);
        eventBus.Subscribe<OrderCancelledEvent>(handlers.OnOrderCancelled);
        eventBus.Subscribe<UserRegisteredEvent>(handlers.OnUserRegistered);
        eventBus.Subscribe<ReviewSubmittedEvent>(handlers.OnReviewSubmitted);
    }
}

/// <summary>
/// Configuration for caching policies across different entity types.
/// </summary>
public class CachePolicyConfiguration
{
    public TimeSpan ProductCacheTtl { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan UserCacheTtl { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan OrderCacheTtl { get; set; } = TimeSpan.FromMinutes(5);
    public TimeSpan SearchResultsCacheTtl { get; set; } = TimeSpan.FromMinutes(10);
    public TimeSpan SessionCacheTtl { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan ConfigurationCacheTtl { get; set; } = TimeSpan.FromHours(12);
}

/// <summary>
/// Application settings configuration.
/// Centralizes all configuration keys and defaults.
/// </summary>
public class ApplicationSettings
{
    public DatabaseSettings Database { get; set; } = new();
    public CachingSettings Caching { get; set; } = new();
    public AuthenticationSettings Authentication { get; set; } = new();
    public NotificationSettings Notifications { get; set; } = new();
}

public class DatabaseSettings
{
    public string ConnectionString { get; set; } = "";
    public int CommandTimeout { get; set; } = 30;
    public bool EnableLogging { get; set; } = false;
}

public class CachingSettings
{
    public bool Enabled { get; set; } = true;
    public string Provider { get; set; } = "Memory"; // Memory, Redis
    public int MaxItemCount { get; set; } = 10000;
}

public class AuthenticationSettings
{
    public bool Enabled { get; set; } = true;
    public int SessionTimeoutMinutes { get; set; } = 60;
    public bool RequireHttpsOnly { get; set; } = true;
}

public class NotificationSettings
{
    public bool Enabled { get; set; } = true;
    public int BatchSize { get; set; } = 100;
    public int RetryCount { get; set; } = 3;
    public string EmailProvider { get; set; } = "SendGrid";
    public string SmsProvider { get; set; } = "Twilio";
}
