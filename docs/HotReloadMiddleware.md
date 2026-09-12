# HotReloadMiddleware

Middleware component that supports development-time asset hot reloading by serving an asset manifest, exposing a Server-Sent Events (SSE) stream for asset changes, and applying service-worker headers to `/sw.js` requests.

## API

### `public HotReloadMiddleware(RequestDelegate next, IAssetVersioningService versioning, IWebHostEnvironment environment, ILogger<HotReloadMiddleware> logger)`

Constructor that initializes the hot reload middleware with the next pipeline delegate and the services required for asset versioning, environment detection, and logging.

- **Parameters**:
  - `next` – The `RequestDelegate` representing the next middleware in the pipeline.
  - `versioning` – The `IAssetVersioningService` used to retrieve the asset manifest and watch for asset changes.
  - `environment` – The `IWebHostEnvironment` used to restrict hot reload behavior to the Development environment.
  - `logger` – The logger used to record HMR client connections and disconnections.
- **Exceptions**:
  - Throws `ArgumentNullException` if any parameter is `null`.

---

### `public async Task InvokeAsync(HttpContext context)`

Processes development-time asset requests and forwards all other requests to the next middleware.

- **Parameters**:
  - `context` – The `HttpContext` for the current HTTP request.
- **Return value**: A `Task` representing the asynchronous operation.
- **Behavior**:
  - Outside the Development environment, immediately forwards the request to the next middleware without adding headers or handling hot reload endpoints.
  - For `/__asset-manifest.json`, returns a non-cached JSON response containing the current Unix-time version, generation timestamp, and asset manifest. The request is not forwarded.
  - For `/__hmr`, opens a non-cached SSE response, sends a `connected` event, and then sends an `asset-changed` event containing the changed path and timestamp for each change reported by the asset versioning service. Proxy buffering is disabled through the `X-Accel-Buffering` response header. The request is not forwarded.
  - For `/sw.js`, adds `Service-Worker-Allowed: /` and `Cache-Control: no-cache, no-store, must-revalidate`, then forwards the request so the service-worker file can be served by later middleware.
  - For every other request, forwards the request unchanged to the next middleware.

## Notes

- **Development only**: Manifest, HMR, and service-worker handling is enabled only when `IWebHostEnvironment.IsDevelopment()` returns `true`.
- **Request cancellation**: Manifest generation, response writes, stream flushing, and asset watching use `HttpContext.RequestAborted`.
- **HMR disconnects**: Cancellation of an HMR stream is treated as a client disconnect and logged at debug level.
- **Manifest caching**: Asset manifest responses use `Cache-Control: no-cache` so clients revalidate the manifest.
- **SSE format**: The HMR stream uses `text/event-stream; charset=utf-8` and emits standard `event` and `data` fields separated by a blank line.
