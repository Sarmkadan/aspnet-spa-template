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
/// Extension methods for dependency injection configuration.
/// Provides convenient methods for registering common service combinations.
/// Reduces boilerplate in Program.cs.
/// </summary>
public static class DependencyInjectionExtensions
{
    /// <summary>
    /// Registers all core application services.
    /// One-shot registration for typical setup.
    /// </summary>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddApplicationServices(new ConfigurationBuilder().Build());
        services.ConfigureCaching();
        services.AddHttpClients(new ConfigurationBuilder().Build());

        return services;
    }

    /// <summary>
    /// Registers logging services with structured logging.
    /// Includes correlation ID tracking.
    /// </summary>
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        return services;
    }

    /// <summary>
    /// Registers only caching services.
    /// For applications that only need caching functionality.
    /// </summary>
    public static IServiceCollection AddCachingOnly(this IServiceCollection services)
    {
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.ConfigureCaching();
        services.AddSingleton<ICacheHealthMonitor, DefaultCacheHealthMonitor>();

        return services;
    }

    /// <summary>
    /// Registers only event bus and handlers.
    /// For event-driven applications.
    /// </summary>
    public static IServiceCollection AddEventBusOnly(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBusImplementation>();
        services.AddSingleton<DomainEventHandlers>();

        return services;
    }

    /// <summary>
    /// Registers only background tasks and scheduler.
    /// For applications with background processing needs.
    /// </summary>
    public static IServiceCollection AddBackgroundTasksOnly(this IServiceCollection services)
    {
        services.AddBackgroundTaskScheduler();
        services.AddBackgroundTask<NotificationWorker>();
        services.AddBackgroundTask<CacheMaintenanceWorker>();

        return services;
    }

    /// <summary>
    /// Registers only integration services.
    /// For applications that need external API communication.
    /// </summary>
    public static IServiceCollection AddIntegrationOnly(this IServiceCollection services)
    {
        services.AddSingleton<IHttpClientFactory, DefaultHttpClientFactory>();
        services.AddScoped<ExternalApiClient>();
        services.AddSingleton<NotificationService>();

        return services;
    }

    /// <summary>
    /// Registers health check endpoints.
    /// Useful for container orchestration and monitoring.
    /// </summary>
    public static IServiceCollection AddHealthChecks(this IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddCheck("database", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("DB OK"))
            .AddCheck("cache", async () =>
            {
                var cache = services.BuildServiceProvider().GetService<ICacheHealthMonitor>();
                var isHealthy = cache != null && await cache.IsCacheHealthyAsync();
                return isHealthy
                    ? Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Cache OK")
                    : Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Cache failed");
            });

        return services;
    }

    /// <summary>
    /// Registers CORS for development.
    /// Allows all origins, headers, and methods. Use carefully!
    /// </summary>
    public static IServiceCollection AddDevelopmentCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .DisallowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Registers CORS for production with specific origins.
    /// </summary>
    public static IServiceCollection AddProductionCors(
        this IServiceCollection services,
        params string[] allowedOrigins)
    {
        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials();
            });
        });

        return services;
    }

    /// <summary>
    /// Registers API versioning services.
    /// Useful for managing multiple API versions.
    /// </summary>
    public static IServiceCollection AddApiVersioning(this IServiceCollection services)
    {
        // Can be extended with Asp.Versioning.Mvc package
        return services;
    }

    /// <summary>
    /// Registers request/response compression.
    /// Reduces bandwidth for APIs returning large payloads.
    /// </summary>
    public static IServiceCollection AddResponseCompression(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProviderOptions>(options =>
        {
            options.Level = System.IO.Compression.CompressionLevel.Optimal;
        });

        services.AddResponseCompression(options =>
        {
            options.Providers.Add<Microsoft.AspNetCore.ResponseCompression.GzipCompressionProvider>();
            options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes
                .Concat(new[] { "application/json" });
        });

        return services;
    }

    /// <summary>
    /// Registers all middleware in proper order.
    /// Called from Program.cs to configure middleware pipeline.
    /// </summary>
    public static IApplicationBuilder UseAllMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseResponseCompression();
        app.UseHttpsRedirection();
        app.UseCors();

        app.UseApplicationMiddleware();

        app.UseStaticFiles();
        app.UseRouting();

        return app;
    }
}

/// <summary>
/// Service collection marker interface for type-safe service resolution.
/// </summary>
public interface IServiceMarker { }

/// <summary>
/// Implementation of service marker.
/// </summary>
public class ServiceMarker : IServiceMarker { }
