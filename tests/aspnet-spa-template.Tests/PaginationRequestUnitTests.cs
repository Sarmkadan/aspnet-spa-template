#nullable enable
using AspNetSpaTemplate.DTOs;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="PaginationRequest"/> class.
/// Tests various pagination parameters and validation logic.
/// </summary>
public sealed class PaginationRequestUnitTests
{
#region Constructor and Default Values Tests

    /// <summary>
    /// Tests that <see cref="PaginationRequest"/> initializes with default values.
    /// </summary>
    [Fact]
    public void Constructor_WithNoParameters_SetsDefaultValues()
    {
        // Arrange & Act
        var request = new PaginationRequest();

        // Assert
        request.PageNumber.Should().Be(1);
        request.PageSize.Should().Be(10);
        request.SortBy.Should().BeNull();
        request.SortDescending.Should().BeFalse();
        request.SearchTerm.Should().BeNull();
        request.Filters.Should().BeNull();
    }

#endregion

#region PageNumber Tests

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageNumber"/> can be set and retrieved.
    /// </summary>
    [Fact]
    public void PageNumber_WithValidValue_SetsAndGetsCorrectly()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageNumber = 5;

        // Assert
        request.PageNumber.Should().Be(5);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageNumber"/> enforces minimum value of 1.
    /// </summary>
    [Fact]
    public void PageNumber_WithZero_EnforcesMinimumValue()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageNumber = 0;

        // Assert
        request.PageNumber.Should().Be(1);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageNumber"/> enforces minimum value of 1 for negative values.
    /// </summary>
    [Fact]
    public void PageNumber_WithNegativeValue_EnforcesMinimumValue()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageNumber = -5;

        // Assert
        request.PageNumber.Should().Be(1);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageNumber"/> accepts large positive values.
    /// </summary>
    [Fact]
    public void PageNumber_WithLargeValue_AcceptsLargeValue()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageNumber = 999999;

        // Assert
        request.PageNumber.Should().Be(999999);
    }

#endregion

#region PageSize Tests

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageSize"/> can be set and retrieved.
    /// </summary>
    [Fact]
    public void PageSize_WithValidValue_SetsAndGetsCorrectly()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageSize = 25;

        // Assert
        request.PageSize.Should().Be(25);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageSize"/> enforces minimum value of 1.
    /// </summary>
    [Fact]
    public void PageSize_WithZero_EnforcesMinimumValue()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageSize = 0;

        // Assert
        request.PageSize.Should().Be(1);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageSize"/> enforces minimum value of 1 for negative values.
    /// </summary>
    [Fact]
    public void PageSize_WithNegativeValue_EnforcesMinimumValue()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageSize = -10;

        // Assert
        request.PageSize.Should().Be(1);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageSize"/> enforces maximum value of 100.
    /// </summary>
    [Fact]
    public void PageSize_WithValueGreaterThan100_EnforcesMaximumValue()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageSize = 150;

        // Assert
        request.PageSize.Should().Be(100);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageSize"/> accepts boundary value of 100.
    /// </summary>
    [Fact]
    public void PageSize_WithValueOf100_AcceptsBoundaryValue()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageSize = 100;

        // Assert
        request.PageSize.Should().Be(100);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.PageSize"/> accepts boundary value of 1.
    /// </summary>
    [Fact]
    public void PageSize_WithValueOf1_AcceptsBoundaryValue()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.PageSize = 1;

        // Assert
        request.PageSize.Should().Be(1);
    }

#endregion

#region SortBy Tests

    /// <summary>
    /// Tests that <see cref="PaginationRequest.SortBy"/> can be set to a valid string.
    /// </summary>
    [Fact]
    public void SortBy_WithValidString_SetsCorrectly()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.SortBy = "Name";

        // Assert
        request.SortBy.Should().Be("Name");
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.SortBy"/> can be set to null.
    /// </summary>
    [Fact]
    public void SortBy_WithNull_SetsToNull()
    {
        // Arrange
        var request = new PaginationRequest { SortBy = "Name" };

        // Act
        request.SortBy = null;

        // Assert
        request.SortBy.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.SortBy"/> can be set to empty string.
    /// </summary>
    [Fact]
    public void SortBy_WithEmptyString_SetsToEmptyString()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.SortBy = string.Empty;

        // Assert
        request.SortBy.Should().BeEmpty();
    }

#endregion

#region SortDescending Tests

    /// <summary>
    /// Tests that <see cref="PaginationRequest.SortDescending"/> defaults to false.
    /// </summary>
    [Fact]
    public void SortDescending_DefaultValue_IsFalse()
    {
        // Arrange & Act
        var request = new PaginationRequest();

        // Assert
        request.SortDescending.Should().BeFalse();
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.SortDescending"/> can be set to true.
    /// </summary>
    [Fact]
    public void SortDescending_WithTrueValue_SetsToTrue()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.SortDescending = true;

        // Assert
        request.SortDescending.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.SortDescending"/> can be set to false.
    /// </summary>
    [Fact]
    public void SortDescending_WithFalseValue_SetsToFalse()
    {
        // Arrange
        var request = new PaginationRequest { SortDescending = true };

        // Act
        request.SortDescending = false;

        // Assert
        request.SortDescending.Should().BeFalse();
    }

#endregion

#region SearchTerm Tests

    /// <summary>
    /// Tests that <see cref="PaginationRequest.SearchTerm"/> can be set to a valid string.
    /// </summary>
    [Fact]
    public void SearchTerm_WithValidString_SetsCorrectly()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.SearchTerm = "test search";

        // Assert
        request.SearchTerm.Should().Be("test search");
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.SearchTerm"/> can be set to null.
    /// </summary>
    [Fact]
    public void SearchTerm_WithNull_SetsToNull()
    {
        // Arrange
        var request = new PaginationRequest { SearchTerm = "search term" };

        // Act
        request.SearchTerm = null;

        // Assert
        request.SearchTerm.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.SearchTerm"/> can be set to empty string.
    /// </summary>
    [Fact]
    public void SearchTerm_WithEmptyString_SetsToEmptyString()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.SearchTerm = string.Empty;

        // Assert
        request.SearchTerm.Should().BeEmpty();
    }

#endregion

#region Filters Tests

    /// <summary>
    /// Tests that <see cref="PaginationRequest.Filters"/> can be set to a valid dictionary.
    /// </summary>
    [Fact]
    public void Filters_WithValidDictionary_SetsCorrectly()
    {
        // Arrange
        var request = new PaginationRequest();
        var filters = new Dictionary<string, string> { { "status", "active" }, { "type", "user" } };

        // Act
        request.Filters = filters;

        // Assert
        request.Filters.Should().BeEquivalentTo(filters);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.Filters"/> can be set to null.
    /// </summary>
    [Fact]
    public void Filters_WithNull_SetsToNull()
    {
        // Arrange
        var request = new PaginationRequest { Filters = new Dictionary<string, string>() };

        // Act
        request.Filters = null;

        // Assert
        request.Filters.Should().BeNull();
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.Filters"/> can be set to empty dictionary.
    /// </summary>
    [Fact]
    public void Filters_WithEmptyDictionary_SetsToEmptyDictionary()
    {
        // Arrange
        var request = new PaginationRequest();

        // Act
        request.Filters = new Dictionary<string, string>();

        // Assert
        request.Filters.Should().NotBeNull();
        request.Filters.Should().BeEmpty();
    }

#endregion

#region GetSkip Tests

    /// <summary>
    /// Tests that <see cref="PaginationRequest.GetSkip()"/> calculates correct skip count for page 1.
    /// </summary>
    [Fact]
    public void GetSkip_WithPage1AndPageSize10_ReturnsZero()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 1, PageSize = 10 };

        // Act
        var skip = request.GetSkip();

        // Assert
        skip.Should().Be(0);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.GetSkip()"/> calculates correct skip count for page 2.
    /// </summary>
    [Fact]
    public void GetSkip_WithPage2AndPageSize10_ReturnsTen()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 2, PageSize = 10 };

        // Act
        var skip = request.GetSkip();

        // Assert
        skip.Should().Be(10);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.GetSkip()"/> calculates correct skip count for page 3 with page size 25.
    /// </summary>
    [Fact]
    public void GetSkip_WithPage3AndPageSize25_ReturnsFifty()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 3, PageSize = 25 };

        // Act
        var skip = request.GetSkip();

        // Assert
        skip.Should().Be(50);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.GetSkip()"/> handles page size of 1.
    /// </summary>
    [Fact]
    public void GetSkip_WithPageSize1_ReturnsCorrectSkip()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 5, PageSize = 1 };

        // Act
        var skip = request.GetSkip();

        // Assert
        skip.Should().Be(4);
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.GetSkip()"/> throws OverflowException when overflow would occur with hostile inputs.
    /// This test verifies the checked arithmetic protection works when values exceed int range.
    /// </summary>
    [Fact]
    public void GetSkip_WithOverflowingValues_ThrowsOverflowException()
    {
        // Arrange - Create a PaginationRequest with values that would overflow if not clamped
        // We need to bypass the setters to test the overflow protection
        var request = new PaginationRequest();

        // Use reflection to bypass the setter and set values that would overflow
        var pageNumberField = typeof(PaginationRequest).GetField("_pageNumber", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var pageSizeField = typeof(PaginationRequest).GetField("_pageSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        pageNumberField.SetValue(request, int.MaxValue);
        pageSizeField.SetValue(request, 100);

        // Act & Assert - This should throw OverflowException due to checked arithmetic
        request.Invoking(r => r.GetSkip())
            .Should().Throw<OverflowException>("The skip calculation uses checked arithmetic to prevent integer overflow");
    }

    /// <summary>
    /// Tests that <see cref="PaginationRequest.GetSkip()"/> handles maximum allowed values (PageNumber = 100, PageSize = 100) without overflow.
    /// </summary>
    [Fact]
    public void GetSkip_WithMaxAllowedValues_ReturnsCorrectSkip()
    {
        // Arrange
        var request = new PaginationRequest { PageNumber = 100, PageSize = 100 };

        // Act
        var skip = request.GetSkip();

        // Assert
        skip.Should().Be(9900); // (100-1)*100 = 9900
    }

#endregion
}
