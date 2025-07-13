@echo off
echo Starting LocalStack for DMS Customer Database...
echo.
echo This will start LocalStack on port 4566 for database operations.
echo Make sure Docker Desktop is running before proceeding.
echo.
pause

echo Checking if Docker is running...
docker info >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: Docker is not running or not installed.
    echo Please start Docker Desktop and try again.
    pause
    exit /b 1
)

echo Docker is running. Starting LocalStack with CORS enabled...
docker run --rm -it -p 4566:4566 -p 4510-4559:4510-4559 ^
  -e SERVICES=dynamodb,s3,lambda,apigateway ^
  -e DEBUG=1 ^
  -e CORS=* ^
  -e EXTRA_CORS_ALLOWED_ORIGINS=http://localhost:3000,http://127.0.0.1:3000,file:// ^
  -e EXTRA_CORS_ALLOWED_HEADERS=Content-Type,Authorization,X-Amz-Date,X-Api-Key,X-Amz-Security-Token ^
  localstack/localstack

echo.
echo LocalStack has stopped.
pause
