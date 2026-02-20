#nullable enable
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class ValidationHelperTests
{
    [Fact]
    public void NotNull_WithNullValue_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.NotNull(null, "TestField");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void NotNull_WithNonNullValue_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.NotNull("value", "TestField");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotNullOrEmpty_WithNullString_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.NotNullOrEmpty(null, "TestField");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void NotNullOrEmpty_WithEmptyString_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.NotNullOrEmpty("", "TestField");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void NotNullOrEmpty_WithValidString_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.NotNullOrEmpty("valid", "TestField");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void InRange_WithValueInRange_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.InRange(50m, 0m, 100m, "Price");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void InRange_WithValueBelowMin_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.InRange(-10m, 0m, 100m, "Price");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void InRange_WithValueAboveMax_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.InRange(150m, 0m, 100m, "Price");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void InRange_Integer_WithValueInRange_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.InRange(5, 1, 10, "Quantity");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void InRange_Integer_WithValueOutOfRange_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.InRange(15, 1, 10, "Quantity");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void LengthBetween_WithValidLength_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.LengthBetween("hello", 3, 10, "Name");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void LengthBetween_WithTooShortString_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.LengthBetween("ab", 3, 10, "Name");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void LengthBetween_WithTooLongString_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.LengthBetween("this is too long", 3, 10, "Name");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void MatchesPattern_WithMatchingPattern_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.MatchesPattern("test123", @"^\w+\d+$", "Code");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MatchesPattern_WithNonMatchingPattern_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.MatchesPattern("test", @"^\d+$", "Code");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ValidEmail_WithValidEmail_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.ValidEmail("test@example.com");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidEmail_WithInvalidEmail_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.ValidEmail("invalid");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ValidPhoneNumber_WithValidPhoneNumber_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.ValidPhoneNumber("1-234-567-8901");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidPhoneNumber_WithTooShortPhoneNumber_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.ValidPhoneNumber("123456");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void ValidPhoneNumber_WithNullPhoneNumber_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.ValidPhoneNumber(null);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void NotEmpty_WithEmptyCollection_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.NotEmpty(new List<int>(), "Items");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void NotEmpty_WithNonEmptyCollection_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.NotEmpty(new List<int> { 1, 2 }, "Items");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void MaxItems_WithTooManyItems_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.MaxItems(new List<int> { 1, 2, 3, 4, 5 }, 3, "Items");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    [Fact]
    public void MaxItems_WithValidItemCount_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.MaxItems(new List<int> { 1, 2, 3 }, 5, "Items");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Equal_WithEqualValues_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.Equal("password", "password", "PasswordConfirm");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void Equal_WithUnequalValues_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.Equal("password", "different", "PasswordConfirm");

        // Assert
        act.Should().Throw<ValidationException>();
    }
}
