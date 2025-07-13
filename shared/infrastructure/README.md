# AWS Infrastructure for Dealership Management System

This directory contains Terraform configurations for provisioning AWS infrastructure components needed by the DMS.

## Structure

- `main.tf` - Main Terraform configuration
- `variables.tf` - Input variables for the Terraform configuration
- `outputs.tf` - Output values
- `modules/` - Reusable Terraform modules
  - `vpc/` - VPC configuration
  - `ecs/` - ECS cluster configuration
  - `rds/` - PostgreSQL RDS configuration
  - `dynamodb/` - DynamoDB tables
  - `s3/` - S3 buckets for file storage
  - `cognito/` - User authentication
  - `cloudfront/` - CDN for static assets
  - `api_gateway/` - API Gateway configuration

## Getting Started

1. Install Terraform
2. Configure AWS credentials
3. Initialize Terraform: `terraform init`
4. Plan deployment: `terraform plan -out=tfplan`
5. Apply changes: `terraform apply tfplan`

## Environment Configurations

- `dev/` - Development environment
- `staging/` - Staging environment
- `prod/` - Production environment
