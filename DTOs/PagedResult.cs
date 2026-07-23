#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Unified envelope for paginated list endpoints.
/// Every paginated endpoint returns this shape so clients can rely on a single
/// contract (<see cref="Items"/>, <see cref="Page"/>, <see cref="PageSize"/>,
/// <see cref="TotalCount"/>, <see cref="TotalPages"/>, <see cref="HasNext"/>)
/// regardless of the underlying resource.
/// </summary>
/// <typeparam name="T">The type of the items in the page.</typeparam>
/// <param name="Items">The items contained in the current page.</param>
/// <param name="Page">The current page number (1-based).</param>
/// <param name="PageSize">The number of items requested per page.</param>
/// <param name="TotalCount">The total number of items across all pages.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    /// <summary>
    /// Gets the total number of pages given <see cref="TotalCount"/> and <see cref="PageSize"/>.
    /// </summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling(TotalCount / (double)PageSize) : 0;

    /// <summary>
    /// Gets a value indicating whether a subsequent page exists after <see cref="Page"/>.
    /// </summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>
    /// Creates an empty <see cref="PagedResult{T}"/> for the given page and page size,
    /// used when a filter (e.g. an unrecognized category) matches no data.
    /// </summary>
    /// <param name="page">The requested page number (1-based).</param>
    /// <param name="pageSize">The requested page size.</param>
    /// <returns>A <see cref="PagedResult{T}"/> with an empty <see cref="Items"/> collection and zero total count.</returns>
    public static PagedResult<T> Empty(int page, int pageSize) =>
        new(Array.Empty<T>(), page, pageSize, 0);

    /// <summary>
    /// Creates a <see cref="PagedResult{T}"/> from a materialized page of items.
    /// </summary>
    /// <param name="items">The items contained in the current page.</param>
    /// <param name="page">The current page number (1-based).</param>
    /// <param name="pageSize">The number of items requested per page.</param>
    /// <param name="totalCount">The total number of items across all pages.</param>
    /// <returns>A new <see cref="PagedResult{T}"/> instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="items"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="totalCount"/> is negative.</exception>
    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        ArgumentNullException.ThrowIfNull(items);
        if (totalCount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalCount), totalCount, "Total count cannot be negative.");

        return new PagedResult<T>(items, page, pageSize, totalCount);
    }
}
