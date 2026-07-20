#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Data transfer object for status history entry.
/// </summary>
public sealed class StatusHistoryResponse
{
    public int Id { get; set; }

    public string FromStatus { get; set; } = string.Empty;

    public string ToStatus { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string? ChangedBy { get; set; }

    public string? Notes { get; set; }
}