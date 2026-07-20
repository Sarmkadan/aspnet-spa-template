#nullable enable
// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using AspNetSpaTemplate.Caching;
using AspNetSpaTemplate.Configuration;
using AspNetSpaTemplate.Data;
using AspNetSpaTemplate.Data.Repositories;
using AspNetSpaTemplate.Middleware;
using AspNetSpaTemplate.Models;
using AspNetSpaTemplate.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=(localdb)\\mssqllocaldb;Database=AspNetSpaTemplate;Trusted_Connection=true;TrustServerCertificate=true;";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Dependency Injection
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(RepositoryBase<>));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<ReviewService>();

// Theme (dark mode) preferences
builder.Services.AddSingleton<ICacheService, MemoryCacheService>(sp =>
    new MemoryCacheService(sp.GetRequiredService<ILogger<MemoryCacheService>>()));
builder.Services.AddScoped<IThemeService, ThemeService>();

// Metrics registry for health metrics
builder.Services.AddSingleton<MetricsRegistry>();

// PWA manifest
builder.Services.AddSingleton<IManifestService, ManifestService>();

// Offline sync queue
builder.Services.AddSingleton<ISyncQueueService, SyncQueueService>();

// Offline support — asset versioning + HMR
builder.Services.AddOfflineSupport();

// Request/response logging — opt-in via "RequestLogging:Enabled" in appsettings.json
builder.Services.Configure<LoggingMiddlewareOptions>(
    builder.Configuration.GetSection(LoggingMiddlewareOptions.SectionName));

// API services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure AspnetSpaTemplateOptions
builder.Services.AddOptions<AspnetSpaTemplateOptions>()
    .Bind(builder.Configuration.GetSection(AspnetSpaTemplateOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// Configure PWA options
builder.Services.AddOptions<PwaOptions>()
    .Bind(builder.Configuration.GetSection(PwaOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });

    options.AddPolicy("AllowLocal", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5000", "http://127.0.0.1:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Support hosting behind a reverse proxy at a sub-path prefix.
// Set "PathBase": "/myapp" in appsettings.json (or via the PATHBASE env var)
// and configure the proxy to forward X-Forwarded-* headers.
// See docs/deployment.md for a full nginx/Caddy example.
var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrWhiteSpace(pathBase))
{
    app.UsePathBase(pathBase);
}

// Middleware pipeline
// UseRouting must precede UseCors so endpoint metadata is available when
// the CORS middleware evaluates policies. UseCors must also run before
// UseHttpsRedirection so that HTTPS-redirect responses carry CORS headers,
// preventing opaque network errors in the browser on 3xx/4xx/5xx replies.
app.UseRouting();
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseOfflineSupport();
app.UseStaticFiles();

app.UseMiddleware<RequestResponseLoggingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

// Serve SPA
app.MapFallbackToFile("index.html");

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
