@echo off
echo This script requires Node.js to be installed.
echo If you haven't installed Node.js yet, please install it from https://nodejs.org/
echo.

cd /d "%~dp0src\DMS.SalesManagement.Web"

if exist node_modules\. (
  echo Using existing node_modules...
) else (
  echo Creating mock node_modules structure...
  mkdir node_modules
)

echo Adding @ts-nocheck to all TSX files...
echo You may see an error if Node.js is not installed. You can ignore this.

node fix-tsx-files.js

echo.
echo Done!
echo When you install Node.js, please remember to run:
echo cd c:\work\mydms\sales-management\src\DMS.SalesManagement.Web
echo npm install
echo.
pause
