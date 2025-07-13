FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8091

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["DMS.Entrypoint.API/DMS.Entrypoint.API.csproj", "DMS.Entrypoint.API/"]
COPY ["DMS.Entrypoint.Core/DMS.Entrypoint.Core.csproj", "DMS.Entrypoint.Core/"]
COPY ["DMS.Entrypoint.Infrastructure/DMS.Entrypoint.Infrastructure.csproj", "DMS.Entrypoint.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "DMS.Entrypoint.API/DMS.Entrypoint.API.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/DMS.Entrypoint.API"
RUN dotnet build "DMS.Entrypoint.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DMS.Entrypoint.API.csproj" -c Release -o /app/publish

FROM base AS final
# Install wget for health checks
RUN apt-get update && apt-get install -y wget && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DMS.Entrypoint.API.dll"]
