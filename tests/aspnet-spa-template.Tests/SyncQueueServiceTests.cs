#nullable enable
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
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
	/// Tests that Enqueue returns a positive ID for valid requests.
	/// </summary>
	[Fact]
	public void Enqueue_ReturnsPositiveId()
	{
		var sut = BuildSut();

		var id = sut.Enqueue(userId: 1, "req-1", "POST", "/api/orders");

		id.Should().BeGreaterThan(0);
	}

	/// <summary>
	/// Tests that subsequent calls to Enqueue return incrementally higher IDs.
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
	/// Tests that Enqueue deduplicates requests with the same client request ID.
	/// </summary>
	[Fact]
	public void Enqueue_DuplicateClientRequestId_ReturnsSameId()
	{
		var sut = BuildSut();

		var id1 = sut.Enqueue(userId: 1, "idempotent-key", "POST", "/api/orders");
		var id2 = sut.Enqueue(userId: 1, "idempotent-key", "POST", "/api/orders");

		id2.Should().Be(id1, "duplicate entries must be silently de-duplicated");
	}

	/// <summary>
	/// Tests that Enqueue normalizes HTTP methods to uppercase.
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

	// ── GetPending ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that GetPending returns an empty collection when no entries exist.
	/// </summary>
	[Fact]
	public void GetPending_WithNoEntries_ReturnsEmpty()
	{
		var sut = BuildSut();

		var result = sut.GetPending(userId: 99);

		result.Should().BeEmpty();
	}

	/// <summary>
	/// Tests that GetPending only returns entries that are still pending (not completed).
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

	/// <summary>
	/// Tests that GetPending isolates entries by user ID.
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
	/// Tests that GetPending returns entries ordered by queued timestamp in ascending order.
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

	// ── Complete ──────────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that Complete returns true when successfully completing a valid entry.
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
	/// Tests that Complete removes the completed entry from the pending queue.
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
	/// Tests that Complete returns false when attempting to complete an unknown entry ID.
	/// </summary>
	[Fact]
	public void Complete_WithUnknownId_ReturnsFalse()
	{
		var sut = BuildSut();

		var result = sut.Complete(id: 9999);

		result.Should().BeFalse();
	}

	// ── Fail ──────────────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that Fail returns true when successfully failing a valid entry.
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
	/// Tests that Fail records the error message and removes the entry from pending queue.
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
	/// Tests that Fail returns false when attempting to fail an unknown entry ID.
	/// </summary>
	[Fact]
	public void Fail_WithUnknownId_ReturnsFalse()
	{
		var sut = BuildSut();

		var result = sut.Fail(id: 1234, "error");

		result.Should().BeFalse();
	}

	// ── PendingCount ────────────────────────────────────────────────────────────

	/// <summary>
	/// Tests that PendingCount reflects the current number of pending entries.
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
	/// Tests that PendingCount decreases after completing an entry.
	/// </summary>
	[Fact]
	public void PendingCount_DecreasesAfterComplete()
	{
		var sut = BuildSut();
		var id = sut.Enqueue(userId: 3, "r-count", "POST", "/api/orders");
		sut.Complete(id);

		sut.PendingCount(userId: 3).Should().Be(0);
	}
}