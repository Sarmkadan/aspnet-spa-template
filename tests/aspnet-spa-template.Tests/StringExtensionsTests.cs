// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public class StringExtensionsTests
{
    [Fact]
    public void Sanitize_WhenInputIsNull_ReturnsEmptyString()
    {
        // Arrange
        string? input = null;

        // Act
        var result = input.Sanitize();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Sanitize_WhenInputHasExcessiveWhitespace_CollapsesToSingleSpaces()
    {
        // Arrange
        var input = "  hello   world  ";

        // Act
        var result = input.Sanitize();

        // Assert
        result.Should().Be("hello world");
    }

    [Fact]
    public void ToSlug_WithUppercaseAndSpecialChars_ReturnsLowercaseHyphenated()
    {
        // Arrange
        var input = "Hello World!";

        // Act
        var result = input.ToSlug();

        // Assert
        result.Should().Be("hello-world");
    }

    [Fact]
    public void Truncate_WhenInputExceedsMaxLength_AppendsEllipsis()
    {
        // Arrange
        var input = "Hello World";

        // Act
        var result = input.Truncate(8);

        // Assert
        result.Should().Be("Hello...");
        result.Length.Should().Be(8);
    }
}
