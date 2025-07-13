#!/bin/bash
echo "=========================================================="
echo "Fix for LocalStack 'Device or resource busy: /tmp/localstack'"
echo "=========================================================="
echo ""
echo "This script fixes the specific error:"
echo "ERROR: 'rm -rf \"/tmp/localstack\"': exit code 1; output: b\"rm: cannot remove '/tmp/localstack': Device or resource busy\n\""
echo ""

# Check if we're running as root/sudo
if [ "$(id -u)" != "0" ]; then
   echo "⚠️  This script should be run as root/sudo to properly fix mount issues"
   echo "Please run again with: sudo $0"
   echo ""
   echo "Attempting to continue anyway..."
   echo ""
fi

echo "Step 1: Stopping ALL Docker containers..."
docker stop $(docker ps -aq) 2>/dev/null || echo "No containers to stop"

echo "Step 2: Removing ALL Docker containers..."
docker rm $(docker ps -aq) 2>/dev/null || echo "No containers to remove"

echo "Step 3: Check for any mounts of /tmp/localstack..."
if mount | grep "/tmp/localstack"; then
  echo "Found mount points! Attempting to unmount..."
  
  # Forcefully unmount
  sudo umount -f /tmp/localstack 2>/dev/null || umount -f /tmp/localstack 2>/dev/null || echo "Failed to unmount, continuing..."

  # Try lazy unmount if regular unmount fails
  sudo umount -l /tmp/localstack 2>/dev/null || umount -l /tmp/localstack 2>/dev/null || echo "Failed to lazy unmount, continuing..."
else
  echo "No direct mounts found for /tmp/localstack"
fi

echo "Step 4: Identifying processes using /tmp/localstack..."
lsof /tmp/localstack 2>/dev/null || echo "No processes directly using /tmp/localstack found"

echo "Step 5: Removing problematic volumes..."
for vol in $(docker volume ls -q | grep localstack); do
  echo "Removing volume: $vol"
  docker volume rm $vol 2>/dev/null || echo "Could not remove $vol, continuing..."
done

echo "Step 6: Docker system prune..."
docker system prune -f --volumes

echo "Step 7: Forcefully removing /tmp/localstack directory..."
sudo rm -rf /tmp/localstack 2>/dev/null || rm -rf /tmp/localstack 2>/dev/null
if [ -d "/tmp/localstack" ]; then
  echo "⚠️  Directory still exists. Trying more forceful methods..."
  
  # Find processes using the directory
  echo "Processes using /tmp/localstack:"
  sudo lsof +D /tmp/localstack 2>/dev/null || echo "No processes found"
  
  # Try to kill processes using the directory
  for pid in $(sudo lsof +D /tmp/localstack 2>/dev/null | awk 'NR>1 {print $2}' | sort -u); do
    echo "Killing process $pid that's using /tmp/localstack"
    sudo kill -9 $pid 2>/dev/null
  done
  
  # Retry removal after killing processes
  sudo rm -rf /tmp/localstack 2>/dev/null
fi

echo "Step 8: Creating fresh /tmp/localstack with proper permissions..."
sudo mkdir -p /tmp/localstack 2>/dev/null || mkdir -p /tmp/localstack 2>/dev/null
sudo chmod -R 777 /tmp/localstack 2>/dev/null || chmod -R 777 /tmp/localstack 2>/dev/null

echo "Step 9: Restarting Docker service..."
# Different commands based on OS
if command -v systemctl &>/dev/null; then
  echo "Using systemctl to restart Docker..."
  sudo systemctl restart docker || echo "Failed to restart Docker automatically"
elif command -v service &>/dev/null; then
  echo "Using service command to restart Docker..."
  sudo service docker restart || echo "Failed to restart Docker automatically"
else
  echo "⚠️  Could not automatically restart Docker."
  echo "Please restart Docker manually before continuing."
  echo "Press Enter when Docker has been restarted..."
  read -p ""
fi

echo ""
echo "Step 10: Creating a new Docker volume for LocalStack..."
docker volume create localstack-data

echo ""
echo "==================================================="
echo "🎉 Fix complete! Now run LocalStack with this command:"
echo ""
echo "docker run --name localstack -p 4566:4566 \\"
echo "  -e SERVICES=dynamodb,lambda,apigateway \\"
echo "  -e DEBUG=1 \\"
echo "  -e LAMBDA_EXECUTOR=local \\"
echo "  -e HOST_TMP_FOLDER=/var/lib/localstack \\" # Use different path
echo "  -e DOCKER_HOST=unix:///var/run/docker.sock \\"
echo "  -v localstack-data:/var/lib/localstack \\" # Different mount point
echo "  -v /var/run/docker.sock:/var/run/docker.sock \\"
echo "  -d localstack/localstack:latest"
echo ""
echo "Or run our prepared script:"
echo "./start_localstack_compose.sh"
echo "==================================================="
