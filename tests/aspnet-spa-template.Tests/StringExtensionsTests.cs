#nullable enable
using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="StringExtensions"/> class.
/// </summary>
public sealed class StringExtensionsTests
{
    [Fact]
    public void Sanitize_WithControlCharacters_RemovesControlCharacters()
    {
        "hello\u0000\u0007world".Sanitize().Should().Be("hello world");
    }

    [Fact]
    public void Sanitize_WithExcessiveWhitespace_CollapsesWhitespace()
    {
        "  hello   world  ".Sanitize().Should().Be("hello world");
    }

    [Fact]
    public void Sanitize_WithWhitespaceOnly_ReturnsEmptyString()
    {
        " \t\r\n ".Sanitize().Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_WithEmptyInput_ReturnsEmptyString()
    {
        string.Empty.Sanitize().Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_WithNullInput_ThrowsArgumentNullException()
    {
        string? input = null;

        var act = () => input.Sanitize();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ToSlug_WithMixedSpacing_UsesSingleHyphens()
    {
        "  hello   world  ".ToSlug().Should().Be("hello-world");
    }

    [Fact]
    public void ToSlug_WithUppercaseInput_UsesLowercase()
    {
        "HelloWORLD".ToSlug().Should().Be("helloworld");
    }

    [Fact]
    public void ToSlug_WithNonAlphanumericCharacters_StripsCharacters()
    {
        "hello, world! -- test".ToSlug().Should().Be("hello-world-test");
    }

    [Fact]
    public void ToSlug_WithUnicodeInput_StripsUnicodeCharacters()
    {
        "Café 東京".ToSlug().Should().Be("caf");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ToSlug_WithEmptyOrWhitespaceInput_ReturnsEmptyString(string input)
    {
        input.ToSlug().Should().BeEmpty();
    }

    [Fact]
    public void ToSlug_WithNullInput_ThrowsArgumentNullException()
    {
        string input = null!;

        var act = () => input.ToSlug();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Truncate_WithInputExceedingMaximumLength_AppendsDefaultSuffix()
    {
        "Hello World".Truncate(8).Should().Be("Hello...");
    }

    [Fact]
    public void Truncate_WithInputAtMaximumLength_ReturnsOriginalInput()
    {
        "Hello".Truncate(5).Should().Be("Hello");
    }

    [Fact]
    public void Truncate_WithCustomSuffix_AppendsCustomSuffix()
    {
        "Hello World".Truncate(7, "~").Should().Be("Hello ~");
    }

    [Fact]
    public void Truncate_WithMaximumLengthShorterThanSuffix_ThrowsArgumentOutOfRangeException()
    {
        var act = () => "Hello".Truncate(2);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ToDisplayName_WithPascalCaseInput_SeparatesWords()
    {
        "ProductName".ToDisplayName().Should().Be("Product Name");
    }

    [Fact]
    public void ToDisplayName_WithConsecutiveUppercaseCharacters_SeparatesEveryUppercaseCharacter()
    {
        "HTMLParser".ToDisplayName().Should().Be("H T M L Parser");
    }

    [Fact]
    public void ToDisplayName_WithEmptyInput_ReturnsEmptyString()
    {
        string.Empty.ToDisplayName().Should().BeEmpty();
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("invalid-email", false)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    [InlineData(null, false)]
    public void IsValidEmail_WithVariousInputs_ReturnsExpectedResult(string? email, bool expected)
    {
        email!.IsValidEmail().Should().Be(expected);
    }

    [Fact]
    public void IsAlphaNumeric_WithLettersAndNumbers_ReturnsTrue()
    {
        "abc123XYZ".IsAlphaNumeric().Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("abc 123")]
    [InlineData("abc!")]
    public void IsAlphaNumeric_WithNonAlphanumericInput_ReturnsFalse(string input)
    {
        input.IsAlphaNumeric().Should().BeFalse();
    }

    [Fact]
    public void IsAlphaNumeric_WithNullInput_ThrowsArgumentNullException()
    {
        string input = null!;

        var act = () => input.IsAlphaNumeric();

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void OrIfEmpty_WithNonEmptyInput_ReturnsInput()
    {
        "hello".OrIfEmpty("fallback").Should().Be("hello");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void OrIfEmpty_WithNullEmptyOrWhitespaceInput_ReturnsFallback(string? input)
    {
        input.OrIfEmpty("fallback").Should().Be("fallback");
    }

    [Fact]
    public void OrIfEmpty_WithNullFallback_ThrowsArgumentNullException()
    {
        var act = () => string.Empty.OrIfEmpty(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void HtmlEncode_WithMarkupCharacters_EncodesMarkupCharacters()
    {
        "<div class=\"test\">&</div>".HtmlEncode()
            .Should().Be("&lt;div class=&quot;test&quot;&gt;&amp;&lt;/div&gt;");
    }

    [Fact]
    public void HtmlEncode_WithEmptyInput_ReturnsEmptyString()
    {
        string.Empty.HtmlEncode().Should().BeEmpty();
    }

    [Fact]
    public void HtmlEncode_WithNullInput_ThrowsArgumentNullException()
    {
        string input = null!;

        var act = () => input.HtmlEncode();

        act.Should().Throw<ArgumentNullException>();
    }
}
