@echo off
echo Checking for Node.js installation...

where node >nul 2>&1
if %errorlevel% NEQ 0 (
    echo Node.js is not installed or not in PATH.
    echo Please install Node.js from https://nodejs.org/ (LTS version recommended)
    echo After installation, make sure to restart your command prompt and run this script again.
    goto :exit
)

echo Node.js is installed. Proceeding with package installation...
echo.
echo Installing Node.js packages for DMS Sales Management Web project...
cd /d "%~dp0src\DMS.SalesManagement.Web"

rem Create a dummy tsconfig.json file to prevent TypeScript errors
echo { > tsconfig.json
echo   "compilerOptions": { >> tsconfig.json
echo     "target": "es5", >> tsconfig.json
echo     "lib": ["dom", "dom.iterable", "esnext"], >> tsconfig.json
echo     "allowJs": true, >> tsconfig.json
echo     "skipLibCheck": true, >> tsconfig.json
echo     "esModuleInterop": true, >> tsconfig.json
echo     "allowSyntheticDefaultImports": true, >> tsconfig.json
echo     "strict": true, >> tsconfig.json
echo     "forceConsistentCasingInFileNames": true, >> tsconfig.json
echo     "noFallthroughCasesInSwitch": true, >> tsconfig.json
echo     "module": "esnext", >> tsconfig.json
echo     "moduleResolution": "node", >> tsconfig.json
echo     "resolveJsonModule": true, >> tsconfig.json
echo     "isolatedModules": true, >> tsconfig.json
echo     "noEmit": true, >> tsconfig.json
echo     "jsx": "react-jsx" >> tsconfig.json
echo   }, >> tsconfig.json
echo   "include": ["src"] >> tsconfig.json
echo } >> tsconfig.json

rem Create a node_modules folder if it doesn't exist
if not exist node_modules mkdir node_modules

echo.
echo ===============================================================
echo IMPORTANT: Please install Node.js if it's not already installed
echo Visit: https://nodejs.org/ and download the LTS version
echo ===============================================================
echo.
echo After Node.js is installed, run the following commands manually:
echo.
echo cd c:\work\mydms\sales-management\src\DMS.SalesManagement.Web
echo npm install
echo npm install --save-dev @types/react @types/react-dom typescript
echo npm install @emotion/react @emotion/styled @mui/icons-material @mui/material @mui/x-data-grid @mui/x-date-pickers @tanstack/react-query axios chart.js date-fns formik jwt-decode react-chartjs-2 react-pdf react-router-dom react-signature-canvas yup

:exit
pause
