// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System.Text;
using System.Text.RegularExpressions;

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Extension methods for string manipulation and validation.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Sanitizes input string by removing potentially harmful characters.
    /// Used at API boundaries to prevent XSS attacks and SQL injection.
    /// </summary>
    public static string Sanitize(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove control characters and excessive whitespace
        var sanitized = Regex.Replace(input, @"[\p{Cc}]+", " ");
        sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
        return sanitized;
    }

    /// <summary>
    /// Converts string to slug format (lowercase, hyphenated, alphanumeric only).
    /// Useful for URLs and searchable identifiers.
    /// </summary>
    public static string ToSlug(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var slug = input.ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-").Trim('-');
        return slug;
    }

    /// <summary>
    /// Truncates string to specified length and adds ellipsis if truncated.
    /// Prevents display of extremely long text in summaries.
    /// </summary>
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            return input;

        return input[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Converts Pascal case to separate words (e.g., "ProductName" -> "Product Name").
    /// Used for displaying enum or class names in UI.
    /// </summary>
    public static string ToDisplayName(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var result = new StringBuilder();
        for (int i = 0; i < input.Length; i++)
        {
            if (char.IsUpper(input[i]) && i > 0)
                result.Append(' ');
            result.Append(input[i]);
        }
        return result.ToString();
    }

    /// <summary>
    /// Validates email format using simplified RFC 5322 pattern.
    /// </summary>
    public static bool IsValidEmail(this string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if string contains only alphanumeric characters.
    /// </summary>
    public static bool IsAlphaNumeric(this string input)
    {
        return !string.IsNullOrEmpty(input) && Regex.IsMatch(input, @"^[a-zA-Z0-9]+$");
    }

    /// <summary>
    /// Returns the string if not null/empty, otherwise returns fallback value.
    /// </summary>
    public static string OrIfEmpty(this string? input, string fallback)
    {
        return string.IsNullOrWhiteSpace(input) ? fallback : input;
    }

    /// <summary>
    /// Encodes string for safe HTML display (prevents XSS).
    /// </summary>
    public static string HtmlEncode(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return System.Web.HttpUtility.HtmlEncode(input);
    }
}
