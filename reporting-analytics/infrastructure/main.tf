terraform {
  required_version = ">= 1.0.0"
  
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
  }
  
  backend "s3" {
    bucket = "dms-terraform-state"
    key    = "reporting-analytics/terraform.tfstate"
    region = "us-east-1"
  }
}

provider "aws" {
  region = var.aws_region
}

# Define common tags to be applied to all resources
locals {
  common_tags = {
    Environment = var.environment
    Project     = "DMS-Reporting"
    ManagedBy   = "Terraform"
    Owner       = "DevOps"
    Application = "Reporting & Analytics"
  }
}

# VPC Configuration
module "vpc" {
  source = "terraform-aws-modules/vpc/aws"
  version = "~> 3.0"

  name = "${var.environment}-reporting-vpc"
  cidr = var.vpc_cidr
  azs             = var.availability_zones
  private_subnets = var.private_subnets
  public_subnets  = var.public_subnets
  database_subnets = var.database_subnets

  enable_nat_gateway = true
  single_nat_gateway = var.environment != "production"
  
  create_database_subnet_group = true
  create_database_subnet_route_table = true
  
  tags = local.common_tags
}

# Security Groups
resource "aws_security_group" "app_sg" {
  name        = "${var.environment}-reporting-app-sg"
  description = "Security group for the reporting application"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(
    local.common_tags,
    {
      Name = "${var.environment}-reporting-app-sg"
    }
  )
}

resource "aws_security_group" "db_sg" {
  name        = "${var.environment}-reporting-db-sg"
  description = "Security group for the reporting database"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.app_sg.id]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(
    local.common_tags,
    {
      Name = "${var.environment}-reporting-db-sg"
    }
  )
}

# PostgreSQL Database
resource "aws_db_instance" "reporting_db" {
  identifier             = "${var.environment}-reporting-db"
  allocated_storage      = var.db_allocated_storage
  engine                 = "postgres"
  engine_version         = "14.6"
  instance_class         = var.db_instance_class
  db_name                = "reporting"
  username               = var.db_username
  password               = var.db_password
  skip_final_snapshot    = true
  multi_az               = var.environment == "production"
  backup_retention_period = var.environment == "production" ? 7 : 1
  vpc_security_group_ids = [aws_security_group.db_sg.id]
  db_subnet_group_name   = module.vpc.database_subnet_group_name
  storage_encrypted      = true
  
  tags = merge(
    local.common_tags,
    {
      Name = "${var.environment}-reporting-db"
    }
  )
}

# DynamoDB Tables
resource "aws_dynamodb_table" "report_cache" {
  name         = "${var.environment}-reporting-cache"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "ReportId"
  range_key    = "ExecutionId"

  attribute {
    name = "ReportId"
    type = "S"
  }

  attribute {
    name = "ExecutionId"
    type = "S"
  }
  
  ttl {
    attribute_name = "TimeToLive"
    enabled        = true
  }

  tags = local.common_tags
}

resource "aws_dynamodb_table" "analytics_data" {
  name         = "${var.environment}-reporting-analytics"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "PartitionKey"
  range_key    = "SortKey"

  attribute {
    name = "PartitionKey"
    type = "S"
  }

  attribute {
    name = "SortKey"
    type = "S"
  }

  tags = local.common_tags
}

# S3 Buckets
resource "aws_s3_bucket" "reports_bucket" {
  bucket = "${var.environment}-dms-reporting-exports"
  
  tags = local.common_tags
}

resource "aws_s3_bucket_lifecycle_configuration" "reports_lifecycle" {
  bucket = aws_s3_bucket.reports_bucket.id

  rule {
    id     = "expire-old-reports"
    status = "Enabled"

    expiration {
      days = 90
    }
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "reports_encryption" {
  bucket = aws_s3_bucket.reports_bucket.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_public_access_block" "reports_access" {
  bucket = aws_s3_bucket.reports_bucket.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

# SQS Queue for report processing
resource "aws_sqs_queue" "report_queue" {
  name                      = "${var.environment}-reporting-queue"
  delay_seconds             = 0
  max_message_size          = 262144
  message_retention_seconds = 86400
  receive_wait_time_seconds = 10
  visibility_timeout_seconds = 180
  
  tags = local.common_tags
}

resource "aws_sqs_queue_policy" "report_queue_policy" {
  queue_url = aws_sqs_queue.report_queue.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          AWS = "*"
        }
        Action = "sqs:*"
        Resource = aws_sqs_queue.report_queue.arn
        Condition = {
          ArnEquals = {
            "aws:SourceArn" = "arn:aws:iam::${data.aws_caller_identity.current.account_id}:role/reporting-service-role"
          }
        }
      }
    ]
  })
}

# Dead Letter Queue
resource "aws_sqs_queue" "report_dlq" {
  name                      = "${var.environment}-reporting-dlq"
  message_retention_seconds = 1209600  # 14 days
  
  tags = local.common_tags
}

resource "aws_sqs_queue_redrive_allow_policy" "report_queue_redrive" {
  queue_url = aws_sqs_queue.report_queue.id

  redrive_allow_policy = jsonencode({
    sourceQueueArns   = [aws_sqs_queue.report_dlq.arn]
    redrivePermission = "byQueue"
  })
}

# Cloudwatch for monitoring
resource "aws_cloudwatch_metric_alarm" "queue_depth_alarm" {
  alarm_name          = "${var.environment}-report-queue-depth"
  comparison_operator = "GreaterThanThreshold"
  evaluation_periods  = 1
  metric_name         = "ApproximateNumberOfMessagesVisible"
  namespace           = "AWS/SQS"
  period              = 300
  statistic           = "Average"
  threshold           = 100
  alarm_description   = "This alarm monitors the depth of the reporting queue"
  alarm_actions       = var.environment == "production" ? [var.sns_alert_topic_arn] : []

  dimensions = {
    QueueName = aws_sqs_queue.report_queue.name
  }
  
  tags = local.common_tags
}

# ECS for report processing
resource "aws_ecs_cluster" "reporting_cluster" {
  name = "${var.environment}-reporting-cluster"
  
  setting {
    name  = "containerInsights"
    value = "enabled"
  }
  
  tags = local.common_tags
}

# IAM Role for ECS tasks
resource "aws_iam_role" "ecs_task_role" {
  name = "${var.environment}-reporting-ecs-task-role"
  
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ecs-tasks.amazonaws.com"
        }
      }
    ]
  })
  
  tags = local.common_tags
}

# Policy for ECS task role
resource "aws_iam_policy" "ecs_task_policy" {
  name        = "${var.environment}-reporting-ecs-task-policy"
  description = "Policy for reporting ECS tasks"
  
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "sqs:ReceiveMessage",
          "sqs:DeleteMessage",
          "sqs:GetQueueAttributes"
        ]
        Effect   = "Allow"
        Resource = aws_sqs_queue.report_queue.arn
      },
      {
        Action = [
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:Query",
          "dynamodb:BatchWriteItem"
        ]
        Effect   = "Allow"
        Resource = [
          aws_dynamodb_table.report_cache.arn,
          aws_dynamodb_table.analytics_data.arn
        ]
      },
      {
        Action = [
          "s3:PutObject",
          "s3:GetObject",
          "s3:ListBucket"
        ]
        Effect   = "Allow"
        Resource = [
          aws_s3_bucket.reports_bucket.arn,
          "${aws_s3_bucket.reports_bucket.arn}/*"
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "ecs_task_policy_attachment" {
  role       = aws_iam_role.ecs_task_role.name
  policy_arn = aws_iam_policy.ecs_task_policy.arn
}

# Data source to get AWS account ID
data "aws_caller_identity" "current" {}

# Output variables
output "vpc_id" {
  value = module.vpc.vpc_id
}

output "database_endpoint" {
  value = aws_db_instance.reporting_db.endpoint
}

output "reports_bucket_name" {
  value = aws_s3_bucket.reports_bucket.id
}

output "report_queue_url" {
  value = aws_sqs_queue.report_queue.id
}

output "reporting_cluster_name" {
  value = aws_ecs_cluster.reporting_cluster.name
}
