#!/bin/bash
echo "====================================================="
echo "Update LocalStack to use /var/lib/localstack instead of /tmp/localstack"
echo "====================================================="
echo

echo "This script updates your LocalStack configuration to use a different"
echo "mount path, avoiding the 'Device or resource busy: /tmp/localstack' error."
echo

echo "Step 1: Stopping all LocalStack containers..."
docker ps -q --filter name=localstack | xargs -r docker stop
docker ps -q --filter ancestor=localstack/localstack | xargs -r docker stop

echo "Step 2: Removing LocalStack containers..."
docker ps -aq --filter name=localstack | xargs -r docker rm
docker ps -aq --filter ancestor=localstack/localstack | xargs -r docker rm

echo "Step 3: Creating updated named volume..."
docker volume create localstack-data

echo "Step 4: Starting LocalStack with updated configuration..."
docker run --name localstack -p 4566:4566 \
  -e SERVICES=dynamodb,lambda,apigateway \
  -e DEBUG=1 \
  -e LAMBDA_EXECUTOR=local \
  -e DATA_DIR=/var/lib/localstack/data \
  -e TMPDIR=/var/lib/localstack \
  -e HOST_TMP_FOLDER=/var/lib/localstack \
  -e DOCKER_HOST=unix:///var/run/docker.sock \
  -v localstack-data:/var/lib/localstack \
  -v /var/run/docker.sock:/var/run/docker.sock \
  -d localstack/localstack:latest

echo
echo "Script complete! LocalStack should now be running with the updated path."
echo
echo "Check status with: docker ps"
echo "View logs with: docker logs localstack"
