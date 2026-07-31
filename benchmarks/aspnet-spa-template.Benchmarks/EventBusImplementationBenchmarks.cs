[MemoryDiagnoser]
public class EventBusImplementationBenchmarks
{
    [Benchmark]
    public void Benchmark_EventBusImplementation()
    {
        // setup and test data
    }

    [Benchmark]
    public void Benchmark_EventBusImplementation_Params([Params(10, 100, 1000)])
    {
        // setup and test data
    }
}
