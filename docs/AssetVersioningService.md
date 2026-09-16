# AssetVersioningService

The `AssetVersioningService` class provides a concrete implementation of the `IAssetVersioningService` interface for managing asset versioning and live-change notifications in the ASP.NET SPA template. It computes truncated SHA-256 content hashes for wwwroot assets, exposes them as an asset manifest for service workers, and in development mode watches the file system to broadcast changes to all active HMR subscribers via async channels.

## API

### `AssetVersioningService`
Represents the concrete implementation of the `IAssetVersioningService` interface. This class is instantiated to provide the actual logic for reading asset manifests and watching for file system changes. It requires dependency injection of `IWebHostEnvironment` and `ILogger<AssetVersioningService>` to function correctly.

### `GetAssetManifestAsync`
```csharp
public async Task<IReadOnlyDictionary<string, string>> GetAssetManifestAsync(CancellationToken cancellationToken = default)
```
Returns a mapping of asset paths to their short content-hash versions. On first call, it builds the manifest by scanning the web root for watched file types and computing their hashes. Subsequent calls return the cached manifest unless `BuildManifestAsync` is invoked again.
*   **Return Value**: A read-only dictionary where the key is the original asset path (relative to web root, starting with '/') and the value is an 8-character lowercase hex string representing the truncated SHA-256 hash of the file's content.
*   **Exceptions**: May throw an `IOException` or `UnauthorizedAccessException` if files cannot be read due to permissions or locking issues. May throw `OperationCanceledException` if the cancellation token is triggered during the manifest build process.

### `WatchForChangesAsync`
```csharp
public async IAsyncEnumerable<string> WatchForChangesAsync(CancellationToken cancellationToken = default)
```
Yields the relative path of each asset that changes while the caller holds a subscription. Creates an isolated subscription channel for each caller, ensuring that change notifications are broadcast to all active subscribers without interference.
*   **Return Value**: An `IAsyncEnumerable<string>` that produces the path (relative to web root, starting with '/') of each changed asset each time a modification is detected and processed.
*   **Exceptions**: May throw if the underlying file system watcher fails to initialize or if the monitoring directory becomes inaccessible. The enumeration completes if the service is stopped or disposed, or if the cancellation token is triggered.

### `StartAsync`
```csharp
public async Task StartAsync(CancellationToken cancellationToken)
```
Initializes the service and begins the background processes required for monitoring asset changes. Builds the initial asset manifest and, in development environments, starts a `FileSystemWatcher` to monitor the web root for file changes.
*   **Return Value**: A `Task` that completes when the service has successfully started and is ready to serve requests or watch for changes.
*   **Exceptions**: May throw if critical resources (such as file handles for the watcher) cannot be acquired. Does not throw if the service is already running; subsequent calls are safe no-ops.

### `StopAsync`
```csharp
public Task StopAsync(CancellationToken cancellationToken)
```
Gracefully shuts down the service, stopping any active file watchers and completing all active subscriber channels. Disposes the `FileSystemWatcher` and signals all subscriber channels to complete.
*   **Return Value**: A `Task` that completes when all background operations have ceased and resources are safely released.
*   **Exceptions**: May throw if the service encounters an error while attempting to close file handles or cancel pending operations, though implementations typically swallow such errors to prevent shutdown failures.

### `Dispose`
```csharp
public void Dispose()
```
Releases unmanaged resources used by the service immediately. Disposes the underlying `FileSystemWatcher` if it exists. Note that this method performs synchronous cleanup and does not await asynchronous operations.
*   **Remarks**: After disposal, calling other methods on the instance may result in undefined behavior. It is recommended to call `StopAsync` before disposing of the instance to ensure graceful termination of file watchers and completion of active subscriptions.

## Implementation Details

The service monitors files with the following extensions: `.js`, `.css`, `.html`, `.json`, `.ico`, `.png`, `.svg`, `.webp`, `.woff2`.

In development mode (`IWebHostEnvironment.IsDevelopment()` returns `true`), the service activates a `FileSystemWatcher` with `IncludeSubdirectories = true` and monitors for `LastWrite`, `FileName`, and `DirectoryName` changes. Changes are debounced with a 120ms delay to absorb rapid successive saves (common during editor auto-save).

Asset hashes are computed using SHA-256, truncated to the first 8 hexadecimal characters, and converted to lowercase invariant format.

The service uses a `ConcurrentDictionary` to store the asset manifest and another to manage subscriber channels, ensuring thread-safe access without locks.

## Usage

### Example: Basic Service Consumption
The following example demonstrates how to inject and use the service in a typical ASP.NET Core application:

```csharp
public class AssetServiceConsumer
{
    private readonly IAssetVersioningService _assetVersioningService;

    public AssetServiceConsumer(IAssetVersioningService assetVersioningService)
    {
        _assetVersioningService = assetVersioningService;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        // Start the background monitoring
        await _assetVersioningService.StartAsync(cancellationToken);
        
        // Get initial manifest
        var manifest = await _assetVersioningService.GetAssetManifestAsync(cancellationToken);
        
        // Process manifest as needed
        foreach (var (assetPath, version) in manifest)
        {
            Console.WriteLine($"Asset: {assetPath} -> v{version}");
        }
    }
}
```

### Example: Subscribing to Changes
This example shows how to subscribe to asset change notifications:

```csharp
public class AssetChangeHandler
{
    private readonly IAssetVersioningService _assetVersioningService;

    public AssetChangeHandler(IAssetVersioningService assetVersioningService)
    {
        _assetVersioningService = assetVersioningService;
    }

    public async Task StartMonitoringAsync(CancellationToken cancellationToken)
    {
        await _assetVersioningService.StartAsync(cancellationToken);
        
        await foreach (var changedAsset in _assetVersioningService.WatchForChangesAsync(cancellationToken))
        {
            Console.WriteLine($"Asset changed: {changedAsset}");
            // Trigger cache invalidation, rebuild, or notification logic here
        }
    }
}
```