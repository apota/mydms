FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5004

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["DMS.LoginManagement.API/DMS.LoginManagement.API.csproj", "DMS.LoginManagement.API/"]
COPY ["DMS.LoginManagement.Core/DMS.LoginManagement.Core.csproj", "DMS.LoginManagement.Core/"]
COPY ["DMS.LoginManagement.Infrastructure/DMS.LoginManagement.Infrastructure.csproj", "DMS.LoginManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "DMS.LoginManagement.API/DMS.LoginManagement.API.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/DMS.LoginManagement.API"
RUN dotnet build "DMS.LoginManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DMS.LoginManagement.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5004/health || exit 1

ENTRYPOINT ["dotnet", "DMS.LoginManagement.API.dll"]
