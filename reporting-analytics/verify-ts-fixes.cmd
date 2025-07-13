@echo off
echo Verifying TypeScript fixes in the Reporting Analytics Web project...

cd %~dp0\src\DMS.ReportingAnalytics.Web

echo.
echo Checking TypeScript compilation...
call npx tsc --noEmit

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo TypeScript errors found! Please run fix-all-tsx-files.cmd to fix them.
    exit /b %ERRORLEVEL%
)

echo.
echo TypeScript verification passed! No compilation errors found.
echo.

pause
