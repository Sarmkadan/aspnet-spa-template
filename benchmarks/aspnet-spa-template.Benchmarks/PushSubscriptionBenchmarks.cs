using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnosers;
using AspNetSpaTemplate.Models;

namespace AspNetSpaTemplate.Benchmarks
{
    [MemoryDiagnoser]
    public class PushSubscriptionBenchmarks
    {
        private PushSubscription _singleSubscription = null!;
        private PushSubscription[] _subscriptionList = null!;

        [Params(10, 100, 1000)]
        public int N { get; set; }

        [GlobalSetup]
        public void Setup()
        {
            // Initialize a single subscription for isolated method benchmarks
            _singleSubscription = new PushSubscription
            {
                Id = 1,
                UserId = 42,
                Endpoint = "https://fcm.googleapis.com/gcm/send/endpoint",
                P256dhKey = "B_P256DH_KEY_BASE64URL",
                AuthKey = "AUTH_KEY_BASE64URL",
                DeviceLabel = "Test Device",
                UserAgent = "Mozilla/5.0",
                IsActive = true
            };

            // Initialize an array of subscriptions for batch processing benchmarks
            _subscriptionList = new PushSubscription[N];
            for (int i = 0; i < N; i++)
            {
                _subscriptionList[i] = new PushSubscription
                {
                    Id = i + 1,
                    UserId = 42,
                    Endpoint = $"https://fcm.googleapis.com/gcm/send/endpoint_{i}",
                    P256dhKey = "B_P256DH_KEY_BASE64URL",
                    AuthKey = "AUTH_KEY_BASE64URL",
                    IsActive = true
                };
            }
        }

        [Benchmark]
        public void RecordDelivery_Single()
        {
            _singleSubscription.RecordDelivery();
        }

        [Benchmark]
        public void Deactivate_Single()
        {
            _singleSubscription.Deactivate();
        }

        [Benchmark]
        public void RecordDelivery_Batch()
        {
            for (int i = 0; i < N; i++)
            {
                _subscriptionList[i].RecordDelivery();
            }
        }

        [Benchmark]
        public void Deactivate_Batch()
        {
            for (int i = 0; i < N; i++)
            {
                _subscriptionList[i].Deactivate();
            }
        }
    }
}
