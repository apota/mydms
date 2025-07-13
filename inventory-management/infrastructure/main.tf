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
    key    = "inventory-management/terraform.tfstate"
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
    Project     = "DMS-Inventory"
    ManagedBy   = "Terraform"
    Owner       = "DevOps"
    Application = "Inventory Management"
  }
}

# VPC Configuration
module "vpc" {
  source = "terraform-aws-modules/vpc/aws"
  version = "~> 3.0"

  name = "${var.environment}-inventory-vpc"
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

# Database - PostgreSQL RDS
module "postgres" {
  source  = "terraform-aws-modules/rds/aws"
  version = "~> 5.0"

  identifier = "${var.environment}-inventory-db"

  engine               = "postgres"
  engine_version       = "15.3"
  family               = "postgres15"
  major_engine_version = "15"
  instance_class       = var.db_instance_class

  allocated_storage     = var.db_allocated_storage
  max_allocated_storage = var.db_max_allocated_storage
  db_name  = "inventory"
  username = var.db_username
  password = var.db_password
  port     = 5432

  multi_az               = var.environment == "production"
  db_subnet_group_name   = module.vpc.database_subnet_group
  vpc_security_group_ids = [aws_security_group.database.id]
  
  maintenance_window              = "Mon:00:00-Mon:03:00"
  backup_window                   = "03:00-06:00"
  enabled_cloudwatch_logs_exports = ["postgresql", "upgrade"]
  
  backup_retention_period = var.environment == "production" ? 30 : 7
  skip_final_snapshot     = var.environment != "production"
  deletion_protection     = var.environment == "production"

  parameters = [
    {
      name  = "autovacuum"
      value = "1"
    },
    {
      name  = "client_encoding"
      value = "utf8"
    }
  ]

  tags = merge(
    local.common_tags,
    {
      Purpose = "Inventory management relational database"
    }
  )
}

# DynamoDB for NoSQL data (vehicle images, documents, etc.)
resource "aws_dynamodb_table" "vehicle_media" {
  name         = "${var.environment}-vehicle-media"
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "VehicleId"
  range_key    = "MediaId"

  attribute {
    name = "VehicleId"
    type = "S"
  }

  attribute {
    name = "MediaId"
    type = "S"
  }

  attribute {
    name = "MediaType"
    type = "S"
  }

  global_secondary_index {
    name               = "MediaTypeIndex"
    hash_key           = "MediaType"
    range_key          = "VehicleId"
    projection_type    = "ALL"
  }

  point_in_time_recovery {
    enabled = var.environment == "production"
  }

  tags = merge(
    local.common_tags,
    {
      Purpose = "Vehicle media storage metadata"
    },
  )
}

# S3 bucket for vehicle images and documents
resource "aws_s3_bucket" "vehicle_media" {
  bucket = "${var.environment}-dms-vehicle-media"

  tags = merge(
    local.common_tags,
    {
      Purpose = "Vehicle media storage"
    },
  )
}

resource "aws_s3_bucket_lifecycle_configuration" "vehicle_media_lifecycle" {
  bucket = aws_s3_bucket.vehicle_media.id

  rule {
    id     = "transition-to-ia"
    status = "Enabled"

    transition {
      days          = 30
      storage_class = "STANDARD_IA"
    }
  }
}

resource "aws_s3_bucket_cors_configuration" "vehicle_media_cors" {
  bucket = aws_s3_bucket.vehicle_media.id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["GET", "PUT", "POST"]
    allowed_origins = var.allowed_origins
    expose_headers  = ["ETag"]
    max_age_seconds = 3000
  }
}

# ECS Cluster for API and background processing
module "ecs" {
  source = "terraform-aws-modules/ecs/aws"
  
  cluster_name = "${var.environment}-inventory-cluster"
  
  cluster_configuration = {
    execute_command_configuration = {
      logging = "OVERRIDE"
      log_configuration = {
        cloud_watch_log_group_name = "/ecs/inventory-management"
      }
    }
  }
  
  tags = local.common_tags
}

# API Service
resource "aws_ecs_task_definition" "api" {
  family                   = "${var.environment}-inventory-api"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = var.api_task_cpu
  memory                   = var.api_task_memory
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn
  task_role_arn            = aws_iam_role.ecs_task.arn

  container_definitions = jsonencode([
    {
      name      = "inventory-api"
      image     = "${var.ecr_repository_url}:${var.api_image_tag}"
      essential = true
      
      portMappings = [
        {
          containerPort = 80
          hostPort      = 80
          protocol      = "tcp"
        }
      ]
      
      environment = [
        { name = "ASPNETCORE_ENVIRONMENT", value = var.dotnet_environment },
        { name = "ConnectionStrings__DefaultConnection", value = "Server=${module.postgres.db_instance_address};Port=5432;Database=inventory;User Id=${var.db_username};Password=${var.db_password};" },
        { name = "AWS__Region", value = var.aws_region },
        { name = "AWS__DynamoDb__VehicleMediaTable", value = aws_dynamodb_table.vehicle_media.name },
        { name = "AWS__S3__VehicleMediaBucket", value = aws_s3_bucket.vehicle_media.bucket }
      ]
      
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = "/ecs/inventory-api"
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "inventory-api"
        }
      }
    }
  ])

  tags = local.common_tags
}

resource "aws_ecs_service" "api" {
  name            = "inventory-api"
  cluster         = module.ecs.cluster_id
  task_definition = aws_ecs_task_definition.api.arn
  desired_count   = var.api_service_count
  launch_type     = "FARGATE"

  network_configuration {
    subnets         = module.vpc.private_subnets
    security_groups = [aws_security_group.api.id]
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.api.arn
    container_name   = "inventory-api"
    container_port   = 80
  }

  depends_on = [aws_lb_listener.api]

  tags = local.common_tags
}

# Background processing service
resource "aws_ecs_task_definition" "worker" {
  family                   = "${var.environment}-inventory-worker"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = var.worker_task_cpu
  memory                   = var.worker_task_memory
  execution_role_arn       = aws_iam_role.ecs_task_execution.arn
  task_role_arn            = aws_iam_role.ecs_task.arn

  container_definitions = jsonencode([
    {
      name      = "inventory-worker"
      image     = "${var.ecr_repository_url}:${var.worker_image_tag}"
      essential = true
      
      environment = [
        { name = "DOTNET_ENVIRONMENT", value = var.dotnet_environment },
        { name = "ConnectionStrings__DefaultConnection", value = "Server=${module.postgres.db_instance_address};Port=5432;Database=inventory;User Id=${var.db_username};Password=${var.db_password};" },
        { name = "AWS__Region", value = var.aws_region },
        { name = "AWS__DynamoDb__VehicleMediaTable", value = aws_dynamodb_table.vehicle_media.name },
        { name = "AWS__S3__VehicleMediaBucket", value = aws_s3_bucket.vehicle_media.bucket },
        { name = "AWS__SQS__InventoryQueueUrl", value = aws_sqs_queue.inventory_queue.url }
      ]
      
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = "/ecs/inventory-worker"
          "awslogs-region"        = var.aws_region
          "awslogs-stream-prefix" = "inventory-worker"
        }
      }
    }
  ])

  tags = local.common_tags
}

resource "aws_ecs_service" "worker" {
  name            = "inventory-worker"
  cluster         = module.ecs.cluster_id
  task_definition = aws_ecs_task_definition.worker.arn
  desired_count   = var.worker_service_count
  launch_type     = "FARGATE"

  network_configuration {
    subnets         = module.vpc.private_subnets
    security_groups = [aws_security_group.worker.id]
  }

  tags = local.common_tags
}

# SQS Queue for background processing
resource "aws_sqs_queue" "inventory_queue" {
  name                      = "${var.environment}-inventory-queue"
  delay_seconds             = 0
  max_message_size          = 262144  # 256 KiB
  message_retention_seconds = 345600  # 4 days
  visibility_timeout_seconds = 300    # 5 minutes
  
  redrive_policy = jsonencode({
    deadLetterTargetArn = aws_sqs_queue.inventory_dlq.arn
    maxReceiveCount     = 5
  })

  tags = local.common_tags
}

resource "aws_sqs_queue" "inventory_dlq" {
  name                      = "${var.environment}-inventory-dlq"
  message_retention_seconds = 1209600  # 14 days

  tags = local.common_tags
}

# Load Balancer for API
resource "aws_lb" "api" {
  name               = "${var.environment}-inventory-api-lb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = module.vpc.public_subnets

  tags = local.common_tags
}

resource "aws_lb_target_group" "api" {
  name        = "${var.environment}-inventory-api-tg"
  port        = 80
  protocol    = "HTTP"
  vpc_id      = module.vpc.vpc_id
  target_type = "ip"

  health_check {
    enabled             = true
    interval            = 30
    path                = "/health"
    port                = "traffic-port"
    healthy_threshold   = 3
    unhealthy_threshold = 3
    timeout             = 5
    matcher             = "200"
  }

  tags = local.common_tags
}

resource "aws_lb_listener" "api" {
  load_balancer_arn = aws_lb.api.arn
  port              = 443
  protocol          = "HTTPS"
  ssl_policy        = "ELBSecurityPolicy-2016-08"
  certificate_arn   = var.certificate_arn

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api.arn
  }
}

resource "aws_lb_listener" "api_redirect" {
  load_balancer_arn = aws_lb.api.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type = "redirect"

    redirect {
      port        = "443"
      protocol    = "HTTPS"
      status_code = "HTTP_301"
    }
  }
}

# Security Groups
resource "aws_security_group" "alb" {
  name        = "${var.environment}-inventory-alb-sg"
  description = "Security group for Inventory API ALB"
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
      Name = "${var.environment}-inventory-alb-sg"
    },
  )
}

resource "aws_security_group" "api" {
  name        = "${var.environment}-inventory-api-sg"
  description = "Security group for Inventory API"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port       = 80
    to_port         = 80
    protocol        = "tcp"
    security_groups = [aws_security_group.alb.id]
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
      Name = "${var.environment}-inventory-api-sg"
    },
  )
}

resource "aws_security_group" "worker" {
  name        = "${var.environment}-inventory-worker-sg"
  description = "Security group for Inventory background workers"
  vpc_id      = module.vpc.vpc_id

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(
    local.common_tags,
    {
      Name = "${var.environment}-inventory-worker-sg"
    },
  )
}

resource "aws_security_group" "database" {
  name        = "${var.environment}-inventory-db-sg"
  description = "Security group for Inventory RDS database"
  vpc_id      = module.vpc.vpc_id

  ingress {
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.api.id, aws_security_group.worker.id]
  }

  tags = merge(
    local.common_tags,
    {
      Name = "${var.environment}-inventory-db-sg"
    },
  )
}

# IAM Roles
resource "aws_iam_role" "ecs_task_execution" {
  name = "${var.environment}-inventory-task-execution-role"

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

resource "aws_iam_role_policy_attachment" "ecs_task_execution" {
  role       = aws_iam_role.ecs_task_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role" "ecs_task" {
  name = "${var.environment}-inventory-task-role"

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

resource "aws_iam_policy" "task_permissions" {
  name        = "${var.environment}-inventory-task-permissions"
  description = "Permissions for inventory management ECS tasks"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "dynamodb:BatchGetItem",
          "dynamodb:GetItem",
          "dynamodb:Query",
          "dynamodb:Scan",
          "dynamodb:BatchWriteItem",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem"
        ]
        Effect   = "Allow"
        Resource = aws_dynamodb_table.vehicle_media.arn
      },
      {
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:DeleteObject",
          "s3:ListBucket"
        ]
        Effect   = "Allow"
        Resource = [
          aws_s3_bucket.vehicle_media.arn,
          "${aws_s3_bucket.vehicle_media.arn}/*"
        ]
      },
      {
        Action = [
          "sqs:SendMessage",
          "sqs:ReceiveMessage",
          "sqs:DeleteMessage",
          "sqs:GetQueueAttributes"
        ]
        Effect   = "Allow"
        Resource = [
          aws_sqs_queue.inventory_queue.arn,
          aws_sqs_queue.inventory_dlq.arn
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "task_permissions" {
  role       = aws_iam_role.ecs_task.name
  policy_arn = aws_iam_policy.task_permissions.arn
}

# CloudWatch Logs
resource "aws_cloudwatch_log_group" "ecs_api" {
  name              = "/ecs/inventory-api"
  retention_in_days = var.log_retention_days

  tags = local.common_tags
}

resource "aws_cloudwatch_log_group" "ecs_worker" {
  name              = "/ecs/inventory-worker"
  retention_in_days = var.log_retention_days

  tags = local.common_tags
}
# AWS Specific outputs
output "api_url" {
  value = "https://${aws_lb.api.dns_name}"
}

output "s3_bucket_url" {
  value = "https://${aws_s3_bucket.vehicle_media.bucket_regional_domain_name}"
}

output "dynamodb_table_name" {
  value = aws_dynamodb_table.vehicle_media.name
}

output "sqs_queue_url" {
  value = aws_sqs_queue.inventory_queue.url
}

output "rds_endpoint" {
  value = module.postgres.db_instance_address
}
