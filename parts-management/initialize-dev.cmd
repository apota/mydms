@echo off
echo === DMS Parts Management - Initialize Development Database ===

echo Building the solution...
dotnet build src\DMS.PartsManagement.API\DMS.PartsManagement.API.csproj

echo Ensuring database is created and migrations are applied...
dotnet ef database update --project src\DMS.PartsManagement.Infrastructure\DMS.PartsManagement.Infrastructure.csproj --startup-project src\DMS.PartsManagement.API\DMS.PartsManagement.API.csproj

echo Initialization complete!
echo.
echo You can now start the API using:
echo   dotnet run --project src\DMS.PartsManagement.API\DMS.PartsManagement.API.csproj
echo.
echo Or using Docker:
echo   docker-compose up -d
echo.
