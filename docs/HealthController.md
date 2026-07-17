# HealthController

The `HealthController` provides endpoints for monitoring the health status of the ASP.NET Core application and its dependent components. It exposes endpoints to check liveness, readiness, diagnostics, and worker status, as well as a trigger for background tasks. This controller is typically used by orchestration systems (e.g., Kubernetes) to determine whether the application is operational and ready to serve traffic.

## API

### `HealthController`
Public controller exposing endpoints for health monitoring.

### `IActionResult Liveness()`
Checks whether the application is alive and responsive.

- **Parameters:** None.
- **Return value:** `IActionResult` with HTTP 200 OK if the application is running.
- **Throws:** No exceptions.

### `async Task<IActionResult> Readiness()`
Checks whether the application is ready to serve traffic by verifying dependent services (e.g., databases, caches).

- **Parameters:** None.
- **Return value:** `Task<IActionResult>` resolving to HTTP 200 OK if all dependencies are ready, or HTTP 503 Service Unavailable if any dependency is not ready.
- **Throws:** No exceptions.

### `async Task<IActionResult> Diagnostics()`
Provides detailed diagnostic information about the application and its components, including status and timestamps.

- **Parameters:** None.
- **Return value:** `Task<IActionResult>` resolving to HTTP 200 OK with a JSON payload containing `Timestamp`, `Status`, and `Components`.
- **Throws:** No exceptions.

### `async Task<IActionResult> TriggerTask()`
Triggers a background task for execution. Used to simulate or initiate asynchronous processing.

- **Parameters:** None.
- **Return value:** `Task<IActionResult>` resolving to HTTP 202 Accepted if the task was successfully queued, or HTTP 500 Internal Server Error if the task could not be triggered.
- **Throws:** No exceptions.

### `IActionResult GetWorkerStatus()`
Retrieves the status of background workers or asynchronous tasks.

- **Parameters:** None.
- **Return value:** `IActionResult` with HTTP 200 OK and a JSON payload containing `WorkerStatus` (derived from `Status`).
- **Throws:** No exceptions.

### `DateTime Timestamp`
Gets the timestamp associated with the current health check or response.

- **Type:** `DateTime`
- **Access:** Read-only property.
- **Usage:** Automatically set when the health status is evaluated.

### `string Status`
Gets the overall status of the application (e.g., "Healthy", "Unhealthy").

- **Type:** `string`
- **Access:** Read-only property.
- **Usage:** Automatically updated based on readiness checks.

### `Dictionary<string, string> Components`
Gets a dictionary of component names and their statuses (e.g., database, cache).

- **Type:** `Dictionary<string, string>`
- **Access:** Read-only property.
- **Usage:** Populated during readiness checks to reflect the state of each dependency.

## Usage

### Example 1: Checking Liveness
