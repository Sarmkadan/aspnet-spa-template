[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 1000)]
[MemoryDiagnoser]
public class NotificationServiceBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Set up realistic test data here
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_SendNotification()
    {
        // Benchmark the SendNotification method
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_SendNotificationAsync()
    {
        // Benchmark the SendNotificationAsync method
    }

    [Benchmark]
    [Params(1, 10, 100)]
    public void Benchmark_GetNotifications()
    {
        // Benchmark the GetNotifications method
    }
}
