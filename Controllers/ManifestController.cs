#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Services;
using Microsoft.AspNetCore.Mvc;

namespace AspNetSpaTemplate.Controllers;

/// <summary>
/// Serves the Web App Manifest (<c>/manifest.json</c>) so the browser can install
/// the application as a Progressive Web App.
///
/// The manifest is generated dynamically so the icon and shortcut URLs can be
/// made absolute using the current request's scheme and host, which supports
/// deployments behind a reverse proxy at a sub-path.
/// </summary>
[ApiController]
public sealed class ManifestController : ControllerBase
{
    private readonly IManifestService _manifestService;

    /// <summary>Initialises the controller with the required manifest service.</summary>
    public ManifestController(IManifestService manifestService)
    {
        _manifestService = manifestService;
    }

    /// <summary>
    /// Returns the Web App Manifest document.
    /// The response is cached for 1 hour with a public cache policy and a
    /// <c>Vary: Host</c> header to ensure separate cache entries for different origins.
    /// </summary>
    /// <response code="200">Returns the <c>application/manifest+json</c> document.</response>
    [HttpGet("/manifest.json")]
    [Produces("application/manifest+json")]
    [ResponseCache(Duration = 3600, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetManifest()
    {
        var scheme = Request.Scheme;
        var host = Request.Host.HasValue ? Request.Host.Value : null;

        var manifest = _manifestService.BuildManifest(scheme, host);

        Response.Headers["Vary"] = "Host";

        return new JsonResult(manifest, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        })
        {
            ContentType = "application/manifest+json"
        };
    }

    /// <summary>
    /// Returns the theme color used in the <c>&lt;meta name="theme-color"&gt;</c> tag.
    /// Useful when rendering the HTML server-side.
    /// </summary>
    /// <response code="200">Returns a JSON object with the <c>themeColor</c> value.</response>
    [HttpGet("/api/v1/manifest/theme-color")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetThemeColor()
    {
        return Ok(new
        {
            themeColor = _manifestService.ThemeColor,
            backgroundColor = _manifestService.BackgroundColor
        });
    }
}
