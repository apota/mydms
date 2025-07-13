#!/bin/bash
# Initialize LocalStack resources for DMS Reporting & Analytics

# Create DynamoDB table
echo "Creating DynamoDB tables..."
awslocal dynamodb create-table \
    --table-name dev-reporting-cache \
    --attribute-definitions AttributeName=CacheKey,AttributeType=S \
    --key-schema AttributeName=CacheKey,KeyType=HASH \
    --provisioned-throughput ReadCapacityUnits=5,WriteCapacityUnits=5

# Create S3 bucket
echo "Creating S3 bucket..."
awslocal s3 mb s3://dev-dms-reporting-exports

# Create SQS queue
echo "Creating SQS queue..."
awslocal sqs create-queue --queue-name reporting-analytics-tasks

echo "LocalStack initialization complete!"
