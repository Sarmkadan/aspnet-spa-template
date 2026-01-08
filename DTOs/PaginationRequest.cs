// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Standard request DTO for paginated API endpoints.
/// Encapsulates pagination parameters with validation.
/// </summary>
public class PaginationRequest
{
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = Math.Max(1, value);
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = Math.Max(1, Math.Min(value, 100)); // Cap at 100 to prevent DOS
    }

    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
    public string? SearchTerm { get; set; }
    public Dictionary<string, string>? Filters { get; set; }

    /// <summary>
    /// Validates pagination parameters.
    /// Throws ArgumentException if invalid.
    /// </summary>
    public void Validate()
    {
        if (PageNumber < 1)
            throw new ArgumentException("PageNumber must be >= 1");
        if (PageSize < 1)
            throw new ArgumentException("PageSize must be >= 1");
        if (PageSize > 100)
            throw new ArgumentException("PageSize cannot exceed 100");
    }

    /// <summary>
    /// Calculates skip count for database queries.
    /// </summary>
    public int GetSkip() => (PageNumber - 1) * PageSize;
}

/// <summary>
/// Standard response DTO for paginated data.
/// Encapsulates data with pagination metadata.
/// </summary>
public class PaginationResponse<T>
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
public class FilterCriteria
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
public class SortCriteria
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
