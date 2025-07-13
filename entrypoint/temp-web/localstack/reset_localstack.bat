@echo off
echo ====================================
echo LocalStack Reset Script for Windows
echo ====================================
echo.

echo Stopping LocalStack container...
docker stop localstack 2>nul || echo Container not running, continuing...

echo Removing LocalStack container...
docker rm localstack 2>nul || echo Container doesn't exist, continuing...

echo Cleaning up any LocalStack volumes...
FOR /F "tokens=*" %%i IN ('docker volume ls -q ^| findstr localstack') DO docker volume rm %%i 2>nul

echo Removing any Docker processes using /tmp/localstack...
FOR /F "tokens=*" %%j IN ('docker ps -a -q') DO (
    docker inspect %%j 2>nul | findstr "/tmp/localstack" >nul
    IF NOT ERRORLEVEL 1 (
        echo Found container %%j using /tmp/localstack, stopping and removing...
        docker stop %%j 2>nul
        docker rm %%j 2>nul
    )
)

echo Pruning unused Docker volumes...
docker volume prune -f

echo Creating fresh volume for LocalStack data with proper permissions...
docker volume create localstack-data

REM Use host networking and a named volume to avoid permission issues
echo Starting a new LocalStack container with optimized settings...
docker run --name localstack ^
  -p 4566:4566 ^
  -e SERVICES=dynamodb,lambda,apigateway ^
  -e DEBUG=1 ^
  -e DOCKER_HOST=unix:///var/run/docker.sock ^
  -e LAMBDA_EXECUTOR=local ^
  -e DATA_DIR=/tmp/localstack/data ^
  -e TMPDIR=/tmp/localstack ^
  -e HOST_TMP_FOLDER=/tmp/localstack ^
  -v localstack-data:/tmp/localstack ^
  -v /var/run/docker.sock:/var/run/docker.sock ^
  -d localstack/localstack:latest

echo.
echo LocalStack reset complete!
echo Wait a few seconds for the service to initialize...
timeout /t 10

echo Checking container status:
docker ps -f name=localstack

echo.
echo If you see the localstack container above, the reset was successful.
echo If not, check Docker installation and try again.
echo.
echo Press any key to exit...
pause > nul
