# AWS Provider Configuration
provider "aws" {
  region = var.aws_region
}

# Variables
variable "aws_region" {
  description = "The AWS region to deploy to"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "The environment (dev, test, prod)"
  type        = string
  default     = "dev"
}

variable "project" {
  description = "The project name"
  type        = string
  default     = "dms-crm"
}

# Import shared infrastructure modules
module "vpc" {
  source = "../../../shared/infrastructure/modules/vpc"
  
  project     = var.project
  environment = var.environment
}

module "rds" {
  source = "../../../shared/infrastructure/modules/rds"
  
  project     = var.project
  environment = var.environment
  vpc_id      = module.vpc.vpc_id
  subnet_ids  = module.vpc.private_subnet_ids
  
  db_name     = "dms_crm"
  db_username = "dms_admin"
  db_password = var.db_password # This should be supplied via a secure method
  engine      = "postgres"
  engine_version = "13.7"
  instance_class = "db.t3.medium"
  allocated_storage = 20
  
  depends_on = [module.vpc]
}

module "dynamodb" {
  source = "../../../shared/infrastructure/modules/dynamodb"
  
  project     = var.project
  environment = var.environment
  
  tables = [
    {
      name = "dms-customer-interactions"
      hash_key = "Id"
      attributes = [
        {
          name = "Id"
          type = "S"
        },
        {
          name = "CustomerId"
          type = "S"
        },
        {
          name = "TimeStamp"
          type = "S"
        }
      ]
      global_secondary_indexes = [
        {
          name               = "CustomerIdIndex"
          hash_key           = "CustomerId"
          range_key          = "TimeStamp"
          projection_type    = "ALL"
          read_capacity      = 5
          write_capacity     = 5
        }
      ]
      billing_mode = "PROVISIONED"
      read_capacity = 5
      write_capacity = 5
    }
  ]
}

# Security group for the ECS tasks that will run the CRM API
resource "aws_security_group" "crm_api_sg" {
  name        = "${var.project}-api-sg-${var.environment}"
  description = "Security group for the CRM API"
  vpc_id      = module.vpc.vpc_id

  # Allow all outbound traffic
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Allow inbound HTTP traffic
  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  # Allow inbound HTTPS traffic
  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.project}-api-sg-${var.environment}"
    Environment = var.environment
    Project     = var.project
  }
}

# ECS Cluster for the CRM services
resource "aws_ecs_cluster" "crm_cluster" {
  name = "${var.project}-cluster-${var.environment}"
  
  setting {
    name  = "containerInsights"
    value = "enabled"
  }
  
  tags = {
    Environment = var.environment
    Project     = var.project
  }
}

# Outputs
output "rds_endpoint" {
  description = "The endpoint of the RDS instance"
  value       = module.rds.endpoint
}

output "dynamodb_table_names" {
  description = "The names of the DynamoDB tables"
  value       = module.dynamodb.table_names
}

output "vpc_id" {
  description = "The ID of the VPC"
  value       = module.vpc.vpc_id
}

output "ecs_cluster_arn" {
  description = "The ARN of the ECS cluster"
  value       = aws_ecs_cluster.crm_cluster.arn
}

# Additional resources to be implemented:
# - ECR Repository for Docker images
# - ECS Task Definition
# - ECS Service
# - ALB
# - CloudWatch Logs
# - IAM Roles and Policies
