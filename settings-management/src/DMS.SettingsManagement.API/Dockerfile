FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8090

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["DMS.SettingsManagement.API/DMS.SettingsManagement.API.csproj", "DMS.SettingsManagement.API/"]
COPY ["DMS.SettingsManagement.Core/DMS.SettingsManagement.Core.csproj", "DMS.SettingsManagement.Core/"]
COPY ["DMS.SettingsManagement.Infrastructure/DMS.SettingsManagement.Infrastructure.csproj", "DMS.SettingsManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "DMS.SettingsManagement.API/DMS.SettingsManagement.API.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/DMS.SettingsManagement.API"
RUN dotnet build "DMS.SettingsManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DMS.SettingsManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final

# Install wget for health checks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
  CMD wget --quiet --tries=1 --spider http://localhost:8090/health || exit 1

ENTRYPOINT ["dotnet", "DMS.SettingsManagement.API.dll"]
