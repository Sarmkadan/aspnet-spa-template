#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Builds the Web App Manifest from application configuration.
/// The manifest object is constructed on first call and reused for the lifetime
/// of the service (registered as a singleton).
/// </summary>
public sealed class ManifestService : IManifestService
{
    private readonly IConfiguration _config;
    private readonly ILogger<ManifestService> _logger;

    // Immutable defaults; can be overridden via the "Manifest" config section.
    private const string DefaultName = "AspNet SPA Template";
    private const string DefaultShortName = "SPA Template";
    private const string DefaultThemeColor = "#2563eb";
    private const string DefaultBackgroundColor = "#f8fafc";

    private const string Icon192File = "icon-192.png";
    private const string Icon512File = "icon-512.png";

    private static readonly string[] DefaultCategories = new[] { "productivity", "utilities" };

    /// <inheritdoc/>
    public string ThemeColor => _config["Manifest:ThemeColor"] ?? DefaultThemeColor;

    /// <inheritdoc/>
    public string BackgroundColor => _config["Manifest:BackgroundColor"] ?? DefaultBackgroundColor;

    /// <summary>Initialises the service with configuration and a logger.</summary>
    public ManifestService(IConfiguration config, ILogger<ManifestService> logger)
    {
        _config = config;
        _logger = logger;
    }

    /// <inheritdoc/>
    public WebAppManifest BuildManifest(string requestScheme, string? requestHost = null)
    {
        var baseUrl = BuildBaseUrl(requestScheme, requestHost);

        var manifest = new WebAppManifest
        {
            Name = _config["Manifest:Name"] ?? DefaultName,
            ShortName = _config["Manifest:ShortName"] ?? DefaultShortName,
            Description = _config["Manifest:Description"]
                ?? "A production-ready ASP.NET Core SPA with vanilla JavaScript frontend",
            StartUrl = "/",
            Scope = "/",
            Display = "standalone",
            Orientation = "portrait-primary",
            BackgroundColor = BackgroundColor,
            ThemeColor = ThemeColor,
            Lang = "en",
            Categories = DefaultCategories,
            PreferRelatedApplications = false,
            Icons = BuildIcons(baseUrl),
            Shortcuts = BuildShortcuts(baseUrl)
        };

        _logger.LogDebug("Web App Manifest built for {Host}", requestHost ?? "relative");
        return manifest;
    }

    private static string BuildBaseUrl(string scheme, string? host)
    {
        return string.IsNullOrWhiteSpace(host) ? string.Empty : $"{scheme}://{host}";
    }

    private static IReadOnlyList<ManifestIcon> BuildIcons(string baseUrl) =>
    [
        new ManifestIcon
        {
            Src = $"{baseUrl}/icons/{Icon192File}",
            Sizes = "192x192",
            Type = "image/png",
            Purpose = "any maskable"
        },
        new ManifestIcon
        {
            Src = $"{baseUrl}/icons/{Icon512File}",
            Sizes = "512x512",
            Type = "image/png",
            Purpose = "any maskable"
        }
    ];

    private static IReadOnlyList<ManifestShortcut> BuildShortcuts(string baseUrl) =>
    [
        new ManifestShortcut
        {
            Name = "Browse Products",
            ShortName = "Products",
            Description = "View the product catalogue",
            Url = "/?page=products",
            Icons =
            [
                new ManifestIcon { Src = $"{baseUrl}/icons/{Icon192File}", Sizes = "192x192" }
            ]
        },
        new ManifestShortcut
        {
            Name = "Shopping Cart",
            ShortName = "Cart",
            Description = "View your shopping cart",
            Url = "/?page=cart",
            Icons =
            [
                new ManifestIcon { Src = $"{baseUrl}/icons/{Icon192File}", Sizes = "192x192" }
            ]
        }
    ];
}
