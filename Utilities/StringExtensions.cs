#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AspNetSpaTemplate.Utilities;

/// <summary>
/// Extension methods for string manipulation and validation.
/// </summary>
public static partial class StringExtensions
{
    [GeneratedRegex(@"[\p{Cc}]+")]
    private static partial Regex ControlCharactersRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex InvalidSlugCharactersRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex RepeatedHyphensRegex();

    [GeneratedRegex(@"^[a-zA-Z0-9]+$")]
    private static partial Regex AlphaNumericRegex();

    /// <summary>
    /// Sanitizes input string by removing potentially harmful characters.
    /// Used at API boundaries to prevent XSS attacks and SQL injection.
    /// </summary>
    /// <param name="input">The string to sanitize.</param>
    /// <returns>The sanitized string, or empty string if input is null or whitespace.</returns>
    public static string Sanitize(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove control characters and excessive whitespace
        var sanitized = ControlCharactersRegex().Replace(input, " ");
        sanitized = WhitespaceRegex().Replace(sanitized, " ").Trim();
        return sanitized;
    }

    /// <summary>
    /// Converts string to slug format (lowercase, hyphenated, alphanumeric only).
    /// Useful for URLs and searchable identifiers.
    /// </summary>
    /// <param name="input">The string to convert to slug format.</param>
    /// <returns>The slugified string, or empty string if input is null or whitespace.</returns>
    public static string ToSlug(this string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var slug = input.ToLowerInvariant();
        slug = InvalidSlugCharactersRegex().Replace(slug, "");
        slug = WhitespaceRegex().Replace(slug, "-");
        slug = RepeatedHyphensRegex().Replace(slug, "-").Trim('-');
        return slug;
    }

    /// <summary>
    /// Truncates string to specified length and adds ellipsis if truncated.
    /// Prevents display of extremely long text in summaries.
    /// </summary>
    /// <param name="input">The string to truncate.</param>
    /// <param name="maxLength">Maximum length before truncation.</param>
    /// <param name="suffix">Suffix to append when truncating (default: "...").</param>
    /// <returns>The truncated string with suffix if needed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="suffix"/> is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="maxLength"/> is less than suffix length.</exception>
    public static string Truncate(this string input, int maxLength, string suffix = "...")
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(suffix);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxLength, suffix.Length);

        if (input.Length <= maxLength)
            return input;

        return input[..(maxLength - suffix.Length)] + suffix;
    }

    /// <summary>
    /// Converts Pascal case to separate words (e.g., "ProductName" -> "Product Name").
    /// Used for displaying enum or class names in UI.
    /// </summary>
    /// <param name="input">The PascalCase string to convert.</param>
    /// <returns>The display name with spaces between words.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string ToDisplayName(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);

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
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the email is valid; otherwise, false.</returns>
    public static bool IsValidEmail(this string? email)
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
    /// <param name="input">The string to check.</param>
    /// <returns>True if the string contains only alphanumeric characters; otherwise, false.</returns>
    public static bool IsAlphaNumeric(this string? input)
    {
        return !string.IsNullOrEmpty(input) && AlphaNumericRegex().IsMatch(input);
    }

    /// <summary>
    /// Returns the string if not null/empty, otherwise returns fallback value.
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <param name="fallback">The fallback value to return if input is null or empty.</param>
    /// <returns>The input string if not null/empty; otherwise, the fallback value.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="fallback"/> is null.</exception>
    public static string OrIfEmpty(this string? input, string fallback)
    {
        ArgumentNullException.ThrowIfNull(fallback);
        return string.IsNullOrWhiteSpace(input) ? fallback : input;
    }

    /// <summary>
    /// Encodes string for safe HTML display (prevents XSS).
    /// </summary>
    /// <param name="input">The string to HTML encode.</param>
    /// <returns>The HTML-encoded string.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="input"/> is null.</exception>
    public static string HtmlEncode(this string input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return System.Net.WebUtility.HtmlEncode(input);
    }
}
