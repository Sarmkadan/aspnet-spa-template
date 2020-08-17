#nullable enable
using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class ThemeServiceTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>Creates a ThemeService backed by a real in-process MemoryCacheService.</summary>
    private static ThemeService BuildSut()
    {
        var cache = new MemoryCacheService(NullLogger<MemoryCacheService>.Instance);
        return new ThemeService(cache, NullLogger<ThemeService>.Instance);
    }

    // ── GetSchemeAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSchemeAsync_WhenNothingStored_ReturnsSystem()
    {
        var sut = BuildSut();

        var result = await sut.GetSchemeAsync(userId: 1);

        result.Should().Be(ColourScheme.System);
    }

    [Fact]
    public async Task GetSchemeAsync_AfterStoringDark_ReturnsDark()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 2, ColourScheme.Dark);

        var result = await sut.GetSchemeAsync(userId: 2);

        result.Should().Be(ColourScheme.Dark);
    }

    [Fact]
    public async Task GetSchemeAsync_AfterStoringLight_ReturnsLight()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 3, ColourScheme.Light);

        var result = await sut.GetSchemeAsync(userId: 3);

        result.Should().Be(ColourScheme.Light);
    }

    // ── SetSchemeAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task SetSchemeAsync_WithSystem_ClearsPreference()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 4, ColourScheme.Dark);

        // Overwrite with System — should revert to default
        await sut.SetSchemeAsync(userId: 4, ColourScheme.System);

        var result = await sut.GetSchemeAsync(userId: 4);
        result.Should().Be(ColourScheme.System);
    }

    [Fact]
    public async Task SetSchemeAsync_OverwritesExistingPreference()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 5, ColourScheme.Light);
        await sut.SetSchemeAsync(userId: 5, ColourScheme.Dark);

        var result = await sut.GetSchemeAsync(userId: 5);

        result.Should().Be(ColourScheme.Dark);
    }

    // ── ClearSchemeAsync ───────────────────────────────────────────────────────

    [Fact]
    public async Task ClearSchemeAsync_ReturnsSystemAfterClear()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 6, ColourScheme.Dark);

        await sut.ClearSchemeAsync(userId: 6);

        var result = await sut.GetSchemeAsync(userId: 6);
        result.Should().Be(ColourScheme.System);
    }

    // ── Isolation ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetSchemeAsync_DifferentUsers_AreIsolated()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 10, ColourScheme.Dark);

        var result = await sut.GetSchemeAsync(userId: 11);

        result.Should().Be(ColourScheme.System,
            "user 11 has no preference; user 10's preference must not bleed over");
    }

    // ── Mock-based: verify cache writes use correct key pattern ───────────────

    [Fact]
    public async Task ClearSchemeAsync_CallsCacheRemoveWithUserIdInKey()
    {
        var cacheMock = new Mock<ICacheService>();
        cacheMock
            .Setup(c => c.RemoveAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var sut = new ThemeService(cacheMock.Object, NullLogger<ThemeService>.Instance);

        await sut.ClearSchemeAsync(userId: 77);

        cacheMock.Verify(
            c => c.RemoveAsync(It.Is<string>(k => k.Contains("77"))),
            Times.Once);
    }
}
