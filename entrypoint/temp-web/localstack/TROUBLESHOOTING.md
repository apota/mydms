# LocalStack Troubleshooting Guide

This guide contains common commands and steps to troubleshoot LocalStack issues, specifically
the "Device or resource busy: '/tmp/localstack'" error.

## The "/tmp/localstack" Busy Device Error

If you encounter this specific error:

```
ERROR: 'rm -rf "/tmp/localstack"': exit code 1; output: b"rm: cannot remove '/tmp/localstack': Device or resource busy"
ERROR: the LocalStack runtime exited unexpectedly: [Errno 16] Device or resource busy: '/tmp/localstack'
```

### Quick Fix Scripts (Recommended)

Use these provided scripts to fix the issue:

```bash
# Windows
fix_busy_device_error.bat

# Linux/Mac
./fix_busy_device_error.sh
```

### Why This Error Occurs

This error happens because Docker can't properly unmount the LocalStack volume when:
1. A container was killed unexpectedly
2. A process inside LocalStack is still holding the mount
3. WSL2 (on Windows) has a stale mount reference
4. Docker didn't clean up properly after a restart

### Manual WSL2 Fix (Windows)

If you're using Docker Desktop with WSL2 backend:

1. Open PowerShell as Administrator
2. Run WSL commands to force unmount:
   ```powershell
   wsl -d docker-desktop sh -c "umount -f /tmp/localstack 2>/dev/null || true"
   wsl -d docker-desktop sh -c "umount -l /tmp/localstack 2>/dev/null || true"
   wsl -d docker-desktop sh -c "rm -rf /tmp/localstack 2>/dev/null || true"
   ```

### Manual Linux/macOS Fix

1. Find processes using the mount:
   ```bash
   sudo lsof +D /tmp/localstack
   ```
2. Kill those processes:
   ```bash
   sudo lsof +D /tmp/localstack | awk '{print $2}' | grep -v PID | xargs -r sudo kill -9
   ```
3. Force unmount:
   ```bash
   sudo umount -f /tmp/localstack
   ```
4. Clean up:
   ```bash
   sudo rm -rf /tmp/localstack
   ```

### Prevention: Use the Updated Docker Compose

Our docker-compose.yml uses `/var/lib/localstack` instead of `/tmp/localstack` to avoid this issue:

```bash
# Windows
start_localstack_compose.bat

# Linux/Mac
./start_localstack_compose.sh
```

## Quick Fix for Common Issues

### Option 1: Use Docker Compose (Recommended)

Docker Compose provides the most reliable way to run LocalStack:

```bash
# Windows
start_localstack_compose.bat

# Linux/Mac
bash ./start_localstack_compose.sh
```

### Option 2: Run Reset Scripts

```bash
# Windows
reset_localstack.bat

# Linux/Mac
bash ./reset_localstack.sh
```

## Manual Troubleshooting Steps

### 1. Check if LocalStack is running

```bash
docker ps | grep localstack
```

### 2. View LocalStack logs

```bash
docker logs localstack
```

### 3. Stop and remove LocalStack container

```bash
docker stop localstack
docker rm localstack
```

### 4. Check for and clean up volumes

```bash
# List all volumes
docker volume ls | grep localstack

# Remove volumes
docker volume rm $(docker volume ls -q | grep localstack)

# Prune all unused volumes
docker volume prune -f
```

### 5. Find containers using /tmp/localstack

```bash
# Find and remove any containers using /tmp/localstack
for container in $(docker ps -a -q); do
  if docker inspect $container | grep -q "/tmp/localstack"; then
    echo "Found container $container using /tmp/localstack"
    docker stop $container
    docker rm $container
  fi
done
```

### 6. Fix permissions on host (Linux only)

```bash
# Fix /tmp/localstack permissions
sudo mkdir -p /tmp/localstack
sudo chmod 777 /tmp/localstack
```

### 7. Restart Docker service

```bash
# Windows (PowerShell as Admin)
Restart-Service docker

# Mac
osascript -e 'quit app "Docker"' && open -a Docker

# Linux
sudo systemctl restart docker
```

## Advanced Troubleshooting

### Fix Docker socket permissions (Linux)

```bash
sudo chmod 666 /var/run/docker.sock
```

### Reset Docker completely (warning: this will remove all containers and images)

```bash
# Stop all containers
docker stop $(docker ps -a -q)

# Remove all containers
docker rm $(docker ps -a -q)

# Remove all volumes
docker volume prune -f

# Remove all networks
docker network prune -f

# Restart Docker
# (See Step 7 above)
```

### Alternative LocalStack Configuration

If all else fails, run LocalStack without a volume mount:

```bash
docker run --name localstack -p 4566:4566 -e SERVICES=dynamodb,lambda,apigateway -d localstack/localstack:latest
```

Note: This will not persist data between restarts, but should resolve the volume permission issues.
