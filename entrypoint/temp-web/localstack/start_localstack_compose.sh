#!/bin/bash
echo "======================================="
echo "LocalStack Docker Compose Setup Script"
echo "======================================="
echo ""

# Get the directory of this script
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

echo "Stopping any existing LocalStack containers..."
docker-compose down 2>/dev/null

echo "Removing any LocalStack volumes..."
docker volume rm $(docker volume ls -q | grep localstack) 2>/dev/null || echo "No volumes to remove"

echo "Purging any temporary directories..."
docker volume prune -f

# Fix permissions if on Linux
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
  echo "Setting up temporary directory with proper permissions..."
  TEMPDIR="/tmp/localstack"
  mkdir -p "$TEMPDIR"
  chmod 777 "$TEMPDIR"
  export TEMPDIR
fi

echo "Starting LocalStack with Docker Compose..."
docker-compose down -v 2>/dev/null  # Make sure to stop and remove previous containers with volumes
docker-compose up -d

echo ""
echo "Waiting for LocalStack to become ready..."
sleep 10

echo "Checking LocalStack health:"
curl -s http://localhost:4566/health || echo "LocalStack health check failed, but it may still be starting up..."

echo ""
echo "LocalStack should now be running!"
echo ""
echo "Use the following commands:"
echo "- 'docker-compose logs -f' to view logs"
echo "- 'docker-compose down' to stop LocalStack"
echo "- 'docker-compose restart' to restart LocalStack"
