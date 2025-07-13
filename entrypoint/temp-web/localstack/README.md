# DMS LocalStack Integration

This document provides instructions for setting up LocalStack to work with the Dealership Management System (DMS) for persistent data storage.

## Prerequisites

1. **Docker**: LocalStack runs in Docker containers. [Download and install Docker](https://www.docker.com/products/docker-desktop/)
2. **Node.js**: Required for running demo Lambda functions. [Download and install Node.js](https://nodejs.org/)
3. **AWS CLI**: Optional, but useful for interacting with LocalStack. [Installation instructions](https://aws.amazon.com/cli/)

## Quick Start

The easiest way to get LocalStack running is to use our provided scripts:

**Windows:**
```
start_localstack_compose.bat
```

**Linux/Mac:**
```bash
chmod +x ./start_localstack_compose.sh
./start_localstack_compose.sh
```

If you encounter any issues, see the [Troubleshooting](#troubleshooting) section below or refer to `TROUBLESHOOTING.md` for detailed steps.

## Manual Setup Instructions

### 1. Install LocalStack

```bash
# Install using pip (Python package manager)
pip install localstack

# Or run with Docker directly
docker run --name localstack -p 4566:4566 -e SERVICES=dynamodb,lambda,apigateway -d localstack/localstack
```

### 2. Configure AWS CLI for LocalStack

```bash
aws configure --profile localstack
```

Enter the following configuration:
- AWS Access Key ID: `test`
- AWS Secret Access Key: `test`
- Region: `us-east-1`
- Output format: `json`

### 3. Test LocalStack Installation

```bash
# Test LocalStack connection using AWS CLI
aws --endpoint-url=http://localhost:4566 --profile localstack dynamodb list-tables
```

If LocalStack is running correctly, this should return a JSON response (empty if no tables exist yet).

## DMS Configuration

The web application is already configured to connect to LocalStack at the default address: `http://localhost:4566`.

### Key Features:

1. **Customer Data Persistence**: All customer data is stored in both LocalStack (DynamoDB) and browser localStorage.
2. **Fallback Mechanism**: If LocalStack is unavailable, the application falls back to using only localStorage.
3. **Initial Data Loading**: When first loading, the application retrieves data from LocalStack if available.

### API Endpoints (Implemented in LocalStack)

- `POST /dms/api/init`: Initialize database tables
- `GET /dms/api/customers`: Get all customers
- `POST /dms/api/customers`: Create a new customer
- `PUT /dms/api/customers/:id`: Update a customer
- `DELETE /dms/api/customers/:id`: Delete a customer

## Deploying the Backend to LocalStack

1. Zip the Lambda function:

```bash
cd localstack
zip -r customer-service.zip customer-service.js
```

2. Create the Lambda function in LocalStack:

```bash
aws --endpoint-url=http://localhost:4566 --profile localstack lambda create-function \
    --function-name dms-customer-service \
    --runtime nodejs14.x \
    --handler customer-service.handler \
    --zip-file fileb://customer-service.zip \
    --role arn:aws:iam::000000000000:role/lambda-role
```

3. Create API Gateway integration:

```bash
# Create REST API
aws --endpoint-url=http://localhost:4566 --profile localstack apigateway create-rest-api \
    --name dms-api

# Get the API ID from response and use it in subsequent commands
API_ID=<api-id-from-response>

# Create resources and methods for each endpoint
# (See detailed commands in the LocalStack documentation)
```

## Troubleshooting

### Common LocalStack Errors

#### 1. Error: "Device or resource busy: '/tmp/localstack'"

```
ERROR: the LocalStack runtime exited unexpectedly: [Errno 16] Device or resource busy: '/tmp/localstack'
```

This error occurs when LocalStack cannot clean up its temporary directory properly. Here's how to fix it:

**Solution A: Restart LocalStack Container**

```bash
# Stop the container
docker stop localstack

# Remove the container
docker rm localstack

# Start a fresh container
docker run --name localstack -p 4566:4566 -e SERVICES=dynamodb,lambda,apigateway -d localstack/localstack
```

**Solution B: Clean Up the Docker Volume**

```bash
# Stop the container
docker stop localstack

# Remove the container
docker rm localstack

# Remove any volume with stale data
docker volume rm $(docker volume ls -q | grep localstack)

# Start a fresh container
docker run --name localstack -p 4566:4566 -e SERVICES=dynamodb,lambda,apigateway -d localstack/localstack
```

**Solution C: Use a Named Volume with Persistent Permissions**

```bash
# Create a volume with proper permissions
docker volume create localstack-data

# Run LocalStack with the volume
docker run --name localstack \
  -p 4566:4566 \
  -e SERVICES=dynamodb,lambda,apigateway \
  -v localstack-data:/tmp/localstack \
  -d localstack/localstack
```

#### 2. Timeout or Connection Refused Errors

If you see timeout errors in the console when trying to connect to LocalStack:

1. Verify that Docker is running
2. Check that the LocalStack container is running:
   ```bash
   docker ps | grep localstack
   ```
3. Check the container logs for errors:
   ```bash
   docker logs localstack
   ```

### Other Issues

- **Connection Issues**: Ensure Docker is running and LocalStack container is active.
- **CORS Errors**: The Lambda includes CORS headers. If needed, modify the headers in customer-service.js.
- **Data Not Persisting**: Check Docker logs for LocalStack container for any errors.

### Fallback Behavior

The DMS application is designed to gracefully handle LocalStack unavailability:

1. If LocalStack is not running or unreachable, the application will show a warning and operate in localStorage-only mode.
2. All customer data will still be saved to browser localStorage.
3. When LocalStack becomes available again, the application will reconnect on page reload and sync the local data.
