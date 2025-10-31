# Fabric Library Orchestration

This directory contains the .NET Aspire orchestration projects for local development and testing.

## What is .NET Aspire?

.NET Aspire is a cloud-ready stack for building observable, production-ready distributed applications. It provides:
- Service orchestration and dependency management
- Built-in health checks and service discovery
- OpenTelemetry integration for observability
- Simplified local development experience

## Project Structure

- **FabricLibrary.AppHost** - Orchestration host that manages all services (backend API, PostgreSQL, pgAdmin)
- **FabricLibrary.ServiceDefaults** - Shared configuration for health checks, telemetry, and resilience

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) - Required for PostgreSQL and pgAdmin containers
- [.NET Aspire Workload](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)

### Installing .NET Aspire Workload

```powershell
dotnet workload update
dotnet workload install aspire
```

## Starting the Orchestration

### Option 1: Using Visual Studio

1. Open the solution in Visual Studio
2. Set `FabricLibrary.AppHost` as the startup project
3. Press F5 or click the Run button

### Option 2: Using Command Line

From the `orchestration/FabricLibrary.AppHost/` directory:

```powershell
dotnet run
```

### Option 3: Using VS Code

1. Open the workspace in VS Code
2. Open the command palette (Ctrl+Shift+P / Cmd+Shift+P)
3. Select "Tasks: Run Task"
4. Choose "Run Aspire AppHost"

Or run from the terminal:

```powershell
cd orchestration/FabricLibrary.AppHost
dotnet run
```

## What Gets Started

When you run the AppHost, it will orchestrate:

1. **PostgreSQL Database** (`postgres`)
   - Runs in a Docker container
   - Database name: `fabriclibrary`
   - Automatically creates the database if it doesn't exist
   - Connection string is automatically injected into the backend

2. **pgAdmin** (Database Management UI)
   - Web-based PostgreSQL administration tool
   - Access via the Aspire Dashboard (see below)
   - Pre-configured to connect to the PostgreSQL instance

3. **Fabric Library Backend API** (`fabriclibrary-backend`)
   - ASP.NET Core Web API
   - Automatically configured with database connection
   - Includes health checks and OpenTelemetry

## Aspire Dashboard

Once the orchestration starts, the Aspire Dashboard will open automatically (usually at `http://localhost:15888` or similar).

The dashboard provides:

### Resources Tab
- View all running services
- See service states and endpoints
- Access service-specific URLs (API, pgAdmin, etc.)
- View environment variables and configuration

### Console Logs Tab
- Real-time logs from all services
- Filter by service or log level
- Search and export logs

### Traces Tab
- Distributed tracing with OpenTelemetry
- View request flows across services
- Performance analysis

### Metrics Tab
- Real-time metrics and performance data
- CPU, memory, and request statistics
- Custom application metrics

## Accessing Services

### Backend API

The backend API will be available at a URL shown in the Aspire Dashboard (typically `http://localhost:5xxx` or `https://localhost:7xxx`).

#### Swagger UI
Access the API documentation at: `https://localhost:{port}/swagger`

#### Health Check
Check if the API is running: `https://localhost:{port}/health`

### pgAdmin (Database Management)

1. Open the Aspire Dashboard
2. Find the `postgres-pgadmin` resource
3. Click the endpoint URL to open pgAdmin
4. Login credentials are automatically configured (check the dashboard for details)

### PostgreSQL Database

**Connection Details** (visible in Aspire Dashboard):
- Host: `localhost`
- Port: Dynamically assigned (check dashboard)
- Database: `fabriclibrary`
- Username: `postgres`
- Password: Automatically generated (check dashboard or use connection from backend)

The backend automatically receives the correct connection string via the Aspire service reference.

## Stopping the Orchestration

### Using Visual Studio
- Click the Stop button or press Shift+F5

### Using Command Line
- Press `Ctrl+C` in the terminal where the AppHost is running

All Docker containers (PostgreSQL, pgAdmin) will be automatically stopped and cleaned up.

## Configuration

### Environment Variables

The orchestration can be configured using:

1. **User Secrets** (recommended for development secrets):
   ```powershell
   cd backend/FabricLibrary.Backend
   dotnet user-secrets set "JWT_SECRET" "your-secret-key-here"
   dotnet user-secrets set "GOOGLE_CLIENT_ID" "your-google-client-id"
   ```

2. **appsettings.Development.json** (for non-secret configuration):
   ```json
   {
     "Logging": {
       "LogLevel": {
         "Default": "Information"
       }
     }
   }
   ```

**Note**: The `.env.example` file in the repository root is for documentation and production deployment reference. For local development with Aspire, use User Secrets or appsettings files as shown above.

### Database Connection in Backend

When running through Aspire, the backend receives the connection string automatically via the parameter name `fabriclibrary`:

```csharp
// In Program.cs - Aspire automatically injects this
builder.AddNpgsqlDbContext<AppDbContext>("fabriclibrary");
```

## Troubleshooting

### Docker Desktop Not Running

**Error**: Cannot connect to Docker daemon

**Solution**: Make sure Docker Desktop is running before starting the orchestration.

### Port Conflicts

**Error**: Port already in use

**Solution**: The Aspire AppHost dynamically assigns ports, but if you have conflicts:
1. Stop other services using common ports (5432 for Postgres, etc.)
2. Check the Aspire Dashboard to see which ports are being used
3. Restart the AppHost

### Database Connection Issues

**Error**: Cannot connect to database

**Solutions**:
1. Check the Aspire Dashboard to ensure PostgreSQL is running (green status)
2. Verify the connection string in the dashboard's environment variables
3. Check Docker Desktop to ensure the PostgreSQL container is healthy
4. Look at console logs in the Aspire Dashboard for database errors

### Aspire Workload Not Installed

**Error**: The Aspire SDK is not installed

**Solution**:
```powershell
dotnet workload install aspire
```

### Container Images Not Found

**Error**: Unable to pull Docker images

**Solution**:
1. Ensure you have internet connectivity
2. Check Docker Desktop is configured correctly
3. Manually pull the images:
   ```powershell
   docker pull postgres:latest
   docker pull dpage/pgadmin4:latest
   ```

## Development Workflow

### Typical Development Session

1. Start Docker Desktop
2. Run the Aspire AppHost
3. Open the Aspire Dashboard to monitor services
4. Develop and test your application
5. Use pgAdmin to inspect/modify database data
6. View logs and traces in the dashboard
7. Stop the AppHost when done

### Database Changes

When you make database schema changes:

1. Update your Entity Framework models
2. Create a migration:
   ```powershell
   cd backend/FabricLibrary.Backend
   dotnet ef migrations add MigrationName
   ```
3. Apply the migration (either manually or via code):
   ```powershell
   dotnet ef database update
   ```
4. Restart the backend (or it may auto-reload)

### Hot Reload

The backend API supports hot reload. Changes to C# code will be automatically recompiled and reloaded without restarting the entire orchestration.

## Additional Resources

- [.NET Aspire Documentation](https://learn.microsoft.com/dotnet/aspire/)
- [.NET Aspire GitHub](https://github.com/dotnet/aspire)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [pgAdmin Documentation](https://www.pgadmin.org/docs/)

## Production Deployment

**Note**: .NET Aspire orchestration is designed for local development. For production:
- Use managed database services (Azure Database for PostgreSQL, AWS RDS, etc.)
- Deploy the backend API as a containerized application
- Configure connection strings via environment variables or secret management
- Use proper monitoring and observability tools

See deployment documentation in `/docs` for production deployment guides.
