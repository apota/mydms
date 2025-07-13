@echo off
REM Script to add a new EF Core migration to the DMS CRM module

set MIGRATION_NAME=%1

if "%MIGRATION_NAME%"=="" (
    echo Migration name is required.
    echo Usage: add-migration.cmd MigrationName
    exit /b 1
)

echo Creating migration: %MIGRATION_NAME%

dotnet ef migrations add %MIGRATION_NAME% --project src\DMS.CRM.Infrastructure --startup-project src\DMS.CRM.API --output-dir Data\Migrations

if %errorlevel% neq 0 (
    echo Migration creation failed.
    exit /b %errorlevel%
)

echo Migration %MIGRATION_NAME% created successfully.
