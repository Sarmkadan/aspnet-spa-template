using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using AspNetSpaTemplate.Integration;

namespace aspnet_spa_template.Benchmarks
{
    /// <summary>
    /// Benchmarks for the <see cref="WebhookHandler"/> class.
    /// </summary>
    [MemoryDiagnoser]
    public class WebhookHandlerBenchmarks
    {
        // The handler is created via reflection so the code compiles even if the
        // concrete type does not have a parameter‑less constructor.
        private dynamic _handler = null!;

        // Payloads used by the benchmarks.
        private List<string> _payloads = null!;

        // Vary the number of payloads to simulate different loads.
        [Params(10, 100, 1000)]
        public int PayloadCount { get; set; }

        /// <summary>
        /// Sets up a fresh <see cref="WebhookHandler"/> instance and generates
        /// dummy JSON payloads for the benchmarks.
        /// </summary>
        [GlobalSetup]
        public void GlobalSetup()
        {
            // Create an instance of WebhookHandler. If the type requires
            // constructor arguments, they can be supplied here; for now we
            // assume a parameter‑less constructor or that nulls are acceptable.
            _handler = Activator.CreateInstance(typeof(WebhookHandler));

            // Generate simple JSON payloads. Each payload contains an id and a
            // string of 100 characters to give the formatter something to work
            // with without being too heavyweight.
            _payloads = new List<string>(PayloadCount);
            for (int i = 0; i < PayloadCount; i++)
            {
                var payload = $"{{\"id\":{i},\"data\":\"{new string('x', 100)}\"}}";
                _payloads.Add(payload);
            }
        }

        /// <summary>
        /// Benchmark handling a single payload – the baseline case.
        /// </summary>
        [Benchmark]
        public async Task HandleSinglePayload()
        {
            await _handler.HandleAsync(_payloads[0]);
        }

        /// <summary>
        /// Benchmark handling multiple payloads sequentially.
        /// </summary>
        [Benchmark]
        public async Task HandleMultiplePayloads()
        {
            foreach (var payload in _payloads)
            {
                await _handler.HandleAsync(payload);
            }
        }

        /// <summary>
        /// Benchmark the signature validation logic (if present).
        /// </summary>
        [Benchmark]
        public async Task ValidateSignatures()
        {
            foreach (var payload in _payloads)
            {
                await _handler.ValidateSignatureAsync(payload);
            }
        }
    }
}
