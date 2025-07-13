#!/bin/bash
echo "====================================================="
echo "Fix for \"Device or resource busy: /tmp/localstack\""
echo "====================================================="
echo ""

echo "This script will fix the common LocalStack error:"
echo "[Errno 16] Device or resource busy: '/tmp/localstack'"
echo ""

echo "Step 1: Stopping all Docker containers..."
docker stop $(docker ps -aq) 2>/dev/null

echo "Step 2: Removing all Docker containers..."
docker rm $(docker ps -aq) 2>/dev/null

echo "Step 3: Removing LocalStack volumes..."
for vol in $(docker volume ls -q | grep localstack); do 
  docker volume rm $vol 2>/dev/null
done

echo "Step 4: Pruning Docker system..."
docker system prune -f

echo "Step 5: Cleaning up /tmp/localstack on host..."
if [ -d "/tmp/localstack" ]; then
  echo "Removing /tmp/localstack directory... (may require sudo)"
  sudo rm -rf /tmp/localstack 2>/dev/null || rm -rf /tmp/localstack 2>/dev/null || echo "Failed to delete directory, please run: sudo rm -rf /tmp/localstack"
  
  echo "Creating clean /tmp/localstack directory..."
  sudo mkdir -p /tmp/localstack 2>/dev/null || mkdir -p /tmp/localstack 2>/dev/null
  
  echo "Setting permissions on /tmp/localstack..."
  sudo chmod 777 /tmp/localstack 2>/dev/null || chmod 777 /tmp/localstack 2>/dev/null
fi

echo "Step 6: Restarting Docker..."
case "$(uname -s)" in
  Darwin)
    echo "Mac OS detected. Please restart Docker Desktop manually."
    ;;
  Linux)
    echo "Restarting Docker service on Linux..."
    sudo systemctl restart docker || echo "Failed to restart Docker. Please restart it manually."
    ;;
  *)
    echo "Unknown OS. Please restart Docker manually."
    ;;
esac

echo ""
echo "==========================================="
echo "Fix complete! Follow these steps to run LocalStack:"
echo "1. Ensure Docker is fully restarted"
echo "2. Run ./start_localstack_compose.sh"
echo "3. Wait 30 seconds for LocalStack to initialize"
echo "4. Refresh your DMS application"
echo "==========================================="
