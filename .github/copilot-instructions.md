# Performance Testing Demo - .NET Microservices

This repository contains a performance testing demonstration comparing monolithic vs microservices architectures using .NET 8.0, Docker, SQL Server, RabbitMQ, and JMeter.

**ALWAYS reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Bootstrap and Build
**CRITICAL - NEVER CANCEL**: All build commands require long timeouts. The initial restore can take up to 60 seconds, builds take 7-15 seconds.

1. **Install .NET 8.0 SDK** (if not available):
   ```bash
   # Verify .NET version
   dotnet --version  # Should show 8.0.x
   ```

2. **Build the solution**:
   ```bash
   cd src/PerformanceDemo
   dotnet clean                    # ~3 seconds
   dotnet restore                  # ~40-60 seconds - NEVER CANCEL
   dotnet build                    # ~7-15 seconds - NEVER CANCEL  
   ```
   - **NEVER CANCEL** build operations - they take time but complete successfully
   - Warnings about System.Data.SqlClient vulnerability are expected and safe to ignore
   - Build warnings about nullable reference types are expected

### Infrastructure Setup (Required for Full Testing)
**CRITICAL - NEVER CANCEL**: Container pulls can take 2-15 minutes. All services must be running for full functionality.

1. **Start infrastructure containers**:
   ```bash
   # SQL Server (takes 2-3 minutes to download + start)
   docker run --name mssql --cpus="2" -e ACCEPT_EULA=Y -e SA_PASSWORD=Password+ -p 1433:1433 -d samuelmarks/mssql-server-fts-sqlpackage-linux

   # RabbitMQ (takes 3-8 minutes to download + start)  
   docker run -d --name rabbit -p 15672:15672 -p 5672:5672 -p 4369:4369 -p 5671:5671 -p 25672:25672 -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=pass rabbitmq:3.12.6-management

   # WireMock (takes 2-5 minutes to download + start)
   docker run -d --name wiremock -p 8080:8080 wiremock/wiremock:latest

   # Grafana (takes 5-15 minutes to download + start)
   docker run -d --name grafana -p 3000:3000 grafana/grafana:latest
   ```

2. **Wait for services to initialize** (60-90 seconds):
   ```bash
   # Verify all containers are running
   docker ps
   
   # Test SQL Server readiness (wait until successful)
   docker exec -i mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Password+' -Q "SELECT @@VERSION"
   ```

3. **Initialize database schema**:
   ```bash
   # Create Demo database and tables
   docker exec -i mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Password+' -Q "
   CREATE DATABASE [Demo];
   USE [Demo];
   CREATE TABLE [Account]([Id] nvarchar(50), [Uri] nvarchar(2000), [AccountName] nvarchar(50));
   CREATE TABLE [Raw]([Id] int IDENTITY(1,1), [json] nvarchar(max), [status] int DEFAULT 0);
   CREATE TABLE [Person]([Id] int IDENTITY(1,1), [CreatedOn] datetime DEFAULT getdate(), [FirstName] nvarchar(50), [LastName] nvarchar(50), [Gender] nvarchar(50), [BirthDate] date);
   CREATE TABLE [Log]([Id] int IDENTITY(1,1), [Date] datetime, [Thread] varchar(255), [Level] varchar(50), [Logger] varchar(255), [Message] varchar(4000), [Exception] varchar(2000));
   INSERT INTO [Account] VALUES ('m1', 'http://monolith:80', 'm1'), ('wiremock', 'http://wiremock:8080', 'wiremock');
   "
   ```

### Running Applications

1. **Monolith Web Application**:
   ```bash
   cd src/PerformanceDemo/WebApplication1
   dotnet run --urls "http://localhost:5000"
   # Access at: http://localhost:5000
   # Takes ~10-15 seconds to start
   ```

2. **Microservice Public API**:
   ```bash
   cd src/PerformanceDemo/MicroservicePublicApi  
   dotnet run --urls "http://localhost:8989"
   # Access at: http://localhost:8989
   ```

3. **Microservice Worker**:
   ```bash
   cd src/PerformanceDemo/MicroserviceWorker
   dotnet run
   # Background service - no web interface
   ```

### Performance Testing with JMeter

1. **Install JMeter** (if not available):
   ```bash
   sudo apt-get update && sudo apt-get install -y jmeter
   jmeter --version  # Should show version 2.13.x
   ```

2. **Run performance tests**:
   ```bash
   cd Tests
   # Test monolith (ensure WebApplication1 is running on localhost:5000)
   jmeter -n -t monolith.jmx -l results.jtl -j jmeter.log
   
   # Test microservices (ensure both microservices are running)
   jmeter -n -t S1.jmx -l microservices_results.jtl -j microservices.log
   ```

## Validation

### Always Test After Changes
1. **Build validation**:
   ```bash
   cd src/PerformanceDemo
   dotnet clean && dotnet restore && dotnet build
   ```

2. **Runtime validation**:
   ```bash
   # Start WebApplication1
   cd src/PerformanceDemo/WebApplication1
   dotnet run --urls "http://localhost:5000" &
   
   # Test HTTP response
   curl -f http://localhost:5000 | head -5
   
   # Should show HTML with "WebApplication1" in title
   ```

3. **Infrastructure validation**:
   ```bash
   # Check all containers are running
   docker ps
   
   # Test database connectivity
   docker exec mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P 'Password+' -Q "SELECT COUNT(*) FROM Demo.dbo.Account"
   
   # Test RabbitMQ management UI
   curl -f http://localhost:15672
   ```

### Expected Timing (NEVER CANCEL these operations)
- **dotnet restore**: 40-60 seconds (downloads packages)
- **dotnet build**: 7-15 seconds (compilation)
- **Docker pulls**: 2-15 minutes per image (depends on image size)
- **SQL Server startup**: 60-90 seconds (database initialization)
- **Application startup**: 10-15 seconds (.NET runtime + dependencies)

## Known Issues and Limitations

### Docker Build Issues
- **Docker build via Dockerfile FAILS** due to NuGet connectivity restrictions in container environment
- **Workaround**: Use direct `dotnet build` and `dotnet run` instead of Docker builds
- **Docker infrastructure** (SQL Server, RabbitMQ, etc.) works perfectly

### Package Vulnerabilities  
- **System.Data.SqlClient 4.8.5** vulnerability warnings are expected
- **Safe to ignore** for demo/testing purposes
- **Do not update** package version as it may break compatibility

### JMeter Compatibility
- **Older .jmx test files** may have compatibility issues with newer JMeter versions
- **Update test plans** if you encounter XStream security exceptions
- **Tests are designed** for specific URL patterns and may need adjustment

## Key Projects

1. **WebApplication1**: Monolithic web application demonstrating traditional architecture
2. **MicroservicePublicApi**: Public-facing API for microservices architecture  
3. **MicroserviceWorker**: Background worker service processing tasks via RabbitMQ
4. **RmqClient**: Shared library for RabbitMQ communication

## Common Tasks

### Repository Root Structure
```
.
├── Build/                  # Build scripts (Windows batch files)
├── Tests/                  # JMeter test plans and deployment scripts
├── src/PerformanceDemo/    # Main solution directory
│   ├── PerformanceDemo.sln # Visual Studio solution file
│   ├── WebApplication1/    # Monolith web app
│   ├── MicroservicePublicApi/ # Microservice API
│   ├── MicroserviceWorker/ # Background worker
│   ├── RmqClient/         # RabbitMQ client library
│   └── dockerfile*        # Docker build files
└── .github/               # GitHub configurations
```

### Infrastructure URLs
- **WebApplication1**: http://localhost:5000
- **MicroservicePublicApi**: http://localhost:8989  
- **RabbitMQ Management**: http://localhost:15672 (user/pass)
- **Grafana**: http://localhost:3000
- **WireMock**: http://localhost:8080
- **SQL Server**: localhost:1433 (sa/Password+)

### Cleanup Commands
```bash
# Stop and remove all containers
docker stop mssql rabbit wiremock grafana || true
docker rm mssql rabbit wiremock grafana || true

# Clean build artifacts  
cd src/PerformanceDemo
dotnet clean
find . -name "bin" -type d -exec rm -rf {} +
find . -name "obj" -type d -exec rm -rf {} +
```

## Development Notes

- **Target Framework**: .NET 8.0 (updated from .NET 6.0 for compatibility)
- **Database**: SQL Server with Demo database
- **Messaging**: RabbitMQ for microservice communication
- **Monitoring**: Grafana dashboards available
- **Testing**: JMeter for load testing both architectures
- **Comparison**: Direct performance comparison between monolith and microservices approaches