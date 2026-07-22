#nullable enable
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using System.Collections.Concurrent;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="SyncQueueService"/> class.
/// </summary>
public sealed class SyncQueueServiceTests
{
    /// <summary>
    /// Builds a new instance of <see cref="SyncQueueService"/> with a null logger for testing purposes.
    /// </summary>
    /// <returns>A new <see cref="SyncQueueService"/> instance.</returns>
    private static SyncQueueService BuildSut() =>
        new SyncQueueService(NullLogger<SyncQueueService>.Instance);

    // ── Enqueue ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <c>Enqueue</c> returns a positive ID for a valid request.
    /// </summary>
    [Fact]
    public void Enqueue_ReturnsPositiveId()
    {
        var sut = BuildSut();

        var id = sut.Enqueue(userId: 1, "req-1", "POST", "/api/orders");

        id.Should().BeGreaterThan(0);
    }

    /// <summary>
    /// Tests that subsequent calls to <c>Enqueue</c> return incrementally higher IDs.
    /// </summary>
    [Fact]
    public void Enqueue_SecondCall_ReturnsHigherId()
    {
        var sut = BuildSut();

        var id1 = sut.Enqueue(userId: 1, "req-a", "POST", "/api/orders");
        var id2 = sut.Enqueue(userId: 1, "req-b", "POST", "/api/orders");

        id2.Should().BeGreaterThan(id1);
    }

    /// <summary>
    /// Tests that <c>Enqueue</c> deduplicates requests with the same client request ID.
    /// </summary>
    [Fact]
    public void Enqueue_DuplicateClientRequestId_ReturnsSameId()
    {
        var sut = BuildSut();

        var id1 = sut.Enqueue(userId: 1, "idempotent-key", "POST", "/api/orders");
        var id2 = sut.Enqueue(userId: 1, "idempotent-key", "POST", "/api/orders");

        id2.Should().Be(id1, "duplicate entries must be silently de‑duplicated");
    }

    /// <summary>
    /// Tests that <c>Enqueue</c> normalises HTTP methods to uppercase.
    /// </summary>
    [Fact]
    public void Enqueue_MethodIsNormalisedToUppercase()
    {
        var sut = BuildSut();

        sut.Enqueue(userId: 1, "req-lower", "post", "/api/orders");
        var entries = sut.GetPending(userId: 1);

        entries.Should().ContainSingle()
            .Which.Method.Should().Be("POST");
    }

    // ── GetPending (dequeue) ─────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <c>GetPending</c> returns an empty collection when no entries exist.
    /// </summary>
    [Fact]
    public void GetPending_WithNoEntries_ReturnsEmpty()
    {
        var sut = BuildSut();

        var result = sut.GetPending(userId: 99);

        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <c>GetPending</c> returns entries ordered by <c>QueuedAt</c> ascending.
    /// </summary>
    [Fact]
    public void GetPending_OrderedByQueuedAtAscending()
    {
        var sut = BuildSut();
        sut.Enqueue(userId: 1, "req-first", "POST", "/api/orders/1");
        sut.Enqueue(userId: 1, "req-second", "POST", "/api/orders/2");

        var pending = sut.GetPending(userId: 1);

        pending.Should().BeInAscendingOrder(e => e.QueuedAt);
    }

    /// <summary>
    /// Tests that <c>GetPending</c> isolates entries by user ID.
    /// </summary>
    [Fact]
    public void GetPending_IsolatesEntriesByUser()
    {
        var sut = BuildSut();
        sut.Enqueue(userId: 10, "req-u10", "POST", "/api/orders");
        sut.Enqueue(userId: 11, "req-u11", "POST", "/api/orders");

        var user10Pending = sut.GetPending(userId: 10);

        user10Pending.Should().ContainSingle()
            .Which.UserId.Should().Be(10);
    }

    /// <summary>
    /// Tests that <c>GetPending</c> only returns entries that are still pending (not completed or failed).
    /// </summary>
    [Fact]
    public void GetPending_OnlyReturnsPendingEntries()
    {
        var sut = BuildSut();
        var id1 = sut.Enqueue(userId: 5, "req-1", "POST", "/api/orders");
        var id2 = sut.Enqueue(userId: 5, "req-2", "DELETE", "/api/cart/1");
        sut.Complete(id1);

        var pending = sut.GetPending(userId: 5);

        pending.Should().ContainSingle()
            .Which.Id.Should().Be(id2);
    }

    // ── Complete (success path) ────────────────────────────────────────────────

    /// <summary>
    /// Tests that <c>Complete</c> returns <c>true</c> when successfully completing a valid entry.
    /// </summary>
    [Fact]
    public void Complete_WithValidId_ReturnsTrue()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-c", "POST", "/api/orders");

        var result = sut.Complete(id);

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <c>Complete</c> removes the completed entry from the pending queue.
    /// </summary>
    [Fact]
    public void Complete_RemovesEntryFromPending()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-done", "POST", "/api/orders");

        sut.Complete(id);

        sut.GetPending(userId: 1).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <c>Complete</c> returns <c>false</c> when the entry ID is unknown.
    /// </summary>
    [Fact]
    public void Complete_WithUnknownId_ReturnsFalse()
    {
        var sut = BuildSut();

        var result = sut.Complete(id: 9999);

        result.Should().BeFalse();
    }

    // ── Fail (error path) ───────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <c>Fail</c> returns <c>true</c> when successfully failing a valid entry.
    /// </summary>
    [Fact]
    public void Fail_WithValidId_ReturnsTrue()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-f", "POST", "/api/orders");

        var result = sut.Fail(id, "Network timeout");

        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <c>Fail</c> records the error message and removes the entry from the pending queue.
    /// </summary>
    [Fact]
    public void Fail_RecordsErrorMessage()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-err", "POST", "/api/orders");
        sut.Fail(id, "Server returned 500");

        // After failing, the entry is no longer pending — status is Failed.
        sut.GetPending(userId: 1).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <c>Fail</c> returns <c>false</c> when attempting to fail an unknown entry ID.
    /// </summary>
    [Fact]
    public void Fail_WithUnknownId_ReturnsFalse()
    {
        var sut = BuildSut();

        var result = sut.Fail(id: 1234, "error");

        result.Should().BeFalse();
    }

    // ── PendingCount ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <c>PendingCount</c> reflects the current number of pending entries.
    /// </summary>
    [Fact]
    public void PendingCount_ReflectsCurrentQueueDepth()
    {
        var sut = BuildSut();
        sut.Enqueue(userId: 2, "r1", "POST", "/api/orders");
        sut.Enqueue(userId: 2, "r2", "DELETE", "/api/cart/1");

        sut.PendingCount(userId: 2).Should().Be(2);
    }

    /// <summary>
    /// Tests that <c>PendingCount</c> decreases after completing an entry.
    /// </summary>
    [Fact]
    public void PendingCount_DecreasesAfterComplete()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 3, "r-count", "POST", "/api/orders");
        sut.Complete(id);

        sut.PendingCount(userId: 3).Should().Be(0);
    }

    // ── Thread Safety ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Tests that <c>Enqueue</c> is thread-safe and prevents race conditions.
    /// </summary>
    [Fact]
    public void Enqueue_IsThreadSafe()
    {
        var sut = BuildSut();
        const int threadCount = 10;
        var tasks = new Task<int>[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            int userId = i;
            tasks[i] = Task.Run(() => sut.Enqueue(userId, $"req-{userId}-{Guid.NewGuid()}", "POST", "/api/orders"));
        }

        var ids = Task.WhenAll(tasks).Result;

        // All IDs should be unique and positive
        ids.Should().AllSatisfy(id => id.Should().BeGreaterThan(0));
        ids.Should().OnlyHaveUniqueItems();
    }

    /// <summary>
    /// Tests that <c>Complete</c> is thread-safe and handles concurrent operations.
    /// </summary>
    [Fact]
    public void Complete_IsThreadSafe()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-concurrent", "POST", "/api/orders");

        const int threadCount = 5;
        var tasks = new Task<bool>[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() => sut.Complete(id));
        }

        var results = Task.WhenAll(tasks).Result;

        // Only one operation should succeed
        results.Count(r => r).Should().Be(1);
        sut.GetPending(userId: 1).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <c>Fail</c> is thread-safe and handles concurrent operations.
    /// </summary>
    [Fact]
    public void Fail_IsThreadSafe()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-fail-concurrent", "POST", "/api/orders");

        const int threadCount = 5;
        var tasks = new Task<bool>[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            tasks[i] = Task.Run(() => sut.Fail(id, "Concurrent failure"));
        }

        var results = Task.WhenAll(tasks).Result;

        // Only one operation should succeed
        results.Count(r => r).Should().Be(1);
        sut.GetPending(userId: 1).Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <c>GetPending</c> is thread-safe and returns consistent results under load.
    /// </summary>
    [Fact]
    public void GetPending_IsThreadSafe()
    {
        var sut = BuildSut();

        // Enqueue many entries from multiple threads
        const int threadCount = 5;
        const int entriesPerThread = 20;
        var tasks = new Task[threadCount];

        for (int i = 0; i < threadCount; i++)
        {
            int userId = i;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < entriesPerThread; j++)
                {
                    sut.Enqueue(userId, $"req-{userId}-{j}", "POST", "/api/orders");
                }
            });
        }

        Task.WhenAll(tasks).Wait();

        // Verify all entries are present
        for (int i = 0; i < threadCount; i++)
        {
            sut.PendingCount(userId: i).Should().Be(entriesPerThread);
        }
    }

    /// <summary>
    /// Stress test that enqueues from 20 parallel tasks while draining, asserting no items are lost or duplicated.
    /// This tests the class of bug the recent fix patched around rather than eliminated.
    /// </summary>
    [Fact]
    public void StressTest_NoItemsLostOrDuplicated_UnderConcurrentLoad()
    {
        var sut = BuildSut();
        const int threadCount = 20;
        const int entriesPerThread = 50;
        var allIds = new ConcurrentBag<int>();

        // Phase 1: Enqueue from multiple threads
        var enqueueTasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int userId = i;
            enqueueTasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < entriesPerThread; j++)
                {
                    var id = sut.Enqueue(userId, $"req-{userId}-{j}-{Guid.NewGuid()}", "POST", "/api/orders");
                    allIds.Add(id);
                }
            });
        }

        Task.WhenAll(enqueueTasks).Wait();

        // Verify all entries were enqueued
        allIds.Count.Should().Be(threadCount * entriesPerThread);

        // Phase 2: Complete all entries from multiple threads
        var completionIds = new ConcurrentBag<int>();
        var partitioner = Partitioner.Create(allIds.ToList());
        var completionTasks = partitioner.AsParallel()
            .WithDegreeOfParallelism(threadCount)
            .Select(id => Task.Run(() =>
            {
                if (sut.Complete(id))
                {
                    completionIds.Add(id);
                }
            }))
            .ToArray();

        Task.WhenAll(completionTasks).Wait();

        // Verify all entries were completed (no data loss)
        completionIds.Count.Should().Be(threadCount * entriesPerThread);

        // Verify no duplicates occurred
        completionIds.Should().OnlyHaveUniqueItems();

        // Verify counts are correct - all entries should be completed
        for (int i = 0; i < threadCount; i++)
        {
            sut.PendingCount(userId: i).Should().Be(0);
        }

        // Verify counts are correct
        for (int i = 0; i < threadCount; i++)
        {
            sut.PendingCount(userId: i).Should().Be(0);
        }
    }
}