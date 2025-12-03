# Technology Stack

## Framework & Runtime

- **.NET 9.0**: Latest .NET framework
- **ASP.NET Core**: Web framework with MVC and Razor Pages
- **C#**: Primary programming language

## Database & ORM

- **SQL Server**: Primary database
- **Entity Framework Core 9.0**: ORM with Code-First migrations
- **ASP.NET Core Identity**: Authentication and user management

## Key Libraries

- **AutoMapper 14.0**: Object-to-object mapping
- **Hangfire 1.8**: Background job processing and scheduling
- **BCrypt.Net-Next 4.0**: Password hashing
- **EPPlus 7.6**: Excel file generation
- **QRCoder 1.6**: QR code generation
- **HtmlAgilityPack 1.12**: HTML parsing
- **MediatR 13.0**: Mediator pattern implementation
- **Newtonsoft.Json 13.0**: JSON serialization

## Authentication

- **JWT Bearer**: Token-based authentication for API
- **Cookie Authentication**: Session-based authentication for CMS
- Dual authentication scheme with JWT for mobile/API and cookies for web admin

## API Documentation

- **Swagger/OpenAPI**: API documentation and testing interface
- Available at `/swagger` endpoint in development

## Common Commands

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Build & Run

```bash
# Restore dependencies
dotnet restore

# Build project
dotnet build

# Run application
dotnet run

# Run with watch (auto-reload)
dotnet watch run
```

### Docker

```bash
# Build Docker image
docker build -t miniappgiba .

# Run with docker-compose
docker-compose up
```

## Configuration

- **appsettings.json**: Production configuration
- **appsettings.Development.json**: Development overrides
- Connection strings for Development and Production environments
- Hangfire separate database configuration
- JWT settings (SecretKey, Issuer, Audience)
