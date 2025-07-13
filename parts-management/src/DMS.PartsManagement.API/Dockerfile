FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY parts-management/src/DMS.PartsManagement.Core/DMS.PartsManagement.Core.csproj ./parts-management/src/DMS.PartsManagement.Core/
COPY parts-management/src/DMS.PartsManagement.Infrastructure/DMS.PartsManagement.Infrastructure.csproj ./parts-management/src/DMS.PartsManagement.Infrastructure/
COPY parts-management/src/DMS.PartsManagement.API/DMS.PartsManagement.API.csproj ./parts-management/src/DMS.PartsManagement.API/
COPY shared/core/src/DMS.Shared.Core.csproj ./shared/core/src/
COPY shared/data/src/DMS.Shared.Data.csproj ./shared/data/src/

# Restore dependencies
WORKDIR /src/parts-management/src/DMS.PartsManagement.API
RUN dotnet restore

# Copy everything else and build
WORKDIR /src
COPY parts-management/src/ ./parts-management/src/
COPY shared/ ./shared/

WORKDIR /src/parts-management/src/DMS.PartsManagement.API
RUN dotnet build "DMS.PartsManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DMS.PartsManagement.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DMS.PartsManagement.API.dll"]
