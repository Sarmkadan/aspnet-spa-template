// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading.Channels;

namespace AspNetSpaTemplate.Services;

/// <summary>Provides asset versioning and live-change notification for the offline-first SPA.</summary>
public interface IAssetVersioningService
{
    /// <summary>Returns a mapping of asset paths to their short content-hash versions.</summary>
    Task<IReadOnlyDictionary<string, string>> GetAssetManifestAsync(CancellationToken cancellationToken = default);

    /// <summary>Yields the relative path of each asset that changes while the caller holds a subscription.</summary>
    IAsyncEnumerable<string> WatchForChangesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Computes truncated SHA-256 content hashes for wwwroot assets, exposes them as an asset
/// manifest for the service worker, and in development mode watches the file system to
/// broadcast changes to all active HMR subscribers via async channels.
/// </summary>
public sealed class AssetVersioningService : IAssetVersioningService, IHostedService, IDisposable
{
    private static readonly string[] WatchedExtensions =
        [".js", ".css", ".html", ".json", ".ico", ".png", ".svg", ".webp", ".woff2"];

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AssetVersioningService> _logger;
    private readonly ConcurrentDictionary<string, string> _manifest = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, Channel<string>> _subscribers = new();
    private FileSystemWatcher? _watcher;

    /// <summary>Initializes the service with the web hosting environment and a logger.</summary>
    public AssetVersioningService(IWebHostEnvironment environment, ILogger<AssetVersioningService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, string>> GetAssetManifestAsync(CancellationToken cancellationToken = default)
    {
        if (_manifest.IsEmpty)
            await BuildManifestAsync(cancellationToken);

        return _manifest;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> WatchForChangesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var id = Guid.NewGuid().ToString("N");
        var channel = Channel.CreateUnbounded<string>(new UnboundedChannelOptions { SingleWriter = false });
        _subscribers.TryAdd(id, channel);

        try
        {
            await foreach (var path in channel.Reader.ReadAllAsync(cancellationToken))
                yield return path;
        }
        finally
        {
            _subscribers.TryRemove(id, out _);
        }
    }

    /// <summary>Builds the initial manifest and, in development, starts watching for file changes.</summary>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await BuildManifestAsync(cancellationToken);

        if (!_environment.IsDevelopment())
            return;

        var root = _environment.WebRootPath;
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            return;

        _watcher = new FileSystemWatcher(root)
        {
            IncludeSubdirectories = true,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
        _watcher.Created += OnFileChanged;
        _watcher.Renamed += (s, e) => OnFileChanged(s, e);

        _logger.LogInformation("HMR file watcher active for {WebRoot}", root);
    }

    /// <summary>Stops the watcher and completes all active subscriber channels.</summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();

        foreach (var (_, ch) in _subscribers)
            ch.Writer.TryComplete();

        return Task.CompletedTask;
    }

    private async Task BuildManifestAsync(CancellationToken cancellationToken)
    {
        var root = _environment.WebRootPath;
        if (string.IsNullOrEmpty(root) || !Directory.Exists(root))
            return;

        var files = Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories)
            .Where(f => WatchedExtensions.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));

        foreach (var file in files)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            try
            {
                var hash = await ComputeHashAsync(file, cancellationToken);
                var key = "/" + Path.GetRelativePath(root, file).Replace('\\', '/');
                _manifest[key] = hash;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not hash asset {File}", file);
            }
        }

        _logger.LogDebug("Asset manifest ready: {Count} entries", _manifest.Count);
    }

    private async void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (!WatchedExtensions.Contains(Path.GetExtension(e.FullPath), StringComparer.OrdinalIgnoreCase))
            return;

        try
        {
            await Task.Delay(120); // absorb rapid successive saves

            var hash = await ComputeHashAsync(e.FullPath, CancellationToken.None);
            var root = _environment.WebRootPath;
            var key = "/" + Path.GetRelativePath(root, e.FullPath).Replace('\\', '/');

            if (_manifest.TryGetValue(key, out var current) && current == hash)
                return; // content unchanged — skip broadcast

            _manifest[key] = hash;
            _logger.LogDebug("Asset updated: {Path} ({Hash})", key, hash);

            foreach (var (_, ch) in _subscribers)
                await ch.Writer.WriteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing change for {File}", e.FullPath);
        }
    }

    private static async Task<string> ComputeHashAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        var bytes = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexString(bytes)[..8].ToLowerInvariant();
    }

    /// <inheritdoc/>
    public void Dispose() => _watcher?.Dispose();
}
