# SyncQueueService

A lightweight in-memory queue service designed for coordinating synchronous background operations within an ASP.NET application. It tracks pending work items, allowing callers to enqueue tasks, inspect pending entries, and mark them as completed or failed. This service is particularly useful for scenarios where operations must be executed sequentially or where progress tracking is required without introducing persistent storage dependencies.

## API

### `public SyncQueueService()`

Initializes a new instance of the `SyncQueueService` with an empty queue.

**Parameters:** None.

**Returns:** A new `SyncQueueService` instance.

**Throws:** None.

---

### `public int Enqueue()`

Adds a new entry to the queue and returns its unique identifier.

**Parameters:** None.

**Returns:** An integer representing the unique identifier (`Id`) of the newly enqueued entry.

**Throws:** None.

---

### `public IReadOnlyList<SyncQueueEntry> GetPending()`

Retrieves a read-only list of all currently pending queue entries.

**Parameters:** None.

**Returns:** An `IReadOnlyList<SyncQueueEntry>` containing all pending entries. The list is a snapshot and will not reflect subsequent modifications to the queue.

**Throws:** None.

---

### `public bool Complete(int id)`

Marks the specified queue entry as completed, removing it from the pending list.

**Parameters:**
- `id` (`int`): The unique identifier of the queue entry to complete.

**Returns:**
- `true` if the entry was found and marked as completed.
- `false` if the entry was not found (either already completed/failed or invalid).

**Throws:** None.

---

### `public bool Fail(int id)`

Marks the specified queue entry as failed, removing it from the pending list.

**Parameters:**
- `id` (`int`): The unique identifier of the queue entry to fail.

**Returns:**
- `true` if the entry was found and marked as failed.
- `false` if the entry was not found (either already completed/failed or invalid).

**Throws:** None.

---

### `public int PendingCount { get; }`

Gets the current number of pending queue entries.

**Parameters:** None.

**Returns:** An integer representing the count of pending entries.

**Throws:** None.

## Usage

### Example 1: Sequential Background Processing
