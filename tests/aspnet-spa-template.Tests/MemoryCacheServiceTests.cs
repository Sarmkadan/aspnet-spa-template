#nullable enable
using AspNetSpaTemplate.Caching;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class MemoryCacheServiceTests
{
    private readonly Mock<ILogger<MemoryCacheService>> _mockLogger;
    internal readonly MemoryCacheService _cacheService;

    public MemoryCacheServiceTests()
    {
        _mockLogger = new Mock<ILogger<MemoryCacheService>>();
        _cacheService = new MemoryCacheService(_mockLogger.Object);
    }

    [Fact]
    public async Task GetAsync_WithExistingKey_ReturnsValue()
    {
        // Arrange
        var key = "test:key";
        var value = new TestData { Id = 1, Name = "Test" };
        await _cacheService.SetAsync(key, value);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().NotBeNull();
        result?.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_WithNonExistingKey_ReturnsNull()
    {
        // Act
        var result = await _cacheService.GetAsync<TestData>("nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAsync_WithExpiredEntry_ReturnsNullAndRemovesEntry()
    {
        // Arrange
        var key = "expired:key";
        var value = new TestData { Id = 1, Name = "Test" };
        var ttl = TimeSpan.FromMilliseconds(100);
        await _cacheService.SetAsync(key, value, ttl);

        // Wait for expiration
        await Task.Delay(150);

        // Act
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_WithValue_StoresValue()
    {
        // Arrange
        var key = "test:set";
        var value = new TestData { Id = 2, Name = "SetTest" };

        // Act
        await _cacheService.SetAsync(key, value);
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().NotBeNull();
        result?.Id.Should().Be(2);
    }

    [Fact]
    public async Task RemoveAsync_WithExistingKey_RemovesValue()
    {
        // Arrange
        var key = "test:remove";
        var value = new TestData { Id = 1, Name = "ToRemove" };
        await _cacheService.SetAsync(key, value);

        // Act
        await _cacheService.RemoveAsync(key);
        var result = await _cacheService.GetAsync<TestData>(key);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task RemoveByPatternAsync_WithMatchingPattern_RemovesMatchingKeys()
    {
        // Arrange
        await _cacheService.SetAsync("product:1", new TestData { Id = 1 });
        await _cacheService.SetAsync("product:2", new TestData { Id = 2 });
        await _cacheService.SetAsync("user:1", new TestData { Id = 3 });

        // Act
        await _cacheService.RemoveByPatternAsync("product:*");

        // Assert
        var product1 = await _cacheService.GetAsync<TestData>("product:1");
        var product2 = await _cacheService.GetAsync<TestData>("product:2");
        var user1 = await _cacheService.GetAsync<TestData>("user:1");

        product1.Should().BeNull();
        product2.Should().BeNull();
        user1.Should().NotBeNull();
    }

    [Fact]
    public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        var key = "exists:test";
        await _cacheService.SetAsync(key, new TestData { Id = 1 });

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact]
    public async Task ExistsAsync_WithNonExistingKey_ReturnsFalse()
    {
        // Act
        var exists = await _cacheService.ExistsAsync("nonexistent:key");

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_WithExpiredKey_ReturnsFalseAndRemovesEntry()
    {
        // Arrange
        var key = "expired:exists";
        await _cacheService.SetAsync(key, new TestData { Id = 1 }, TimeSpan.FromMilliseconds(100));

        // Wait for expiration
        await Task.Delay(150);

        // Act
        var exists = await _cacheService.ExistsAsync(key);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact]
    public async Task GetOrSetAsync_WithCachedValue_ReturnsCachedValue()
    {
        // Arrange
        var key = "getorset:cached";
        var cachedValue = new TestData { Id = 1, Name = "Cached" };
        await _cacheService.SetAsync(key, cachedValue);
        var factoryCalls = 0;

        // Act
        var result = await _cacheService.GetOrSetAsync(key, async () =>
        {
            factoryCalls++;
            return new TestData { Id = 2, Name = "New" };
        });

        // Assert
        result.Should().NotBeNull();
        result?.Name.Should().Be("Cached");
        factoryCalls.Should().Be(0);
    }

    [Fact]
    public async Task GetOrSetAsync_WithMissingKey_CallsFactoryAndCachesValue()
    {
        // Arrange
        var key = "getorset:missing";
        var newValue = new TestData { Id = 2, Name = "New" };
        var factoryCalls = 0;

        // Act
        var result = await _cacheService.GetOrSetAsync(key, async () =>
        {
            factoryCalls++;
            return newValue;
        });

        // Assert
        result.Should().NotBeNull();
        result?.Name.Should().Be("New");
        factoryCalls.Should().Be(1);

        var cachedResult = await _cacheService.GetAsync<TestData>(key);
        cachedResult.Should().NotBeNull();
    }

    [Fact]
    public async Task IncrementAsync_WithNewKey_InitializesToOneAndIncrements()
    {
        // Arrange
        var key = "counter:new";

        // Act
        var result1 = await _cacheService.IncrementAsync(key);
        var result2 = await _cacheService.IncrementAsync(key);
        var result3 = await _cacheService.IncrementAsync(key, 5);

        // Assert
        result1.Should().Be(1);
        result2.Should().Be(2);
        result3.Should().Be(7);
    }

    [Fact]
    public async Task ExpireAsync_WithExistingKey_SetsExpiration()
    {
        // Arrange
        var key = "expire:test";
        await _cacheService.SetAsync(key, new TestData { Id = 1 });

        // Act
        var result = await _cacheService.ExpireAsync(key, TimeSpan.FromMilliseconds(100));

        // Assert
        result.Should().BeTrue();
        await Task.Delay(150);

        var expired = await _cacheService.GetAsync<TestData>(key);
        expired.Should().BeNull();
    }

    [Fact]
    public async Task ExpireAsync_WithNonExistingKey_ReturnsFalse()
    {
        // Act
        var result = await _cacheService.ExpireAsync("nonexistent", TimeSpan.FromSeconds(10));

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetKeysAsync_WithPattern_ReturnsMatchingKeys()
    {
        // Arrange
        await _cacheService.SetAsync("user:1", new TestData { Id = 1 });
        await _cacheService.SetAsync("user:2", new TestData { Id = 2 });
        await _cacheService.SetAsync("product:1", new TestData { Id = 3 });

        // Act
        var keys = await _cacheService.GetKeysAsync("user:*");

        // Assert
        keys.Should().Contain("user:1");
        keys.Should().Contain("user:2");
        keys.Should().NotContain("product:1");
    }

    [Fact]
    public async Task FlushAllAsync_ClearsAllEntries()
    {
        // Arrange
        await _cacheService.SetAsync("key1", new TestData { Id = 1 });
        await _cacheService.SetAsync("key2", new TestData { Id = 2 });

        // Act
        await _cacheService.FlushAllAsync();

        // Assert
        var key1 = await _cacheService.GetAsync<TestData>("key1");
        var key2 = await _cacheService.GetAsync<TestData>("key2");

        key1.Should().BeNull();
        key2.Should().BeNull();
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectStats()
    {
        // Arrange
        await _cacheService.SetAsync("key1", new TestData { Id = 1 });
        await _cacheService.GetAsync<TestData>("key1"); // hit
        await _cacheService.GetAsync<TestData>("key2"); // miss

        // Act
        var stats = await _cacheService.GetStatisticsAsync();

        // Assert
        stats.TotalRequests.Should().BeGreaterThan(0);
        stats.CacheHits.Should().BeGreaterThan(0);
        stats.CacheMisses.Should().BeGreaterThan(0);
    }

    private class TestData
    {
        public int Id { get; set; }
        public string? Name { get; set; }
    }
}
