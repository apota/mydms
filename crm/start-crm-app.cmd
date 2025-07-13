@echo off
echo Starting CRM Application...
echo.

echo Building API...
dotnet build c:\work\mydms\crm\src\DMS.CRM.API\DMS.CRM.API.csproj
if %errorlevel% neq 0 (
    echo API Build failed!
    pause
    exit /b 1
)

echo.
echo Starting API server...
start "CRM API" cmd /k "cd /d c:\work\mydms\crm\src\DMS.CRM.API && dotnet run"

echo.
echo Waiting for API to start...
timeout /t 5

echo.
echo Starting Web Application...
start "CRM Web" cmd /k "cd /d c:\work\mydms\crm\src\DMS.CRM.Web && npm start"

echo.
echo CRM Application is starting...
echo API will be available at: https://localhost:7001
echo Web app will be available at: http://localhost:3000
echo.
echo Press any key to close this window...
pause
