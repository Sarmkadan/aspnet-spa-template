using AspNetSpaTemplate.Data;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace AspNetSpaTemplate.Benchmarks;

/// <summary>
/// Benchmark tests for the <see cref="ProductService"/> class to measure performance of common product-related operations.
/// Uses in-memory database with 1000 seeded products for consistent benchmarking.
/// </summary>
[MemoryDiagnoser]
public class ProductServiceBenchmarks
{
    /// <summary>
    /// In-memory database context used for benchmarking.
    /// </summary>
    private AppDbContext _context = null!;
    /// <summary>
    /// Instance of ProductService under test.
    /// </summary>
    private ProductService _productService = null!;

    /// <summary>
    /// Sets up the benchmark environment by configuring an in-memory database and seeding it with test data.
    /// Creates a ProductService instance with a ProductRepository and null logger for benchmarking purposes.
    /// Seeds 1000 products into the database for consistent performance testing.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "BenchmarksDb")
            .Options;

        _context = new AppDbContext(options);
        var repo = new ProductRepository(_context);
        _productService = new ProductService(repo, NullLogger<ProductService>.Instance);

        // Seed data
        for (int i = 1; i <= 1000; i++)
        {
            _context.Products.Add(new Product { Id = i, Name = $"Product {i}", IsAvailable = true, Price = 10.0m });
        }
        _context.SaveChanges();
    }

    /// <summary>
    /// Benchmarks the GetAllProductsAsync method which retrieves a paginated list of products.
    /// Measures performance when fetching the first page of 10 products from a dataset of 1000 products.
    /// </summary>
    /// <returns>List of ProductResponse objects representing the first page of products.</returns>
    [Benchmark]
    public async Task<List<AspNetSpaTemplate.DTOs.ProductResponse>> GetAllProducts()
    {
        var result = await _productService.GetAllProductsAsync(1, 10);
        return result.Products;
    }

    /// <summary>
    /// Benchmarks the GetProductByIdAsync method which retrieves a single product by its ID.
    /// Measures performance when fetching a product with ID 1 from the seeded dataset.
    /// </summary>
    /// <returns>The ProductResponse object for the requested product, or null if not found.</returns>
    [Benchmark]
    public async Task<AspNetSpaTemplate.DTOs.ProductResponse?> GetProductById()
    {
        return await _productService.GetProductByIdAsync(1);
    }

    /// <summary>
    /// Benchmarks the GetFeaturedProductsAsync method which retrieves featured products.
    /// Measures performance when fetching up to 10 featured products from the dataset.
    /// </summary>
    /// <returns>List of ProductResponse objects representing featured products.</returns>
    [Benchmark]
    public async Task<List<AspNetSpaTemplate.DTOs.ProductResponse>> GetFeaturedProducts()
    {
        return await _productService.GetFeaturedProductsAsync(10);
    }
}
