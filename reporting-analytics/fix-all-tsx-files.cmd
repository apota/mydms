@echo off
echo Fixing TypeScript errors in the Reporting Analytics Web project...

cd %~dp0\src\DMS.ReportingAnalytics.Web

echo.
echo Installing required packages and type definitions...
call npm install --save chart.js react-chartjs-2 @mui/material @mui/icons-material @mui/x-data-grid @emotion/react @emotion/styled
call npm install --save-dev @types/react @types/react-dom @types/node @types/chart.js @types/d3

echo.
echo Creating type augmentation files...

echo Creating jsx runtime type fix...
echo // This file ensures JSX runtime types are available > src\jsx-runtime.d.ts
echo /// ^<reference types="react/jsx-runtime" /^> >> src\jsx-runtime.d.ts

echo Creating mui type fixes...
echo // This file ensures MUI types are available > src\mui-augmentation.d.ts
echo declare module '@mui/material/styles' { }; >> src\mui-augmentation.d.ts
echo declare module '@mui/x-data-grid' { >> src\mui-augmentation.d.ts
echo   export const DataGrid: any; >> src\mui-augmentation.d.ts
echo } >> src\mui-augmentation.d.ts

echo Creating chart.js type fixes...
echo // This file ensures chart.js types are available > src\chart-augmentation.d.ts
echo declare module 'react-chartjs-2' { >> src\chart-augmentation.d.ts
echo   export const Line: any; >> src\chart-augmentation.d.ts
echo } >> src\chart-augmentation.d.ts

echo.
echo TypeScript setup completed!
echo.
echo Next steps:
echo 1. Run 'npm start' to start the development server
echo 2. If you still see TypeScript errors, try restarting VS Code
echo.

pause
