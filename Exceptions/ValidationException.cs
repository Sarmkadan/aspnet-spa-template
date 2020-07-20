// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Exception thrown when data validation fails.
/// </summary>
public class ValidationException : Exception
{
    public Dictionary<string, List<string>> Errors { get; }

    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, List<string>>();
    }

    public ValidationException(Dictionary<string, List<string>> errors)
        : base("Validation failed. See Errors property for details.")
    {
        Errors = errors ?? new Dictionary<string, List<string>>();
    }

    public ValidationException(string fieldName, string errorMessage)
        : base($"Validation failed: {errorMessage}")
    {
        Errors = new Dictionary<string, List<string>>
        {
            { fieldName, new List<string> { errorMessage } }
        };
    }

    public void AddError(string fieldName, string errorMessage)
    {
        if (!Errors.ContainsKey(fieldName))
            Errors[fieldName] = new List<string>();

        Errors[fieldName].Add(errorMessage);
    }
}
