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
    public int Id { get; set; public override string ToString() => $"StatusHistoryResponse {{ Id = {Id}, FromStatus = {FromStatus}, ToStatus = {ToStatus}, ChangedAt = {ChangedAt}, ChangedBy = {ChangedBy}, Notes = {Notes} }}";
}

    public string FromStatus { get; set; public override string ToString() => $"StatusHistoryResponse {{ Id = {Id}, FromStatus = {FromStatus}, ToStatus = {ToStatus}, ChangedAt = {ChangedAt}, ChangedBy = {ChangedBy}, Notes = {Notes} }}";
} = string.Empty;

    public string ToStatus { get; set; public override string ToString() => $"StatusHistoryResponse {{ Id = {Id}, FromStatus = {FromStatus}, ToStatus = {ToStatus}, ChangedAt = {ChangedAt}, ChangedBy = {ChangedBy}, Notes = {Notes} }}";
} = string.Empty;

    public DateTime ChangedAt { get; set; public override string ToString() => $"StatusHistoryResponse {{ Id = {Id}, FromStatus = {FromStatus}, ToStatus = {ToStatus}, ChangedAt = {ChangedAt}, ChangedBy = {ChangedBy}, Notes = {Notes} }}";
}

    public string? ChangedBy { get; set; public override string ToString() => $"StatusHistoryResponse {{ Id = {Id}, FromStatus = {FromStatus}, ToStatus = {ToStatus}, ChangedAt = {ChangedAt}, ChangedBy = {ChangedBy}, Notes = {Notes} }}";
}

    public string? Notes { get; set; public override string ToString() => $"StatusHistoryResponse {{ Id = {Id}, FromStatus = {FromStatus}, ToStatus = {ToStatus}, ChangedAt = {ChangedAt}, ChangedBy = {ChangedBy}, Notes = {Notes} }}";
}
public override string ToString() => $"StatusHistoryResponse {{ Id = {Id}, FromStatus = {FromStatus}, ToStatus = {ToStatus}, ChangedAt = {ChangedAt}, ChangedBy = {ChangedBy}, Notes = {Notes} }}";
}