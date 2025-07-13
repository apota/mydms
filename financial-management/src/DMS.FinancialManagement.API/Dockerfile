FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["financial-management/src/DMS.FinancialManagement.API/DMS.FinancialManagement.API.csproj", "financial-management/src/DMS.FinancialManagement.API/"]
COPY ["financial-management/src/DMS.FinancialManagement.Core/DMS.FinancialManagement.Core.csproj", "financial-management/src/DMS.FinancialManagement.Core/"]
COPY ["financial-management/src/DMS.FinancialManagement.Infrastructure/DMS.FinancialManagement.Infrastructure.csproj", "financial-management/src/DMS.FinancialManagement.Infrastructure/"]
COPY ["shared/core/src/DMS.Shared.Core.csproj", "shared/core/src/"]
COPY ["shared/data/src/DMS.Shared.Data.csproj", "shared/data/src/"]
RUN dotnet restore "financial-management/src/DMS.FinancialManagement.API/DMS.FinancialManagement.API.csproj"
COPY . .
WORKDIR "/src/financial-management/src/DMS.FinancialManagement.API"
RUN dotnet build "DMS.FinancialManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DMS.FinancialManagement.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DMS.FinancialManagement.API.dll"]
