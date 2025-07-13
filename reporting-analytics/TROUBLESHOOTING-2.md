# Troubleshooting TypeScript and Python Issues

## TypeScript/React Issues

### Missing Type Declarations

If you're seeing errors about missing type declarations like:

```
Cannot find module '@mui/material/Box' or its corresponding type declarations.
```

or

```
This JSX tag requires the module path 'react/jsx-runtime' to exist, but none could be found.
```

**Solution:**

Run the provided fix script:

```
fix-all-tsx-files.cmd
```

This script will:
1. Install all required dependencies and type declarations
2. Create necessary declaration files to fix type errors
3. Configure TypeScript correctly

### Manual Fix for TypeScript Issues

If the automated script doesn't work, you can manually fix the issues:

1. Install required dependencies:
   ```
   cd src\DMS.ReportingAnalytics.Web
   npm install --save chart.js react-chartjs-2 @mui/material @mui/icons-material @mui/x-data-grid @emotion/react @emotion/styled
   npm install --save-dev @types/react @types/react-dom @types/node @types/chart.js @types/d3
   ```

2. Create type declaration files:
   - Create `src/jsx-runtime.d.ts` with:
     ```typescript
     /// <reference types="react/jsx-runtime" />
     ```

   - Create `src/mui-augmentation.d.ts` with:
     ```typescript
     declare module '@mui/material/styles' { };
     declare module '@mui/x-data-grid' {
       export const DataGrid: any;
     }
     ```

   - Create `src/chart-augmentation.d.ts` with:
     ```typescript
     declare module 'react-chartjs-2' {
       export const Line: any;
     }
     ```

3. Update `tsconfig.json` to include:
   ```json
   "typeRoots": [
     "node_modules/@types",
     "src/types"
   ]
   ```

## Python Issues

### Missing Python Dependencies

If you see errors about missing Python packages when running the analytics scripts:

**Solution:**

Run the provided initialization script:

```
initialize-python-env.cmd
```

This script will:
1. Install all required Python dependencies
2. Verify that packages can be imported correctly
3. Test the database connection

### Manual Fix for Python Issues

If the automated script doesn't work, manually install the requirements:

```
cd scripts
pip install -r requirements.txt
```

For specific package errors, install them individually:

```
pip install pandas numpy scikit-learn psycopg2-binary sqlalchemy matplotlib seaborn
```

### Database Connection Issues

If you encounter database connection errors:

1. Verify that PostgreSQL is running
2. Check the database connection parameters in `scripts/config.json`
3. Ensure network access to the database server

## Build and Run Issues

### Frontend Build Failures

If you encounter errors building the frontend:

1. Verify Node.js version (v16+ recommended)
2. Clear npm cache: `npm cache clean --force`
3. Delete node_modules and reinstall: 
   ```
   cd src\DMS.ReportingAnalytics.Web
   rmdir /s /q node_modules
   npm install
   ```

### Backend Build Failures

If you encounter errors building the backend:

1. Verify .NET SDK version (v7.0+ recommended)
2. Restore packages: `dotnet restore`
3. Build with detailed output: `dotnet build -v detailed`

## Still Having Issues?

If you continue to experience problems:

1. Try restarting Visual Studio Code
2. Check for conflicting TypeScript versions
3. Verify environment variables are set correctly
4. Remove any global TypeScript or React installations that might conflict

For further assistance, contact the development team.
