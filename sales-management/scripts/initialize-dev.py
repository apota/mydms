#!/usr/bin/env python3
"""
Sales Management System Development Environment Setup Script
This script initializes the development environment for the Sales Management module.
"""

import os
import sys
import subprocess
import json
import boto3
import time
from argparse import ArgumentParser

def parse_arguments():
    parser = ArgumentParser(description="Initialize the Sales Management development environment")
    parser.add_argument("--reset", action="store_true", help="Reset existing environment")
    return parser.parse_args()

def check_prerequisites():
    """Check if required tools are installed"""
    prerequisites = ["docker", "dotnet", "aws"]
    
    for tool in prerequisites:
        try:
            subprocess.run([tool, "--version"], stdout=subprocess.PIPE, stderr=subprocess.PIPE, check=True)
            print(f"✓ {tool} is installed")
        except (subprocess.CalledProcessError, FileNotFoundError):
            print(f"✗ {tool} is not installed. Please install it and try again.")
            sys.exit(1)

def setup_localstack():
    """Set up LocalStack for AWS services simulation"""
    print("\nSetting up LocalStack...")
    
    # Create directory for LocalStack initialization scripts
    os.makedirs("localstack-init", exist_ok=True)
    
    # Create initialization script
    with open("localstack-init/init-aws.sh", "w") as f:
        f.write("""#!/bin/bash
echo "Initializing LocalStack resources..."

# Wait for LocalStack to be ready
awslocal --endpoint-url=http://localhost:4566 s3 ls
while [ $? -ne 0 ]; do
    echo "Waiting for LocalStack to be ready..."
    sleep 2
    awslocal --endpoint-url=http://localhost:4566 s3 ls
done

# Create S3 bucket
awslocal --endpoint-url=http://localhost:4566 s3 mb s3://dms-sales-documents
awslocal --endpoint-url=http://localhost:4566 s3api put-bucket-acl --bucket dms-sales-documents --acl private
echo "Created S3 bucket: dms-sales-documents"

# Create DynamoDB table
awslocal --endpoint-url=http://localhost:4566 dynamodb create-table \\
    --table-name dev-sales-documents \\
    --attribute-definitions \\
        AttributeName=id,AttributeType=S \\
        AttributeName=dealId,AttributeType=S \\
        AttributeName=type,AttributeType=S \\
    --key-schema \\
        AttributeName=id,KeyType=HASH \\
        AttributeName=dealId,KeyType=RANGE \\
    --global-secondary-indexes \\
        IndexName=dealId-type-index,KeySchema=[{AttributeName=dealId,KeyType=HASH},{AttributeName=type,KeyType=RANGE}],Projection={ProjectionType=ALL} \\
    --billing-mode PAY_PER_REQUEST
echo "Created DynamoDB table: dev-sales-documents"

echo "LocalStack initialization complete!"
""")
    
    # Make script executable
    os.chmod("localstack-init/init-aws.sh", 0o755)
    print("✓ LocalStack initialization script created")

def create_appsettings():
    """Create appsettings.Development.json for the API project"""
    print("\nCreating development settings...")
    
    settings_path = "src/DMS.SalesManagement.API/appsettings.Development.json"
    settings = {
        "Logging": {
            "LogLevel": {
                "Default": "Information",
                "Microsoft.AspNetCore": "Warning"
            }
        },
        "ConnectionStrings": {
            "SalesDatabase": "Host=localhost;Database=salesmanagement;Username=admin;Password=admin_password"
        },
        "AWS": {
            "Region": "us-east-1",
            "ServiceURL": "http://localhost:4566",
            "DocumentBucketName": "dms-sales-documents"
        },
        "AllowedOrigins": [
            "http://localhost:3000"
        ]
    }
    
    with open(settings_path, "w") as f:
        json.dump(settings, f, indent=2)
    
    print(f"✓ Development settings created at {settings_path}")

def main():
    args = parse_arguments()
    
    print("=== Sales Management Development Environment Setup ===\n")
    
    # Check prerequisites
    check_prerequisites()
    
    # Setup LocalStack
    setup_localstack()
    
    # Create appsettings
    create_appsettings()
    
    print("\nSetup Complete! You can now run the following command to start the environment:")
    print("  docker-compose up -d")
    print("\nAccess the services at:")
    print("  Web UI: http://localhost:3000")
    print("  API: http://localhost:5000")
    print("  Swagger: http://localhost:5000/swagger")
    print("  LocalStack: http://localhost:4566")
    print("  PostgreSQL: localhost:5432")

if __name__ == "__main__":
    main()
