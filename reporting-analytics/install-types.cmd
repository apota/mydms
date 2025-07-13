@echo off
echo Installing missing type definitions...
cd .\src\DMS.ReportingAnalytics.Web\
call npm install --save-dev @types/react @types/react-dom @types/node
if %errorlevel% neq 0 (
    echo Error installing type definitions!
    exit /b %errorlevel%
)
echo Type definitions installed successfully!
cd ..\..\
