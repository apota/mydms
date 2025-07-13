FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 7001

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the shared projects first
COPY shared/core/src/DMS.Shared.Core.csproj ./shared/core/src/
COPY shared/data/src/DMS.Shared.Data.csproj ./shared/data/src/
COPY shared/core/src/ ./shared/core/src/
COPY shared/data/src/ ./shared/data/src/

# Copy the CRM projects
COPY crm/src/ ./crm/src/

# Restore and build the API project
WORKDIR "/src/crm/src/DMS.CRM.API"
RUN dotnet restore
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DMS.CRM.API.dll"]
