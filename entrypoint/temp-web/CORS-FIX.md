# CORS Issue Fix for LocalStack

## The Problem
You're seeing: `Blocked CORS request from forbidden origin http://localhost:3000`

This happens because LocalStack by default doesn't allow web browsers to make requests from different origins (ports/domains).

## Quick Fix (Recommended)

### Step 1: Stop Current LocalStack
If LocalStack is running, stop it first:
```bash
# Press Ctrl+C in the LocalStack terminal
# Or if running in background:
docker stop localstack-dms
```

### Step 2: Start LocalStack with CORS Support
Use the new CORS-enabled script:
```bash
# Navigate to your project directory
cd c:\work\mydms\entrypoint\temp-web\

# Run the CORS-enabled version
start-localstack-cors.cmd
```

### Step 3: Verify the Fix
1. Open your CRM page: `modules/crm.html`
2. Look for the connection status indicator
3. Should show "🟢 LocalStack Connected" instead of red

## Alternative Manual Fix

If you prefer to start LocalStack manually:

```bash
docker run --name localstack-dms --rm -it \
  -p 4566:4566 \
  -e SERVICES=dynamodb,s3,lambda,apigateway \
  -e DEBUG=1 \
  -e CORS=* \
  -e EXTRA_CORS_ALLOWED_ORIGINS="http://localhost:3000,http://127.0.0.1:3000,file://" \
  -e EXTRA_CORS_ALLOWED_HEADERS="Content-Type,Authorization,X-Amz-Date,X-Api-Key,X-Amz-Security-Token" \
  localstack/localstack
```

## What These Settings Do

- `CORS=*` - Enables CORS for all origins (development only)
- `EXTRA_CORS_ALLOWED_ORIGINS` - Specifically allows localhost:3000 and file:// origins
- `EXTRA_CORS_ALLOWED_HEADERS` - Allows the headers our app needs to send

## Verification

After restarting LocalStack with CORS support:

1. **Browser Console**: Should see "LocalStack connection successful" instead of CORS errors
2. **Network Tab**: Requests to localhost:4566 should succeed (status 200) instead of failing
3. **CRM Page**: Connection indicator should be green, customer creation should work

## Still Having Issues?

1. **Check Docker**: Make sure Docker Desktop is running
2. **Check Ports**: Ensure port 4566 isn't used by another service
3. **Clear Browser Cache**: Sometimes browsers cache CORS failures
4. **Check LocalStack Logs**: Look for any error messages in the Docker container output

## For Development Team

The CORS configuration is now built into:
- `start-localstack-cors.cmd` - Enhanced startup script
- `.localstack.env` - Configuration file
- LocalStack API error handling - Automatically detects and suggests CORS fixes

This ensures all team members can develop without CORS issues.
