FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5003

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["DMS.UserManagement.API/DMS.UserManagement.API.csproj", "DMS.UserManagement.API/"]
COPY ["DMS.UserManagement.Core/DMS.UserManagement.Core.csproj", "DMS.UserManagement.Core/"]
COPY ["DMS.UserManagement.Infrastructure/DMS.UserManagement.Infrastructure.csproj", "DMS.UserManagement.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "DMS.UserManagement.API/DMS.UserManagement.API.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/DMS.UserManagement.API"
RUN dotnet build "DMS.UserManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DMS.UserManagement.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:5003/health || exit 1

ENTRYPOINT ["dotnet", "DMS.UserManagement.API.dll"]
