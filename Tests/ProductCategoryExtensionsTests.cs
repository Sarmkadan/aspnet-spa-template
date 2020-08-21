using System;
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Caching;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests
{
    public class ProductCategoryExtensionsTests
    {
        [Theory]
        [InlineData(ProductCategory.Food, 0.05d)]
        [InlineData(ProductCategory.Books, 0.0d)]
        [InlineData(ProductCategory.Clothing, 0.10d)]
        [InlineData((ProductCategory)999, 0.15d)] // unknown category falls back to default
        public void GetTaxRate_ShouldReturnExpectedRate(ProductCategory category, double expectedRate)
        {
            // Act
            var actualRate = category.GetTaxRate();

            // Assert
            actualRate.Should().Be((decimal)expectedRate);
        }

        [Fact]
        public void ProductCategoryCacheKey_ShouldContainAllParts()
        {
            // Arrange
            var category = "electronics";

            // Act
            var cacheKey = CacheKeyBuilder.ProductCategory(category);

            // Assert
            cacheKey.Should().Contain("product");
            cacheKey.Should().Contain("category");
            cacheKey.Should().EndWith(category);
        }
    }
}
