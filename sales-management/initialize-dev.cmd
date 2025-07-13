@echo off
echo Initializing Sales Management development environment...
python scripts\initialize-dev.py %*
if %ERRORLEVEL% EQU 0 (
    echo Environment setup complete. Run 'docker-compose up -d' to start the services.
)
