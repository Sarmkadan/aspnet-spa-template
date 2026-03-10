// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Middleware;
using AspNetSpaTemplate.Services;

namespace AspNetSpaTemplate.Configuration;

/// <summary>
/// Extension methods for registering offline-first SPA support with hot module replacement.
/// </summary>
public static class OfflineSupportExtensions
{
    /// <summary>
    /// Registers <see cref="AssetVersioningService"/> as a singleton <see cref="IAssetVersioningService"/>
    /// and as an <see cref="IHostedService"/>, ensuring a single shared instance handles both roles.
    /// </summary>
    /// <param name="services">The application service collection.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddOfflineSupport(this IServiceCollection services)
    {
        services.AddSingleton<AssetVersioningService>();
        services.AddSingleton<IAssetVersioningService>(sp => sp.GetRequiredService<AssetVersioningService>());
        services.AddHostedService(sp => sp.GetRequiredService<AssetVersioningService>());

        return services;
    }

    /// <summary>
    /// Inserts <see cref="HotReloadMiddleware"/> into the pipeline.
    /// </summary>
    /// <remarks>
    /// Place this call before <c>UseStaticFiles</c> so that requests for <c>/sw.js</c>,
    /// <c>/__asset-manifest.json</c>, and <c>/__hmr</c> are intercepted before the static
    /// file handler can short-circuit them.
    /// </remarks>
    /// <param name="app">The application builder.</param>
    /// <returns>The same <see cref="IApplicationBuilder"/> for chaining.</returns>
    public static IApplicationBuilder UseOfflineSupport(this IApplicationBuilder app)
        => app.UseMiddleware<HotReloadMiddleware>();
}
