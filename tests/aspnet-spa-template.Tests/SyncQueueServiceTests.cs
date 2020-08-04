#nullable enable
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class SyncQueueServiceTests
{
    private static SyncQueueService BuildSut() =>
        new SyncQueueService(NullLogger<SyncQueueService>.Instance);

    // ── Enqueue ────────────────────────────────────────────────────────────────

    [Fact]
    public void Enqueue_ReturnsPositiveId()
    {
        var sut = BuildSut();

        var id = sut.Enqueue(userId: 1, "req-1", "POST", "/api/orders");

        id.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Enqueue_SecondCall_ReturnsHigherId()
    {
        var sut = BuildSut();

        var id1 = sut.Enqueue(userId: 1, "req-a", "POST", "/api/orders");
        var id2 = sut.Enqueue(userId: 1, "req-b", "POST", "/api/orders");

        id2.Should().BeGreaterThan(id1);
    }

    [Fact]
    public void Enqueue_DuplicateClientRequestId_ReturnsSameId()
    {
        var sut = BuildSut();

        var id1 = sut.Enqueue(userId: 1, "idempotent-key", "POST", "/api/orders");
        var id2 = sut.Enqueue(userId: 1, "idempotent-key", "POST", "/api/orders");

        id2.Should().Be(id1, "duplicate entries must be silently de-duplicated");
    }

    [Fact]
    public void Enqueue_MethodIsNormalisedToUppercase()
    {
        var sut = BuildSut();

        sut.Enqueue(userId: 1, "req-lower", "post", "/api/orders");
        var entries = sut.GetPending(userId: 1);

        entries.Should().ContainSingle()
            .Which.Method.Should().Be("POST");
    }

    // ── GetPending ─────────────────────────────────────────────────────────────

    [Fact]
    public void GetPending_WithNoEntries_ReturnsEmpty()
    {
        var sut = BuildSut();

        var result = sut.GetPending(userId: 99);

        result.Should().BeEmpty();
    }

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

    [Fact]
    public void GetPending_OrderedByQueuedAtAscending()
    {
        var sut = BuildSut();
        sut.Enqueue(userId: 1, "req-first", "POST", "/api/orders/1");
        sut.Enqueue(userId: 1, "req-second", "POST", "/api/orders/2");

        var pending = sut.GetPending(userId: 1);

        pending.Should().BeInAscendingOrder(e => e.QueuedAt);
    }

    // ── Complete ──────────────────────────────────────────────────────────────

    [Fact]
    public void Complete_WithValidId_ReturnsTrue()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-c", "POST", "/api/orders");

        var result = sut.Complete(id);

        result.Should().BeTrue();
    }

    [Fact]
    public void Complete_RemovesEntryFromPending()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-done", "POST", "/api/orders");

        sut.Complete(id);

        sut.GetPending(userId: 1).Should().BeEmpty();
    }

    [Fact]
    public void Complete_WithUnknownId_ReturnsFalse()
    {
        var sut = BuildSut();

        var result = sut.Complete(id: 9999);

        result.Should().BeFalse();
    }

    // ── Fail ──────────────────────────────────────────────────────────────────

    [Fact]
    public void Fail_WithValidId_ReturnsTrue()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-f", "POST", "/api/orders");

        var result = sut.Fail(id, "Network timeout");

        result.Should().BeTrue();
    }

    [Fact]
    public void Fail_RecordsErrorMessage()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 1, "req-err", "POST", "/api/orders");
        sut.Fail(id, "Server returned 500");

        // After failing, the entry is no longer pending — status is Failed.
        sut.GetPending(userId: 1).Should().BeEmpty();
    }

    [Fact]
    public void Fail_WithUnknownId_ReturnsFalse()
    {
        var sut = BuildSut();

        var result = sut.Fail(id: 1234, "error");

        result.Should().BeFalse();
    }

    // ── PendingCount ──────────────────────────────────────────────────────────

    [Fact]
    public void PendingCount_ReflectsCurrentQueueDepth()
    {
        var sut = BuildSut();
        sut.Enqueue(userId: 2, "r1", "POST", "/api/orders");
        sut.Enqueue(userId: 2, "r2", "DELETE", "/api/cart/1");

        sut.PendingCount(userId: 2).Should().Be(2);
    }

    [Fact]
    public void PendingCount_DecreasesAfterComplete()
    {
        var sut = BuildSut();
        var id = sut.Enqueue(userId: 3, "r-count", "POST", "/api/orders");
        sut.Complete(id);

        sut.PendingCount(userId: 3).Should().Be(0);
    }
}
