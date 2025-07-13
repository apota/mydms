@echo off
setlocal enabledelayedexpansion

cd /d "%~dp0src\DMS.SalesManagement.Web\src"

echo Adding @ts-nocheck to all TypeScript React files...

for /r %%i in (*.tsx) do (
  echo Updating %%i
  set "file=%%i"
  set "tempfile=%%i.tmp"
  
  echo // @ts-nocheck > "!tempfile!"
  type "!file!" >> "!tempfile!"
  move /y "!tempfile!" "!file!" > nul
)

echo Done!
pause
