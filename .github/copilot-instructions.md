# Performance Testing Repository
This repository contains a .NET 6.0 microservices performance testing suite with JMeter integration. The system includes microservices architecture (API + Worker) and a monolith application, designed to compare performance characteristics under load.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Environment Setup
- Install .NET 6.0 SDK and runtime (required - .NET 8.0 alone is insufficient):
  ```bash
  wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  sudo apt-get update
  sudo apt-get install -y aspnetcore-runtime-6.0 dotnet-sdk-6.0
  ```
- Verify installation: `dotnet --list-runtimes` should show both 6.0.x and 8.0.x runtimes

### Build and Test the Applications
- Build the .NET solution:
  ```bash
  cd src/PerformanceDemo
  dotnet build PerformanceDemo.sln
  ```
  - Takes approximately 51 seconds. NEVER CANCEL. Set timeout to 120+ seconds.
  - Expect warnings about System.Data.SqlClient vulnerability (NU1903) - this is normal.
  - Expect nullable reference type warnings - this is normal.

- No unit tests are present in this solution. `dotnet test` will complete immediately with no tests run.

### Run Applications Locally

#### Monolith Web Application (WebApplication1)
- Run standalone (without infrastructure dependencies):
  ```bash
  cd src/PerformanceDemo/WebApplication1
  dotnet run --urls http://localhost:5000
  ```
  - Starts in ~3-5 seconds
  - Access at http://localhost:5000
  - Serves ASP.NET Core Razor Pages application
  - Will show database connection warnings but runs without SQL Server for basic testing

#### Microservice Public API (MicroservicePublicApi)
- Requires RabbitMQ to start. Will crash without it.
- With RabbitMQ running:
  ```bash
  cd src/PerformanceDemo/MicroservicePublicApi
  dotnet run --urls http://localhost:8989
  ```
  - API endpoints: GET /api/data/{id}, GET /api/ok

#### Microservice Worker (MicroserviceWorker)
- Requires both SQL Server and RabbitMQ to start
- Consumes messages from RabbitMQ and processes them

### Infrastructure Setup with Docker

#### Start Required Infrastructure Services
Run these commands in order, waiting for each to fully start:

1. **SQL Server**:
   ```bash
   docker run -d --name mssql --cpus="1" -p 1433:1433 -e ACCEPT_EULA=Y -e SA_PASSWORD=Password+ samuelmarks/mssql-server-fts-sqlpackage-linux
   ```
   - Wait 30-60 seconds for SQL Server to fully initialize

2. **RabbitMQ**:
   ```bash
   docker run -d --name rabbit -p 15672:15672 -p 5672:5672 -e RABBITMQ_DEFAULT_USER=user -e RABBITMQ_DEFAULT_PASS=pass rabbitmq:3.12.6-management
   ```
   - Management UI available at http://localhost:15672 (user/pass)
   - Wait 30 seconds for RabbitMQ to fully start

3. **Database Setup**:
   ```bash
   # Wait 60 seconds after SQL Server starts, then create database
   docker exec -i mssql /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P Password+ -Q "CREATE DATABASE Demo"
   ```

#### Docker Build Process
Build Docker images for the applications:
```bash
cd src/PerformanceDemo
docker build . -f dockerfileMicroservicePublicApi -t s1
docker build . -f dockerfileMicroserviceWorker -t wc1  
docker build . -f dockerfileMonolith -t monolith
```
- **CRITICAL**: Docker builds take 10-15 minutes each due to NuGet package downloads. NEVER CANCEL.
- Set timeout to 30+ minutes for Docker builds.
- Builds may show NU1301 NuGet errors initially but will eventually succeed with retries.

### Performance Testing with JMeter

#### JMeter Test Plans Location
- Performance test plans are in `Tests/` directory:
  - `S1.jmx` - Tests for MicroservicePublicApi
  - `wc1.jmx` - Tests for MicroserviceWorker
  - `monolith.jmx` - Tests for monolith application
  - `e2e.jmx` - End-to-end testing

#### Running Performance Tests
- JMeter is not installed by default. Install if needed:
  ```bash
  sudo apt-get install -y openjdk-11-jdk
  wget https://archive.apache.org/dist/jmeter/binaries/apache-jmeter-5.6.tgz
  tar -xzf apache-jmeter-5.6.tgz
  ```

### Validation Scenarios

#### Always Test These Scenarios After Making Changes:
1. **Build Validation**:
   ```bash
   cd src/PerformanceDemo && dotnet build PerformanceDemo.sln
   ```
   - Must complete successfully in ~51 seconds

2. **Monolith Functionality**:
   ```bash
   cd src/PerformanceDemo/WebApplication1 && dotnet run --urls http://localhost:5000 &
   sleep 5
   curl -s http://localhost:5000 | grep "Welcome"
   ```
   - Should return HTML containing "Welcome"

3. **API Endpoint Test** (requires RabbitMQ):
   ```bash
   # Start RabbitMQ first, then:
   cd src/PerformanceDemo/MicroservicePublicApi && dotnet run --urls http://localhost:8989 &
   sleep 5
   curl http://localhost:8989/api/ok
   ```
   - Should return 200 OK with ":)" response

### Common Tasks and Commands

#### Repository Structure
```
├── Build/                 # Windows batch build scripts
├── Tests/                 # JMeter performance test plans
│   ├── S1.jmx            # API performance tests
│   ├── monolith.jmx      # Monolith performance tests
│   └── local deploy/     # Infrastructure setup scripts
└── src/PerformanceDemo/   # .NET 6.0 solution
    ├── MicroservicePublicApi/    # REST API service
    ├── MicroserviceWorker/       # Background worker service
    ├── WebApplication1/          # Monolith web application
    └── RmqClient/               # RabbitMQ client library
```

#### Key Configuration Files
- `src/PerformanceDemo/PerformanceDemo.sln` - Main solution file
- `src/PerformanceDemo/WebApplication1/appsettings.json` - Monolith configuration
- `Tests/local deploy/CreateDb.sql` - Database schema setup

### Troubleshooting

#### Common Issues and Solutions:
- **"No .NET SDKs were found"**: Install .NET 6.0 SDK using the prerequisite commands above
- **RabbitMQ connection errors**: Ensure RabbitMQ container is running with correct ports exposed
- **SQL Server connection errors**: Wait 60+ seconds after starting SQL Server container
- **Docker build timeouts**: NEVER CANCEL - builds can take 15+ minutes due to network conditions
- **NU1903 package warnings**: These are expected due to older package versions - not blocking errors

#### Performance Testing Notes:
- The system is designed to compare microservices vs monolith performance under load
- Infrastructure (SQL Server, RabbitMQ) must be running for realistic performance testing
- JMeter tests simulate realistic user loads with database and message queue operations

### Time Expectations:
- **.NET Build**: ~51 seconds (set 120+ second timeout)
- **Docker Builds**: 10-15 minutes each (set 30+ minute timeout) 
- **Application Startup**: 3-5 seconds
- **Infrastructure Startup**: 30-60 seconds per service
- **Full Environment Setup**: 5-10 minutes including infrastructure

**NEVER CANCEL long-running builds or infrastructure setup commands.**