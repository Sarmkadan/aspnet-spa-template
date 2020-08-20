#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

namespace AspNetSpaTemplate.Exceptions;

/// <summary>
/// Exception thrown when data validation fails.
/// </summary>
public sealed class ValidationException : AspNetSpaTemplateException
{
    public Dictionary<string, List<string>> Errors { get; }

    /// <summary>
    /// The name of the field that failed validation, when this exception represents
    /// a single-field failure. Returns the first key in <see cref="Errors"/>, if any.
    /// </summary>
    public string? Field => Errors.Keys.FirstOrDefault();

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
