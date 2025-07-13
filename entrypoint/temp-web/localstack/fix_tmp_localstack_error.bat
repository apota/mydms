@echo off
echo =====================================================
echo Fix for "Device or resource busy: /tmp/localstack"
echo =====================================================
echo.

echo This script will fix the common LocalStack error:
echo [Errno 16] Device or resource busy: '/tmp/localstack'
echo.

echo Step 1: Stopping LocalStack containers specifically...
FOR /F "tokens=*" %%i IN ('docker ps -q --filter name^=localstack') DO docker stop %%i
FOR /F "tokens=*" %%i IN ('docker ps -q --filter ancestor^=localstack/localstack') DO docker stop %%i

echo Step 2: Removing LocalStack containers...
FOR /F "tokens=*" %%i IN ('docker ps -aq --filter name^=localstack') DO docker rm %%i
FOR /F "tokens=*" %%i IN ('docker ps -aq --filter ancestor^=localstack/localstack') DO docker rm %%i

echo Step 3: Removing LocalStack volumes...
FOR /F "tokens=*" %%i IN ('docker volume ls -q ^| findstr localstack') DO docker volume rm %%i

echo Step 4: Running specific WSL fix for /tmp/localstack...
wsl --list >nul 2>&1
if %errorlevel% equ 0 (
    echo WSL detected, attempting advanced WSL mount fix...
    wsl -d docker-desktop sh -c "umount /tmp/localstack 2>/dev/null || true"
    wsl -d docker-desktop sh -c "rm -rf /tmp/localstack 2>/dev/null || true"
    echo WSL mount cleanup completed.
) else (
    echo Not using WSL or WSL not detected.
)

echo Step 5: Pruning Docker system (volumes and unused resources)...
docker system prune -f --volumes

echo Step 5: Restarting Docker...
echo Please restart Docker Desktop manually if you're using Windows
echo or press any key to try automatically restarting the Docker service...
pause > nul

echo Attempting to restart Docker service...
net stop docker
net start docker

echo.
echo ==========================================
echo Fix complete! Follow these steps to run LocalStack:
echo 1. Ensure Docker is fully restarted
echo 2. Run start_localstack_compose.bat
echo 3. Wait 30 seconds for LocalStack to initialize
echo 4. Refresh your DMS application
echo ==========================================
echo.
echo Press any key to exit...
pause > nul
