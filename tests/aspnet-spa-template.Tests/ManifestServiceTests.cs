#nullable enable
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

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

    [Fact]
    public void BuildManifest_WithHost_IconSrcsAreAbsolute()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", "my.example.com");

        manifest.Icons.Should().AllSatisfy(i =>
            i.Src.Should().StartWith("https://my.example.com"));
    }

    [Fact]
    public void BuildManifest_WithoutHost_IconSrcsAreRelative()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", null);

        manifest.Icons.Should().AllSatisfy(i =>
            i.Src.Should().StartWith("/icons/"));
    }

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

    // ── Configuration overrides ────────────────────────────────────────────────

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

    [Fact]
    public void ThemeColor_WithNoConfig_ReturnsDefaultBlue()
    {
        var sut = BuildSut();

        sut.ThemeColor.Should().Be("#2563eb");
    }

    [Fact]
    public void BackgroundColor_WithNoConfig_ReturnsDefaultLight()
    {
        var sut = BuildSut();

        sut.BackgroundColor.Should().Be("#f8fafc");
    }

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

    [Fact]
    public void BuildManifest_ScopeIsRoot()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", null);

        manifest.Scope.Should().Be("/");
    }

    [Fact]
    public void BuildManifest_PreferRelatedApplicationsIsFalse()
    {
        var sut = BuildSut();

        var manifest = sut.BuildManifest("https", null);

        manifest.PreferRelatedApplications.Should().BeFalse();
    }
}
