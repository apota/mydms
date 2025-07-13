#!/bin/bash
echo "===================================="
echo "LocalStack Reset Script for Linux/Mac"
echo "===================================="
echo ""

echo "Stopping LocalStack container..."
docker stop localstack 2>/dev/null || echo "Container not running, continuing..."

echo "Removing LocalStack container..."
docker rm localstack 2>/dev/null || echo "Container doesn't exist, continuing..."

echo "Finding and cleaning up any containers using /tmp/localstack..."
for container in $(docker ps -a -q); do
  if docker inspect "$container" 2>/dev/null | grep -q "/tmp/localstack"; then
    echo "Found container $container using /tmp/localstack, stopping and removing..."
    docker stop "$container" 2>/dev/null
    docker rm "$container" 2>/dev/null
  fi
done

echo "Cleaning up any LocalStack volumes..."
docker volume rm $(docker volume ls -q | grep localstack) 2>/dev/null || echo "No volumes to remove, continuing..."

echo "Pruning unused Docker volumes and containers..."
docker volume prune -f
docker container prune -f

echo "Creating fresh volume for LocalStack data with proper permissions..."
docker volume create localstack-data

# Fix permissions if running on Linux
if [[ "$OSTYPE" == "linux-gnu"* ]]; then
  echo "Fixing permissions for /tmp/localstack on Linux..."
  sudo mkdir -p /tmp/localstack
  sudo chmod 777 /tmp/localstack
fi

echo "Starting a new LocalStack container with optimized settings..."
docker run --name localstack \
  -p 4566:4566 \
  -e SERVICES=dynamodb,lambda,apigateway \
  -e DEBUG=1 \
  -e DOCKER_HOST=unix:///var/run/docker.sock \
  -e LAMBDA_EXECUTOR=local \
  -e DATA_DIR=/tmp/localstack/data \
  -e TMPDIR=/tmp/localstack \
  -e HOST_TMP_FOLDER=/tmp/localstack \
  -v localstack-data:/tmp/localstack \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -d localstack/localstack:latest

echo ""
echo "LocalStack reset complete!"
echo "Wait a few seconds for the service to initialize..."
sleep 10

echo "Checking container status:"
docker ps -f name=localstack

echo ""
echo "If you see the localstack container above, the reset was successful."
echo "If not, check Docker installation and try again."
