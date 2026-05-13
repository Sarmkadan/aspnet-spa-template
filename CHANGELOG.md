// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Changelog

All notable changes to the aspnet-spa-template project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-06-09

### Added
- Comprehensive documentation suite (`docs/` directory: getting-started, architecture, api-reference, deployment, faq)
- Example guides for common patterns (add-new-endpoint, authentication-jwt, background-tasks, database-seeding)
- Docker support with Dockerfile and Docker Compose for multi-container setup
- GitHub Actions CI/CD workflow with build, test, and NuGet publish jobs
- CodeQL security scanning workflow
- Dependabot configuration for NuGet and GitHub Actions updates
- `.editorconfig` for consistent code style across editors
- `Makefile` with common development targets
- `SECURITY.md`, `CONTRIBUTING.md`, and `CODE_OF_CONDUCT.md`
- NuGet packaging metadata in `.csproj`

### Changed
- Promoted to stable v1.0.0 after beta validation
- Polished API response format across all endpoints
- Improved middleware pipeline ordering for correctness

### Fixed
- Race condition in cache invalidation under concurrent writes
- CORS policy not applied correctly in development environment
- Timezone handling inconsistency in database date comparisons

---

## [0.9.0] - 2025-05-26

### Added
- Unit and integration test project (`tests/aspnet-spa-template.Tests/`)
- `ProductModelTests.cs` covering model validation and business rules
- `OrderAndCacheTests.cs` covering order creation and cache invalidation paths
- `StringExtensionsTests.cs` covering utility extension methods
- `DataExportHelper` for CSV and XML data export
- `PerformanceHelper` for timing and throughput measurement utilities

### Changed
- Switched test framework to xunit with FluentAssertions and Moq
- Improved `ValidationHelper` with additional guard methods

### Fixed
- Null reference in product filtering when category is unset
- Off-by-one error in pagination result counts

---

## [0.8.0] - 2025-05-12

### Added
- Vanilla JavaScript SPA in `wwwroot/` (index.html, css/style.css, js/app.js)
- Service Worker (`wwwroot/sw.js`) for offline support and asset caching
- `OfflineSupportExtensions` for configuring PWA-related middleware
- `AssetVersioningService` for cache-busting static file URLs
- `HotReloadMiddleware` for development-time asset refresh
- Responsive CSS layout with no external framework dependencies

### Changed
- Updated `Program.cs` to serve static files and fall back to `index.html` for SPA routing
- Improved error display in the frontend with structured user notifications

---

## [0.7.0] - 2025-04-28

### Added
- `IEventBus` abstraction and `EventBusImplementation` for in-process event dispatch
- `EventHandlers` with typed handlers for domain events
- `WebhookHandler` and `WebhooksController` for outbound webhook delivery
- `ExternalApiClient` and `IHttpClientFactory` abstraction for third-party HTTP calls
- `NotificationService` for email/SMS notification dispatch
- `CsvFormatter` and `XmlFormatter` output formatters

### Changed
- Decoupled order status transitions from direct service calls to event-driven pattern
- Improved error propagation from external API failures

### Fixed
- Missing cancellation token propagation in async HTTP client calls

---

## [0.6.0] - 2025-04-14

### Added
- `IBackgroundTask` interface for pluggable background work units
- `BackgroundTaskScheduler` hosted service for periodic task execution
- `CacheMaintenanceWorker` for scheduled cache eviction
- `NotificationWorker` for queued notification dispatch
- `EncryptionHelper` and `RequestContextHelper` utilities

### Changed
- Registered background workers via `IHostedService` in DI configuration
- Improved graceful shutdown handling in background workers

---

## [0.5.0] - 2025-04-01

### Added
- `ICacheService` abstraction and `MemoryCacheService` implementation
- `CacheKeyBuilder` for consistent, collision-free cache key construction
- Cache-aside pattern applied in `ProductService` and `OrderService`
- `RateLimitingMiddleware` with configurable request-per-minute threshold
- `CorrelationIdMiddleware` for request tracing across logs

### Changed
- Wrapped hot-path repository reads with cache layer
- Increased default cache TTL to 1 hour for read-heavy list endpoints

### Fixed
- Rate limiter not resetting correctly after sliding window expiry

---

## [0.4.0] - 2025-03-17

### Added
- `AuthenticationMiddleware` with Bearer token extraction
- `ExceptionHandlingMiddleware` returning structured `ErrorResponse` JSON
- `LoggingMiddleware` and `RequestResponseLoggingMiddleware` for full request tracing
- `BusinessException`, `NotFoundException`, and `ValidationException` custom types
- `ErrorMappingHelper` for mapping exception types to HTTP status codes
- `AppConstants`, `OrderStatus`, and `ProductCategory` constants

### Changed
- Replaced ad-hoc try/catch blocks in controllers with centralized exception middleware
- Standardized all error responses to use `ErrorResponse` DTO

---

## [0.3.0] - 2025-03-03

### Added
- `IRepository<T>` generic interface and `RepositoryBase<T>` base implementation
- `UserRepository`, `ProductRepository`, and `OrderRepository` concrete implementations
- `AppDbContext` with EF Core entity registrations
- `PaginationRequest` DTO and pagination support in repository queries
- `CollectionExtensions` and `DateTimeExtensions` utility helpers

### Changed
- Moved all direct `DbContext` access out of services into repository layer
- Applied `Nullable` reference types across all data and model files

### Fixed
- Missing index on `Orders.CreatedAt` causing slow sort queries

---

## [0.2.0] - 2025-02-17

### Added
- `ProductsController`, `OrdersController`, `UsersController`, and `HealthController`
- `ApiControllerBase` with shared response helpers
- `ProductService`, `OrderService`, `UserService`, and `ReviewService`
- DTOs: `CreateProductRequest`, `UpdateProductRequest`, `CreateOrderRequest`, `CreateUserRequest`, `ApiResponse<T>`, `PaginationRequest`
- `DependencyInjectionExtensions` and `ServiceConfiguration` for DI wiring
- `JsonSerializationHelper` and `StringExtensions` utilities

### Changed
- Unified API response envelope: `{ success, data, message }` across all endpoints

---

## [0.1.0] - 2025-02-03

### Added
- Initial repository and solution structure (`aspnet-spa-template.sln`, `.slnx`)
- `AspNetSpaTemplate.csproj` targeting .NET 10
- Domain models: `Product`, `Order`, `OrderItem`, `User`, `Review`
- `appsettings.json` and `appsettings.Development.json` configuration files
- `.gitignore` and `.editorconfig` baseline
- MIT `LICENSE`
- Skeleton `Program.cs` with minimal ASP.NET Core host setup

---

## Contributing

When contributing changes, please update this CHANGELOG with:
- New features in `Added` section
- Breaking changes in `Changed` section
- Bug fixes in `Fixed` section
- Deprecated features in `Deprecated` section

Follow the format: `- {Brief description of change}`

---

## Versioning Policy

- **Major (X.0.0)**: Breaking API changes
- **Minor (0.X.0)**: New features, backwards compatible
- **Patch (0.0.X)**: Bug fixes, backwards compatible

---

For more information, visit https://github.com/Sarmkadan/aspnet-spa-template
