using System;
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Caching;
using FluentAssertions;
using Xunit;

namespace AspNetSpaTemplate.Tests
{
    /// <summary>
    /// Contains unit tests for <see cref="ProductCategory"/> extension methods.
    /// </summary>
    public class ProductCategoryExtensionsTests
    {
        /// <summary>
        /// Tests that <see cref="ProductCategory.GetTaxRate()"/> returns the expected tax rate for known categories
        /// and the default rate for unknown categories.
        /// </summary>
        /// <param name="category">The product category to test.</param>
        /// <param name="expectedRate">The expected tax rate for the given category.</param>
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

        /// <summary>
        /// Tests that the product category cache key contains all required parts:
        /// "product", "category", and the category name itself.
        /// </summary>
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