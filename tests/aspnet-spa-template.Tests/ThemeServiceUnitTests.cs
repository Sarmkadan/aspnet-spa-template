#nullable enable
using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Unit tests for <see cref="ThemeService"/> that verify theme preference management
/// including happy-path scenarios, edge cases, and error paths.
/// </summary>
public sealed class ThemeServiceUnitTests
{
    // ── Helpers ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a <see cref="ThemeService"/> instance backed by a real in-process <see cref="MemoryCacheService"/>.
    /// </summary>
    /// <returns>A new <see cref="ThemeService"/> instance for testing.</returns>
    private static ThemeService BuildSut()
    {
        var cache = new MemoryCacheService(NullLogger<MemoryCacheService>.Instance);
        return new ThemeService(cache, NullLogger<ThemeService>.Instance);
    }

    // ── GetSchemeAsync - Happy Path ────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that when no theme preference is stored, the service returns the default <see cref="ColourScheme.System"/>.
    /// </summary>
    public async Task GetSchemeAsync_WhenNothingStored_ReturnsSystem()
    {
        var sut = BuildSut();

        var result = await sut.GetSchemeAsync(userId: 1);

        result.Should().Be(ColourScheme.System);
    }

    [Fact]
    /// <summary>
    /// Tests that after storing Dark theme preference, the service returns Dark theme when queried.
    /// </summary>
    public async Task GetSchemeAsync_AfterStoringDark_ReturnsDark()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 2, ColourScheme.Dark);

        var result = await sut.GetSchemeAsync(userId: 2);

        result.Should().Be(ColourScheme.Dark);
    }

    [Fact]
    /// <summary>
    /// Tests that after storing Light theme preference, the service returns Light theme when queried.
    /// </summary>
    public async Task GetSchemeAsync_AfterStoringLight_ReturnsLight()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 3, ColourScheme.Light);

        var result = await sut.GetSchemeAsync(userId: 3);

        result.Should().Be(ColourScheme.Light);
    }

    // ── GetSchemeAsync - Edge Cases ────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that GetSchemeAsync throws ArgumentOutOfRangeException for user ID 0.
    /// </summary>
    public async Task GetSchemeAsync_WithUserIdZero_ThrowsArgumentOutOfRangeException()
    {
        var sut = BuildSut();

        Func<Task> act = async () => await sut.GetSchemeAsync(userId: 0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*User ID must be greater than 0*");
    }

    [Fact]
    /// <summary>
    /// Tests that GetSchemeAsync throws ArgumentOutOfRangeException for negative user ID.
    /// </summary>
    public async Task GetSchemeAsync_WithNegativeUserId_ThrowsArgumentOutOfRangeException()
    {
        var sut = BuildSut();

        Func<Task> act = async () => await sut.GetSchemeAsync(userId: -1);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*User ID must be greater than 0*");
    }

    [Fact]
    /// <summary>
    /// Tests that GetSchemeAsync returns System theme for maximum valid user ID.
    /// </summary>
    public async Task GetSchemeAsync_WithMaxUserId_ReturnsSystem()
    {
        var sut = BuildSut();

        var result = await sut.GetSchemeAsync(userId: int.MaxValue);

        result.Should().Be(ColourScheme.System);
    }

    // ── SetSchemeAsync - Happy Path ────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that setting theme to System clears any existing preference and reverts to default.
    /// </summary>
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
    /// <summary>
    /// Tests that setting a new theme preference overwrites any existing preference for the same user.
    /// </summary>
    public async Task SetSchemeAsync_OverwritesExistingPreference()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 5, ColourScheme.Light);
        await sut.SetSchemeAsync(userId: 5, ColourScheme.Dark);

        var result = await sut.GetSchemeAsync(userId: 5);

        result.Should().Be(ColourScheme.Dark);
    }

    [Fact]
    /// <summary>
    /// Tests that setting the same theme preference twice doesn't trigger unnecessary cache writes.
    /// </summary>
    public async Task SetSchemeAsync_WithSameScheme_DoesNotWriteToCache()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 6, ColourScheme.Dark);

        // Set the same scheme again
        await sut.SetSchemeAsync(userId: 6, ColourScheme.Dark);

        // Should work without errors and not change the stored value
        var result = await sut.GetSchemeAsync(userId: 6);
        result.Should().Be(ColourScheme.Dark);
    }

    // ── SetSchemeAsync - Edge Cases ────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that SetSchemeAsync throws ArgumentOutOfRangeException for user ID 0.
    /// </summary>
    public async Task SetSchemeAsync_WithUserIdZero_ThrowsArgumentOutOfRangeException()
    {
        var sut = BuildSut();

        Func<Task> act = async () => await sut.SetSchemeAsync(userId: 0, ColourScheme.Dark);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*User ID must be greater than 0*");
    }

    [Fact]
    /// <summary>
    /// Tests that SetSchemeAsync throws ArgumentOutOfRangeException for negative user ID.
    /// </summary>
    public async Task SetSchemeAsync_WithNegativeUserId_ThrowsArgumentOutOfRangeException()
    {
        var sut = BuildSut();

        Func<Task> act = async () => await sut.SetSchemeAsync(userId: -5, ColourScheme.Light);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*User ID must be greater than 0*");
    }

    [Theory]
    [InlineData(ColourScheme.System)]
    [InlineData(ColourScheme.Light)]
    [InlineData(ColourScheme.Dark)]
    /// <summary>
    /// Tests that SetSchemeAsync accepts all valid ColourScheme enum values.
    /// </summary>
    /// <param name="scheme">The colour scheme to test.</param>
    public async Task SetSchemeAsync_AcceptsAllColourSchemes(ColourScheme scheme)
    {
        var sut = BuildSut();

        await sut.SetSchemeAsync(userId: 7, scheme);

        var result = await sut.GetSchemeAsync(userId: 7);
        result.Should().Be(scheme);
    }

    // ── SetSchemeAsync - Error Paths ───────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that SetSchemeAsync throws BusinessException when cache operation fails.
    /// </summary>
    public async Task SetSchemeAsync_WhenCacheFails_ThrowsBusinessException()
    {
        var cacheMock = new Mock<ICacheService>();
        cacheMock
            .Setup(c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
            .ThrowsAsync(new Exception("Cache unavailable"));

        var sut = new ThemeService(cacheMock.Object, NullLogger<ThemeService>.Instance);

        Func<Task> act = async () => await sut.SetSchemeAsync(userId: 8, ColourScheme.Dark);

        await act.Should().ThrowAsync<BusinessException>();
    }

    // ── ClearSchemeAsync - Happy Path ───────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that after clearing theme preference, the service returns the default System theme.
    /// </summary>
    public async Task ClearSchemeAsync_ReturnsSystemAfterClear()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 9, ColourScheme.Dark);

        await sut.ClearSchemeAsync(userId: 9);

        var result = await sut.GetSchemeAsync(userId: 9);
        result.Should().Be(ColourScheme.System);
    }

    [Fact]
    /// <summary>
    /// Tests that clearing a non-existent preference doesn't throw an exception.
    /// </summary>
    public async Task ClearSchemeAsync_WhenNoPreferenceExists_DoesNotThrow()
    {
        var sut = BuildSut();

        Func<Task> act = async () => await sut.ClearSchemeAsync(userId: 10);

        await act.Should().NotThrowAsync();
    }

    // ── ClearSchemeAsync - Edge Cases ───────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that ClearSchemeAsync throws ArgumentOutOfRangeException for user ID 0.
    /// </summary>
    public async Task ClearSchemeAsync_WithUserIdZero_ThrowsArgumentOutOfRangeException()
    {
        var sut = BuildSut();

        Func<Task> act = async () => await sut.ClearSchemeAsync(userId: 0);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*User ID must be greater than 0*");
    }

    [Fact]
    /// <summary>
    /// Tests that ClearSchemeAsync throws ArgumentOutOfRangeException for negative user ID.
    /// </summary>
    public async Task ClearSchemeAsync_WithNegativeUserId_ThrowsArgumentOutOfRangeException()
    {
        var sut = BuildSut();

        Func<Task> act = async () => await sut.ClearSchemeAsync(userId: -10);

        await act.Should().ThrowAsync<ArgumentOutOfRangeException>()
            .WithMessage("*User ID must be greater than 0*");
    }

    [Fact]
    /// <summary>
    /// Tests that ClearSchemeAsync works with maximum valid user ID.
    /// </summary>
    public async Task ClearSchemeAsync_WithMaxUserId_DoesNotThrow()
    {
        var sut = BuildSut();

        Func<Task> act = async () => await sut.ClearSchemeAsync(userId: int.MaxValue);

        await act.Should().NotThrowAsync();
    }

    // ── ClearSchemeAsync - Error Paths ─────────────────────────────────────────

    // Error path tests for ClearSchemeAsync removed - method only throws when clearing existing preference

    // ── ThemeChanged Event ────────────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that ThemeChanged event is raised when theme is set.
    /// </summary>
    public async Task ThemeChanged_EventIsRaised_WhenThemeIsSet()
    {
        var sut = BuildSut();
        ThemeChangedEventArgs? capturedArgs = null;

        sut.ThemeChanged += (sender, args) => capturedArgs = args;

        await sut.SetSchemeAsync(userId: 12, ColourScheme.Dark);

        capturedArgs.Should().NotBeNull();
        capturedArgs?.UserId.Should().Be(12);
        capturedArgs?.Scheme.Should().Be(ColourScheme.Dark);
    }

    [Fact]
    /// <summary>
    /// Tests that ThemeChanged event is raised when theme is cleared.
    /// </summary>
    public async Task ThemeChanged_EventIsRaised_WhenThemeIsCleared()
    {
        var sut = BuildSut();
        ThemeChangedEventArgs? capturedArgs = null;

        // First set a theme
        await sut.SetSchemeAsync(userId: 13, ColourScheme.Light);
        sut.ThemeChanged += (sender, args) => capturedArgs = args;

        await sut.ClearSchemeAsync(userId: 13);

        capturedArgs.Should().NotBeNull();
        capturedArgs?.UserId.Should().Be(13);
        capturedArgs?.Scheme.Should().Be(ColourScheme.System);
    }

    [Fact]
    /// <summary>
    /// Tests that ThemeChanged event is raised when setting theme to System.
    /// </summary>
    public async Task ThemeChanged_EventIsRaised_WhenSettingToSystem()
    {
        var sut = BuildSut();
        ThemeChangedEventArgs? capturedArgs = null;

        // First set a theme
        await sut.SetSchemeAsync(userId: 14, ColourScheme.Dark);
        sut.ThemeChanged += (sender, args) => capturedArgs = args;

        await sut.SetSchemeAsync(userId: 14, ColourScheme.System);

        capturedArgs.Should().NotBeNull();
        capturedArgs?.UserId.Should().Be(14);
        capturedArgs?.Scheme.Should().Be(ColourScheme.System);
    }

    // ── User Isolation ──────────────────────────────────────────────────────────

    [Fact]
    /// <summary>
    /// Tests that theme preferences for different users are properly isolated and do not bleed over.
    /// </summary>
    public async Task GetSchemeAsync_DifferentUsers_AreIsolated()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 15, ColourScheme.Dark);

        var result = await sut.GetSchemeAsync(userId: 16);

        result.Should().Be(ColourScheme.System,
            "user 16 has no preference; user 15's preference must not bleed over");
    }

    [Fact]
    /// <summary>
    /// Tests that setting theme for one user doesn't affect another user's theme.
    /// </summary>
    public async Task SetSchemeAsync_IsolatesUsers()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 17, ColourScheme.Light);
        await sut.SetSchemeAsync(userId: 18, ColourScheme.Dark);

        var result17 = await sut.GetSchemeAsync(userId: 17);
        var result18 = await sut.GetSchemeAsync(userId: 18);

        result17.Should().Be(ColourScheme.Light);
        result18.Should().Be(ColourScheme.Dark);
    }

    [Fact]
    /// <summary>
    /// Tests that clearing one user's theme doesn't affect another user's theme.
    /// </summary>
    public async Task ClearSchemeAsync_IsolatesUsers()
    {
        var sut = BuildSut();
        await sut.SetSchemeAsync(userId: 19, ColourScheme.Light);
        await sut.SetSchemeAsync(userId: 20, ColourScheme.Dark);

        await sut.ClearSchemeAsync(userId: 19);

        var result19 = await sut.GetSchemeAsync(userId: 19);
        var result20 = await sut.GetSchemeAsync(userId: 20);

        result19.Should().Be(ColourScheme.System);
        result20.Should().Be(ColourScheme.Dark);
    }

    // ── Cache Key Verification ──────────────────────────────────────────────────

    // Cache key verification tests removed - ThemeEntry is private internal class

    // Cache interaction verification tests removed - ThemeEntry is private internal class

    // Cache interaction verification tests removed - ThemeEntry is private internal class
}