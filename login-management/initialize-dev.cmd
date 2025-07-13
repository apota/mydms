@echo off
echo Starting DMS Login Management Development Environment...
echo.

REM Check if Docker is running
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker is not installed or not running
    echo Please install Docker Desktop and make sure it's running
    pause
    exit /b 1
)

REM Check if .NET 8 SDK is installed
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET 8 SDK is not installed
    echo Please install .NET 8 SDK from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

REM Check if Node.js is installed
node --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Node.js is not installed
    echo Please install Node.js from https://nodejs.org/
    pause
    exit /b 1
)

echo All prerequisites are installed!
echo.

REM Navigate to the login-management directory
cd /d "%~dp0"

echo Step 1: Installing backend dependencies...
cd src\DMS.LoginManagement.API
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore .NET packages
    pause
    exit /b 1
)

echo Step 2: Installing frontend dependencies...
cd ..\DMS.LoginManagement.Web
call npm install
if %errorlevel% neq 0 (
    echo ERROR: Failed to install npm packages
    pause
    exit /b 1
)

echo Step 3: Building Docker containers...
cd ..\..
docker-compose build
if %errorlevel% neq 0 (
    echo ERROR: Failed to build Docker containers
    pause
    exit /b 1
)

echo Step 4: Starting services...
docker-compose up -d
if %errorlevel% neq 0 (
    echo ERROR: Failed to start services
    pause
    exit /b 1
)

echo.
echo Waiting for services to be ready...
timeout /t 30 /nobreak

echo Step 5: Running database migrations...
docker-compose exec login-management-api dotnet ef database update
if %errorlevel% neq 0 (
    echo WARNING: Database migration failed. You may need to run it manually.
)

echo.
echo ========================================
echo   DMS Login Management Setup Complete!
echo ========================================
echo.
echo Services are now running:
echo   - API: http://localhost:5004
echo   - Web App: http://localhost:3004
echo   - Database: localhost:1433
echo.
echo Demo Credentials:
echo   - Admin: admin@dms-demo.com / Admin123!
echo   - Sales: sales@dms-demo.com / Demo123!
echo   - Service: service@dms-demo.com / Demo123!
echo.
echo To stop services, run: docker-compose down
echo To view logs, run: docker-compose logs -f
echo.
pause
