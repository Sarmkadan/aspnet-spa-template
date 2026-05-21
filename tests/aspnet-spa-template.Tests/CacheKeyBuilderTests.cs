// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Caching;
using FluentAssertions;

namespace AspNetSpaTemplate.Tests;

public class CacheKeyBuilderTests
{
    [Fact]
    public void UserById_ReturnsCorrectFormat()
    {
        CacheKeyBuilder.UserById(42).Should().Be("user:id:42");
    }

    [Fact]
    public void UserByEmail_NormalizesToLowerCase()
    {
        CacheKeyBuilder.UserByEmail("Admin@Example.COM").Should().Be("user:email:admin@example.com");
    }

    [Fact]
    public void ProductSearch_NormalizesSearchTerm()
    {
        CacheKeyBuilder.ProductSearch("Laptop").Should().Be("product:search:laptop");
    }

    [Fact]
    public void ProductFeatured_ReturnsStaticKey()
    {
        CacheKeyBuilder.ProductFeatured.Should().Be("product:featured");
    }

    [Fact]
    public void SessionData_IncludesSessionIdAndKey()
    {
        CacheKeyBuilder.SessionData("sess-123", "cart").Should().Be("session:sess-123:cart");
    }

    [Fact]
    public void Pattern_AppendsWildcard()
    {
        CacheKeyBuilder.Pattern("product:").Should().Be("product:*");
    }

    [Fact]
    public void ValidateKey_EmptyKey_ThrowsArgumentException()
    {
        var act = () => CacheKeyBuilder.ValidateKey("");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateKey_WhitespaceKey_ThrowsArgumentException()
    {
        var act = () => CacheKeyBuilder.ValidateKey("   ");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateKey_KeyWithSpaces_ThrowsArgumentException()
    {
        var act = () => CacheKeyBuilder.ValidateKey("my key");
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateKey_TooLongKey_ThrowsArgumentException()
    {
        var longKey = new string('x', 1025);
        var act = () => CacheKeyBuilder.ValidateKey(longKey);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateKey_ValidKey_DoesNotThrow()
    {
        var act = () => CacheKeyBuilder.ValidateKey("user:id:42");
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateKey_ExactlyMaxLength_DoesNotThrow()
    {
        var key = new string('x', 1024);
        var act = () => CacheKeyBuilder.ValidateKey(key);
        act.Should().NotThrow();
    }

    [Fact]
    public void LockKey_ReturnsCorrectFormat()
    {
        CacheKeyBuilder.LockKey("checkout").Should().Be("lock:checkout");
    }

    [Fact]
    public void RateLimitKey_ReturnsCorrectFormat()
    {
        CacheKeyBuilder.RateLimitKey("client-1").Should().Be("ratelimit:client-1");
    }

    [Fact]
    public void InvalidationPatterns_AreCorrectWildcards()
    {
        CacheKeyBuilder.InvalidationPatterns.AllProducts.Should().Be("product:*");
        CacheKeyBuilder.InvalidationPatterns.AllOrders.Should().Be("order:*");
        CacheKeyBuilder.InvalidationPatterns.AllUsers.Should().Be("user:*");
    }
}
