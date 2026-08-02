[MemoryDiagnoser]
public class UserBenchmarks
{
    [Benchmark]
    public void BenchmarkMethod1()
    {
        // Test data setup in [GlobalSetup]
        var users = new List<User>();
        for (int i = 0; i < 100; i++)
        {
            users.Add(new User());
        }
        // Benchmark code here
    }

    [Benchmark]
    public void BenchmarkMethod2([Params(10)] int n)
    {
        // Test data setup in [GlobalSetup]
        var users = new List<User>();
        for (int i = 0; i < n; i++)
        {
            users.Add(new User());
        }
        // Benchmark code here
    }

    [Benchmark]
    public void BenchmarkMethod3()
    {
        // Test data setup in [GlobalSetup]
        var users = new List<User>();
        for (int i = 0; i < 1000; i++)
        {
            users.Add(new User());
        }
        // Benchmark code here
    }
}