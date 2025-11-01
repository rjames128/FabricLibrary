# Fabric Library Backend

ASP.NET Core Web API (.NET 8) for the Fabric Library application.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (for running PostgreSQL)
- [.NET Aspire Workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling) (for orchestration)

## Project Structure

- `FabricLibrary.Backend/` - Main Web API project
- `/orchestration/FabricLibrary.AppHost/` - .NET Aspire orchestration for local development
- `/orchestration/FabricLibrary.ServiceDefaults/` - Shared service configuration (health checks, telemetry)

## Getting Started

### 1. Install Dependencies

From the `backend/FabricLibrary.Backend/` directory:

```powershell
dotnet restore
```

### 2. Run with .NET Aspire (Recommended)

The easiest way to run the backend with all required services is using the .NET Aspire orchestration:

```powershell
# From the repository root
cd orchestration/FabricLibrary.AppHost
dotnet run
```

This will:
- Start the backend API
- Configure health checks and OpenTelemetry
- Provide the Aspire dashboard for monitoring (typically at http://localhost:15888)

### 3. Run Standalone (Without Orchestration)

If you prefer to run just the backend API:

```powershell
# From the backend directory
cd backend/FabricLibrary.Backend
dotnet run
```

The API will be available at:
- HTTPS: `https://localhost:7xxx` (port may vary)
- HTTP: `http://localhost:5xxx`

Check the console output for the exact ports.

## Development

### Building

```powershell
dotnet build
```

For release build:

```powershell
dotnet build --configuration Release
```

### Running Tests

```powershell
dotnet test
```

### Swagger/OpenAPI

When running in development mode, Swagger UI is available at:
- `https://localhost:{port}/swagger`

This provides interactive API documentation and testing capabilities.

## Database Migrations

The project uses Entity Framework Core for database management.

### Creating a New Migration

After making changes to your data models:

```powershell
cd backend/FabricLibrary.Backend
dotnet ef migrations add MigrationName
```

### Applying Migrations

To apply pending migrations to the database:

```powershell
dotnet ef database update
```

**Note**: When using the Aspire orchestration, the database connection is automatically configured. Just ensure the AppHost is running or has run at least once to create the database.

### Removing the Last Migration

If you need to remove the last migration (before applying it):

```powershell
dotnet ef migrations remove
```

### Viewing Migration SQL

To see the SQL that will be executed:

```powershell
dotnet ef migrations script
```

### Database Management Tools

When running with Aspire orchestration, pgAdmin is available through the Aspire Dashboard for visual database management and querying.

## Configuration

### Environment Variables

The backend uses the following configuration (to be added via `.env.example` or user secrets):

- `ConnectionStrings__Default` - PostgreSQL connection string
- `JWT_SECRET` - Secret key for signing JWT tokens
- `GOOGLE_CLIENT_ID` - Google OAuth client ID for token validation
- `SENDGRID_API_KEY` - (Optional) SendGrid API key for email functionality

### User Secrets (Development)

For local development, use .NET User Secrets to store sensitive configuration:

```powershell
# From the backend/FabricLibrary.Backend directory
dotnet user-secrets init
dotnet user-secrets set "JWT_SECRET" "your-secret-key-here"
dotnet user-secrets set "GOOGLE_CLIENT_ID" "your-google-client-id"
```

### Google OAuth Setup

To set up Google OAuth credentials:

1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Create a new project or select an existing one
3. Navigate to "APIs & Services" > "Credentials"
4. Click "Create Credentials" > "OAuth 2.0 Client ID"
5. Configure the consent screen if prompted
6. Choose "Web application" as the application type
7. Add authorized JavaScript origins (e.g., `http://localhost:3000` for local dev)
8. Add authorized redirect URIs
9. Copy the Client ID and set it in your configuration

## Database Migrations (Coming Soon)

Once Entity Framework Core is configured:

```powershell
# Create a new migration
dotnet ef migrations add MigrationName

# Apply migrations to database
dotnet ef database update

# Remove last migration (if not applied)
dotnet ef migrations remove
```

## API Endpoints

### Current Endpoints

- `GET /weatherforecast` - Sample weather forecast endpoint (scaffolded)
- `GET /health` - Health check endpoint (development only)
- `GET /alive` - Liveness check endpoint (development only)

### Planned Endpoints (Milestone 1)

- `POST /api/auth/google` - Google authentication endpoint
- `GET /api/me` - Get current authenticated user

## Docker

A Dockerfile is included for containerization:

```powershell
# Build image
docker build -t fabriclibrary-backend .

# Run container
docker run -p 8080:80 fabriclibrary-backend
```

## Troubleshooting

### Port Already in Use

If you get a port conflict error, you can specify custom ports:

```powershell
dotnet run --urls "https://localhost:7001;http://localhost:5001"
```

### HTTPS Certificate Issues

If you encounter HTTPS certificate issues in development:

```powershell
dotnet dev-certs https --trust
```

## Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [Entity Framework Core Documentation](https://docs.microsoft.com/ef/core/)

## Contributing

Please follow the coding standards defined in the `.editorconfig` file:
- Use semicolons at the end of statements
- Use LF line endings
- Follow C# naming conventions

For more details, see the root-level README and project documentation in `/docs`.
