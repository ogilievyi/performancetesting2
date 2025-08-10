# Performance Testing Demo - .NET Microservices

Performance testing demonstration using .NET 6 microservices architecture with Docker containerization, SQL Server, RabbitMQ messaging, and Apache JMeter load testing.

**ALWAYS reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.**

## Working Effectively

### Prerequisites and Setup
- **REQUIRED: Install .NET 6.0 SDK** (projects target net6.0 specifically)
  - Download from: https://dotnet.microsoft.com/download/dotnet/6.0
  - **OR** update project files to target .NET 8 by changing `<TargetFramework>net6.0</TargetFramework>` to `<TargetFramework>net8.0</TargetFramework>` in all .csproj files
- Install Docker Desktop or Docker Engine
- Install Apache JMeter for performance testing (not included in repository)

### Building the Solution
- Navigate to the solution: `cd src/PerformanceDemo`
- Restore packages: `dotnet restore` - takes ~30-40 seconds, may show vulnerability warnings for System.Data.SqlClient
- Build solution: `dotnet build` - takes ~10 seconds. NEVER CANCEL.
- The build succeeds with warnings about System.Data.SqlClient vulnerability (known issue, not critical for demo)

### Docker Build Process
**WARNING: Docker builds may fail due to NuGet connectivity issues in constrained environments**
- `cd src/PerformanceDemo`
- Build microservice API image: `docker build . -f dockerfileMicroservicePublicApi -t s1` - **NEVER CANCEL: Can take 15-60 minutes due to network issues. Set timeout to 90+ minutes.**
- Build worker service image: `docker build . -f dockerfileMicroserviceWorker -t wc1` - **NEVER CANCEL: Can take 15-60 minutes. Set timeout to 90+ minutes.**
- Build monolith image: `docker build . -f dockerfileMonolith -t monolith` - **NEVER CANCEL: Can take 15-60 minutes. Set timeout to 90+ minutes.**

**Alternative if Docker builds fail:**
- Use local development mode with `dotnet run` commands
- Set up external dependencies using the local deployment scripts

### Infrastructure Dependencies
Set up required services using Docker containers:

1. **SQL Server**: 
   ```bash
   docker run --name mssql --cpus="2" -e ACCEPT_EULA=Y -e SA_PASSWORD=Password+ -p 1433:1433 -d samuelmarks/mssql-server-fts-sqlpackage-linux
   ```
   - **NEVER CANCEL: Container download takes 2-5 minutes. Set timeout to 10+ minutes.**
   - Verify running: `docker ps | grep mssql`

2. **RabbitMQ**: 
   ```bash
   docker run -d --name rabbit -p 15672:15672 -p 5672:5672 -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=pass rabbitmq:3.12.6-management
   ```
   - **NEVER CANCEL: Container download takes 3-8 minutes. Set timeout to 15+ minutes.**
   - Verify running: `docker ps | grep rabbit`
   - Management UI: http://localhost:15672 (user/pass)

3. **Database Schema Setup**:
   ```bash
   # Wait 30 seconds for SQL Server to fully start
   sleep 30
   cd Tests/local\ deploy
   docker exec -i mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "Password+" < CreateDb.sql
   ```
   - Creates Demo database with Account, Person, Raw, and Log tables
   - Verify: `docker exec -it mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "Password+" -Q "USE Demo; SELECT name FROM sys.tables"`

### Running Applications

#### Local Development Mode (Recommended)
**Prerequisites: .NET 6.0 runtime installed OR update project target frameworks to net8.0**
- **Monolith/WebApplication1**: `cd src/PerformanceDemo/WebApplication1 && dotnet run --urls=http://localhost:5000`
  - Access at: http://localhost:5000
  - Swagger UI typically available at: http://localhost:5000/swagger (if configured)
- **Microservice API**: `cd src/PerformanceDemo/MicroservicePublicApi && dotnet run --urls=http://localhost:5001`
  - Access at: http://localhost:5001
- **Worker Service**: `cd src/PerformanceDemo/MicroserviceWorker && dotnet run`
  - Runs as background service, no web interface

#### Docker Deployment Mode
**Only if Docker builds completed successfully:**
- API: `docker run -d --cpus="1" --name api -p 8989:80 --link rabbit -e ConnectionStrings__rmq="amqp://user:pass@rabbit:5672/" s1`
- Worker: `docker run -d --cpus="1" --name worker --link mssql --link rabbit -e ConnectionStrings__db="Server=mssql;Database=demo;User Id=sa;Password=Password+;..." -e ConnectionStrings__rmq="amqp://user:pass@rabbit:5672/" wc1`
- Monolith: `docker run --cpus="1" --name monolith --link mssql -p 98:80 -e timeoutSecond=1 -e _batchCount=10 -e ConnectionStrings__db="Server=mssql;Database=demo;User Id=sa;Password=Password+;..." monolith`

## Performance Testing

### JMeter Test Execution
**Note: Apache JMeter must be installed separately**
- Install JMeter from https://jmeter.apache.org/download_jmeter.cgi
- Test files are located in `Tests/` directory:
  - `S1.jmx` - Microservice API load test
  - `wc1.jmx` - Worker service test
  - `monolith.jmx` - Monolith application test
  - `e2e.jmx` - End-to-end test scenario

### Running Performance Tests
- GUI mode: `jmeter -t Tests/S1.jmx`
- Command line: `jmeter -n -t Tests/S1.jmx -l results.jtl`
- **NEVER CANCEL: Performance tests can run for extended periods (15-60 minutes) depending on configuration. Set timeout to 90+ minutes.**

## Validation

### Manual Testing Scenarios
**ALWAYS run these validation steps after making changes:**

1. **Database Connectivity Test**:
   - Verify SQL Server container is running: `docker ps | grep mssql`
   - Test connection: Connect to localhost:1433 with sa/Password+
   - Verify Demo database exists with Account, Person, Raw, and Log tables

2. **Messaging System Test**:
   - Verify RabbitMQ container is running: `docker ps | grep rabbit`
   - Access management UI: http://localhost:15672 (user/pass)
   - Verify queues are created when applications start

3. **Application Health Check**:
   - Monolith: Access http://localhost:5000 (or configured port)
   - Microservice API: Access http://localhost:8989 (Docker) or http://localhost:5000 (local)
   - Verify Swagger UI is accessible on API endpoints

4. **End-to-End Workflow Test**:
   - Submit a Person record through the web interface
   - Verify record appears in SQL Server Person table
   - Verify message processing through RabbitMQ queues
   - Check application logs for successful processing

### Build Validation
- **ALWAYS run** `dotnet build` before committing changes
- Verify no new errors are introduced (warnings about System.Data.SqlClient are expected)
- **NEVER** ignore build errors - only vulnerability warnings are acceptable

## Common Tasks

### Troubleshooting
- **Docker build hangs**: Check network connectivity to nuget.org. If problematic, use local development mode instead
- **Application won't start**: Verify all dependencies (SQL Server, RabbitMQ) are running and accessible
- **Performance tests fail**: Ensure target applications are running and responding before starting JMeter tests
- **Database connection errors**: Verify SQL Server container is running and database schema is created

### Architecture Overview
The repository contains 4 main projects:
- **WebApplication1**: Monolithic web application (.NET 6 Web App)
- **MicroservicePublicApi**: Public API service (.NET 6 Console App with AspNetCore)
- **MicroserviceWorker**: Background worker service (.NET 6 Console App)
- **RmqClient**: Shared RabbitMQ client library (.NET 6 Class Library)

### File Structure Reference
```
├── Build/                    # Build and deployment scripts
│   ├── build.bat            # Docker image build script
│   └── run.bat             # Container deployment script
├── Tests/                   # Performance tests and setup
│   ├── *.jmx               # JMeter test plans
│   └── local deploy/       # Local environment setup scripts
└── src/PerformanceDemo/     # Source code
    ├── PerformanceDemo.sln  # Visual Studio solution
    ├── MicroservicePublicApi/
    ├── MicroserviceWorker/
    ├── RmqClient/
    ├── WebApplication1/
    └── dockerfile*          # Docker build definitions
```

## Critical Warnings

- **NEVER CANCEL builds or tests that take longer than expected** - Network issues can cause 15-60 minute delays
- **ALWAYS use timeouts of 90+ minutes** for Docker builds and performance tests
- **DO NOT ignore vulnerability warnings** about System.Data.SqlClient - document but proceed (demo limitation)
- **ALWAYS verify infrastructure dependencies** (SQL Server, RabbitMQ) before starting applications
- **NEVER commit without running** `dotnet build` to verify no new compilation errors

## Known Issues

1. **System.Data.SqlClient vulnerability warnings** - This is a known issue with the demo using an older SQL client package. Update to Microsoft.Data.SqlClient for production use.

2. **Docker build network issues** - In constrained environments, NuGet package restoration may timeout. Use local development mode as alternative.

3. **No unit tests** - Repository contains only integration/performance tests. Consider adding unit test projects for individual components.

4. **Hardcoded connection strings** - Connection strings are embedded in Docker run commands. Use configuration files or environment variables for production.