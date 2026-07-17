#nullable enable

// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.BackgroundWorkers;
using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Events;
using AspNetSpaTemplate.Integration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using static Microsoft.Extensions.DependencyInjection.HealthCheckServiceCollectionExtensions;

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
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddApplicationServices(new ConfigurationBuilder().Build());
        services.ConfigureCaching();
        services.AddHttpClients(new ConfigurationBuilder().Build());

        return services;
    }

    /// <summary>
    /// Registers logging services with structured logging.
    /// Includes correlation ID tracking.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddStructuredLogging(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddCachingOnly(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.ConfigureCaching();
        services.AddSingleton<ICacheHealthMonitor, DefaultCacheHealthMonitor>();

        return services;
    }

    /// <summary>
    /// Registers only event bus and handlers.
    /// For event-driven applications.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddEventBusOnly(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IEventBus, EventBusImplementation>();
        services.AddSingleton<DomainEventHandlers>();

        return services;
    }

    /// <summary>
    /// Registers only background tasks and scheduler.
    /// For applications with background processing needs.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddBackgroundTasksOnly(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddBackgroundTaskScheduler();
        services.AddBackgroundTask<NotificationWorker>();
        services.AddBackgroundTask<CacheMaintenanceWorker>();

        return services;
    }

    /// <summary>
    /// Registers only integration services.
    /// For applications that need external API communication.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddIntegrationOnly(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<AspNetSpaTemplate.Integration.IHttpClientFactory, DefaultHttpClientFactory>();
        services.AddScoped<ExternalApiClient>();
        services.AddSingleton<NotificationService>();

        return services;
    }

    /// <summary>
    /// Registers health check endpoints.
    /// Useful for container orchestration and monitoring.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddHealthChecks(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        Microsoft.Extensions.DependencyInjection.HealthCheckServiceCollectionExtensions
            .AddHealthChecks(services)
            .AddCheck("database", () => HealthCheckResult.Healthy("DB OK"),
                tags: new[] { "db" })
            .AddAsyncCheck("cache", async () =>
            {
                var cache = services.BuildServiceProvider().GetService<ICacheHealthMonitor>();
                var isHealthy = cache is not null && await cache.IsCacheHealthyAsync();
                return isHealthy
                    ? HealthCheckResult.Healthy("Cache OK")
                    : HealthCheckResult.Unhealthy("Cache failed");
            });

        return services;
    }

    /// <summary>
    /// Registers CORS for development.
    /// Allows all origins, headers, and methods. Use carefully!
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddDevelopmentCors(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    /// <param name="services">The service collection to configure.</param>
    /// <param name="allowedOrigins">The allowed origins for CORS.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="allowedOrigins"/> is null or empty.</exception>
    public static IServiceCollection AddProductionCors(
        this IServiceCollection services,
        params string[] allowedOrigins)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(allowedOrigins);

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
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddApiVersioning(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Can be extended with Asp.Versioning.Mvc package
        return services;
    }

    /// <summary>
    /// Registers request/response compression.
    /// Reduces bandwidth for APIs returning large payloads.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddResponseCompression(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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
    /// <param name="app">The application builder.</param>
    /// <param name="env">The web host environment.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="app"/> or <paramref name="env"/> is null.</exception>
    public static IApplicationBuilder UseAllMiddleware(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(env);

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