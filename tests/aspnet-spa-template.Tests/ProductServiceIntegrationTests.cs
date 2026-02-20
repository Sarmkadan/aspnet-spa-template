#nullable enable
using AspNetSpaTemplate.Constants;
using AspNetSpaTemplate.Data;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.DTOs;
using AspNetSpaTemplate.Exceptions;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace AspNetSpaTemplate.Tests;

public sealed class ProductServiceIntegrationTests : IAsyncLifetime
{
    private readonly DbContextOptions<AppDbContext> _dbOptions;
    private AppDbContext _dbContext = null!;
    private ProductService _productService = null!;
    private ProductRepository _productRepository = null!;

    public ProductServiceIntegrationTests()
    {
        _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    }

    public async Task InitializeAsync()
    {
        _dbContext = new AppDbContext(_dbOptions);
        await _dbContext.Database.EnsureCreatedAsync();
        _productRepository = new ProductRepository(_dbContext);
        _productService = new ProductService(_productRepository);
    }

    public async Task DisposeAsync()
    {
        await _dbContext.Database.EnsureDeletedAsync();
        _dbContext.Dispose();
    }

    [Fact]
    public async Task EndToEnd_CreateUpdateAndSearchProduct_CompleteWorkflow()
    {
        // Arrange
        var createRequest = new CreateProductRequest
        {
            Name = "Amazing Gadget",
            Description = "The best gadget ever",
            Price = 129.99m,
            StockQuantity = 100,
            Category = ProductCategory.Electronics,
            ImageUrl = "https://example.com/gadget.jpg",
            Sku = "GAD-001"
        };

        // Act - Create Product
        var product = await _productService.CreateProductAsync(createRequest);

        // Assert - Product Created
        product.Should().NotBeNull();
        product.Name.Should().Be("Amazing Gadget");
        product.Price.Should().Be(129.99m);
        product.IsAvailable.Should().BeTrue();

        // Act - Update Product
        var updateRequest = new UpdateProductRequest
        {
            Name = "Updated Gadget",
            Description = "Even better now",
            Price = 99.99m,
            Category = ProductCategory.Electronics,
            ImageUrl = "https://example.com/gadget-v2.jpg",
            IsAvailable = true
        };
        var updated = await _productService.UpdateProductAsync(product.Id, updateRequest);

        // Assert - Product Updated
        updated.Name.Should().Be("Updated Gadget");
        updated.Price.Should().Be(99.99m);

        // Act - Search Product
        var searchResults = await _productService.SearchProductsAsync("Gadget");

        // Assert - Search Found Product
        searchResults.Should().HaveCount(1);
        searchResults[0].Name.Should().Contain("Gadget");
    }

    [Fact]
    public async Task CreateMultipleProducts_PaginationWorks()
    {
        // Arrange - Create 25 products
        for (int i = 1; i <= 25; i++)
        {
            var request = new CreateProductRequest
            {
                Name = $"Product {i:D3}",
                Description = $"Description {i}",
                Price = 10m + i,
                StockQuantity = 50,
                Category = ProductCategory.Books,
                ImageUrl = $"https://example.com/product-{i}.jpg",
                Sku = $"PRD-{i:D3}"
            };
            await _productService.CreateProductAsync(request);
        }

        // Act - Get first page
        var page1 = await _productService.GetAllProductsAsync(pageNumber: 1, pageSize: 10);

        // Assert - Page 1 has 10 items
        page1.Products.Should().HaveCount(10);
        page1.TotalCount.Should().Be(25);
        page1.TotalPages.Should().Be(3);

        // Act - Get second page
        var page2 = await _productService.GetAllProductsAsync(pageNumber: 2, pageSize: 10);

        // Assert - Page 2 has 10 items
        page2.Products.Should().HaveCount(10);
        page2.Products[0].Name.Should().Be("Product 011");
    }

    [Fact]
    public async Task ProductByCategory_FiltersCorrectly()
    {
        // Arrange - Create products in different categories
        var electronicRequest = new CreateProductRequest
        {
            Name = "Laptop",
            Description = "Computer",
            Price = 999m,
            StockQuantity = 10,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "ELEC-001"
        };

        var bookRequest = new CreateProductRequest
        {
            Name = "Programming Book",
            Description = "Learn to code",
            Price = 49m,
            StockQuantity = 50,
            Category = ProductCategory.Books,
            ImageUrl = "",
            Sku = "BOOK-001"
        };

        await _productService.CreateProductAsync(electronicRequest);
        await _productService.CreateProductAsync(bookRequest);

        // Act
        var electronics = await _productService.GetProductsByCategoryAsync(ProductCategory.Electronics);
        var books = await _productService.GetProductsByCategoryAsync(ProductCategory.Books);

        // Assert
        electronics.Products.Should().HaveCount(1);
        electronics.Products[0].Name.Should().Be("Laptop");
        books.Products.Should().HaveCount(1);
        books.Products[0].Name.Should().Be("Programming Book");
    }

    [Fact]
    public async Task FeaturedProducts_ReturnsOnlyFeatured()
    {
        // Arrange - Create featured and non-featured products
        var featured = new CreateProductRequest
        {
            Name = "Featured Item",
            Description = "This is featured",
            Price = 100m,
            StockQuantity = 20,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "FEAT-001"
        };

        var notFeatured = new CreateProductRequest
        {
            Name = "Regular Item",
            Description = "Not featured",
            Price = 50m,
            StockQuantity = 30,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "REG-001"
        };

        var featuredProduct = await _productService.CreateProductAsync(featured);
        await _productService.CreateProductAsync(notFeatured);

        // Act - Set featured
        await _productService.SetProductFeaturedAsync(featuredProduct.Id, true);

        // Get featured
        var featuredProducts = await _productService.GetFeaturedProductsAsync();

        // Assert
        featuredProducts.Should().HaveCount(1);
        featuredProducts[0].Name.Should().Be("Featured Item");
    }

    [Fact]
    public async Task ToggleAvailability_ProductCanBeHiddenAndShown()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "Availability Test",
            Description = "Test availability toggle",
            Price = 75m,
            StockQuantity = 40,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "AVAIL-001"
        };

        var product = await _productService.CreateProductAsync(request);

        // Assert - Initially available
        product.IsAvailable.Should().BeTrue();

        // Act - Make unavailable
        await _productService.SetProductAvailabilityAsync(product.Id, false);

        // Assert - Not in available products listing
        var allProducts = await _productService.GetAllProductsAsync();
        allProducts.Products.Should().NotContain(p => p.Id == product.Id);

        // Act - Make available again
        await _productService.SetProductAvailabilityAsync(product.Id, true);

        // Assert - Back in listing
        var allProductsAgain = await _productService.GetAllProductsAsync();
        allProductsAgain.Products.Should().Contain(p => p.Id == product.Id);
    }

    [Fact]
    public async Task InvalidPrice_PreventProductCreation()
    {
        // Arrange
        var invalidRequest = new CreateProductRequest
        {
            Name = "Expensive Item",
            Description = "Too expensive",
            Price = 999999m,
            StockQuantity = 10,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "EXP-001"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ValidationException>(() => _productService.CreateProductAsync(invalidRequest));
    }

    [Fact]
    public async Task DeleteProduct_RemovesFromDatabase()
    {
        // Arrange
        var request = new CreateProductRequest
        {
            Name = "To Delete",
            Description = "This will be deleted",
            Price = 50m,
            StockQuantity = 10,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "DEL-001"
        };

        var product = await _productService.CreateProductAsync(request);

        // Act - Verify exists
        var retrieved = await _productService.GetProductByIdAsync(product.Id);
        retrieved.Should().NotBeNull();

        // Act - Delete
        await _productService.DeleteProductAsync(product.Id);

        // Assert - Not found
        await Assert.ThrowsAsync<NotFoundException>(() => _productService.GetProductByIdAsync(product.Id));
    }

    [Fact]
    public async Task TopRatedProducts_ReturnsHighestRated()
    {
        // Arrange - Create products and set ratings manually
        var product1Request = new CreateProductRequest
        {
            Name = "Good Product",
            Description = "4.5 stars",
            Price = 100m,
            StockQuantity = 20,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "GOOD-001"
        };

        var product2Request = new CreateProductRequest
        {
            Name = "Best Product",
            Description = "5 stars",
            Price = 200m,
            StockQuantity = 15,
            Category = ProductCategory.Electronics,
            ImageUrl = "",
            Sku = "BEST-001"
        };

        var product1 = await _productService.CreateProductAsync(product1Request);
        var product2 = await _productService.CreateProductAsync(product2Request);

        // Manually set ratings in database
        var dbProduct1 = await _productRepository.GetByIdAsync(product1.Id);
        var dbProduct2 = await _productRepository.GetByIdAsync(product2.Id);
        if (dbProduct1 != null) dbProduct1.UpdateRating(4.5m, 10);
        if (dbProduct2 != null) dbProduct2.UpdateRating(5.0m, 20);
        await _productRepository.SaveChangesAsync();

        // Act
        var topRated = await _productService.GetTopRatedProductsAsync(10);

        // Assert
        topRated.Should().HaveCountGreaterThanOrEqualTo(1);
        topRated[0].Name.Should().Be("Best Product");
    }
}
