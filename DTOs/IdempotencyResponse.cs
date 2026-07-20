#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =====================================================================

namespace AspNetSpaTemplate.DTOs;

/// <summary>
/// Response containing idempotency key information.
/// </summary>
public sealed class IdempotencyResponse
{
    public string IdempotencyKey { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string Message { get; set; } = "Idempotency key generated";
}