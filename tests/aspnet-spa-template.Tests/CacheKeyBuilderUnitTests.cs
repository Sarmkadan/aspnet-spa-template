#nullable enable
using AspNetSpaTemplate.Caching;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Contains unit tests for the <see cref="CacheKeyBuilder"/> class.
/// Tests various cache key generation methods including user keys, product keys, order keys,
/// review keys, configuration keys, rate limiting keys, session data keys, and validation methods.
/// </summary>
public sealed class CacheKeyBuilderUnitTests
{
    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.UserById"/> generates correct cache key for user by ID.
    /// </summary>
    [Fact]
    public void UserById_WithValidUserId_ReturnsCorrectKey()
    {
        // Arrange
        var userId = 123;

        // Act
        var key = CacheKeyBuilder.UserById(userId);

        // Assert
        key.Should().Be("user:id:123");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.UserByEmail"/> generates correct cache key for user by email.
    /// </summary>
    [Fact]
    public void UserByEmail_WithValidEmail_ReturnsCorrectKey()
    {
        // Arrange
        var email = "Test@Example.COM";

        // Act
        var key = CacheKeyBuilder.UserByEmail(email);

        // Assert
        key.Should().Be("user:email:test@example.com");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.UserByEmail"/> converts email to lowercase.
    /// </summary>
    [Fact]
    public void UserByEmail_WithMixedCaseEmail_ConvertsToLowercase()
    {
        // Arrange
        var email = "TeSt@ExAmPlE.CoM";

        // Act
        var key = CacheKeyBuilder.UserByEmail(email);

        // Assert
        key.Should().Be("user:email:test@example.com");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.UserByEmail"/> throws exception when email is null.
    /// </summary>
    [Fact]
    public void UserByEmail_WithNullEmail_ThrowsException()
    {
        // Arrange
        string? email = null;

        // Act
        var act = () => CacheKeyBuilder.UserByEmail(email!);

        // Assert
        act.Should().Throw<NullReferenceException>();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.UserSession"/> generates correct cache key for user session.
    /// </summary>
    [Fact]
    public void UserSession_WithValidSessionId_ReturnsCorrectKey()
    {
        // Arrange
        var sessionId = "abc-123-def-456";

        // Act
        var key = CacheKeyBuilder.UserSession(sessionId);

        // Assert
        key.Should().Be("user:session:abc-123-def-456");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.UserSession"/> returns correct key even when session ID is null.
    /// </summary>
    [Fact]
    public void UserSession_WithNullSessionId_ReturnsKey()
    {
        // Arrange
        string? sessionId = null;

        // Act
        var key = CacheKeyBuilder.UserSession(sessionId!);

        // Assert
        key.Should().Be("user:session:");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProductById"/> generates correct cache key for product by ID.
    /// </summary>
    [Fact]
    public void ProductById_WithValidProductId_ReturnsCorrectKey()
    {
        // Arrange
        var productId = 456;

        // Act
        var key = CacheKeyBuilder.ProductById(productId);

        // Assert
        key.Should().Be("product:id:456");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProductCategory"/> generates correct cache key for product category.
    /// </summary>
    [Fact]
    public void ProductCategory_WithValidCategory_ReturnsCorrectKey()
    {
        // Arrange
        var category = "Electronics";

        // Act
        var key = CacheKeyBuilder.ProductCategory(category);

        // Assert
        key.Should().Be("product:category:Electronics");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProductCategory"/> preserves original case.
    /// </summary>
    [Fact]
    public void ProductCategory_WithMixedCaseCategory_PreservesOriginalCase()
    {
        // Arrange
        var category = "ElEcTrOnIcS";

        // Act
        var key = CacheKeyBuilder.ProductCategory(category);

        // Assert
        key.Should().Be("product:category:ElEcTrOnIcS");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProductFeatured"/> returns constant cache key for featured products.
    /// </summary>
    [Fact]
    public void ProductFeatured_ReturnsConstantKey()
    {
        // Act
        var key = CacheKeyBuilder.ProductFeatured;

        // Assert
        key.Should().Be("product:featured");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProductSearch"/> generates correct cache key for product search.
    /// </summary>
    [Fact]
    public void ProductSearch_WithValidSearchTerm_ReturnsCorrectKey()
    {
        // Arrange
        var term = "Laptop";

        // Act
        var key = CacheKeyBuilder.ProductSearch(term);

        // Assert
        key.Should().Be("product:search:laptop");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProductSearch"/> converts search term to lowercase.
    /// </summary>
    [Fact]
    public void ProductSearch_WithMixedCaseSearchTerm_ConvertsToLowercase()
    {
        // Arrange
        var term = "LaPtOp";

        // Act
        var key = CacheKeyBuilder.ProductSearch(term);

        // Assert
        key.Should().Be("product:search:laptop");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.OrderById"/> generates correct cache key for order by ID.
    /// </summary>
    [Fact]
    public void OrderById_WithValidOrderId_ReturnsCorrectKey()
    {
        // Arrange
        var orderId = 789;

        // Act
        var key = CacheKeyBuilder.OrderById(orderId);

        // Assert
        key.Should().Be("order:id:789");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.OrdersByUserId"/> generates correct cache key for orders by user ID.
    /// </summary>
    [Fact]
    public void OrdersByUserId_WithValidUserId_ReturnsCorrectKey()
    {
        // Arrange
        var userId = 123;

        // Act
        var key = CacheKeyBuilder.OrdersByUserId(userId);

        // Assert
        key.Should().Be("order:user:123");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.OrdersByStatus"/> generates correct cache key for orders by status.
    /// </summary>
    [Fact]
    public void OrdersByStatus_WithValidStatus_ReturnsCorrectKey()
    {
        // Arrange
        var status = "Completed";

        // Act
        var key = CacheKeyBuilder.OrdersByStatus(status);

        // Assert
        key.Should().Be("order:status:Completed");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.OrdersByStatus"/> preserves original case.
    /// </summary>
    [Fact]
    public void OrdersByStatus_WithMixedCaseStatus_PreservesOriginalCase()
    {
        // Arrange
        var status = "CoMpLeTeD";

        // Act
        var key = CacheKeyBuilder.OrdersByStatus(status);

        // Assert
        key.Should().Be("order:status:CoMpLeTeD");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ReviewsByProductId"/> generates correct cache key for reviews by product ID.
    /// </summary>
    [Fact]
    public void ReviewsByProductId_WithValidProductId_ReturnsCorrectKey()
    {
        // Arrange
        var productId = 456;

        // Act
        var key = CacheKeyBuilder.ReviewsByProductId(productId);

        // Assert
        key.Should().Be("review:product:456");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ReviewsByUserId"/> generates correct cache key for reviews by user ID.
    /// </summary>
    [Fact]
    public void ReviewsByUserId_WithValidUserId_ReturnsCorrectKey()
    {
        // Arrange
        var userId = 123;

        // Act
        var key = CacheKeyBuilder.ReviewsByUserId(userId);

        // Assert
        key.Should().Be("review:user:123");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.Config"/> generates correct cache key for configuration.
    /// </summary>
    [Fact]
    public void Config_WithValidConfigKey_ReturnsCorrectKey()
    {
        // Arrange
        var configKey = "AppSettings:Database:ConnectionString";

        // Act
        var key = CacheKeyBuilder.Config(configKey);

        // Assert
        key.Should().Be("config:AppSettings:Database:ConnectionString");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.Settings"/> returns constant cache key for settings.
    /// </summary>
    [Fact]
    public void Settings_ReturnsConstantKey()
    {
        // Act
        var key = CacheKeyBuilder.Settings;

        // Assert
        key.Should().Be("settings");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.RateLimitKey"/> generates correct cache key for rate limiting.
    /// </summary>
    [Fact]
    public void RateLimitKey_WithValidClientId_ReturnsCorrectKey()
    {
        // Arrange
        var clientId = "client-123";

        // Act
        var key = CacheKeyBuilder.RateLimitKey(clientId);

        // Assert
        key.Should().Be("ratelimit:client-123");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.RequestCount"/> generates correct cache key for request counting.
    /// </summary>
    [Fact]
    public void RequestCount_WithValidEndpoint_ReturnsCorrectKey()
    {
        // Arrange
        var endpoint = "/api/products";

        // Act
        var key = CacheKeyBuilder.RequestCount(endpoint);

        // Assert
        key.Should().Be("requests:/api/products");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.SessionData"/> generates correct cache key for session data.
    /// </summary>
    [Fact]
    public void SessionData_WithValidSessionIdAndKey_ReturnsCorrectKey()
    {
        // Arrange
        var sessionId = "session-abc";
        var dataKey = "user-preferences";

        // Act
        var key = CacheKeyBuilder.SessionData(sessionId, dataKey);

        // Assert
        key.Should().Be("session:session-abc:user-preferences");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.LockKey"/> generates correct cache key for distributed locks.
    /// </summary>
    [Fact]
    public void LockKey_WithValidResource_ReturnsCorrectKey()
    {
        // Arrange
        var resource = "user:123:profile";

        // Act
        var key = CacheKeyBuilder.LockKey(resource);

        // Assert
        key.Should().Be("lock:user:123:profile");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ProcessingKey"/> generates correct cache key for processing jobs.
    /// </summary>
    [Fact]
    public void ProcessingKey_WithValidJobId_ReturnsCorrectKey()
    {
        // Arrange
        var jobId = "job-456";

        // Act
        var key = CacheKeyBuilder.ProcessingKey(jobId);

        // Assert
        key.Should().Be("processing:job-456");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.Pattern"/> generates correct pattern for cache invalidation.
    /// </summary>
    [Fact]
    public void Pattern_WithValidPrefix_ReturnsCorrectPattern()
    {
        // Arrange
        var prefix = "product:id";

        // Act
        var pattern = CacheKeyBuilder.Pattern(prefix);

        // Assert
        pattern.Should().Be("product:id*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.InvalidationPatterns.AllProducts"/> returns correct pattern.
    /// </summary>
    [Fact]
    public void InvalidationPatterns_AllProducts_ReturnsCorrectPattern()
    {
        // Act
        var pattern = CacheKeyBuilder.InvalidationPatterns.AllProducts;

        // Assert
        pattern.Should().Be("product:*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.InvalidationPatterns.AllOrders"/> returns correct pattern.
    /// </summary>
    [Fact]
    public void InvalidationPatterns_AllOrders_ReturnsCorrectPattern()
    {
        // Act
        var pattern = CacheKeyBuilder.InvalidationPatterns.AllOrders;

        // Assert
        pattern.Should().Be("order:*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.InvalidationPatterns.AllReviews"/> returns correct pattern.
    /// </summary>
    [Fact]
    public void InvalidationPatterns_AllReviews_ReturnsCorrectPattern()
    {
        // Act
        var pattern = CacheKeyBuilder.InvalidationPatterns.AllReviews;

        // Assert
        pattern.Should().Be("review:*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.InvalidationPatterns.AllUsers"/> returns correct pattern.
    /// </summary>
    [Fact]
    public void InvalidationPatterns_AllUsers_ReturnsCorrectPattern()
    {
        // Act
        var pattern = CacheKeyBuilder.InvalidationPatterns.AllUsers;

        // Assert
        pattern.Should().Be("user:*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.InvalidationPatterns.AllSessions"/> returns correct pattern.
    /// </summary>
    [Fact]
    public void InvalidationPatterns_AllSessions_ReturnsCorrectPattern()
    {
        // Act
        var pattern = CacheKeyBuilder.InvalidationPatterns.AllSessions;

        // Assert
        pattern.Should().Be("user:session:*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.InvalidationPatterns.AllRateLimits"/> returns correct pattern.
    /// </summary>
    [Fact]
    public void InvalidationPatterns_AllRateLimits_ReturnsCorrectPattern()
    {
        // Act
        var pattern = CacheKeyBuilder.InvalidationPatterns.AllRateLimits;

        // Assert
        pattern.Should().Be("ratelimit:*");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ValidateKey"/> does not throw when key is valid.
    /// </summary>
    [Fact]
    public void ValidateKey_WithValidKey_DoesNotThrow()
    {
        // Arrange
        var key = "user:id:123";

        // Act
        var act = () => CacheKeyBuilder.ValidateKey(key);

        // Assert
        act.Should().NotThrow();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ValidateKey"/> throws exception when key is null.
    /// </summary>
    [Fact]
    public void ValidateKey_WithNullKey_ThrowsArgumentException()
    {
        // Arrange
        string? key = null;

        // Act
        var act = () => CacheKeyBuilder.ValidateKey(key!);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ValidateKey"/> throws exception when key is empty.
    /// </summary>
    [Fact]
    public void ValidateKey_WithEmptyKey_ThrowsArgumentException()
    {
        // Arrange
        var key = "";

        // Act
        var act = () => CacheKeyBuilder.ValidateKey(key);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ValidateKey"/> throws exception when key contains whitespace.
    /// </summary>
    [Fact]
    public void ValidateKey_WithWhitespaceKey_ThrowsArgumentException()
    {
        // Arrange
        var key = "user id:123";

        // Act
        var act = () => CacheKeyBuilder.ValidateKey(key);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.ValidateKey"/> throws exception when key exceeds maximum length.
    /// </summary>
    [Fact]
    public void ValidateKey_WithTooLongKey_ThrowsArgumentException()
    {
        // Arrange
        var key = new string('x', 1025); // 1025 characters

        // Act
        var act = () => CacheKeyBuilder.ValidateKey(key);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.TemporaryKey"/> generates a key with correct prefix.
    /// </summary>
    [Fact]
    public void TemporaryKey_WithValidPrefix_ReturnsKeyWithPrefix()
    {
        // Arrange
        var prefix = "test";

        // Act
        var key = CacheKeyBuilder.TemporaryKey(prefix);

        // Assert
        key.Should().StartWith("temp:");
        key.Should().Contain("test");
        key.Should().Contain(":");
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.TemporaryKey"/> generates different keys on subsequent calls.
    /// </summary>
    [Fact]
    public void TemporaryKey_WithSamePrefix_GeneratesDifferentKeys()
    {
        // Arrange
        var prefix = "unique";

        // Act
        var key1 = CacheKeyBuilder.TemporaryKey(prefix);
        var key2 = CacheKeyBuilder.TemporaryKey(prefix);

        // Assert
        key1.Should().NotBe(key2);
    }

    /// <summary>
    /// Tests that <see cref="CacheKeyBuilder.TemporaryKey"/> generates keys with consistent format.
    /// </summary>
    [Fact]
    public void TemporaryKey_WithValidPrefix_HasConsistentFormat()
    {
        // Arrange
        var prefix = "format";

        // Act
        var key = CacheKeyBuilder.TemporaryKey(prefix);

        // Assert
        var parts = key.Split(':');
        parts.Should().HaveCount(4);
        parts[0].Should().Be("temp");
        parts[1].Should().Be(prefix);
    }
}