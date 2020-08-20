#nullable enable
using System.Linq.Expressions;
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Moq;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class ProductServiceTests
{
    private readonly Mock<ProductRepository> _mockProductRepository;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockProductRepository = new Mock<ProductRepository>();
        _productService = new ProductService(_mockProductRepository.Object);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithValidId_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product
        {
            Id = productId,
            Name = "Test Product",
            Price = 50m,
            StockQuantity = 10,
            IsAvailable = true,
            Category = ProductCategory.Electronics
        };
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(productId);

        // Assert
        result.Should().NotBeNull();
        result?.Name.Should().Be("Test Product");
        result?.Price.Should().Be(50m);
    }

    [Fact]
    public async Task GetProductByIdAsync_WithInvalidId_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 999;
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync((Product?)null);

        // Act
        var act = () => _productService.GetProductByIdAsync(productId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetAllProductsAsync_ReturnsPagedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", IsAvailable = true },
            new Product { Id = 2, Name = "Product 2", IsAvailable = true }
        };
        _mockProductRepository.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(2);
        _mockProductRepository.Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllProductsAsync();

        // Assert
        result.Should().NotBeNull();
        result.Products.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetProductsByCategoryAsync_ReturnsProductsInCategory()
    {
        // Arrange
        var category = ProductCategory.Books;
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Book 1", Category = category, IsAvailable = true }
        };
        _mockProductRepository.Setup(r => r.GetPagedByCategoryAsync(category, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(products);
        _mockProductRepository.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Product, bool>>>())).ReturnsAsync(1);

        // Act
        var result = await _productService.GetProductsByCategoryAsync(category);

        // Assert
        result.Products.Should().HaveCount(1);
        result.Products[0].Category.Should().Contain("Books");
    }

    [Fact]
    public async Task GetFeaturedProductsAsync_ReturnsFeaturedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Featured 1", IsFeatured = true, IsAvailable = true }
        };
        _mockProductRepository.Setup(r => r.GetFeaturedProductsAsync(It.IsAny<int>())).ReturnsAsync(products);

        // Act
        var result = await _productService.GetFeaturedProductsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetTopRatedProductsAsync_ReturnsTopRatedProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Top Rated", Rating = 4.8m, IsAvailable = true }
        };
        _mockProductRepository.Setup(r => r.GetTopRatedAsync(It.IsAny<int>())).ReturnsAsync(products);

        // Act
        var result = await _productService.GetTopRatedProductsAsync();

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchProductsAsync_WithValidTerm_ReturnsMatchingProducts()
    {
        // Arrange
        var searchTerm = "widget";
        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Test Widget", IsAvailable = true }
        };
        _mockProductRepository.Setup(r => r.SearchAsync(searchTerm)).ReturnsAsync(products);

        // Act
        var result = await _productService.SearchProductsAsync(searchTerm);

        // Assert
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchProductsAsync_WithEmptyTerm_ReturnsEmptyList()
    {
        // Arrange
        var searchTerm = "";

        // Act
        var result = await _productService.SearchProductsAsync(searchTerm);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchProductsAsync_WithNullTerm_ReturnsEmptyList()
    {
        // Arrange
        string? searchTerm = null;

        // Act
        var result = await _productService.SearchProductsAsync(searchTerm ?? "");

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateProductAsync_WithValidRequest_CreatesProduct()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "New Product",
            Description = "A great product",
            Price = 99.99m,
            StockQuantity = 50,
            Category = ProductCategory.Electronics,
            ImageUrl = "https://example.com/image.jpg",
            Sku = "SKU-001"
        };
        _mockProductRepository.Setup(r => r.Add(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _productService.CreateProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("New Product");
        result.Price.Should().Be(99.99m);
        _mockProductRepository.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task CreateProductAsync_WithEmptyName_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "",
            Description = "A product",
            Price = 50m,
            StockQuantity = 10,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "SKU"
        };

        // Act
        var act = () => _productService.CreateProductAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateProductAsync_WithInvalidPrice_ThrowsValidationException()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Product",
            Description = "Description",
            Price = -10m,
            StockQuantity = 10,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "SKU"
        };

        // Act
        var act = () => _productService.CreateProductAsync(request);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateProductAsync_WithValidId_UpdatesProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Old Name", Price = 50m, IsAvailable = true };
        var request = new UpdateProductRequest
        {
            Name = "New Name",
            Description = "Updated",
            Price = 75m,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            IsAvailable = true
        };
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _productService.UpdateProductAsync(productId, request);

        // Assert
        result.Should().NotBeNull();
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task SetProductAvailabilityAsync_WithValidId_UpdatesAvailability()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, IsAvailable = true };
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _mockProductRepository.Setup(r => r.Update(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _productService.SetProductAvailabilityAsync(productId, false);

        // Assert
        _mockProductRepository.Verify(r => r.Update(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_WithValidId_DeletesProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "To Delete" };
        _mockProductRepository.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
        _mockProductRepository.Setup(r => r.Remove(It.IsAny<Product>()));
        _mockProductRepository.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _productService.DeleteProductAsync(productId);

        // Assert
        _mockProductRepository.Verify(r => r.Remove(product), Times.Once);
        _mockProductRepository.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}
