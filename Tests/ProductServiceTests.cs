#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace AspNetSpaTemplate.Tests;

/// <summary>
/// Tests for ProductService bulk price update functionality.
/// </summary>
public sealed class ProductServiceTests
{
    private readonly Mock<ProductRepository> _mockProductRepository;
    private readonly Mock<ILogger<ProductService>> _mockLogger;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockProductRepository = new Mock<ProductRepository>(null);
        _mockLogger = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_mockProductRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UpdatePricesAsync_WithNullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        UpdateProductPriceRequest? nullRequest = null;

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => _productService.UpdatePricesAsync(nullRequest!));
    }

    [Fact]
    public async Task UpdatePricesAsync_WithEmptyPriceUpdates_ThrowsValidationException()
    {
        // Arrange
        var request = new UpdateProductPriceRequest
        {
            PriceUpdates = new List<ProductPriceUpdate>()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _productService.UpdatePricesAsync(request));
        Assert.Equal("PriceUpdates", exception.Field);
    }

    [Fact]
    public async Task UpdatePricesAsync_WithTooManyProducts_ThrowsValidationException()
    {
        // Arrange
        var request = new UpdateProductPriceRequest
        {
            PriceUpdates = Enumerable.Range(1, 1001)
                .Select(i => new ProductPriceUpdate { ProductId = i, NewPrice = 10.00m })
                .ToList()
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<ValidationException>(() => _productService.UpdatePricesAsync(request));
        Assert.Equal("PriceUpdates", exception.Field);
    }

    [Fact]
    public async Task UpdatePricesAsync_WithInvalidPriceRange_UpdatesNoneAndReportsErrors()
    {
        // Arrange
        var product1 = new Product { Id = 1, Name = "Product 1", Price = 10.00m, Description = "Test", Category = ProductCategory.Electronics };
        var product2 = new Product { Id = 2, Name = "Product 2", Price = 20.00m, Description = "Test", Category = ProductCategory.Clothing };

        _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product1);
        _mockProductRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(product2);

        var request = new UpdateProductPriceRequest
        {
            PriceUpdates = new List<ProductPriceUpdate>
            {
                new ProductPriceUpdate { ProductId = 1, NewPrice = 100.00m },
                new ProductPriceUpdate { ProductId = 2, NewPrice = -5.00m }
            }
        };

        // Act
        var response = await _productService.UpdatePricesAsync(request);

        // Assert
        Assert.Equal(2, response.TotalProcessed);
        Assert.Equal(0, response.SuccessCount);
        Assert.Equal(2, response.FailureCount);
        Assert.Equal(2, response.Results.Count);
        Assert.All(response.Results, r => Assert.False(r.Success));
        Assert.Contains(response.Results, r => r.ErrorCode == "INVALID_PRICE_RANGE");
    }

    [Fact]
    public async Task UpdatePricesAsync_WithNonExistentProduct_ReportsError()
    {
        // Arrange
        _mockProductRepository.Setup(x => x.GetByIdAsync(999)).ReturnsAsync((Product?)null);

        var request = new UpdateProductPriceRequest
        {
            PriceUpdates = new List<ProductPriceUpdate>
            {
                new ProductPriceUpdate { ProductId = 999, NewPrice = 50.00m }
            }
        };

        // Act
        var response = await _productService.UpdatePricesAsync(request);

        // Assert
        Assert.Equal(1, response.TotalProcessed);
        Assert.Equal(0, response.SuccessCount);
        Assert.Equal(1, response.FailureCount);
        Assert.Single(response.Results);
        Assert.False(response.Results[0].Success);
        Assert.Equal("PRODUCT_NOT_FOUND", response.Results[0].ErrorCode);
    }

    [Fact]
    public async Task UpdatePricesAsync_WithValidData_UpdatesPricesAndReturnsSuccess()
    {
        // Arrange
        var product1 = new Product { Id = 1, Name = "Product 1", Price = 10.00m, Description = "Test", Category = ProductCategory.Electronics };
        var product2 = new Product { Id = 2, Name = "Product 2", Price = 20.00m, Description = "Test", Category = ProductCategory.Clothing };

        _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product1);
        _mockProductRepository.Setup(x => x.GetByIdAsync(2)).ReturnsAsync(product2);
        _mockProductRepository.Setup(x => x.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var request = new UpdateProductPriceRequest
        {
            PriceUpdates = new List<ProductPriceUpdate>
            {
                new ProductPriceUpdate { ProductId = 1, NewPrice = 15.99m },
                new ProductPriceUpdate { ProductId = 2, NewPrice = 25.50m }
            }
        };

        // Act
        var response = await _productService.UpdatePricesAsync(request);

        // Assert
        Assert.Equal(2, response.TotalProcessed);
        Assert.Equal(2, response.SuccessCount);
        Assert.Equal(0, response.FailureCount);
        Assert.Equal(2, response.Results.Count);
        Assert.All(response.Results, r => Assert.True(r.Success));
        Assert.Equal("Product 1", response.Results[0].ProductName);
        Assert.Equal("Product 2", response.Results[1].ProductName);
        Assert.Equal(15.99m, response.Results[0].NewPrice);
        Assert.Equal(25.50m, response.Results[1].NewPrice);

        // Verify product was updated
        Assert.Equal(15.99m, product1.Price);
        Assert.Equal(25.50m, product2.Price);

        // Verify repository was called
        _mockProductRepository.Verify(x => x.Update(It.IsAny<Product>()), Times.Exactly(2));
        _mockProductRepository.Verify(x => x.SaveChangesAsync(), Times.Exactly(1));
    }

    [Fact]
    public async Task UpdatePricesAsync_WithDatabaseError_ReportsErrorForFailedUpdate()
    {
        // Arrange
        var product1 = new Product { Id = 1, Name = "Product 1", Price = 10.00m, Description = "Test", Category = ProductCategory.Electronics };

        _mockProductRepository.Setup(x => x.GetByIdAsync(1)).ReturnsAsync(product1);
        _mockProductRepository.Setup(x => x.Update(It.IsAny<Product>())).Throws<InvalidOperationException>();

        var request = new UpdateProductPriceRequest
        {
            PriceUpdates = new List<ProductPriceUpdate>
            {
                new ProductPriceUpdate { ProductId = 1, NewPrice = 15.99m }
            }
        };

        // Act
        var response = await _productService.UpdatePricesAsync(request);

        // Assert
        Assert.Equal(1, response.TotalProcessed);
        Assert.Equal(0, response.SuccessCount);
        Assert.Equal(1, response.FailureCount);
        Assert.Single(response.Results);
        Assert.False(response.Results[0].Success);
        Assert.Equal("PRICE_UPDATE_FAILED", response.Results[0].ErrorCode);
    }
}
