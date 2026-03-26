# Stage 1: Build & Publish
# Uses the .NET 8 SDK to compile the source code and generate the binaries.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copies only the project files first to optimize Docker layer caching for the restore process.
COPY ["MeuSitePessoal.Api/MeuSitePessoal.Api.csproj", "MeuSitePessoal.Api/"]
COPY ["MeuSitePessoal.Application/MeuSitePessoal.Application.csproj", "MeuSitePessoal.Application/"]
COPY ["MeuSitePessoal.Domain/MeuSitePessoal.Domain.csproj", "MeuSitePessoal.Domain/"]
COPY ["MeuSitePessoal.Infrastructure/MeuSitePessoal.Infrastructure.csproj", "MeuSitePessoal.Infrastructure/"]
COPY ["MeuSitePessoal.Infrastructure.Logging/MeuSitePessoal.Infrastructure.Logging.csproj", "MeuSitePessoal.Infrastructure.Logging/"]

# Executes the restoration of project dependencies.
RUN dotnet restore "MeuSitePessoal.Api/MeuSitePessoal.Api.csproj"

# Copies the remaining source code and performs the build process.
COPY . .
WORKDIR "/src/MeuSitePessoal.Api"
RUN dotnet build "MeuSitePessoal.Api.csproj" -c Release -o /app/build

# Publishes the application into the /app/publish directory, excluding the app host.
RUN dotnet publish "MeuSitePessoal.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
# Utilizes a clean and lightweight image containing only the essentials to run the application.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Copies the compiled files from the build stage into the final production environment.
COPY --from=build /app/publish .

# Defines the startup command for the application.
ENTRYPOINT ["dotnet", "MeuSitePessoal.Api.dll"]