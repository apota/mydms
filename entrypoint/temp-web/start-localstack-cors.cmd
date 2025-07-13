@echo off
echo ================================================
echo Starting LocalStack for DMS with CORS Support
echo ================================================
echo.
echo This will start LocalStack with proper CORS configuration
echo to allow web browser access from your DMS application.
echo.
echo Configuration:
echo - Port: 4566 (main LocalStack endpoint)
echo - CORS: Enabled for localhost origins
echo - Services: DynamoDB, S3, Lambda, API Gateway
echo - Debug: Enabled
echo.
pause

echo Checking if Docker is running...
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ❌ ERROR: Docker is not running or not installed.
    echo.
    echo Please:
    echo 1. Start Docker Desktop
    echo 2. Wait for it to fully initialize
    echo 3. Try running this script again
    echo.
    pause
    exit /b 1
)

echo ✅ Docker is running.
echo.

echo Stopping any existing LocalStack containers...
docker stop localstack-dms 2>nul
docker rm localstack-dms 2>nul

echo.
echo 🚀 Starting LocalStack with CORS configuration...
echo.

docker run --name localstack-dms --rm -it ^
  -p 4566:4566 ^
  -p 4510-4559:4510-4559 ^
  -e SERVICES=dynamodb,s3,lambda,apigateway,cloudformation ^
  -e DEBUG=1 ^
  -e CORS=* ^
  -e EXTRA_CORS_ALLOWED_ORIGINS="http://localhost:3000,http://127.0.0.1:3000,http://localhost:8080,file://" ^
  -e EXTRA_CORS_ALLOWED_HEADERS="Content-Type,Authorization,X-Amz-Date,X-Api-Key,X-Amz-Security-Token,X-Amz-Target,X-Amz-User-Agent" ^
  -e GATEWAY_LISTEN=0.0.0.0:4566 ^
  -e PERSISTENCE=1 ^
  -e DATA_DIR=/tmp/localstack/data ^
  -e SKIP_SSL_CERT_DOWNLOAD=1 ^
  -v "%cd%\.localstack:/tmp/localstack" ^
  localstack/localstack

echo.
echo LocalStack has stopped.
echo Data is persisted in .localstack directory for next startup.
pause
