#nullable enable
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="ValidationHelper"/> class.
/// Tests various validation methods including null checks, range validation, string length, patterns, emails, phone numbers, collections, and equality.
/// </summary>
public sealed class ValidationHelperTests
{
    /// <summary>
    /// Tests that <see cref="ValidationHelper.NotNull"/> throws a <see cref="ValidationException"/> when the value is null.
    /// </summary>
    [Fact]
    public void NotNull_WithNullValue_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.NotNull(null, "TestField");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.NotNull"/> does not throw when the value is not null.
    /// </summary>
    [Fact]
    public void NotNull_WithNonNullValue_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.NotNull("value", "TestField");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.NotNullOrEmpty"/> throws a <see cref="ValidationException"/> when the string value is null.
    /// </summary>
    [Fact]
    public void NotNullOrEmpty_WithNullString_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.NotNullOrEmpty(null, "TestField");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.NotNullOrEmpty"/> throws a <see cref="ValidationException"/> when the string value is empty.
    /// </summary>
    [Fact]
    public void NotNullOrEmpty_WithEmptyString_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.NotNullOrEmpty("", "TestField");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.NotNullOrEmpty"/> does not throw when the string value is not null or empty.
    /// </summary>
    [Fact]
    public void NotNullOrEmpty_WithValidString_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.NotNullOrEmpty("valid", "TestField");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.InRange(decimal, decimal, decimal, string)"/> does not throw when the value is within the specified range.
    /// </summary>
    [Fact]
    public void InRange_WithValueInRange_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.InRange(50m, 0m, 100m, "Price");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.InRange(decimal, decimal, decimal, string)"/> throws a <see cref="ValidationException"/> when the value is below the minimum.
    /// </summary>
    [Fact]
    public void InRange_WithValueBelowMin_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.InRange(-10m, 0m, 100m, "Price");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.InRange(decimal, decimal, decimal, string)"/> throws a <see cref="ValidationException"/> when the value is above the maximum.
    /// </summary>
    [Fact]
    public void InRange_WithValueAboveMax_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.InRange(150m, 0m, 100m, "Price");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.InRange(int, int, int, string)"/> does not throw when the integer value is within the specified range.
    /// </summary>
    [Fact]
    public void InRange_Integer_WithValueInRange_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.InRange(5, 1, 10, "Quantity");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.InRange(int, int, int, string)"/> throws a <see cref="ValidationException"/> when the integer value is out of the specified range.
    /// </summary>
    [Fact]
    public void InRange_Integer_WithValueOutOfRange_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.InRange(15, 1, 10, "Quantity");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.LengthBetween(string, int, int, string)"/> does not throw when the string length is within the specified range.
    /// </summary>
    [Fact]
    public void LengthBetween_WithValidLength_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.LengthBetween("hello", 3, 10, "Name");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.LengthBetween(string, int, int, string)"/> throws a <see cref="ValidationException"/> when the string is too short.
    /// </summary>
    [Fact]
    public void LengthBetween_WithTooShortString_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.LengthBetween("ab", 3, 10, "Name");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.LengthBetween(string, int, int, string)"/> throws a <see cref="ValidationException"/> when the string is too long.
    /// </summary>
    [Fact]
    public void LengthBetween_WithTooLongString_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.LengthBetween("this is too long", 3, 10, "Name");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.MatchesPattern(string, string, string)"/> does not throw when the string matches the specified pattern.
    /// </summary>
    [Fact]
    public void MatchesPattern_WithMatchingPattern_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.MatchesPattern("test123", @"^\w+\d+$", "Code");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.MatchesPattern(string, string, string)"/> throws a <see cref="ValidationException"/> when the string does not match the specified pattern.
    /// </summary>
    [Fact]
    public void MatchesPattern_WithNonMatchingPattern_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.MatchesPattern("test", @"^\d+$", "Code");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.ValidEmail(string)"/> does not throw when the email is valid.
    /// </summary>
    [Fact]
    public void ValidEmail_WithValidEmail_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.ValidEmail("test@example.com");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.ValidEmail(string)"/> throws a <see cref="ValidationException"/> when the email is invalid.
    /// </summary>
    [Fact]
    public void ValidEmail_WithInvalidEmail_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.ValidEmail("invalid");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.ValidPhoneNumber(string)"/> does not throw when the phone number is valid.
    /// </summary>
    [Fact]
    public void ValidPhoneNumber_WithValidPhoneNumber_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.ValidPhoneNumber("1-234-567-8901");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.ValidPhoneNumber(string)"/> throws a <see cref="ValidationException"/> when the phone number is too short.
    /// </summary>
    [Fact]
    public void ValidPhoneNumber_WithTooShortPhoneNumber_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.ValidPhoneNumber("123456");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.ValidPhoneNumber(string)"/> does not throw when the phone number is null.
    /// </summary>
    [Fact]
    public void ValidPhoneNumber_WithNullPhoneNumber_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.ValidPhoneNumber(null);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.NotEmpty{T}(IEnumerable{T}, string)"/> throws a <see cref="ValidationException"/> when the collection is empty.
    /// </summary>
    [Fact]
    public void NotEmpty_WithEmptyCollection_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.NotEmpty(new List<int>(), "Items");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.NotEmpty{T}(IEnumerable{T}, string)"/> does not throw when the collection is not empty.
    /// </summary>
    [Fact]
    public void NotEmpty_WithNonEmptyCollection_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.NotEmpty(new List<int> { 1, 2 }, "Items");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.MaxItems{T}(IEnumerable{T}, int, string)"/> throws a <see cref="ValidationException"/> when the collection has too many items.
    /// </summary>
    [Fact]
    public void MaxItems_WithTooManyItems_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.MaxItems(new List<int> { 1, 2, 3, 4, 5 }, 3, "Items");

        // Assert
        act.Should().Throw<ValidationException>();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.MaxItems{T}(IEnumerable{T}, int, string)"/> does not throw when the collection has a valid item count.
    /// </summary>
    [Fact]
    public void MaxItems_WithValidItemCount_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.MaxItems(new List<int> { 1, 2, 3 }, 5, "Items");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.Equal(string, string, string)"/> does not throw when the values are equal.
    /// </summary>
    [Fact]
    public void Equal_WithEqualValues_DoesNotThrow()
    {
        // Act
        var act = () => ValidationHelper.Equal("password", "password", "PasswordConfirm");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="ValidationHelper.Equal(string, string, string)"/> throws a <see cref="ValidationException"/> when the values are unequal.
    /// </summary>
    [Fact]
    public void Equal_WithUnequalValues_ThrowsValidationException()
    {
        // Act
        var act = () => ValidationHelper.Equal("password", "different", "PasswordConfirm");

        // Assert
        act.Should().Throw<ValidationException>();
    }
}
