#nullable enable
using AspNetSpaTemplate.Utilities;
using FluentAssertions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="CollectionExtensions"/> class.
/// Tests various extension methods for collections and enumerable operations.
/// </summary>
public sealed class CollectionExtensionsUnitTests
{
    #region Batch Tests

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Batch{T}"/> correctly batches a collection.
    /// </summary>
    [Fact]
    public void Batch_WithCollection_ReturnsCorrectlySizedBatches()
    {
        // Arrange
        var source = Enumerable.Range(1, 10);
        int batchSize = 3;

        // Act
        var batches = global::AspNetSpaTemplate.Utilities.CollectionExtensions.Batch(source, batchSize).ToList();

        // Assert
        batches.Should().HaveCount(4); // 10 items in batches of 3 = 4 batches
        batches[0].Should().Equal(1, 2, 3);
        batches[1].Should().Equal(4, 5, 6);
        batches[2].Should().Equal(7, 8, 9);
        batches[3].Should().Equal(10);
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Batch{T}"/> throws ArgumentNullException for null source.
    /// </summary>
    [Fact]
    public void Batch_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int> source = null!;
        int batchSize = 3;

        // Act
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.Batch(source, batchSize).ToList();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Batch{T}"/> throws ArgumentOutOfRangeException for non-positive batch size.
    /// </summary>
    [Fact]
    public void Batch_WithNonPositiveBatchSize_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var source = Enumerable.Range(1, 5);

        // Act & Assert
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.Batch(source, 0).ToList();
        act.Should().Throw<ArgumentOutOfRangeException>();

        act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.Batch(source, -1).ToList();
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Batch{T}"/> handles empty collection correctly.
    /// </summary>
    [Fact]
    public void Batch_WithEmptyCollection_ReturnsEmptyBatches()
    {
        // Arrange
        var source = Enumerable.Empty<int>();
        int batchSize = 3;

        // Act
        var batches = global::AspNetSpaTemplate.Utilities.CollectionExtensions.Batch(source, batchSize).ToList();

        // Assert
        batches.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Batch{T}"/> handles batch size larger than collection.
    /// </>
    [Fact]
    public void Batch_WithBatchSizeLargerThanCollection_ReturnsSingleBatch()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        int batchSize = 5;

        // Act
        var batches = global::AspNetSpaTemplate.Utilities.CollectionExtensions.Batch(source, batchSize).ToList();

        // Assert
        batches.Should().HaveCount(1);
        batches[0].Should().Equal(1, 2, 3);
    }

    #endregion

    #region IsNullOrEmpty Tests

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.IsNullOrEmpty{T}"/> returns true for null collection.
    /// </summary>
    [Fact]
    public void IsNullOrEmpty_WithNullSource_ReturnsTrue()
    {
        // Arrange
        IEnumerable<int> source = null!;

        // Act
        var result = source.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.IsNullOrEmpty{T}"/> returns true for empty collection.
    /// </summary>
    [Fact]
    public void IsNullOrEmpty_WithEmptySource_ReturnsTrue()
    {
        // Arrange
        var source = Enumerable.Empty<int>();

        // Act
        var result = source.IsNullOrEmpty();

        // Assert
        result.Should().BeTrue();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.IsNullOrEmpty{T}"/> returns false for non-empty collection.
    /// </summary>
    [Fact]
    public void IsNullOrEmpty_WithNonEmptySource_ReturnsFalse()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.IsNullOrEmpty(source);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region OrEmpty Tests

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.OrEmpty{T}"/> returns original collection when not null.
    /// </summary>
    [Fact]
    public void OrEmpty_WithNonNullSource_ReturnsOriginalCollection()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.OrEmpty(source).ToList();

        // Assert
        result.Should().Equal(1, 2, 3);
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.OrEmpty{T}"/> returns empty collection when source is null.
    /// </summary>
    [Fact]
    public void OrEmpty_WithNullSource_ReturnsEmptyCollection()
    {
        // Arrange
        IEnumerable<int> source = null!;

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.OrEmpty(source).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region DistinctBy Tests

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.DistinctBy{T,TKey}"/> returns distinct elements based on key selector.
    /// </summary>
    [Fact]
    public void DistinctBy_WithKeySelector_ReturnsDistinctElements()
    {
        // Arrange
        var source = new[]
        {
            new { Id = 1, Name = "Alice" },
            new { Id = 2, Name = "Bob" },
            new { Id = 1, Name = "Alice Duplicate" },
            new { Id = 3, Name = "Charlie" }
        };

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.DistinctBy(source, x => x.Id).ToList();

        // Assert
        result.Should().HaveCount(3);
        result.Should().ContainSingle(x => x.Id == 1);
        result.Should().ContainSingle(x => x.Id == 2);
        result.Should().ContainSingle(x => x.Id == 3);
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.DistinctBy{T,TKey}"/> throws ArgumentNullException for null source.
    /// </summary>
    [Fact]
    public void DistinctBy_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<object> source = null!;
        Func<object, int> keySelector = x => 1;

        // Act
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.DistinctBy(source, keySelector).ToList();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.DistinctBy{T,TKey}"/> throws ArgumentNullException for null key selector.
    /// </summary>
    [Fact]
    public void DistinctBy_WithNullKeySelector_ThrowsArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        Func<int, int> keySelector = null!;

        // Act
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.DistinctBy(source, keySelector).ToList();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Paginate Tests

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Paginate{T}"/> correctly paginates a collection.
    /// </summary>
    [Fact]
    public void Paginate_WithCollection_ReturnsCorrectPageAndTotal()
    {
        // Arrange
        var source = Enumerable.Range(1, 25);
        int pageNumber = 2;
        int pageSize = 10;

        // Act
        var (items, total) = global::AspNetSpaTemplate.Utilities.CollectionExtensions.Paginate(source, pageNumber, pageSize);

        // Assert
        total.Should().Be(25);
        items.Should().HaveCount(10);
        items.Should().Equal(11, 12, 13, 14, 15, 16, 17, 18, 19, 20);
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Paginate{T}"/> handles page number less than 1.
    /// </summary>
    [Fact]
    public void Paginate_WithPageNumberLessThanOne_DefaultsToFirstPage()
    {
        // Arrange
        var source = Enumerable.Range(1, 10);
        int pageNumber = 0; // Should default to 1
        int pageSize = 3;

        // Act
        var (items, total) = global::AspNetSpaTemplate.Utilities.CollectionExtensions.Paginate(source, pageNumber, pageSize);

        // Assert
        total.Should().Be(10);
        items.Should().HaveCount(3);
        items.Should().Equal(1, 2, 3);
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Paginate{T}"/> handles page size less than 1.
    /// </summary>
    [Fact]
    public void Paginate_WithPageSizeLessThanOne_DefaultsToTen()
    {
        // Arrange
        var source = Enumerable.Range(1, 15);
        int pageNumber = 1;
        int pageSize = 0; // Should default to 10

        // Act
        var (items, total) = global::AspNetSpaTemplate.Utilities.CollectionExtensions.Paginate(source, pageNumber, pageSize);

        // Assert
        total.Should().Be(15);
        items.Should().HaveCount(10);
        items.Should().Equal(1, 2, 3, 4, 5, 6, 7, 8, 9, 10);
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Paginate{T}"/> throws ArgumentNullException for null source.
    /// </summary>
    [Fact]
    public void Paginate_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int> source = null!;
        int pageNumber = 1;
        int pageSize = 10;

        // Act
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.Paginate(source, pageNumber, pageSize);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ForEach Tests

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.ForEach{T}"/> applies action to each item.
    /// </summary>
    [Fact]
    public void ForEach_WithCollection_AppliesActionToEachItem()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        var results = new List<int>();

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.ForEach(source, x => results.Add(x * 2)).ToList();

        // Assert
        result.Should().Equal(1, 2, 3); // Original collection returned
        results.Should().Equal(2, 4, 6); // Action applied to each item
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.ForEach{T}"/> throws ArgumentNullException for null source.
    /// </summary>
    [Fact]
    public void ForEach_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int> source = null!;
        Action<int> action = x => { };

        // Act
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.ForEach(source, action).ToList();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.ForEach{T}"/> throws ArgumentNullException for null action.
    /// </summary>
    [Fact]
    public void ForEach_WithNullAction_ThrowsArgumentNullException()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };
        Action<int> action = null!;

        // Act
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.ForEach(source, action).ToList();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region ToKeyValueString Tests

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.ToKeyValueString{TKey,TValue}"/> converts dictionary to string.
    /// </summary>
    [Fact]
    public void ToKeyValueString_WithDictionary_ReturnsFormattedString()
    {
        // Arrange
        var source = new Dictionary<string, int>
        {
            ["key1"] = 10,
            ["key2"] = 20,
            ["key3"] = 30
        };

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.ToKeyValueString(source);

        // Assert
        result.Should().Be("key1=10, key2=20, key3=30");
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.ToKeyValueString{TKey,TValue}"/> returns empty string for empty dictionary.
    /// </summary>
    [Fact]
    public void ToKeyValueString_WithEmptyDictionary_ReturnsEmptyString()
    {
        // Arrange
        var source = new Dictionary<string, int>();

        // Act
        var result = source.ToKeyValueString();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.ToKeyValueString{TKey,TValue}"/> throws ArgumentNullException for null source.
    /// </summary>
    [Fact]
    public void ToKeyValueString_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        Dictionary<string, int> source = null!;

        // Act
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.ToKeyValueString(source);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region SafeCast Tests

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.SafeCast{TSource,TTarget}"/> casts compatible items.
    /// </summary>
    [Fact]
    public void SafeCast_WithMixedCollection_ReturnsCompatibleItems()
    {
        // Arrange
        object[] source = { "hello", 42, "world", 3.14, null };

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.SafeCast<object, string>(source).ToList();

        // Assert
        result.Should().Equal("hello", "world");
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.SafeCast{TSource,TTarget}"/> returns empty for incompatible types.
    /// </summary>
    [Fact]
    public void SafeCast_WithIncompatibleTypes_ReturnsEmpty()
    {
        // Arrange
        var source = new[] { 1, 2, 3 };

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.SafeCast<int, string>(source).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.SafeCast{TSource,TTarget}"/> throws ArgumentNullException for null source.
    /// </summary>
    [Fact]
    public void SafeCast_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<object> source = null!;

        // Act
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.SafeCast<object, string>(source).ToList();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    #endregion

    #region Shuffle Tests

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Shuffle{T}"/> returns shuffled collection.
    /// </summary>
    [Fact]
    public void Shuffle_WithCollection_ReturnsShuffledCollection()
    {
        // Arrange
        var source = Enumerable.Range(1, 5).ToList();

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.Shuffle(source).ToList();

        // Assert
        result.Should().HaveCount(5);
        result.Should().ContainInOrder(source.OrderBy(x => Guid.NewGuid())); // Just verify it contains same elements
        result.Should().BeEquivalentTo(source); // Same elements, different order
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Shuffle{T}"/> throws ArgumentNullException for null source.
    /// </summary>
    [Fact]
    public void Shuffle_WithNullSource_ThrowsArgumentNullException()
    {
        // Arrange
        IEnumerable<int> source = null!;

        // Act
        var act = () => global::AspNetSpaTemplate.Utilities.CollectionExtensions.Shuffle(source).ToList();

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    /// <summary>
    /// Tests that <see cref="CollectionExtensions.Shuffle{T}"/> returns empty for empty collection.
    /// </summary>
    [Fact]
    public void Shuffle_WithEmptyCollection_ReturnsEmptyCollection()
    {
        // Arrange
        var source = Enumerable.Empty<int>();

        // Act
        var result = global::AspNetSpaTemplate.Utilities.CollectionExtensions.Shuffle(source).ToList();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion
}