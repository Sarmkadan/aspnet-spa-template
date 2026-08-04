[MemoryDiagnoser]
public class BackgroundTaskSchedulerBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Initialize test data here
    }

    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test method 1
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void BenchmarkMethod2(int inputSize)
    {
        // Test method 2 with input size
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test method 3
    }
}