#nullable enable
using AspNetSpaTemplate.Caching;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class CacheKeyBuilderTests
{
    [Fact]
    public void UserById_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.UserById(123);

        // Assert
        key.Should().Be("user:id:123");
    }

    [Fact]
    public void UserByEmail_GeneratesCorrectKeyInLowercase()
    {
        // Act
        var key = CacheKeyBuilder.UserByEmail("Test@Example.COM");

        // Assert
        key.Should().Be("user:email:test@example.com");
    }

    [Fact]
    public void ProductById_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.ProductById(456);

        // Assert
        key.Should().Be("product:id:456");
    }

    [Fact]
    public void ProductCategory_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.ProductCategory("Electronics");

        // Assert
        key.Should().Be("product:category:Electronics");
    }

    [Fact]
    public void ProductSearch_GeneratesCorrectKeyInLowercase()
    {
        // Act
        var key = CacheKeyBuilder.ProductSearch("Widget");

        // Assert
        key.Should().Be("product:search:widget");
    }

    [Fact]
    public void OrderById_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.OrderById(789);

        // Assert
        key.Should().Be("order:id:789");
    }

    [Fact]
    public void ReviewsByProductId_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.ReviewsByProductId(111);

        // Assert
        key.Should().Be("review:product:111");
    }

    [Fact]
    public void Pattern_GeneratesWildcardPattern()
    {
        // Act
        var pattern = CacheKeyBuilder.Pattern("product");

        // Assert
        pattern.Should().Be("product*");
    }

    [Fact]
    public void InvalidationPatterns_AllProductsPattern_IsCorrect()
    {
        // Assert
        CacheKeyBuilder.InvalidationPatterns.AllProducts.Should().Be("product:*");
    }

    [Fact]
    public void InvalidationPatterns_AllOrdersPattern_IsCorrect()
    {
        // Assert
        CacheKeyBuilder.InvalidationPatterns.AllOrders.Should().Be("order:*");
    }

    [Fact]
    public void ValidateKey_WithValidKey_DoesNotThrow()
    {
        // Act
        var act = () => CacheKeyBuilder.ValidateKey("valid:key:123");

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void ValidateKey_WithEmptyKey_ThrowsArgumentException()
    {
        // Act
        var act = () => CacheKeyBuilder.ValidateKey("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateKey_WithKeyContainingSpaces_ThrowsArgumentException()
    {
        // Act
        var act = () => CacheKeyBuilder.ValidateKey("invalid key");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ValidateKey_WithKeyExceedingMaxLength_ThrowsArgumentException()
    {
        // Act
        var longKey = new string('a', 1025);
        var act = () => CacheKeyBuilder.ValidateKey(longKey);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

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

    [Fact]
    public void RateLimitKey_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.RateLimitKey("client:192.168.1.1");

        // Assert
        key.Should().Be("ratelimit:client:192.168.1.1");
    }

    [Fact]
    public void SessionData_GeneratesCorrectKey()
    {
        // Act
        var key = CacheKeyBuilder.SessionData("session123", "preferences");

        // Assert
        key.Should().Be("session:session123:preferences");
    }
}
