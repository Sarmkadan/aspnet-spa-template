#nullable enable
using AspNetSpaTemplate.Caching;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="CacheKeyBuilder"/> class.
/// Tests various cache key generation methods and validation logic.
/// </summary>
public sealed class CacheKeyBuilderTests
{
    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.UserById(int)"/> generates the correct cache key format for user IDs.
    /// </summary>
    [Fact]
    public void UserById_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.UserById(123);

        // Assert
        key.Should().Be("user:id:123");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.UserByEmail(string)"/> generates the correct cache key format for user emails.
    /// Verifies that email addresses are converted to lowercase in the generated key.
    /// </summary>
    [Fact]
    public void UserByEmail_GeneratesCorrectKeyInLowercase()
    {
        // Act
        var key = CacheKeyBuilder.UserByEmail("Test@Example.COM");

        // Assert
        key.Should().Be("user:email:test@example.com");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProductById(int)"/> generates the correct cache key format for product IDs.
    /// </summary>
    [Fact]
    public void ProductById_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.ProductById(456);

        // Assert
        key.Should().Be("product:id:456");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProductCategory(string)"/> generates the correct cache key format for product categories.
    /// </summary>
    [Fact]
    public void ProductCategory_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.ProductCategory("Electronics");

        // Assert
        key.Should().Be("product:category:Electronics");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProductSearch(string)"/> generates the correct cache key format for product searches.
    /// Verifies that search terms are converted to lowercase in the generated key.
    /// </summary>
    [Fact]
    public void ProductSearch_GeneratesCorrectKeyInLowercase()
    {
        // Act
        var key = CacheKeyBuilder.ProductSearch("Widget");

        // Assert
        key.Should().Be("product:search:widget");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.OrderById(int)"/> generates the correct cache key format for order IDs.
    /// </summary>
    [Fact]
    public void OrderById_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.OrderById(789);

        // Assert
        key.Should().Be("order:id:789");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ReviewsByProductId(int)"/> generates the correct cache key format for product reviews.
    /// </summary>
    [Fact]
    public void ReviewsByProductId_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.ReviewsByProductId(111);

        // Assert
        key.Should().Be("review:product:111");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.Pattern(string)"/> generates a wildcard pattern for cache key matching.
    /// </summary>
    [Fact]
    public void Pattern_GeneratesWildcardPattern()
    {
        // Act
        var pattern = CacheKeyBuilder.Pattern("product");

        // Assert
        pattern.Should().Be("product*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.InvalidationPatterns.AllProducts"/> returns the correct pattern for invalidating all product cache keys.
    /// </summary>
    [Fact]
    public void InvalidationPatterns_AllProductsPattern_IsCorrect()
    {
        // Assert
        CacheKeyBuilder.InvalidationPatterns.AllProducts.Should().Be("product:*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.InvalidationPatterns.AllOrders"/> returns the correct pattern for invalidating all order cache keys.
    /// </summary>
    [Fact]
    public void InvalidationPatterns_AllOrdersPattern_IsCorrect()
    {
        // Assert
        CacheKeyBuilder.InvalidationPatterns.AllOrders.Should().Be("order:*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ValidateKey(string)"/> does not throw an exception when given a valid cache key.
    /// </summary>
    [Fact]
    public void ValidateKey_WithValidKey_DoesNotThrow()
    {
        // Act
        var act = () => CacheKeyBuilder.ValidateKey("valid:key:123");

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ValidateKey(string)"/> throws an <see cref="ArgumentException"/> when given an empty cache key.
    /// </summary>
    [Fact]
    public void ValidateKey_WithEmptyKey_ThrowsArgumentException()
    {
        // Act
        var act = () => CacheKeyBuilder.ValidateKey("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ValidateKey(string)"/> throws an <see cref="ArgumentException"/> when given a cache key containing spaces.
    /// </summary>
    [Fact]
    public void ValidateKey_WithKeyContainingSpaces_ThrowsArgumentException()
    {
        // Act
        var act = () => CacheKeyBuilder.ValidateKey("invalid key");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ValidateKey(string)"/> throws an <see cref="ArgumentException"/> when given a cache key exceeding the maximum allowed length.
    /// </summary>
    [Fact]
    public void ValidateKey_WithKeyExceedingMaxLength_ThrowsArgumentException()
    {
        // Act
        var longKey = new string('a', 1025);
        var act = () => CacheKeyBuilder.ValidateKey(longKey);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.TemporaryKey(string)"/> generates unique cache keys for the same prefix.
    /// Verifies that each call produces a different key with the same prefix.
    /// </summary>
    [Fact]
    public void TemporaryKey_GeneratesUniqueKey()
    {
        // Act
        var key1 = CacheKeyBuilder.TemporaryKey("upload");
        var key2 = CacheKeyBuilder.TemporaryKey("upload");

        // Assert
        key1.Should().StartWith("temp:upload:");
        key2.Should().StartWith("temp:upload:");
        key1.Should().NotBe(key2);
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.RateLimitKey(string)"/> generates the correct cache key format for rate limiting.
    /// </summary>
    [Fact]
    public void RateLimitKey_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.RateLimitKey("client:192.168.1.1");

        // Assert
        key.Should().Be("ratelimit:client:192.168.1.1");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.SessionData(string, string)"/> generates the correct cache key format for session data.
    /// </summary>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="dataType">The type of session data being stored.</param>
    [Fact]
    public void SessionData_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.SessionData("session123", "preferences");

        // Assert
        key.Should().Be("session:session123:preferences");
    }
}
