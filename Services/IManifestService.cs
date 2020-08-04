#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Services;

/// <summary>
/// Generates and provides access to the Web App Manifest metadata used by the browser
/// to install the application as a Progressive Web App.
/// </summary>
public interface IManifestService
{
    /// <summary>
    /// Returns the complete Web App Manifest object ready to be serialised as JSON.
    /// The manifest is built once and cached for the lifetime of the service.
    /// </summary>
    /// <param name="requestScheme">
    /// The scheme of the originating request (e.g. <c>"https"</c>) used to build
    /// absolute icon and screenshot URLs when a <paramref name="requestHost"/> is also provided.
    /// </param>
    /// <param name="requestHost">
    /// The host of the originating request (e.g. <c>"example.com"</c>) used to build
    /// absolute icon and screenshot URLs. When <c>null</c>, icon paths remain relative.
    /// </param>
    WebAppManifest BuildManifest(string requestScheme, string? requestHost = null);

    /// <summary>
    /// Returns the value of the <c>theme_color</c> field from the current manifest.
    /// Clients may embed this in the HTML <c>&lt;meta name="theme-color"&gt;</c> tag.
    /// </summary>
    string ThemeColor { get; }

    /// <summary>
    /// Returns the value of the <c>background_color</c> field from the current manifest.
    /// </summary>
    string BackgroundColor { get; }
}

/// <summary>
/// Serialisable representation of the W3C Web App Manifest.
/// Property names use the snake_case convention required by the spec.
/// </summary>
public sealed class WebAppManifest
{
    /// <summary>Full application name shown during installation.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Short name used on the home screen where space is limited.</summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>Human-readable description of the application's purpose.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// URL loaded when the application is launched from the installed entry point.
    /// Should be a path relative to the manifest's scope.
    /// </summary>
    public string StartUrl { get; set; } = "/";

    /// <summary>Navigation scope; requests outside the scope open in the browser.</summary>
    public string Scope { get; set; } = "/";

    /// <summary>
    /// How the application should be displayed: <c>standalone</c>, <c>fullscreen</c>,
    /// <c>minimal-ui</c>, or <c>browser</c>.
    /// </summary>
    public string Display { get; set; } = "standalone";

    /// <summary>Default screen orientation.</summary>
    public string Orientation { get; set; } = "portrait-primary";

    /// <summary>Background colour shown in the splash screen before the app loads.</summary>
    public string BackgroundColor { get; set; } = "#f8fafc";

    /// <summary>Colour used by the browser for the address bar and surrounding UI.</summary>
    public string ThemeColor { get; set; } = "#2563eb";

    /// <summary>Primary language of the application.</summary>
    public string Lang { get; set; } = "en";

    /// <summary>Application category hints for app stores and search engines.</summary>
    public IReadOnlyList<string> Categories { get; set; } = [];

    /// <summary>Application icon set in various resolutions.</summary>
    public IReadOnlyList<ManifestIcon> Icons { get; set; } = [];

    /// <summary>Deep-link shortcuts accessible from the home screen long-press menu.</summary>
    public IReadOnlyList<ManifestShortcut> Shortcuts { get; set; } = [];

    /// <summary>
    /// When <c>true</c>, the browser may suggest a native application as an alternative.
    /// Almost always <c>false</c> for pure PWAs.
    /// </summary>
    public bool PreferRelatedApplications { get; set; }
}

/// <summary>A single icon entry inside <see cref="WebAppManifest.Icons"/>.</summary>
public sealed class ManifestIcon
{
    /// <summary>Relative or absolute URL of the image resource.</summary>
    public string Src { get; set; } = string.Empty;

    /// <summary>Space-separated list of pixel dimensions, e.g. <c>"192x192"</c>.</summary>
    public string Sizes { get; set; } = string.Empty;

    /// <summary>MIME type of the image, e.g. <c>"image/png"</c>.</summary>
    public string Type { get; set; } = "image/png";

    /// <summary>
    /// Intended display context: <c>"any"</c>, <c>"maskable"</c>, or <c>"monochrome"</c>.
    /// Multiple values may be separated by spaces.
    /// </summary>
    public string Purpose { get; set; } = "any";
}

/// <summary>A deep-link shortcut shown in the home screen context menu.</summary>
public sealed class ManifestShortcut
{
    /// <summary>Full name of the shortcut action.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Abbreviated name used when space is limited.</summary>
    public string ShortName { get; set; } = string.Empty;

    /// <summary>Brief description of the shortcut's purpose.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>URL to navigate to when the shortcut is activated.</summary>
    public string Url { get; set; } = "/";

    /// <summary>Icons associated with the shortcut.</summary>
    public IReadOnlyList<ManifestIcon> Icons { get; set; } = [];
}
