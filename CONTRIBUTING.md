# Contributing to ASP.NET SPA Template

Thank you for your interest in contributing! This document describes how to build, test, and submit changes.

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Node.js 18+](https://nodejs.org/) (for the dev proxy server)
- Git

## Building Locally

```bash
# Clone the repository
git clone https://github.com/your-username/aspnet-spa-template.git
cd aspnet-spa-template

# Restore dependencies
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode
dotnet build --configuration Release
```

## Running the Application

```bash
# Run the ASP.NET backend (serves the SPA from wwwroot/)
dotnet run

# Run with the development proxy (hot-reload JS changes)
node dev-server.js
```

The application will be available at `https://localhost:5001` by default.

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with TRX report output
dotnet test --verbosity normal --logger "trx" --results-directory ./test-results

# Run a specific test project
dotnet test tests/aspnet-spa-template.Tests/aspnet-spa-template.Tests.csproj
```

## Making Changes

### 1. Fork and Create a Branch

```bash
git clone https://github.com/your-username/aspnet-spa-template.git
cd aspnet-spa-template
git checkout -b feature/your-feature-name
```

Branch naming conventions:
- `feature/description` — new features
- `fix/description` — bug fixes
- `docs/description` — documentation updates
- `refactor/description` — code refactoring

### 2. Implement Your Changes

- Write or update tests for any changed behaviour.
- Keep commits small and focused with clear commit messages.
- Follow the [Conventional Commits](https://www.conventionalcommits.org/) format where possible (`feat:`, `fix:`, `docs:`, `refactor:`, `test:`).

### 3. Verify Before Submitting

```bash
# Ensure the project builds cleanly
dotnet build --configuration Release

# Ensure all tests pass
dotnet test --no-build --configuration Release

# Check code formatting
dotnet format --verify-no-changes
```

### 4. Submit a Pull Request

1. Push your branch to your fork.
2. Open a Pull Request against the `main` branch.
3. Fill out the PR description: what changed, why, and how to verify it.
4. Ensure all CI checks pass before requesting a review.

## Code Style

This project uses `.editorconfig` for consistent formatting. Key conventions:

- **Indentation**: 4 spaces for C# files, 2 spaces for JSON/YAML/HTML/CSS/JS.
- **Braces**: Allman style (opening brace on its own line).
- **Namespaces**: File-scoped (`namespace Foo;`).
- **`var`**: Use `var` for built-in types and when the type is apparent from the right-hand side.
- **Interfaces**: Prefix with `I` (e.g., `ICacheService`).
- **XML docs**: Provide `<summary>` comments for all public types and members.
- **Author headers**: Keep existing author headers. If you significantly modify a file, you may add your name but do not remove the original header.

Run `dotnet format` to automatically fix formatting issues before committing.

## Reporting Issues

Use GitHub Issues for bugs and feature requests. When filing a bug, include:

- Steps to reproduce
- Expected behaviour
- Actual behaviour
- .NET SDK version (`dotnet --version`)
- OS and version

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
