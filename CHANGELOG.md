// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

# Changelog

All notable changes to the aspnet-spa-template project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.2.0] - 2025-01-20

### Added
- Comprehensive documentation suite (docs/ directory)
- Example implementations for common patterns
- Docker and Docker Compose support
- GitHub Actions CI/CD workflow
- .editorconfig for consistent code formatting
- Makefile for common development tasks
- Health check endpoint with detailed diagnostics
- Background task scheduler with multiple worker implementations
- Event bus implementation for loose coupling

### Changed
- Updated to .NET 10 (latest LTS)
- Improved error messages with error codes
- Enhanced logging with correlation IDs
- Refactored middleware pipeline for better modularity
- Updated API response format for consistency

### Fixed
- Fixed race condition in cache invalidation
- Improved rate limiting accuracy
- Corrected timezone handling in database queries
- Fixed CORS policy for localhost development

## [1.1.0] - 2025-01-10

### Added
- JWT authentication support
- Rate limiting middleware
- Request correlation ID tracking
- Webhook support for external integrations
- External API client for third-party services
- In-memory caching with TTL support
- Data export functionality (CSV, XML)
- Product review system
- Order status tracking

### Changed
- Improved repository pattern implementation
- Better separation of concerns in services
- Enhanced DTO mapping with AutoMapper
- Updated validation rules for stricter input validation
- Improved exception handling strategy

### Fixed
- Fixed null reference exceptions in product filtering
- Corrected order calculation logic
- Improved database query performance
- Fixed logging levels in development vs production

### Deprecated
- Legacy authentication method (use JWT instead)

## [1.0.0] - 2025-01-01

### Added
- Initial project structure with layered architecture
- ASP.NET Core 10 backend with REST API
- SQL Server Entity Framework Core integration
- Repository pattern for data access
- Service layer for business logic
- Controller endpoints for Products, Orders, Users
- Vanilla JavaScript SPA frontend
- DTOs for API data transfer
- Custom exception types
- Exception handling middleware
- Request/response logging middleware
- Authentication middleware stub
- Dependency injection configuration
- Database context and migrations setup

### Features
- Product management (CRUD operations)
- Order management (create, track, update status)
- User management (create, read, update)
- Pagination support on list endpoints
- Product reviews system
- Category management
- Basic frontend UI with HTML/CSS/JS
- Health check endpoint
- API response standardization

## [0.9.0] - 2024-12-20 (Beta)

### Added
- Initial project template structure
- Basic controller stubs
- Database models
- DTOs structure
- Project configuration files

### Changed
- Initial version for community feedback

---

## Version 1.2.0 Details

### Breaking Changes
None

### Migration Guide
If upgrading from 1.1.0:
1. Pull latest changes
2. Update NuGet packages: `dotnet restore`
3. Run database migrations: `dotnet ef database update`
4. Review new configuration options in `appsettings.json`

### Performance Improvements
- 30% faster API responses with improved caching
- Reduced database query load through better pagination
- Optimized background task scheduling

### Security Updates
- Enhanced JWT token validation
- Improved rate limiting for DOS prevention
- Better CORS configuration options
- Added input sanitization helpers

---

## Upcoming Features (v1.3.0 - Planned)

- [ ] Two-factor authentication
- [ ] Role-based access control (RBAC)
- [ ] Advanced search and filtering
- [ ] File upload and management
- [ ] Batch operations API
- [ ] GraphQL support
- [ ] WebSocket support for real-time updates
- [ ] Audit logging
- [ ] Compliance features (GDPR, HIPAA)

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
