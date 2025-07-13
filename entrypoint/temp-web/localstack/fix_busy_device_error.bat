@echo off
echo ========================================================
echo Fix for LocalStack 'Device or resource busy: /tmp/localstack'
echo ========================================================
echo.
echo This script fixes the specific error:
echo ERROR: 'rm -rf "/tmp/localstack"': exit code 1; output: b"rm: cannot remove '/tmp/localstack': Device or resource busy"
echo.

echo Step 1: Stopping LocalStack containers...
for /f "tokens=*" %%i in ('docker ps -q --filter name^=localstack') do (
    echo Stopping container: %%i
    docker stop %%i 2>nul
)
for /f "tokens=*" %%i in ('docker ps -q --filter ancestor^=localstack/localstack') do (
    echo Stopping container: %%i
    docker stop %%i 2>nul
)

echo Step 2: Removing LocalStack containers...
for /f "tokens=*" %%i in ('docker ps -aq --filter name^=localstack') do (
    echo Removing container: %%i
    docker rm %%i 2>nul
)
for /f "tokens=*" %%i in ('docker ps -aq --filter ancestor^=localstack/localstack') do (
    echo Removing container: %%i
    docker rm %%i 2>nul
)

echo Step 3: Removing problematic LocalStack volumes...
for /f "tokens=*" %%i in ('docker volume ls -q ^| findstr localstack') do (
    echo Removing volume: %%i
    docker volume rm %%i 2>nul
)

echo Step 4: Advanced WSL2 fix for /tmp/localstack...
wsl --list >nul 2>&1
if %errorlevel% equ 0 (
    echo WSL detected, attempting advanced fixes...
    
    echo 4.1: Forcing unmount of /tmp/localstack in WSL...
    wsl -d docker-desktop sh -c "umount -f /tmp/localstack 2>/dev/null || true"
    
    echo 4.2: Using lazy unmount as fallback...
    wsl -d docker-desktop sh -c "umount -l /tmp/localstack 2>/dev/null || true"
    
    echo 4.3: Finding and killing any processes using /tmp/localstack...
    wsl -d docker-desktop sh -c "lsof +D /tmp/localstack 2>/dev/null | awk '{print \$2}' | grep -v PID | xargs -r kill -9 2>/dev/null || true"
    
    echo 4.4: Forcefully removing /tmp/localstack directory...
    wsl -d docker-desktop sh -c "rm -rf /tmp/localstack 2>/dev/null || true"
    
    echo 4.5: Verifying cleanup status...
    wsl -d docker-desktop sh -c "ls -la /tmp | grep localstack || echo 'Cleanup successful - no localstack folder found'"
) else (
    echo WSL not detected. This error is typically related to Docker Desktop with WSL2 backend.
    echo Will continue with standard Docker cleanup.
)

echo Step 5: Pruning Docker system (only volumes related to LocalStack)...

echo Step 5: Shutting down Docker...
echo Please completely quit Docker Desktop if you're using it.
echo Then, restart Docker Desktop when prompted later.
echo.
echo If you're using Docker service, we'll try to restart it.
echo.
echo Press any key when ready to proceed...
pause > nul

echo Attempting to restart Docker service...
net stop docker 2>nul
net start docker 2>nul

echo.
echo Step 6: Creating a new Docker volume for LocalStack...
docker volume create localstack-data

echo.
echo ===================================================
echo Fix complete! Now run LocalStack with this command:
echo.
echo docker run --name localstack -p 4566:4566 ^
echo   -e SERVICES=dynamodb,lambda,apigateway ^
echo   -e DEBUG=1 ^
echo   -e LAMBDA_EXECUTOR=local ^
echo   -e HOST_TMP_FOLDER=/var/lib/localstack ^
echo   -e DOCKER_HOST=unix:///var/run/docker.sock ^
echo   -v localstack-data:/var/lib/localstack ^
echo   -v /var/run/docker.sock:/var/run/docker.sock ^
echo   -d localstack/localstack:latest
echo.
echo Or run our prepared script:
echo start_localstack_compose.bat
echo ===================================================
echo.
echo Press any key to exit...
pause > nul
