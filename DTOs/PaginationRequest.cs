#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Standard request DTO for paginated API endpoints.
/// Encapsulates pagination parameters with automatic normalization and bounds enforcement.
/// Prevents unbounded page sizes and integer overflow attacks.
/// </summary>
public sealed class PaginationRequest
{
    private const int MaxPageSize = 100;
    private int _pageNumber = 1;
    private int _pageSize = 10;

    /// <summary>
    /// Gets or sets the page number (1-based).
    /// Values less than 1 are coerced to 1.
    /// </summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = Math.Max(1, value);
    }

    /// <summary>
    /// Gets or sets the page size.
    /// Values less than 1 are coerced to 1.
    /// Values greater than <see cref="MaxPageSize"/> are coerced to <see cref="MaxPageSize"/>.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Max(1, Math.Min(value, MaxPageSize));
    }

    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public string? SearchTerm { get; set; }
    public Dictionary<string, string>? Filters { get; set; }

    /// <summary>
    /// Calculates skip count for database queries with overflow protection.
    /// Uses checked arithmetic to prevent integer overflow from hostile inputs like PageNumber = int.MaxValue.
    /// </summary>
    /// <exception cref="OverflowException">Thrown when the calculated skip value would overflow.</exception>
    public int GetSkip()
    {
        // Calculate skip with overflow protection: (PageNumber - 1) * PageSize
        // If PageNumber is 1, skip is 0 regardless of PageSize
        // If PageSize is 0 (shouldn't happen due to setter), result is 0
        // Check for potential overflow before performing the multiplication
        if (PageNumber > 1)
        {
            checked
            {
                return (PageNumber - 1) * PageSize;
            }
        }
        return 0;
    }
}

/// <summary>
/// Standard response DTO for paginated data.
/// Encapsulates data with pagination metadata.
/// </summary>
public sealed class PaginationResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

    public static PaginationResponse<T> Create(List<T> items, int pageNumber, int pageSize, int totalCount)
    {
        return new PaginationResponse<T>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
}

/// <summary>
/// Filter criteria for advanced searching.
/// </summary>
public sealed class FilterCriteria
{
    public string Field { get; set; } = "";
    public string Operator { get; set; } = "="; // =, !=, >, <, >=, <=, contains, startswith, endswith
    public string Value { get; set; } = "";

    public bool IsValid()
    {
        var validOperators = new[] { "=", "!=", ">", "<", ">=", "<=", "contains", "startswith", "endswith" };
        return !string.IsNullOrEmpty(Field) && validOperators.Contains(Operator) && !string.IsNullOrEmpty(Value);
    }
}

/// <summary>
/// Sort criteria for ordering results.
/// </summary>
public sealed class SortCriteria
{
    public string Field { get; set; } = "";
    public SortDirection Direction { get; set; } = SortDirection.Ascending;

    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Field);
    }
}

public enum SortDirection
{
    Ascending = 0,
    Descending = 1
}
