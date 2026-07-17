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
        manifest.Display.Should().Be("standalone");
    }

    /// <summary>
    /// Verifies that the default manifest contains exactly two icons with required properties.
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
        });
    }

    /// <summary>
    /// Verifies that icon source URLs are absolute when a host is provided.
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
    /// </summary>
    [Fact]
    public void BuildManifest_ContainsShortcuts()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", "example.com");

        manifest.Shortcuts.Should().NotBeEmpty();
        manifest.Shortcuts.Should().AllSatisfy(s =>
        {
            s.Name.Should().NotBeNullOrWhiteSpace();
            s.Url.Should().NotBeNullOrWhiteSpace();
        });
    }

    // ── Configuration overrides ─────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the manifest name is overridden by configuration.
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

    // ── ThemeColor / BackgroundColor properties ───────────────────────────────

    /// <summary>
    /// Verifies that <see cref="ManifestService.ThemeColor"/> returns the default blue color when no configuration is set.
    /// </summary>
    [Fact]
    public void ThemeColor_WithNoConfig_ReturnsDefaultBlue()
    {
        var sut = BuildSut();

        sut.ThemeColor.Should().Be("#2563eb");
    }

    /// <summary>
    /// Verifies that <see cref="ManifestService.BackgroundColor"/> returns the default light color when no configuration is set.
    /// </summary>
    [Fact]
    public void BackgroundColor_WithNoConfig_ReturnsDefaultLight()
    {
        var sut = BuildSut();

        sut.BackgroundColor.Should().Be("#f8fafc");
    }

    /// <summary>
    /// Verifies that <see cref="ManifestService.ThemeColor"/> returns the configured value.
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

    // ── Scope and display ─────────────────────────────────────────────────────

    /// <summary>
    /// Verifies that the manifest scope is set to root '/' by default.
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
    /// </summary>
    [Fact]
    public void BuildManifest_PreferRelatedApplicationsIsFalse()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", null);

        manifest.PreferRelatedApplications.Should().BeFalse();
    }
}
