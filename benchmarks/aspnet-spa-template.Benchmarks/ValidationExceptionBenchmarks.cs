[Benchmark]
[Benchmark(MinTime = 100, MaxTime = 500)]
[MemoryDiagnoser]
public class ValidationExceptionBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
        // Setup test data
    }

    [Benchmark]
    public void Benchmark_ValidationException_ValidationException()
    {
        // Test ValidationException validation
    }

    [Benchmark]
    [Params(10, 100, 1000)]
    public void Benchmark_ValidationException_ValidationException_ValidationExceptionWithParams(int n)
    {
        // Test ValidationException validation with input size n
    }
}