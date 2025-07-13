#!/bin/bash

# LocalStack initialization script for DMS
echo "🚀 Initializing LocalStack AWS services for DMS..."

# Wait for LocalStack to be ready
while ! aws --endpoint-url=http://localstack:4566 sts get-caller-identity &> /dev/null; do
    echo "⏳ Waiting for LocalStack to be ready..."
    sleep 2
done

echo "✅ LocalStack is ready, creating AWS resources..."

# Create S3 buckets
echo "📦 Creating S3 buckets..."
aws --endpoint-url=http://localstack:4566 s3 mb s3://dms-vehicle-images
aws --endpoint-url=http://localstack:4566 s3 mb s3://dms-documents
aws --endpoint-url=http://localstack:4566 s3 mb s3://dms-reports
aws --endpoint-url=http://localstack:4566 s3 mb s3://dms-backups

# Set bucket policies to allow public read for images
aws --endpoint-url=http://localstack:4566 s3api put-bucket-policy --bucket dms-vehicle-images --policy '{
    "Version": "2012-10-17",
    "Statement": [
        {
            "Sid": "PublicReadGetObject",
            "Effect": "Allow",
            "Principal": "*",
            "Action": "s3:GetObject",
            "Resource": "arn:aws:s3:::dms-vehicle-images/*"
        }
    ]
}'

# Create Cognito User Pool
echo "🔐 Creating Cognito User Pool..."
POOL_ID=$(aws --endpoint-url=http://localstack:4566 cognito-idp create-user-pool \
    --pool-name dms-users \
    --policies PasswordPolicy='{MinimumLength=8,RequireUppercase=true,RequireLowercase=true,RequireNumbers=true,RequireSymbols=true}' \
    --mfa-configuration OPTIONAL \
    --query 'UserPool.Id' \
    --output text)

echo "Created User Pool with ID: $POOL_ID"

# Create User Pool Client
CLIENT_ID=$(aws --endpoint-url=http://localstack:4566 cognito-idp create-user-pool-client \
    --user-pool-id $POOL_ID \
    --client-name dms-client \
    --generate-secret \
    --query 'UserPoolClient.ClientId' \
    --output text)

echo "Created User Pool Client with ID: $CLIENT_ID"

# Create SNS topics for notifications
echo "📢 Creating SNS topics..."
aws --endpoint-url=http://localstack:4566 sns create-topic --name dms-notifications
aws --endpoint-url=http://localstack:4566 sns create-topic --name dms-alerts
aws --endpoint-url=http://localstack:4566 sns create-topic --name dms-service-reminders

# Create SQS queues
echo "📬 Creating SQS queues..."
aws --endpoint-url=http://localstack:4566 sqs create-queue --queue-name dms-task-queue
aws --endpoint-url=http://localstack:4566 sqs create-queue --queue-name dms-email-queue
aws --endpoint-url=http://localstack:4566 sqs create-queue --queue-name dms-analytics-queue

# Create DynamoDB tables for caching and sessions
echo "🗄️ Creating DynamoDB tables..."
aws --endpoint-url=http://localstack:4566 dynamodb create-table \
    --table-name dms-sessions \
    --attribute-definitions AttributeName=sessionId,AttributeType=S \
    --key-schema AttributeName=sessionId,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST

aws --endpoint-url=http://localstack:4566 dynamodb create-table \
    --table-name dms-cache \
    --attribute-definitions AttributeName=cacheKey,AttributeType=S \
    --key-schema AttributeName=cacheKey,KeyType=HASH \
    --billing-mode PAY_PER_REQUEST

# Create Lambda function for image processing
echo "⚡ Creating Lambda functions..."
cat > /tmp/image-processor.js << 'EOF'
exports.handler = async (event) => {
    console.log('Processing image upload:', JSON.stringify(event, null, 2));
    
    // Simulate image processing
    const response = {
        statusCode: 200,
        body: JSON.stringify({
            message: 'Image processed successfully',
            thumbnailUrl: event.thumbnailUrl || 'processed-image-url'
        })
    };
    
    return response;
};
EOF

zip -j /tmp/image-processor.zip /tmp/image-processor.js

aws --endpoint-url=http://localstack:4566 lambda create-function \
    --function-name dms-image-processor \
    --runtime nodejs18.x \
    --role arn:aws:iam::000000000000:role/lambda-role \
    --handler index.handler \
    --zip-file fileb:///tmp/image-processor.zip

# Create IAM role for Lambda (LocalStack doesn't require real permissions)
aws --endpoint-url=http://localstack:4566 iam create-role \
    --role-name lambda-role \
    --assume-role-policy-document '{
        "Version": "2012-10-17",
        "Statement": [
            {
                "Effect": "Allow",
                "Principal": {
                    "Service": "lambda.amazonaws.com"
                },
                "Action": "sts:AssumeRole"
            }
        ]
    }'

# Create API Gateway
echo "🌐 Creating API Gateway..."
API_ID=$(aws --endpoint-url=http://localstack:4566 apigateway create-rest-api \
    --name dms-api \
    --description "DMS API Gateway" \
    --query 'id' \
    --output text)

echo "Created API Gateway with ID: $API_ID"

# Store configuration in a file for services to use
cat > /tmp/localstack-config.json << EOF
{
    "s3": {
        "vehicleImagesBucket": "dms-vehicle-images",
        "documentsBucket": "dms-documents",
        "reportsBucket": "dms-reports",
        "backupsBucket": "dms-backups"
    },
    "cognito": {
        "userPoolId": "$POOL_ID",
        "clientId": "$CLIENT_ID"
    },
    "sns": {
        "notificationsTopic": "arn:aws:sns:us-east-1:000000000000:dms-notifications",
        "alertsTopic": "arn:aws:sns:us-east-1:000000000000:dms-alerts",
        "remindersTopic": "arn:aws:sns:us-east-1:000000000000:dms-service-reminders"
    },
    "sqs": {
        "taskQueue": "http://localhost:4566/000000000000/dms-task-queue",
        "emailQueue": "http://localhost:4566/000000000000/dms-email-queue",
        "analyticsQueue": "http://localhost:4566/000000000000/dms-analytics-queue"
    },
    "dynamodb": {
        "sessionsTable": "dms-sessions",
        "cacheTable": "dms-cache"
    },
    "lambda": {
        "imageProcessor": "dms-image-processor"
    },
    "apigateway": {
        "apiId": "$API_ID"
    }
}
EOF

# Copy config to a shared location
cp /tmp/localstack-config.json /var/lib/localstack/localstack-config.json

echo "✅ LocalStack initialization complete!"
echo "📋 AWS Resources created:"
echo "   - S3 Buckets: dms-vehicle-images, dms-documents, dms-reports, dms-backups"
echo "   - Cognito User Pool: $POOL_ID"
echo "   - Cognito Client: $CLIENT_ID"
echo "   - SNS Topics: dms-notifications, dms-alerts, dms-service-reminders"
echo "   - SQS Queues: dms-task-queue, dms-email-queue, dms-analytics-queue"
echo "   - DynamoDB Tables: dms-sessions, dms-cache"
echo "   - Lambda Functions: dms-image-processor"
echo "   - API Gateway: $API_ID"
echo ""
echo "🔗 LocalStack Dashboard: http://localhost:4566"
echo "📄 Configuration saved to: /var/lib/localstack/localstack-config.json"
