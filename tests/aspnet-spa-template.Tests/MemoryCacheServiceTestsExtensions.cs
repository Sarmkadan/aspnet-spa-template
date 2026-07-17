#nullable enable

using System.Globalization;
using AspNetSpaTemplate.Caching;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Extension methods for <see cref="MemoryCacheServiceTests"/> that provide additional testing utilities
/// for the <see cref="MemoryCacheService"/> cache implementation.
/// </summary>
public static class MemoryCacheServiceTestsExtensions
{
    /// <summary>
    /// Creates a new cache entry, verifies it was stored correctly, then removes it.
    /// A convenience method for testing cache operations.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <exception cref="ArgumentNullException"><paramref name="test"/>, <paramref name="key"/>, or <paramref name="value"/> is <see langword="null"/>.</exception>
    public static async Task SetGetAndRemoveAsync<T>(
        this MemoryCacheServiceTests test,
        string key,
        T value) where T : class
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        // Arrange - set a value using the test's cache service
        await test._cacheService.SetAsync(key, value);

        // Act & Assert - verify it was stored
        var result = await test._cacheService.GetAsync<T>(key);
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(value);

        // Act & Assert - remove it
        await test._cacheService.RemoveAsync(key);
        var afterRemove = await test._cacheService.GetAsync<T>(key);
        afterRemove.Should().BeNull();
    }

    /// <summary>
    /// Tests the GetOrSetAsync pattern by setting a value, then retrieving it,
    /// verifying the factory function is only called once.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <exception cref="ArgumentNullException"><paramref name="test"/> or <paramref name="key"/> is <see langword="null"/>.</exception>
    public static async Task TestGetOrSetPatternAsync<T>(
        this MemoryCacheServiceTests test,
        string key,
        T value) where T : class
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(value);

        var factoryCallCount = 0;

        // First call - should use factory
        var result1 = await test._cacheService.GetOrSetAsync(key, async () =>
        {
            factoryCallCount++;
            return value;
        });

        result1.Should().BeEquivalentTo(value);
        factoryCallCount.Should().Be(1);

        // Second call - should use cached value
        var result2 = await test._cacheService.GetOrSetAsync(key, async () =>
        {
            factoryCallCount++;
            return value;
        });

        result2.Should().BeEquivalentTo(value);
        factoryCallCount.Should().Be(1); // Factory should not be called again
    }

    /// <summary>
    /// Tests incrementing a counter and verifies the final value.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="key">The cache key for the counter.</param>
    /// <param name="expectedFinalValue">The expected final counter value.</param>
    /// <exception cref="ArgumentNullException"><paramref name="test"/> or <paramref name="key"/> is <see langword="null"/>.</exception>
    public static async Task TestIncrementAndVerifyAsync(
        this MemoryCacheServiceTests test,
        string key,
        int expectedFinalValue)
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(key);

        // Act
        var result1 = await test._cacheService.IncrementAsync(key);
        var result2 = await test._cacheService.IncrementAsync(key);
        var result3 = await test._cacheService.IncrementAsync(key, 5);

        // Assert
        result1.Should().Be(1);
        result2.Should().Be(2);
        result3.Should().Be(expectedFinalValue);
    }

    /// <summary>
    /// Tests the RemoveByPatternAsync functionality by setting multiple keys,
    /// removing by pattern, and verifying only matching keys were removed.
    /// </summary>
    /// <param name="test">The test instance.</param>
    /// <param name="pattern">The pattern to match (e.g., "user:*").</param>
    /// <param name="matchingKeys">Keys that should match the pattern.</param>
    /// <param name="nonMatchingKeys">Keys that should NOT match the pattern.</param>
    /// <exception cref="ArgumentNullException"><paramref name="test"/>, <paramref name="pattern"/>, <paramref name="matchingKeys"/>, or <paramref name="nonMatchingKeys"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException"><paramref name="pattern"/> is empty or whitespace.</exception>
    public static async Task TestRemoveByPatternAsync<T>(
        this MemoryCacheServiceTests test,
        string pattern,
        string[] matchingKeys,
        string[] nonMatchingKeys) where T : class
    {
        ArgumentNullException.ThrowIfNull(test);
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(matchingKeys);
        ArgumentNullException.ThrowIfNull(nonMatchingKeys);

        if (string.IsNullOrWhiteSpace(pattern))
        {
            throw new ArgumentException("Pattern cannot be empty or whitespace.", nameof(pattern));
        }

        // Arrange - set up test data
        foreach (var key in matchingKeys)
        {
            await test._cacheService.SetAsync(key, new TestData { Id = Array.IndexOf(matchingKeys, key) });
        }

        foreach (var key in nonMatchingKeys)
        {
            await test._cacheService.SetAsync(key, new TestData { Id = Array.IndexOf(nonMatchingKeys, key) });
        }

        // Act - remove by pattern
        await test._cacheService.RemoveByPatternAsync(pattern);

        // Assert - matching keys should be gone
        foreach (var key in matchingKeys)
        {
            var value = await test._cacheService.GetAsync<TestData>(key);
            value.Should().BeNull($"because key '{key}' matching pattern '{pattern}' should have been removed");
        }

        // Assert - non-matching keys should still exist
        foreach (var key in nonMatchingKeys)
        {
            var value = await test._cacheService.GetAsync<TestData>(key);
            value.Should().NotBeNull($"because key '{key}' should not match pattern '{pattern}' and should remain in cache");
        }
    }

    private sealed class TestData
    {
        public int Id { get; set; }
    }
}
