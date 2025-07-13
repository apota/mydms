provider "aws" {
  region = var.aws_region
}

# S3 bucket for any static content
resource "aws_s3_bucket" "service_management_static" {
  bucket = "${var.environment}-${var.project_name}-static"

  tags = {
    Name        = "${var.environment}-${var.project_name}-static"
    Environment = var.environment
  }
}

# DynamoDB table for service job history - this is the NoSQL use case
resource "aws_dynamodb_table" "service_job_history" {
  name           = "${var.environment}-${var.project_name}-job-history"
  billing_mode   = "PROVISIONED"
  read_capacity  = var.dynamodb_read_capacity
  write_capacity = var.dynamodb_write_capacity
  hash_key       = "ServiceJobId"
  range_key      = "Timestamp"

  attribute {
    name = "ServiceJobId"
    type = "S"
  }

  attribute {
    name = "Timestamp"
    type = "S"
  }

  attribute {
    name = "TechnicianId"
    type = "S"
  }

  global_secondary_index {
    name               = "TechnicianIndex"
    hash_key           = "TechnicianId"
    range_key          = "Timestamp"
    write_capacity     = var.dynamodb_gsi_write_capacity
    read_capacity      = var.dynamodb_gsi_read_capacity
    projection_type    = "INCLUDE"
    non_key_attributes = ["Status", "Description", "Duration"]
  }

  tags = {
    Name        = "${var.environment}-${var.project_name}-job-history"
    Environment = var.environment
  }
}

# RDS PostgreSQL instance for relational data - this is the RDBMS use case
resource "aws_db_instance" "service_management_db" {
  allocated_storage    = var.rds_allocated_storage
  storage_type         = "gp2"
  engine               = "postgres"
  engine_version       = var.rds_engine_version
  instance_class       = var.rds_instance_class
  db_name              = var.environment == "prod" ? "service_management" : "service_management_${var.environment}"
  username             = var.db_username
  password             = var.db_password
  parameter_group_name = "default.postgres13"
  skip_final_snapshot  = true
  multi_az             = var.environment == "prod" ? true : false
  
  vpc_security_group_ids = [aws_security_group.rds.id]
  db_subnet_group_name   = aws_db_subnet_group.service_management.name

  tags = {
    Name        = "${var.environment}-${var.project_name}-db"
    Environment = var.environment
  }
}

# Subnet group for RDS
resource "aws_db_subnet_group" "service_management" {
  name       = "${var.environment}-${var.project_name}-subnet-group"
  subnet_ids = var.subnet_ids

  tags = {
    Name        = "${var.environment}-${var.project_name}-subnet-group"
    Environment = var.environment
  }
}

# Security group for RDS
resource "aws_security_group" "rds" {
  name        = "${var.environment}-${var.project_name}-rds-sg"
  description = "Allow database traffic"
  vpc_id      = var.vpc_id

  ingress {
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = var.db_access_cidr_blocks
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.environment}-${var.project_name}-rds-sg"
    Environment = var.environment
  }
}

# ECS Cluster for API and background processing
resource "aws_ecs_cluster" "service_management" {
  name = "${var.environment}-${var.project_name}-cluster"
  
  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = {
    Name        = "${var.environment}-${var.project_name}-cluster"
    Environment = var.environment
  }
}

# IAM role for ECS tasks
resource "aws_iam_role" "ecs_task_execution_role" {
  name = "${var.environment}-${var.project_name}-execution-role"
  
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
}

# Attach the AWS managed policy for ECS task execution
resource "aws_iam_role_policy_attachment" "ecs_task_execution_role_policy" {
  role       = aws_iam_role.ecs_task_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# Security group for ECS services
resource "aws_security_group" "ecs_service" {
  name        = "${var.environment}-${var.project_name}-ecs-service-sg"
  description = "Allow traffic to ECS service"
  vpc_id      = var.vpc_id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.environment}-${var.project_name}-ecs-service-sg"
    Environment = var.environment
  }
}

# CloudWatch log group
resource "aws_cloudwatch_log_group" "service_management" {
  name              = "/ecs/${var.environment}-${var.project_name}"
  retention_in_days = var.logs_retention_days

  tags = {
    Name        = "${var.environment}-${var.project_name}-logs"
    Environment = var.environment
  }
}

# API Task Definition
resource "aws_ecs_task_definition" "service_management_api" {
  family                   = "${var.environment}-${var.project_name}-api"
  execution_role_arn       = aws_iam_role.ecs_task_execution_role.arn
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = 256
  memory                   = 512
  
  container_definitions = jsonencode([{
    name  = "${var.environment}-${var.project_name}-api-container"
    image = "${var.ecr_repository_url}:latest"
    
    essential = true
    
    portMappings = [{
      containerPort = 80
      hostPort      = 80
      protocol      = "tcp"
    }]
    
    environment = [
      {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment
      },
      {
        name  = "ConnectionStrings__ServiceManagementDb"
        value = "Host=${aws_db_instance.service_management_db.address};Database=${aws_db_instance.service_management_db.db_name};Username=${var.db_username};Password=${var.db_password}"
      },
      {
        name  = "DynamoDb__TableName"
        value = aws_dynamodb_table.service_job_history.name
      },
      {
        name  = "DynamoDb__Region"
        value = var.aws_region
      },
      {
        name  = "IntegrationUrls__CrmApi"
        value = var.crm_api_url
      },
      {
        name  = "IntegrationUrls__InventoryApi"
        value = var.inventory_api_url
      },
      {
        name  = "IntegrationUrls__PartsApi"
        value = var.parts_api_url
      },
      {
        name  = "IntegrationUrls__FinancialApi"
        value = var.financial_api_url
      },
      {
        name  = "IntegrationUrls__NotificationApi"
        value = var.notification_api_url
      }
    ]
    
    logConfiguration = {
      logDriver = "awslogs"
      options = {
        "awslogs-group"         = aws_cloudwatch_log_group.service_management.name
        "awslogs-region"        = var.aws_region
        "awslogs-stream-prefix" = "api"
      }
    }
  }])

  tags = {
    Name        = "${var.environment}-${var.project_name}-api-task"
    Environment = var.environment
  }
}

# API Service
resource "aws_ecs_service" "service_management_api" {
  name            = "${var.environment}-${var.project_name}-api-service"
  cluster         = aws_ecs_cluster.service_management.id
  task_definition = aws_ecs_task_definition.service_management_api.arn
  desired_count   = var.api_task_count
  launch_type     = "FARGATE"
  
  network_configuration {
    subnets          = var.subnet_ids
    security_groups  = [aws_security_group.ecs_service.id]
    assign_public_ip = true
  }
  
  load_balancer {
    target_group_arn = aws_lb_target_group.service_management_api.arn
    container_name   = "${var.environment}-${var.project_name}-api-container"
    container_port   = 80
  }

  depends_on = [aws_lb_listener.http]
  
  tags = {
    Name        = "${var.environment}-${var.project_name}-api-service"
    Environment = var.environment
  }
}

# Application Load Balancer
resource "aws_lb" "service_management" {
  name               = "${var.environment}-${var.project_name}-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = var.subnet_ids

  enable_deletion_protection = var.environment == "prod" ? true : false
  
  tags = {
    Name        = "${var.environment}-${var.project_name}-alb"
    Environment = var.environment
  }
}

# ALB Target Group
resource "aws_lb_target_group" "service_management_api" {
  name        = "${var.environment}-${var.project_name}-tg"
  port        = 80
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "ip"
  
  health_check {
    enabled             = true
    interval            = 30
    path                = "/health"
    port                = "traffic-port"
    healthy_threshold   = 3
    unhealthy_threshold = 3
    timeout             = 5
    protocol            = "HTTP"
    matcher             = "200"
  }
  
  tags = {
    Name        = "${var.environment}-${var.project_name}-tg"
    Environment = var.environment
  }
}

# ALB Listener
resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.service_management.arn
  port              = 80
  protocol          = "HTTP"
  
  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.service_management_api.arn
  }
}

# Security group for ALB
resource "aws_security_group" "alb" {
  name        = "${var.environment}-${var.project_name}-alb-sg"
  description = "Allow HTTP inbound traffic"
  vpc_id      = var.vpc_id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
  
  tags = {
    Name        = "${var.environment}-${var.project_name}-alb-sg"
    Environment = var.environment
  }
}
