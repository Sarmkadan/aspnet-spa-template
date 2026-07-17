# IAssetVersioningService

The `IAssetVersioningService` interface defines the contract for managing asset versioning and manifest retrieval within the ASP.NET SPA template architecture. It provides mechanisms to asynchronously fetch the current mapping of asset paths to their versioned identifiers, monitor the underlying file system or build artifacts for changes in real-time, and control the lifecycle of the background monitoring process. This service is essential for ensuring that client-side applications always reference the correct, cache-busted versions of static assets without requiring manual intervention or application restarts.

## API

### `AssetVersioningService`
Represents the concrete implementation of the `IAssetVersioningService` interface. This class is instantiated to provide the actual logic for reading asset manifests and watching for file system changes. It typically requires dependency injection of file providers or hosting environment services to function correctly.

### `GetAssetManifestAsync`
```csharp
public async Task<IReadOnlyDictionary<string, string>> GetAssetManifestAsync()
```
Retrieves the current asset manifest asynchronously. This method reads the latest mapping of logical asset names to their versioned physical paths (e.g., mapping `app.js` to `app.a1b2c3.js`).
*   **Return Value**: A read-only dictionary where the key is the original asset path and the value is the versioned asset path.
*   **Exceptions**: May throw an `IOException` or `FileNotFoundException` if the manifest file is missing, locked, or malformed. It may also throw `OperationCanceledException` if the underlying cancellation token is triggered during the read operation.

### `WatchForChangesAsync`
```csharp
public async IAsyncEnumerable<string> WatchForChangesAsync()
```
Starts an asynchronous stream that yields notifications whenever a change is detected in the monitored assets or the manifest file itself. This allows consumers to react immediately to new builds or file modifications without polling.
*   **Return Value**: An `IAsyncEnumerable<string>` that produces the path or identifier of the changed asset each time a modification occurs.
*   **Exceptions**: May throw if the underlying file watcher fails to initialize or if the monitoring directory becomes inaccessible. The enumeration completes if the service is stopped or disposed.

### `StartAsync`
```csharp
public async Task StartAsync()
```
Initializes the service and begins the background processes required for monitoring asset changes. This method should be called during application startup, typically within a hosted service lifecycle.
*   **Return Value**: A `Task` that completes when the service has successfully started and is ready to serve requests or watch for changes.
*   **Exceptions**: May throw if the service is already running or if critical resources (such as file handles) cannot be acquired.

### `StopAsync`
```csharp
public Task StopAsync()
```
Gracefully shuts down the service, stopping any active file watchers and releasing resources associated with the monitoring process. This is intended to be called during application shutdown.
*   **Return Value**: A `Task` that completes when all background operations have ceased and resources are safely released.
*   **Exceptions**: May throw if the service encounters an error while attempting to close file handles or cancel pending operations.

### `Dispose`
```csharp
public void Dispose()
```
Releases unmanaged resources used by the service immediately. This method implements the standard disposal pattern and should be called if the service is instantiated outside of a dependency injection container that manages its lifetime.
*   **Remarks**: After disposal, calling other methods on the instance may result in an `ObjectDisposedException`.

## Usage

### Example 1: Retrieving the Manifest at Startup
The following example demonstrates how to inject the service into a hosted service to load the initial asset manifest during application startup, ensuring the SPA entry point references the correct bundled files.

```csharp
public class SpaInitializationService : IHostedService
{
    private readonly IAssetVersioningService _versioningService;
    private IReadOnlyDictionary<string, string> _manifest;

    public SpaInitializationService(IAssetVersioningService versioningService)
    {
        _versioningService = versioningService;
        _manifest = new Dictionary<string, string>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        // Start the background watcher
        await _versioningService.StartAsync();
        
        // Fetch the initial manifest
        _manifest = await _versioningService.GetAssetManifestAsync();
        
        if (_manifest.TryGetValue("main.js", out var versionedMain))
        {
            Console.WriteLine($"SPA Entry Point: {versionedMain}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _versioningService.StopAsync();
    }
}
```

### Example 2: Reacting to File Changes in Real-Time
This example illustrates how to consume the `WatchForChangesAsync` stream to invalidate cached responses or trigger hot-reload signals when underlying assets are modified.

```csharp
public class AssetChangeMonitor
{
    private readonly IAssetVersioningService _versioningService;

    public AssetChangeMonitor(IAssetVersioningService versioningService)
    {
        _versioningService = versioningService;
    }

    public async Task MonitorLoopAsync(CancellationToken cancellationToken)
    {
        await _versioningService.StartAsync();

        try
        {
            await foreach (var changedAsset in _versioningService.WatchForChangesAsync().WithCancellation(cancellationToken))
            {
                Console.WriteLine($"Detected change in asset: {changedAsset}");
                
                // Logic to invalidate cache or notify connected clients
                InvalidateCdnCache(changedAsset);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
        }
        finally
        {
            await _versioningService.StopAsync();
        }
    }

    private void InvalidateCdnCache(string assetPath)
    {
        // Implementation specific to CDN or local cache invalidation
    }
}
```

## Notes

*   **Thread Safety**: The `GetAssetManifestAsync` method is safe for concurrent calls; however, the state of the returned dictionary represents a snapshot at the time of the call. Consumers should not assume the dictionary updates automatically; they must call the method again to retrieve fresh data after a change notification.
*   **Lifecycle Management**: `StartAsync` must be invoked before `WatchForChangesAsync` or `GetAssetManifestAsync` is called in scenarios where the underlying file watcher requires explicit initialization. Calling `GetAssetManifestAsync` prior to `StartAsync` may succeed depending on implementation details but is not guaranteed if the manifest relies on the watcher's context.
*   **Disposal Behavior**: The `Dispose` method performs a synchronous cleanup. If `StopAsync` has not been called prior to `Dispose`, pending asynchronous operations in `WatchForChangesAsync` may be aborted abruptly. It is recommended to call `StopAsync` before disposing of the instance to ensure graceful termination of file watchers.
*   **Enumeration Completion**: The `IAsyncEnumerable<string>` returned by `WatchForChangesAsync` will complete naturally if `StopAsync` is called or if the service is disposed. Consumers should handle the completion of the stream gracefully rather than expecting it to run indefinitely without external lifecycle management.
