#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Tests for string extension methods.
/// </summary>
public sealed class StringExtensionsTests
{
    /// <summary>
    /// Tests the Sanitize method when the input is null or whitespace.
    /// </summary>
    /// <param name="input">The input string to test.</param>
    /// <param name="expected">The expected result of the Sanitize method.</param>
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Sanitize_WhenInputIsNullOrWhiteSpace_ReturnsEmptyString(string? input, string expected)
    {
        input.Sanitize().Should().Be(expected);
    }

    /// <summary>
    /// Tests the Sanitize method when the input has excessive whitespace.
    /// </summary>
    [Fact]
    public void Sanitize_WhenInputHasExcessiveWhitespace_CollapsesToSingleSpaces()
    {
        var input = "  hello   world  ";
        input.Sanitize().Should().Be("hello world");
    }

    /// <summary>
    /// Tests the ToSlug method with uppercase and special characters.
    /// </summary>
    [Fact]
    public void ToSlug_WithUppercaseAndSpecialChars_ReturnsLowercaseHyphenated()
    {
        var input = "Hello World!";
        input.ToSlug().Should().Be("hello-world");
    }

    /// <summary>
    /// Tests the ToSlug method with an empty input.
    /// </summary>
    [Fact]
    public void ToSlug_WithEmptyInput_ReturnsEmptyString()
    {
        string.Empty.ToSlug().Should().Be("");
    }

    /// <summary>
    /// Tests the Truncate method when the input exceeds the maximum length.
    /// </summary>
    [Fact]
    public void Truncate_WhenInputExceedsMaxLength_AppendsEllipsis()
    {
        var input = "Hello World";
        input.Truncate(8).Should().Be("Hello...");
    }

    /// <summary>
    /// Tests the Truncate method when the input is within the maximum length.
    /// </summary>
    [Fact]
    public void Truncate_WhenInputIsWithinMaxLength_ReturnsOriginalString()
    {
        var input = "Hello";
        input.Truncate(10).Should().Be("Hello");
    }

    /// <summary>
    /// Tests the ToDisplayName method with a PascalCase string.
    /// </summary>
    [Fact]
    public void ToDisplayName_ConvertsPascalCaseToWords()
    {
        "ProductName".ToDisplayName().Should().Be("Product Name");
    }

    /// <summary>
    /// Tests the IsValidEmail method with various email addresses.
    /// </summary>
    /// <param name="email">The email address to test.</param>
    /// <param name="expected">The expected result of the IsValidEmail method.</param>
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_ReturnsExpectedResult(string? email, bool expected)
    {
        (email ?? "").IsValidEmail().Should().Be(expected);
    }

    /// <summary>
    /// Tests the IsAlphaNumeric method with various strings.
    /// </summary>
    /// <param name="input">The string to test.</param>
    /// <param name="expected">The expected result of the IsAlphaNumeric method.</param>
    [Theory]
    [InlineData("abc123", true)]
    [InlineData("abc 123", false)]
    [InlineData("abc!", false)]
    public void IsAlphaNumeric_ReturnsExpectedResult(string input, bool expected)
    {
        input.IsAlphaNumeric().Should().Be(expected);
    }

    /// <summary>
    /// Tests the OrIfEmpty method when the input is not null or empty.
    /// </summary>
    [Fact]
    public void OrIfEmpty_ReturnsInputIfNotNullOrEmpty()
    {
        "hello".OrIfEmpty("fallback").Should().Be("hello");
    }

    /// <summary>
    /// Tests the OrIfEmpty method when the input is null or empty.
    /// </summary>
    [Fact]
    public void OrIfEmpty_ReturnsFallbackIfInputIsNullOrEmpty()
    {
        "".OrIfEmpty("fallback").Should().Be("fallback");
        ((string?)null).OrIfEmpty("fallback").Should().Be("fallback");
    }

    /// <summary>
    /// Tests the HtmlEncode method with a string containing special characters.
    /// </summary>
    [Fact]
    public void HtmlEncode_EncodesSpecialCharacters()
    {
        "<script>alert('xss')</script>".HtmlEncode().Should().Contain("&lt;script&gt;");
    }
}
