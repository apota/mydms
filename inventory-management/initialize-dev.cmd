@echo off
REM Initialization script for Inventory Management module development environment

echo Initializing Inventory Management development environment...

REM Create Docker Postgres database if needed
echo Setting up Docker database...
docker-compose up -d

REM Restore NuGet packages
echo Restoring NuGet packages...
dotnet restore src/DMS.InventoryManagement.API/DMS.InventoryManagement.API.csproj
if %errorlevel% neq 0 goto error

REM Build the solution
echo Building solution...
dotnet build src/DMS.InventoryManagement.API/DMS.InventoryManagement.API.csproj
if %errorlevel% neq 0 goto error

REM Install NPM dependencies for Web project
echo Installing NPM dependencies for Web project...
cd src/DMS.InventoryManagement.Web/ClientApp
call npm install
cd ../../../
if %errorlevel% neq 0 goto error

REM Apply database migrations if needed
echo Applying database migrations...
call update-database.cmd
if %errorlevel% neq 0 goto error

REM Add any seed data if needed
echo Seeding initial data...
dotnet run --project src/DMS.InventoryManagement.API/DMS.InventoryManagement.API.csproj -- --seed-data
if %errorlevel% neq 0 goto error

echo.
echo Inventory Management development environment initialized successfully!
echo.
echo To run the API project:
echo   dotnet run --project src\DMS.InventoryManagement.API\DMS.InventoryManagement.API.csproj
echo.
echo To run the Web project:
echo   cd src\DMS.InventoryManagement.Web\ClientApp
echo   npm start
echo.

goto end

:error
echo.
echo An error occurred during initialization. Please check the output above.
exit /b %errorlevel%

:end
