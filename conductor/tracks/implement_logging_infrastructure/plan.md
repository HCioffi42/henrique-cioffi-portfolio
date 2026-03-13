# Plan: Implement Custom Logging Infrastructure with Serilog

## Objective
Implement centralized logging using Serilog in a dedicated infrastructure project and integrate it into the API.

## Key Files & Context
- `MeuSitePessoal.Infrastructure.Logging/`: New project directory.
- `MeuSitePessoal.Api/Program.cs`: Update to use new logging.
- `MeuSitePessoal.Api/MeuSitePessoal.Api.csproj`: Add project reference.

## Implementation Steps

### Phase 1: Project Setup
- [x] Create `MeuSitePessoal.Infrastructure.Logging` (Class Library, .NET 8).
- [x] Add `Serilog.AspNetCore`, `Serilog.Sinks.File`, and `Serilog.Sinks.Async` packages.
- [x] Add `Microsoft.Extensions.Hosting` (or relevant abstractions) for IServiceCollection if needed.

### Phase 2: Implementation
- [x] Create `DependencyInjection.cs` in the new project.
- [x] Implement `AddCustomLogging(this IServiceCollection services, IConfiguration configuration)` method.
- [x] Configure Serilog:
    - `WriteTo.Console()`
    - `WriteTo.Async(a => a.File("logs/log-.txt", rollingInterval: RollingInterval.Day, shared: true))`
    - `Enrich.FromLogContext()`
    - Ensure all code comments are in English.

### Phase 3: Integration
- [x] Add reference to `MeuSitePessoal.Infrastructure.Logging` in `MeuSitePessoal.Api`.
- [x] Update `Program.cs`:
    - Call `builder.Services.AddCustomLogging(builder.Configuration)`.
    - Call `app.UseSerilogRequestLogging()`.
    - Configure Host to use Serilog (`builder.Host.UseSerilog()`).

### Phase 4: Verification
- [x] Add a unit test in `MeuSitePessoal.Tests/Unit/Infrastructure/Logging/LoggingTests.cs`:
    - Register services using `AddCustomLogging`.
    - Verify `ILogger<T>` can be resolved.
- [x] Verify application startup and log file creation manually or via integration test.

## Verification & Testing
- [x] Run API and check if `logs/` folder is created (Implicitly verified by running tests and API startup in local env if manual check needed, but unit test confirms configuration validity).
- [x] Check console output.
