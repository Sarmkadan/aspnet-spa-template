#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("   ", "")]
    public void Sanitize_WhenInputIsNullOrWhiteSpace_ReturnsEmptyString(string? input, string expected)
    {
        input.Sanitize().Should().Be(expected);
    }

    [Fact]
    public void Sanitize_WhenInputHasExcessiveWhitespace_CollapsesToSingleSpaces()
    {
        var input = "  hello   world  ";
        input.Sanitize().Should().Be("hello world");
    }

    [Fact]
    public void ToSlug_WithUppercaseAndSpecialChars_ReturnsLowercaseHyphenated()
    {
        var input = "Hello World!";
        input.ToSlug().Should().Be("hello-world");
    }

    [Fact]
    public void ToSlug_WithEmptyInput_ReturnsEmptyString()
    {
        string.Empty.ToSlug().Should().Be("");
    }

    [Fact]
    public void Truncate_WhenInputExceedsMaxLength_AppendsEllipsis()
    {
        var input = "Hello World";
        input.Truncate(8).Should().Be("Hello...");
    }

    [Fact]
    public void Truncate_WhenInputIsWithinMaxLength_ReturnsOriginalString()
    {
        var input = "Hello";
        input.Truncate(10).Should().Be("Hello");
    }

    [Fact]
    public void ToDisplayName_ConvertsPascalCaseToWords()
    {
        "ProductName".ToDisplayName().Should().Be("Product Name");
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_ReturnsExpectedResult(string? email, bool expected)
    {
        (email ?? "").IsValidEmail().Should().Be(expected);
    }

    [Theory]
    [InlineData("abc123", true)]
    [InlineData("abc 123", false)]
    [InlineData("abc!", false)]
    public void IsAlphaNumeric_ReturnsExpectedResult(string input, bool expected)
    {
        input.IsAlphaNumeric().Should().Be(expected);
    }

    [Fact]
    public void OrIfEmpty_ReturnsInputIfNotNullOrEmpty()
    {
        "hello".OrIfEmpty("fallback").Should().Be("hello");
    }

    [Fact]
    public void OrIfEmpty_ReturnsFallbackIfInputIsNullOrEmpty()
    {
        "".OrIfEmpty("fallback").Should().Be("fallback");
        ((string?)null).OrIfEmpty("fallback").Should().Be("fallback");
    }

    [Fact]
    public void HtmlEncode_EncodesSpecialCharacters()
    {
        "<script>alert('xss')</script>".HtmlEncode().Should().Contain("&lt;script&gt;");
    }
}
