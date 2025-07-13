@echo off
echo =======================================
echo LocalStack Docker Compose Setup Script
echo =======================================
echo.

cd %~dp0

echo Stopping any existing LocalStack containers...
docker-compose down 2>nul

echo Removing any LocalStack volumes...
FOR /F "tokens=*" %%i IN ('docker volume ls -q ^| findstr localstack') DO docker volume rm %%i 2>nul

echo Purging any temp directories...
docker volume prune -f

echo Starting LocalStack with Docker Compose...
docker-compose down -v 2>nul
docker-compose up -d

echo.
echo Waiting for LocalStack to become ready...
timeout /t 10

echo Checking LocalStack health:
curl -s http://localhost:4566/health || echo LocalStack health check failed, but it may still be starting up...

echo.
echo LocalStack should now be running!
echo.
echo Use the following commands:
echo - "docker-compose logs -f" to view logs
echo - "docker-compose down" to stop LocalStack
echo - "docker-compose restart" to restart LocalStack
echo.
echo Press any key to exit...
pause > nul
