#nullable enable
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="AssetVersioningService"/> class.
/// </summary>
public sealed class AssetVersioningServiceTests
{
    /// <summary>
    /// Builds a new instance of <see cref="AssetVersioningService"/> with a null logger and mock environment for testing purposes.
    /// </summary>
    /// <param name="webRootPath">The web root path to use for testing.</param>
    /// <returns>A new <see cref="AssetVersioningService"/> instance.</returns>
    private static AssetVersioningService BuildSut(string? webRootPath = null)
    {
        var mockEnv = new Mock<IWebHostEnvironment>();
        mockEnv.SetupGet(e => e.WebRootPath).Returns(webRootPath);
        mockEnv.SetupGet(e => e.EnvironmentName).Returns("Development");
        mockEnv.Setup(e => e.WebRootFileProvider).Returns(new NullFileProvider());

        return new AssetVersioningService(mockEnv.Object, NullLogger<AssetVersioningService>.Instance);
    }

    // ── GetAssetManifestAsync ──────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> returns an empty dictionary when web root path is null.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_WithNullWebRoot_ReturnsEmptyDictionary()
    {
        var sut = BuildSut(webRootPath: null);

        var result = await sut.GetAssetManifestAsync();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> returns an empty dictionary when web root path is empty.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_WithEmptyWebRoot_ReturnsEmptyDictionary()
    {
        var sut = BuildSut(webRootPath: string.Empty);

        var result = await sut.GetAssetManifestAsync();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> returns an empty dictionary when web root directory does not exist.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_WithNonExistentWebRoot_ReturnsEmptyDictionary()
    {
        var sut = BuildSut(webRootPath: "/nonexistent/path");

        var result = await sut.GetAssetManifestAsync();

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <c>GetManifestAsync</c> returns consistent version strings for the same content.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_ReturnsDeterministicVersionStrings()
    {
        // Create a temporary directory with test assets
        using var tempDir = new TempDirectory();
        var testFile = Path.Combine(tempDir.Path, "test.js");
        await File.WriteAllTextAsync(testFile, "console.log('Hello World');");

        var sut = BuildSut(tempDir.Path);

        // Get manifest multiple times
        var manifest1 = await sut.GetAssetManifestAsync();
        var manifest2 = await sut.GetAssetManifestAsync();

        // Should return the same manifest
        manifest1.Should().BeEquivalentTo(manifest2);
    }

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> returns different version strings for different content.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_ReturnsDifferentVersionsForDifferentContent()
    {
        // Create two temporary directories with different content
        using var tempDir1 = new TempDirectory();
        var testFile1 = Path.Combine(tempDir1.Path, "test.js");
        await File.WriteAllTextAsync(testFile1, "console.log('Hello World');");

        using var tempDir2 = new TempDirectory();
        var testFile2 = Path.Combine(tempDir2.Path, "test.js");
        await File.WriteAllTextAsync(testFile2, "console.log('Goodbye World');");

        var sut1 = BuildSut(tempDir1.Path);
        var sut2 = BuildSut(tempDir2.Path);

        var manifest1 = await sut1.GetAssetManifestAsync();
        var manifest2 = await sut2.GetAssetManifestAsync();

        // Should have the same number of entries
        manifest1.Count.Should().Be(manifest2.Count);

        // But different version strings
        manifest1.Should().NotBeEquivalentTo(manifest2);
    }

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> includes all watched file extensions.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_IncludesAllWatchedExtensions()
    {
        using var tempDir = new TempDirectory();

        // Create files with different watched extensions
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "app.js"), "console.log('app');");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "styles.css"), "body { margin: 0; }");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "index.html"), "<html></html>");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "data.json"), "{\"key\": \"value\"}");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "favicon.ico"), "fake ico content");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "logo.png"), "fake png content");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "icon.svg"), "<svg></svg>");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "image.webp"), "fake webp content");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "font.woff2"), "fake font content");

        var sut = BuildSut(tempDir.Path);
        var manifest = await sut.GetAssetManifestAsync();

        // Should include all files
        manifest.Count.Should().Be(9);

        // All paths should start with /
        manifest.Keys.Should().AllSatisfy(key => key.Should().StartWith("/"));
    }

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> handles files with uppercase extensions.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_HandlesUppercaseExtensions()
    {
        using var tempDir = new TempDirectory();

        // Create files with uppercase extensions
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "test.JS"), "console.log('test');");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "test.CSS"), "body { margin: 0; }");

        var sut = BuildSut(tempDir.Path);
        var manifest = await sut.GetAssetManifestAsync();

        // Should include both files
        manifest.Count.Should().Be(2);
    }

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> normalizes Windows-style paths to Unix-style.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_NormalizesPathsToUnixStyle()
    {
        using var tempDir = new TempDirectory();
        var testFile = Path.Combine(tempDir.Path, "test.js");
        await File.WriteAllTextAsync(testFile, "console.log('test');");

        var sut = BuildSut(tempDir.Path);
        var manifest = await sut.GetAssetManifestAsync();

        // Path should use forward slashes
        manifest.Keys.Should().ContainSingle(key => key == "/test.js");
    }

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> handles subdirectories.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_IncludesFilesFromSubdirectories()
    {
        using var tempDir = new TempDirectory();

        // Create files in subdirectories
        Directory.CreateDirectory(Path.Combine(tempDir.Path, "js"));
        Directory.CreateDirectory(Path.Combine(tempDir.Path, "css", "themes"));

        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "js", "app.js"), "console.log('app');");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "css", "styles.css"), "body { margin: 0; }");
        await File.WriteAllTextAsync(Path.Combine(tempDir.Path, "css", "themes", "dark.css"), "body { background: #000; }");

        var sut = BuildSut(tempDir.Path);
        var manifest = await sut.GetAssetManifestAsync();

        // Should include all files from subdirectories
        manifest.Count.Should().Be(3);

        // Paths should be relative to web root
        manifest.Should().ContainKey("/js/app.js");
        manifest.Should().ContainKey("/css/styles.css");
        manifest.Should().ContainKey("/css/themes/dark.css");
    }

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> returns version strings of consistent length (8 characters).
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_VersionStringsHaveConsistentLength()
    {
        using var tempDir = new TempDirectory();
        var testFile = Path.Combine(tempDir.Path, "test.js");
        await File.WriteAllTextAsync(testFile, "console.log('test');");

        var sut = BuildSut(tempDir.Path);
        var manifest = await sut.GetAssetManifestAsync();

        foreach (var version in manifest.Values)
        {
            version.Should().MatchRegex("^[a-f0-9]{8}$", "version should be 8 hex characters");
        }
    }

    /// <summary>
    /// Tests that <c>GetAssetManifestAsync</c> returns lowercase version strings.
    /// </summary>
    [Fact]
    public async Task GetAssetManifestAsync_VersionStringsAreLowercase()
    {
        using var tempDir = new TempDirectory();
        var testFile = Path.Combine(tempDir.Path, "test.js");
        await File.WriteAllTextAsync(testFile, "console.log('test');");

        var sut = BuildSut(tempDir.Path);
        var manifest = await sut.GetAssetManifestAsync();

        foreach (var version in manifest.Values)
        {
            version.Should().MatchRegex("^[a-z0-9]{8}$");
        }
    }

    // ── WatchForChangesAsync ──────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <c>WatchForChangesAsync</c> yields paths when files change.
    /// </summary>
    [Fact]
    public async Task WatchForChangesAsync_YieldsChangedPaths()
    {
        using var tempDir = new TempDirectory();
        var testFile = Path.Combine(tempDir.Path, "test.js");
        await File.WriteAllTextAsync(testFile, "console.log('original');");

        var sut = BuildSut(tempDir.Path);
        await sut.StartAsync(CancellationToken.None);

        // Start watching for changes
        var changes = new List<string>();
        var watchTask = Task.Run(async () =>
        {
            await foreach (var path in sut.WatchForChangesAsync())
            {
                changes.Add(path);
            }
        });

        // Wait a bit for the watcher to be ready
        await Task.Delay(200);

        // Modify the file
        await File.WriteAllTextAsync(testFile, "console.log('modified');");

        // Wait for the change to be detected
        await Task.Delay(200);

        // The watcher should have detected the change
        changes.Should().NotBeEmpty();
        changes.Should().Contain("/test.js");

        await sut.StopAsync(CancellationToken.None);
    }

    /// <summary>
    /// Tests that <c>WatchForChangesAsync</c> handles cancellation.
    /// </summary>
    [Fact]
    public async Task WatchForChangesAsync_HandlesCancellation()
    {
        var sut = BuildSut();

        var cts = new CancellationTokenSource();
        cts.CancelAfter(100);

        var changes = new List<string>();
        var watchTask = Task.Run(async () =>
        {
            try
            {
                await foreach (var path in sut.WatchForChangesAsync(cts.Token))
                {
                    changes.Add(path);
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelled
            }
        });

        await watchTask;

        // Should complete
        watchTask.IsCompleted.Should().BeTrue();
    }

    // ── Helper classes ────────────────────────────────────────────────────────────

    /// <summary>
    /// Helper class to create and clean up temporary directories.
    /// </summary>
    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));

        public TempDirectory()
        {
            Directory.CreateDirectory(Path);
        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(Path, true);
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}