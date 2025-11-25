# =============================================================================
# Author: Vladyslav Zaiets | https://sarmkadan.com
# CTO & Software Architect
# =============================================================================

.PHONY: help build run test clean restore migrate format lint

DOTNET_PROJ := AspNetSpaTemplate.csproj
CONFIG := Release
ASPNETCORE_ENV := Development

help:
	@echo "ASP.NET SPA Template - Makefile Commands"
	@echo "========================================"
	@echo ""
	@echo "Available targets:"
	@echo "  make restore          - Restore NuGet packages"
	@echo "  make build            - Build the project"
	@echo "  make run              - Run the application"
	@echo "  make test             - Run tests"
	@echo "  make clean            - Clean build artifacts"
	@echo "  make migrate          - Run database migrations"
	@echo "  make migrate-add      - Create new migration (use: make migrate-add NAME=YourMigration)"
	@echo "  make db-drop          - Drop the database"
	@echo "  make db-create        - Create the database"
	@echo "  make watch            - Run with file watching"
	@echo "  make format           - Format code with dotnet format"
	@echo "  make lint             - Run code analysis"
	@echo "  make docker-build     - Build Docker image"
	@echo "  make docker-run       - Run Docker container"
	@echo "  make docker-compose   - Run with Docker Compose"
	@echo "  make docker-clean     - Clean up Docker resources"
	@echo "  make publish          - Publish application"
	@echo "  make help             - Display this help message"

restore:
	@echo "Restoring packages..."
	dotnet restore $(DOTNET_PROJ)

build: restore
	@echo "Building project..."
	dotnet build --configuration $(CONFIG) --no-restore

run: build
	@echo "Running application..."
	ASPNETCORE_ENVIRONMENT=$(ASPNETCORE_ENV) dotnet run

watch: restore
	@echo "Running with file watcher..."
	dotnet watch run

test: build
	@echo "Running tests..."
	dotnet test --configuration $(CONFIG) --no-build

clean:
	@echo "Cleaning build artifacts..."
	dotnet clean
	rm -rf bin/ obj/ publish/

publish: clean
	@echo "Publishing application..."
	dotnet publish -c Release -o ./publish

migrate:
	@echo "Running database migrations..."
	dotnet ef database update

migrate-add:
	@echo "Creating migration: $(NAME)"
	dotnet ef migrations add $(NAME)

migrate-remove:
	@echo "Removing last migration..."
	dotnet ef migrations remove

db-create:
	@echo "Creating database..."
	dotnet ef database create

db-drop:
	@echo "Dropping database..."
	dotnet ef database drop --force

format:
	@echo "Formatting code..."
	dotnet format

lint:
	@echo "Running code analysis..."
	dotnet build --configuration $(CONFIG) /p:TreatWarningsAsErrors=true

docker-build:
	@echo "Building Docker image..."
	docker build -t aspnet-spa-template:latest .

docker-run:
	@echo "Running Docker container..."
	docker run -p 5000:5000 -p 5001:5001 \
	  -e ASPNETCORE_ENVIRONMENT=Development \
	  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal,1433;Database=AspNetSpaTemplate;User ID=sa;Password=YourSecurePassword123!;TrustServerCertificate=true;" \
	  aspnet-spa-template:latest

docker-compose:
	@echo "Starting Docker Compose..."
	docker-compose up --build

docker-clean:
	@echo "Cleaning up Docker resources..."
	docker-compose down -v
	docker rmi aspnet-spa-template:latest

requirements:
	@echo "System requirements:"
	@echo "- .NET 10 SDK"
	@dotnet --version
	@echo ""
	@echo "- SQL Server"
	@echo "  Connection string: Check appsettings.json"

info:
	@echo "Project Information:"
	@echo "- Name: ASP.NET SPA Template"
	@echo "- Version: 1.2.0"
	@echo "- .NET Version: 10.0"
	@echo "- Language: C# 13"
	@echo "- Database: SQL Server"
	@echo "- Frontend: Vanilla JavaScript"
	@echo ""
	@echo "Project structure:"
	@grep -E "^[a-zA-Z0-9_-]+/$" .gitignore || find . -maxdepth 1 -type d -not -path '.*' | sort

# Development workflow targets
dev: clean restore build run

dev-watch: clean restore watch

dev-test: restore test

dev-full: dev-test lint format migrate

# CI/CD targets
ci: clean restore build test lint

ci-docker: clean restore docker-build

deploy-docker: ci-docker docker-push

# Database targets
db-reset: db-drop db-create migrate

db-seed:
	@echo "Seeding database with sample data..."
	dotnet run -- --seed

# Documentation
docs:
	@echo "Documentation files:"
	@ls -la docs/

examples:
	@echo "Example files:"
	@ls -la examples/

.DEFAULT_GOAL := help
