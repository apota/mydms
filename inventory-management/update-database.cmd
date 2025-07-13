@echo off
REM Script to apply EF Core migrations to the database for DMS Inventory Management module

echo Applying pending migrations to the database...

dotnet ef database update --project src\DMS.InventoryManagement.Infrastructure --startup-project src\DMS.InventoryManagement.API

if %errorlevel% neq 0 (
    echo Database update failed.
    exit /b %errorlevel%
)

echo Migrations applied successfully.
