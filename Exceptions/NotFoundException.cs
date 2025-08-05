// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found in the system.
/// </summary>
public class NotFoundException : Exception
{
    public string? ResourceType { get; }
    public object? ResourceId { get; }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string resourceType, object resourceId)
        : base($"{resourceType} with ID '{resourceId}' was not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }
}
