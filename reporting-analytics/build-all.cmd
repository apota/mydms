@echo off
echo Building DMS Reporting Analytics solution...

echo.
echo Building API project...
dotnet build %~dp0\src\DMS.ReportingAnalytics.API\DMS.ReportingAnalytics.API.csproj

if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to build API project
    exit /b %ERRORLEVEL%
)

echo.
echo Building Web project...
cd %~dp0\src\DMS.ReportingAnalytics.Web
call npm run build

if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to build Web project
    exit /b %ERRORLEVEL%
)

echo.
echo Build completed successfully!
