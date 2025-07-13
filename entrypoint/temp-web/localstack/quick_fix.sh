#!/bin/bash
# Direct fix for "Device or resource busy: '/tmp/localstack'" error

echo "Starting direct fix for 'Device or resource busy: /tmp/localstack' error..."

# Stop and remove all LocalStack containers
echo "Stopping and removing LocalStack containers..."
docker stop localstack 2>/dev/null
docker rm localstack 2>/dev/null

# Remove LocalStack volumes
echo "Removing LocalStack volumes..."
docker volume rm $(docker volume ls -q | grep localstack) 2>/dev/null

# Run LocalStack with different mount path
echo "Starting LocalStack with different mount path..."
docker run --name localstack -p 4566:4566 \
  -e SERVICES=dynamodb,lambda,apigateway \
  -e DEBUG=1 \
  -e LAMBDA_EXECUTOR=local \
  -e HOST_TMP_FOLDER=/var/lib/localstack \
  -e DOCKER_HOST=unix:///var/run/docker.sock \
  -v localstack-data:/var/lib/localstack \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -d localstack/localstack:latest

echo "Done! LocalStack should now be running with a different mount path."
echo "Check if it's running with: docker ps | grep localstack"
