# BackgroundTaskScheduler
The `BackgroundTaskScheduler` class is designed to manage and execute background tasks in an ASP.NET application. It provides a way to register tasks, start and stop the scheduler, and monitor the status of tasks. This allows developers to offload computationally expensive or time-consuming operations to the background, improving the overall performance and responsiveness of the application.

## API
* `public BackgroundTaskScheduler`: The constructor for the `BackgroundTaskScheduler` class.
* `public void RegisterTask`: Registers a task with the scheduler. This method does not return a value and does not throw any exceptions.
* `public async Task StartAsync`: Starts the scheduler. This method returns a `Task` that represents the asynchronous operation and throws an exception if the scheduler is already running.
* `public async Task StopAsync`: Stops the scheduler. This method returns a `Task` that represents the asynchronous operation and throws an exception if the scheduler is not running.
* `public IEnumerable<BackgroundTaskStatus> GetStatus`: Retrieves the status of all registered tasks. This method returns a collection of `BackgroundTaskStatus` objects and does not throw any exceptions.
* `public async Task TriggerTaskAsync`: Triggers the execution of a task. This method returns a `Task` that represents the asynchronous operation and throws an exception if the task is not registered.
* `public void Dispose`: Disposes of the scheduler and releases any unmanaged resources. This method does not return a value and does not throw any exceptions.
* `public string TaskName`: Gets the name of the task. This property returns a `string` value and does not throw any exceptions.
* `public bool IsRunning`: Gets a value indicating whether the scheduler is running. This property returns a `bool` value and does not throw any exceptions.
* `public DateTime? LastExecutedAt`: Gets the date and time when the task was last executed. This property returns a nullable `DateTime` value and does not throw any exceptions.
* `public DateTime? NextExecutionAt`: Gets the date and time when the task is scheduled to be executed next. This property returns a nullable `DateTime` value and does not throw any exceptions.
* `public TimeSpan? LastExecutionDuration`: Gets the duration of the last execution of the task. This property returns a nullable `TimeSpan` value and does not throw any exceptions.
* `public int ExecutionCount`: Gets the number of times the task has been executed. This property returns an `int` value and does not throw any exceptions.
* `public int FailureCount`: Gets the number of times the task has failed. This property returns an `int` value and does not throw any exceptions.
* `public string? LastError`: Gets the error message of the last failure. This property returns a nullable `string` value and does not throw any exceptions.
* `public static IServiceCollection AddBackgroundTaskScheduler`: Adds the background task scheduler to the service collection. This method returns the `IServiceCollection` instance and does not throw any exceptions.
* `public static IServiceCollection AddBackgroundTask<T>`: Adds a background task of type `T` to the service collection. This method returns the `IServiceCollection` instance and does not throw any exceptions.
* `public static async Task<IApplicationBuilder> UseBackgroundTaskScheduler`: Configures the application to use the background task scheduler. This method returns a `Task` that represents the asynchronous operation and throws an exception if the configuration fails.

## Usage
The following example demonstrates how to register a task and start the scheduler:
```csharp
public class MyTask : IBackgroundTask
{
    public async Task ExecuteAsync()
    {
        // Task implementation
    }
}

public void ConfigureServices(IServiceCollection services)
{
    services.AddBackgroundTaskScheduler();
    services.AddBackgroundTask<MyTask>();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseBackgroundTaskScheduler();
}
```
The following example demonstrates how to trigger a task and monitor its status:
```csharp
public class MyController : Controller
{
    private readonly BackgroundTaskScheduler _scheduler;

    public MyController(BackgroundTaskScheduler scheduler)
    {
        _scheduler = scheduler;
    }

    public async Task<IActionResult> TriggerTask()
    {
        await _scheduler.TriggerTaskAsync();
        return Ok();
    }

    public IActionResult GetTaskStatus()
    {
        var status = _scheduler.GetStatus();
        return Ok(status);
    }
}
```

## Notes
* The `BackgroundTaskScheduler` class is not thread-safe. It is recommended to use a singleton instance or a scoped instance per request.
* The `StartAsync` and `StopAsync` methods are asynchronous and may throw exceptions if the scheduler is already running or not running, respectively.
* The `TriggerTaskAsync` method may throw an exception if the task is not registered.
* The `GetStatus` method returns a collection of `BackgroundTaskStatus` objects, which may be empty if no tasks are registered.
* The `AddBackgroundTaskScheduler` and `AddBackgroundTask<T>` methods are extension methods that add the background task scheduler and tasks to the service collection, respectively.
* The `UseBackgroundTaskScheduler` method configures the application to use the background task scheduler and returns a `Task` that represents the asynchronous operation.
