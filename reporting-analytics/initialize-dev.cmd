@echo off
echo Starting Reporting Analytics development environment...

REM Determine the directory where the script is located
set SCRIPT_DIR=%~dp0
cd %SCRIPT_DIR%

echo Setting up Python environment...
cd scripts
pip install -r requirements.txt
if %errorlevel% neq 0 (
    echo Error installing Python dependencies!
    exit /b %errorlevel%
)
cd ..

echo Building .NET API project...
dotnet build src\DMS.ReportingAnalytics.API\DMS.ReportingAnalytics.API.csproj
if %errorlevel% neq 0 (
    echo Error building API project!
    exit /b %errorlevel%
)

echo Running API and Web project in parallel...
start cmd /k "cd src\DMS.ReportingAnalytics.API && dotnet run --urls=https://localhost:7127"
timeout /T 5 /NOBREAK > nul
start cmd /k "cd src\DMS.ReportingAnalytics.Web && npm start"

echo Development environment started!
echo API: https://localhost:7127/swagger
echo Web: http://localhost:3000
