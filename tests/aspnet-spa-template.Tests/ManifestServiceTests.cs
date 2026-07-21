#nullable enable
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Tests for the <see cref="ManifestService"/> class.
/// </summary>
public sealed class ManifestServiceTests
{
    private static ManifestService BuildSut(Dictionary<string, string?>? config = null)
    {
        var configurationBuilder = new ConfigurationBuilder();
        if (config is not null)
            configurationBuilder.AddInMemoryCollection(config);

        return new ManifestService(
            configurationBuilder.Build(),
            NullLogger<ManifestService>.Instance);
    }

    // ── BuildManifest – required fields ───────────────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ManifestService.BuildManifest(string,string?)"/> returns a valid manifest with default values when no configuration is provided.
    /// </summary>
    [Fact]
    public void BuildManifest_WithDefaults_ReturnsValidManifest()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", "example.com");

        manifest.Should().NotBeNull();
        manifest.Name.Should().NotBeNullOrWhiteSpace();
        manifest.StartUrl.Should().Be("/");
        manifest.Scope.Should().Be("/");
        manifest.Display.Should().Be("standalone");
        manifest.Orientation.Should().Be("portrait-primary");
        manifest.Lang.Should().Be("en");
        manifest.PreferRelatedApplications.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the default manifest contains exactly two icons with required properties.
    /// Tests the icons list structure and content.
    /// </summary>
    [Fact]
    public void BuildManifest_WithDefaults_ContainsTwoIcons()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", "example.com");

        manifest.Icons.Should().HaveCount(2);
        manifest.Icons.Should().AllSatisfy(i =>
        {
            i.Src.Should().NotBeNullOrWhiteSpace();
            i.Sizes.Should().NotBeNullOrWhiteSpace();
            i.Type.Should().Be("image/png");
            i.Purpose.Should().NotBeNullOrWhiteSpace();
        });
    }

    /// <summary>
    /// Verifies that icon source URLs are absolute when a host is provided.
    /// Tests that icons list contains absolute paths.
    /// </summary>
    [Fact]
    public void BuildManifest_WithHost_IconSrcsAreAbsolute()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", "my.example.com");

        manifest.Icons.Should().AllSatisfy(i =>
            i.Src.Should().StartWith("https://my.example.com"));
    }

    /// <summary>
    /// Verifies that icon source URLs are relative when no host is provided.
    /// Tests that icons list contains relative paths.
    /// </summary>
    [Fact]
    public void BuildManifest_WithoutHost_IconSrcsAreRelative()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", null);

        manifest.Icons.Should().AllSatisfy(i =>
            i.Src.Should().StartWith("/icons/"));
    }

    /// <summary>
    /// Verifies that the manifest contains shortcuts with non-empty name and URL.
    /// Tests that manifest generation includes shortcuts.
    /// </summary>
    [Fact]
    public void BuildManifest_ContainsShortcuts()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", "example.com");

        manifest.Shortcuts.Should().NotBeEmpty();
        manifest.Shortcuts.Should().HaveCount(2);
        manifest.Shortcuts.Should().AllSatisfy(s =>
        {
            s.Name.Should().NotBeNullOrWhiteSpace();
            s.ShortName.Should().NotBeNullOrWhiteSpace();
            s.Description.Should().NotBeNullOrWhiteSpace();
            s.Url.Should().NotBeNullOrWhiteSpace();
            s.Icons.Should().NotBeEmpty();
        });
    }

    /// <summary>
    /// Verifies that the manifest contains all required PWA fields.
    /// Tests comprehensive manifest structure validation.
    /// </summary>
    [Fact]
    public void BuildManifest_ContainsAllRequiredPwaFields()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", "example.com");

        manifest.Should().NotBeNull();

        // Required PWA fields
        manifest.Name.Should().NotBeNullOrWhiteSpace();
        manifest.ShortName.Should().NotBeNullOrWhiteSpace();
        manifest.StartUrl.Should().Be("/");
        manifest.Scope.Should().Be("/");
        manifest.Display.Should().Be("standalone");
        manifest.Lang.Should().Be("en");

        // Theme values
        manifest.ThemeColor.Should().NotBeNullOrWhiteSpace();
        manifest.BackgroundColor.Should().NotBeNullOrWhiteSpace();

        // Icons list
        manifest.Icons.Should().NotBeEmpty();
        manifest.Icons.Should().HaveCount(2);

        // Categories
        manifest.Categories.Should().NotBeEmpty();
        manifest.Categories.Should().Contain("productivity");
        manifest.Categories.Should().Contain("utilities");
    }

    /// <summary>
    /// Verifies that icon sizes are correct for both icon entries.
    /// Tests specific icon properties.
    /// </summary>
    [Fact]
    public void BuildManifest_IconsHaveCorrectSizes()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", "example.com");

        manifest.Icons[0].Sizes.Should().Be("192x192");
        manifest.Icons[1].Sizes.Should().Be("512x512");
    }

    /// <summary>
    /// Verifies that icon purposes are correct for both icon entries.
    /// Tests specific icon properties.
    /// </summary>
    [Fact]
    public void BuildManifest_IconsHaveCorrectPurpose()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", "example.com");

        manifest.Icons[0].Purpose.Should().Be("any maskable");
        manifest.Icons[1].Purpose.Should().Be("any maskable");
    }

    // ── Configuration overrides ─────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the manifest name is overridden by configuration.
    /// Tests configuration override functionality.
    /// </summary>
    [Fact]
    public void BuildManifest_WhenNameConfigured_UsesConfiguredName()
    {
        var sut = BuildSut(new Dictionary<string, string?>
        {
            ["Manifest:Name"] = "My Custom App"
        });

        var manifest = sut.BuildManifest("https", null);

        manifest.Name.Should().Be("My Custom App");
    }

    /// <summary>
    /// Verifies that the manifest theme color is overridden by configuration.
    /// Tests theme color configuration override.
    /// </summary>
    [Fact]
    public void BuildManifest_WhenThemeColorConfigured_UsesConfiguredColor()
    {
        var sut = BuildSut(new Dictionary<string, string?>
        {
            ["Manifest:ThemeColor"] = "#ff0000"
        });

        var manifest = sut.BuildManifest("https", null);

        manifest.ThemeColor.Should().Be("#ff0000");
    }

    /// <summary>
    /// Verifies that the manifest background color is overridden by configuration.
    /// Tests background color configuration override.
    /// </summary>
    [Fact]
    public void BuildManifest_WhenBackgroundColorConfigured_UsesConfiguredColor()
    {
        var sut = BuildSut(new Dictionary<string, string?>
        {
            ["Manifest:BackgroundColor"] = "#ffffff"
        });

        var manifest = sut.BuildManifest("https", null);

        manifest.BackgroundColor.Should().Be("#ffffff");
    }

    /// <summary>
    /// Verifies that the manifest short name is overridden by configuration.
    /// Tests short name configuration override.
    /// </summary>
    [Fact]
    public void BuildManifest_WhenShortNameConfigured_UsesConfiguredShortName()
    {
        var sut = BuildSut(new Dictionary<string, string?>
        {
            ["Manifest:ShortName"] = "MyApp"
        });

        var manifest = sut.BuildManifest("https", null);

        manifest.ShortName.Should().Be("MyApp");
    }

    /// <summary>
    /// Verifies that the manifest description is overridden by configuration.
    /// Tests description configuration override.
    /// </summary>
    [Fact]
    public void BuildManifest_WhenDescriptionConfigured_UsesConfiguredDescription()
    {
        var sut = BuildSut(new Dictionary<string, string?>
        {
            ["Manifest:Description"] = "Custom application description"
        });

        var manifest = sut.BuildManifest("https", null);

        manifest.Description.Should().Be("Custom application description");
    }

    // ── ThemeColor / BackgroundColor properties ───────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ManifestService.ThemeColor"/> returns the default blue color when no configuration is set.
    /// Tests default theme color value.
    /// </summary>
    [Fact]
    public void ThemeColor_WithNoConfig_ReturnsDefaultBlue()
    {
        var sut = BuildSut();

        sut.ThemeColor.Should().Be("#2563eb");
    }

    /// <summary>
    /// Verifies that <see cref="ManifestService.BackgroundColor"/> returns the default light color when no configuration is set.
    /// Tests default background color value.
    /// </summary>
    [Fact]
    public void BackgroundColor_WithNoConfig_ReturnsDefaultLight()
    {
        var sut = BuildSut();

        sut.BackgroundColor.Should().Be("#f8fafc");
    }

    /// <summary>
    /// Verifies that <see cref="ManifestService.ThemeColor"/> returns the configured value.
    /// Tests ThemeColor property with configuration.
    /// </summary>
    [Fact]
    public void ThemeColor_WhenConfigured_ReturnsConfiguredValue()
    {
        var sut = BuildSut(new Dictionary<string, string?>
        {
            ["Manifest:ThemeColor"] = "#111827"
        });

        sut.ThemeColor.Should().Be("#111827");
    }

    /// <summary>
    /// Verifies that <see cref="ManifestService.BackgroundColor"/> returns the configured value.
    /// Tests BackgroundColor property with configuration.
    /// </summary>
    [Fact]
    public void BackgroundColor_WhenConfigured_ReturnsConfiguredValue()
    {
        var sut = BuildSut(new Dictionary<string, string?>
        {
            ["Manifest:BackgroundColor"] = "#334155"
        });

        sut.BackgroundColor.Should().Be("#334155");
    }

    // ── Scope and display ─────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the manifest scope is set to root '/' by default.
    /// Tests manifest scope field.
    /// </summary>
    [Fact]
    public void BuildManifest_ScopeIsRoot()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", null);

        manifest.Scope.Should().Be("/");
    }

    /// <summary>
    /// Verifies that <see cref="ManifestService.PreferRelatedApplications"/> is false by default.
    /// Tests manifest PreferRelatedApplications field.
    /// </summary>
    [Fact]
    public void BuildManifest_PreferRelatedApplicationsIsFalse()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", null);

        manifest.PreferRelatedApplications.Should().BeFalse();
    }

    /// <summary>
    /// Verifies that the manifest display mode is 'standalone' by default.
    /// Tests manifest display field.
    /// </summary>
    [Fact]
    public void BuildManifest_DisplayModeIsStandalone()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", null);

        manifest.Display.Should().Be("standalone");
    }

    /// <summary>
    /// Verifies that the manifest orientation is 'portrait-primary' by default.
    /// Tests manifest orientation field.
    /// </summary>
    [Fact]
    public void BuildManifest_OrientationIsPortraitPrimary()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", null);

        manifest.Orientation.Should().Be("portrait-primary");
    }
}