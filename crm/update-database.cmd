@echo off
REM Script to update the database with the latest EF Core migrations for DMS CRM module

echo Updating CRM database with latest migrations...

dotnet ef database update --project src\DMS.CRM.Infrastructure --startup-project src\DMS.CRM.API

if %errorlevel% neq 0 (
    echo Database update failed.
    exit /b %errorlevel%
)

echo Database updated successfully.
