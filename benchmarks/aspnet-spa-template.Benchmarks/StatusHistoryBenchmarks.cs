using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace aspnet_spa_template.Benchmarks;

[MemoryDiagnoser]
public class StatusHistoryBenchmarks
{
    private List<StatusHistory> _statusHistories = null!;

    [Params(10, 100, 1000)]
    public int N { get; set; }

    [GlobalSetup]
    public void GlobalSetup()
    {
        _statusHistories = new List<StatusHistory>(N);
        for (int i = 0; i < N; i++)
        {
            _statusHistories.Add(new StatusHistory
            {
                Id = i,
                OrderId = i % 50,
                FromStatus = OrderStatus.Pending,
                ToStatus = i % 2 == 0 ? OrderStatus.Shipped : OrderStatus.Delivered,
                ChangedAt = DateTime.UtcNow.AddDays(-i),
                ChangedBy = "User" + i,
                Notes = "Status change note " + i
            });
        }
    }

    [Benchmark]
    public List<StatusHistory> Filter_By_OrderId()
    {
        return _statusHistories.Where(h => h.OrderId == 10).ToList();
    }

    [Benchmark]
    public List<StatusHistory> Filter_By_Status_Range()
    {
        return _statusHistories
            .Where(h => h.FromStatus == OrderStatus.Pending && h.ToStatus == OrderStatus.Shipped)
            .ToList();
    }

    [Benchmark]
    public string Serialize_List()
    {
        return JsonSerializationHelper.Serialize(_statusHistories);
    }
}
