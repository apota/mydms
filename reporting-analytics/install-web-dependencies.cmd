@echo off
echo Installing NPM dependencies for DMS Reporting and Analytics Web...
cd .\src\DMS.ReportingAnalytics.Web\
call npm install
if %errorlevel% neq 0 (
    echo Error installing dependencies!
    exit /b %errorlevel%
)
echo Dependencies installed successfully!
echo.
echo You can now run the web application with: npm start
cd ..\..\
