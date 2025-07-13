@echo off
echo Fixing TypeScript errors in the Reporting Analytics Web project...

cd %~dp0\src\DMS.ReportingAnalytics.Web

echo.
echo Installing required type definitions...
call npm install --save-dev @types/react @types/react-dom @types/node @types/chart.js @types/d3

echo.
echo Installing required packages that may be missing...
call npm install --save chart.js react-chartjs-2 @mui/material @mui/icons-material @mui/x-data-grid @emotion/react @emotion/styled

echo.
echo TypeScript setup completed!
echo.
echo Next steps:
echo 1. Run 'npm start' to start the development server
echo 2. If you still see TypeScript errors, try restarting VS Code
echo.

pause
