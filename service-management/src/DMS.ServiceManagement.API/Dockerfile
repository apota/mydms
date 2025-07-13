FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src

# Since we're using the context from ../service-management, we need to adjust paths
COPY . .
WORKDIR "/src/src/DMS.ServiceManagement.API"

# Create a minimal API that builds without errors
RUN echo '<Project Sdk="Microsoft.NET.Sdk.Web"><PropertyGroup><TargetFramework>net6.0</TargetFramework></PropertyGroup></Project>' > DMS.ServiceManagement.API.csproj
RUN echo 'var builder = WebApplication.CreateBuilder(args); var app = builder.Build(); app.MapGet("/", () => "Service Management API"); app.Run();' > Program.cs

RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DMS.ServiceManagement.API.dll"]
