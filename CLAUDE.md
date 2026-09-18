# CLAUDE.md

ASP.NET Core 10 Web API template (C# 13, EF Core + SQL Server) with a vanilla JavaScript PWA frontend served from `wwwroot/`.

## Build

```bash
dotnet restore
dotnet build --configuration Release --no-restore   # or: make build
dotnet run                                           # or: make run / make watch
dotnet publish -c Release -o ./publish               # or: make publish
docker-compose up --build                            # app + SQL Server
```

Main project: `AspNetSpaTemplate.csproj` (root). Solution: `aspnet-spa-template.sln`. The root csproj excludes `tests/**` and `benchmarks/**` from compilation.

Frontend dev proxy (optional): `npm install && npm run dev` (`dev-server.js`, proxies to the API per `proxy.config.json`).

## Test

```bash
dotnet test --configuration Release          # or: make test
dotnet test --filter "FullyQualifiedName~OrderService"
```

Test project: `tests/aspnet-spa-template.Tests/` (xUnit + FluentAssertions + Moq, EF Core InMemory). Benchmarks: `benchmarks/aspnet-spa-template.Benchmarks/` (BenchmarkDotNet). CI runs restore/build/test on .NET 10 (`.github/workflows/ci.yml`).

## Lint / Format

```bash
dotnet format                                              # make format
dotnet build -c Release /p:TreatWarningsAsErrors=true      # make lint
```

Style rules live in `.editorconfig` (4-space indent, file-scoped namespaces, PascalCase types/members, `I`-prefixed interfaces). `GenerateDocumentationFile` is on, so public members need XML doc comments.

## Database

```bash
dotnet ef database update                  # make migrate
dotnet ef migrations add <Name>            # make migrate-add NAME=<Name>
dotnet run -- --seed                       # make db-seed
```

Connection string: `ConnectionStrings:DefaultConnection` in `appsettings.json` (copy from `appsettings.example.json`; `appsettings.Development.json` is gitignored).

## Layout

- `Program.cs` - composition root: DI registrations, options binding (`ValidateOnStart`), middleware pipeline, Swagger, CORS.
- `Controllers/` - API controllers; all derive from `ApiControllerBase`.
- `Services/` - business logic (`ProductService`, `OrderService`, `ReviewService`, `UserService`); `CachedProductService` decorates `IProductService`.
- `Data/` - `AppDbContext` and `Data/Repositories/` (`IRepository<T>`, `RepositoryBase<T>`, per-entity repositories).
- `Models/` - EF entities plus `*Extensions.cs` helpers. `DTOs/` - request/response contracts, `ApiResponse`, `PagedResult`.
- `Middleware/` - exception handling, correlation id, auth, rate limiting, request logging, hot reload.
- `Caching/` - `ICacheService`, `MemoryCacheService`, `CacheKeyBuilder`.
- `Events/` - in-process event bus and domain event handlers. `BackgroundWorkers/` - hosted background tasks.
- `Integration/` - external API client, notifications, webhook handler.
- `Configuration/` - options classes and `DependencyInjectionExtensions`. `Constants/`, `Utilities/`, `Formatters/`, `Exceptions/`.
- `wwwroot/` - `index.html`, `sw.js`, `manifest.json`, `offline.html` (PWA, no framework).
- `docs/` - one markdown file per class. `examples/` - usage samples.

## Conventions

- Namespace root `AspNetSpaTemplate.<Folder>`; file-scoped namespaces; `Nullable` and `ImplicitUsings` enabled.
- Async methods end in `Async`. Interfaces `I*`; implementations registered in `Program.cs` (scoped for services/repositories, singleton for cache, metrics, manifest, sync queue).
- Domain errors are typed exceptions in `Exceptions/` (`NotFoundException`, `ValidationException`, `BusinessException`, `ExternalApiException`, `ConfigurationException`) mapped to HTTP responses by `ExceptionHandlingMiddleware`.
- Options classes expose `SectionName` and are bound with data-annotation validation.
- Test naming: `Method_Scenario_ExpectedResult`; unit tests are `*UnitTests.cs`, integration tests `*IntegrationTests.cs`.
- Do not commit `bin/`, `obj/`, `.aider*`, `build_output.txt` or `*.backup` files.
