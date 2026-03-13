# Specification: Implement Custom Logging Infrastructure with Serilog

## Goal
Establish a robust logging infrastructure using Serilog, configured to write to both the console and daily rolling files. This configuration should be encapsulated in a reusable class library and integrated into the main API.

## Scope
- Create a new .NET 8 Class Library: `MeuSitePessoal.Infrastructure.Logging`.
- Install NuGet packages: `Serilog.AspNetCore`, `Serilog.Sinks.File`.
- Implement `IServiceCollection` extension method: `AddCustomLogging`.
- Configure Serilog:
    - Console sink.
    - File sink: `logs/log-.txt`, rolling daily.
- Update `MeuSitePessoal.Api` to reference the new library.
- Update `Program.cs` to use `AddCustomLogging` and `UseSerilogRequestLogging`.
- Ensure all comments are in English.
- Verify logging works (e.g., check if logs are created).

## Constraints
- Use .NET 8.
- Maintain existing project structure.
- Follow existing coding style but enforce English comments for this new module.
