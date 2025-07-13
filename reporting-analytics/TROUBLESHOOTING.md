# Troubleshooting Guide

## Common Issues and Solutions

### TypeScript/React Errors

If you encounter TypeScript errors related to missing types or React JSX issues, follow these steps:

1. Install the required type definitions:
   ```bash
   npm run install-types
   ```
   or use the provided script:
   ```
   install-types.cmd
   ```

2. Run the comprehensive fix script to resolve TypeScript issues:
   ```
   fix-all-tsx-files.cmd
   ```

3. Make sure your tsconfig.json includes the correct settings:
   ```json
   {
     "compilerOptions": {
       "jsx": "react-jsx",
       "esModuleInterop": true,
       "allowSyntheticDefaultImports": true,
       // other settings...
     }
   }
   ```

3. If you see "Cannot find module" errors for dependencies:
   ```bash
   npm install
   ```
   or use the provided script:
   ```
   install-web-dependencies.cmd
   ```

### Backend Connection Issues

If the frontend can't connect to the backend API:

1. Check that the API is running:
   ```bash
   cd src/DMS.ReportingAnalytics.API
   dotnet run
   ```

2. Verify the API URL in the frontend:
   - Check that the proxy setting in package.json points to the correct API URL
   - Create a .env file with REACT_APP_API_URL setting if needed

### Missing AWS Credentials for Development

If you see AWS credential errors when running the application locally:

1. Make sure LocalStack is running:
   ```bash
   docker-compose up -d localstack
   ```

2. Check that AWS configuration is set to use LocalStack:
   - In appsettings.Development.json, verify that "UseLocalStack": true is set

## Error Codes and Solutions

### TS2307: Cannot find module 'X' or its corresponding type declarations

Install the required type definitions:
```bash
npm install --save-dev @types/X
```

### TS7016: Could not find a declaration file for module 'X'

Create a custom type declaration file in src/types directory:
```typescript
declare module 'X' {
  const content: any;
  export default content;
}
```

### CORS errors when connecting to the API

Ensure the allowed origins are correctly set in the API:
```json
"AllowedOrigins": [
  "http://localhost:3000",
  "https://localhost:3000"
]
```

### Python Script Errors

If you encounter issues with the Python analytics scripts:

1. Initialize the Python environment using the provided script:
   ```
   initialize-python-env.cmd
   ```

2. Verify that all required packages are installed:
   ```bash
   pip install -r scripts/requirements.txt
   ```

3. If you see "ModuleNotFoundError", install the specific missing package:
   ```bash
   pip install pandas numpy scikit-learn matplotlib seaborn
   ```

4. For database connection issues, check the configuration in scripts/config.json
   ```json
   {
     "database": {
       "host": "localhost",
       "port": 5432,
       "database": "dms_reporting",
       "user": "postgres",
       "password": "postgres"
     }
   }
   ```

5. Run the diagnostic script to identify issues:
   ```bash
   python scripts/initialize-env.py
   ```
