provider "aws" {
  region = var.region
}

locals {
  app_name   = "dms-parts-management"
  env_prefix = "${var.environment}-${local.app_name}"
  common_tags = {
    Environment = var.environment
    Project     = "DMS"
    Module      = "PartsManagement"
    ManagedBy   = "Terraform"
  }
}

# ======= DATABASE RESOURCES =======

resource "aws_db_subnet_group" "main" {
  name        = "${local.env_prefix}-db-subnet-group"
  description = "DB subnet group for ${local.app_name}"
  subnet_ids  = var.private_subnet_ids

  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-db-subnet-group"
    }
  )
}

resource "aws_security_group" "database" {
  name        = "${local.env_prefix}-db-sg"
  description = "Security group for ${local.app_name} database"
  vpc_id      = var.vpc_id

  ingress {
    description     = "PostgreSQL from ECS Tasks"
    from_port       = 5432
    to_port         = 5432
    protocol        = "tcp"
    security_groups = [aws_security_group.ecs_tasks.id]
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
      Name = "${local.env_prefix}-db-sg"
    }
  )
}

resource "aws_db_instance" "postgres" {
  identifier             = "${local.env_prefix}-db"
  allocated_storage      = 20
  storage_type           = "gp2"
  engine                 = "postgres"
  engine_version         = "14"
  instance_class         = var.db_instance_class
  db_name                = "DMS_PartsManagement"
  username               = var.db_username
  password               = var.db_password
  parameter_group_name   = "default.postgres14"
  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [aws_security_group.database.id]
  skip_final_snapshot    = true
  multi_az               = var.environment == "prod" ? true : false
  backup_retention_period = var.environment == "prod" ? 7 : 1
  
  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-db"
    }
  )
}

# ======= DYNAMODB RESOURCES =======

resource "aws_dynamodb_table" "part_catalog" {
  name           = "${local.env_prefix}-part-catalog"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "PartNumber"
  
  attribute {
    name = "PartNumber"
    type = "S"
  }
  
  attribute {
    name = "ManufacturerName"
    type = "S"
  }
  
  global_secondary_index {
    name               = "ManufacturerIndex"
    hash_key           = "ManufacturerName"
    projection_type    = "ALL"
  }
  
  point_in_time_recovery {
    enabled = var.environment == "prod" ? true : false
  }
  
  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-part-catalog"
    }
  )
}

# ======= ECR REPOSITORY =======

resource "aws_ecr_repository" "api" {
  name                 = "${local.env_prefix}-api"
  image_tag_mutability = "MUTABLE"
  
  image_scanning_configuration {
    scan_on_push = true
  }
  
  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-api"
    }
  )
}

# ======= ECS RESOURCES =======

resource "aws_security_group" "ecs_tasks" {
  name        = "${local.env_prefix}-ecs-sg"
  description = "Security group for ${local.app_name} ECS tasks"
  vpc_id      = var.vpc_id

  ingress {
    description      = "HTTP from ALB"
    from_port        = 80
    to_port          = 80
    protocol         = "tcp"
    security_groups  = [aws_security_group.alb.id]
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
      Name = "${local.env_prefix}-ecs-sg"
    }
  )
}

resource "aws_security_group" "alb" {
  name        = "${local.env_prefix}-alb-sg"
  description = "Security group for ${local.app_name} ALB"
  vpc_id      = var.vpc_id

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
      Name = "${local.env_prefix}-alb-sg"
    }
  )
}

resource "aws_lb" "main" {
  name               = "${local.env_prefix}-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.alb.id]
  subnets            = var.public_subnet_ids

  enable_deletion_protection = var.environment == "prod" ? true : false

  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-alb"
    }
  )
}

resource "aws_lb_target_group" "api" {
  name        = "${local.env_prefix}-tg"
  port        = 80
  protocol    = "HTTP"
  vpc_id      = var.vpc_id
  target_type = "ip"

  health_check {
    path                = "/health"
    interval            = 30
    timeout             = 5
    healthy_threshold   = 3
    unhealthy_threshold = 3
    matcher             = "200"
  }

  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-tg"
    }
  )
}

resource "aws_lb_listener" "http" {
  load_balancer_arn = aws_lb.main.arn
  port              = "80"
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.api.arn
  }

  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-http-listener"
    }
  )
}

resource "aws_ecs_cluster" "main" {
  name = "${local.env_prefix}-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-cluster"
    }
  )
}

resource "aws_ecs_task_definition" "api" {
  family                   = "${local.env_prefix}-api"
  requires_compatibilities = ["FARGATE"]
  network_mode             = "awsvpc"
  cpu                      = 256
  memory                   = 512
  execution_role_arn       = aws_iam_role.ecs_execution.arn
  task_role_arn            = aws_iam_role.ecs_task.arn

  container_definitions = jsonencode([
    {
      name      = "${local.env_prefix}-api"
      image     = "${aws_ecr_repository.api.repository_url}:latest"
      essential = true
      
      portMappings = [
        {
          containerPort = 80
          hostPort      = 80
          protocol      = "tcp"
        }
      ]
      
      environment = [
        {
          name  = "ASPNETCORE_ENVIRONMENT"
          value = var.environment
        },
        {
          name  = "ConnectionStrings__DefaultConnection"
          value = "Host=${aws_db_instance.postgres.address};Database=DMS_PartsManagement;Username=${var.db_username};Password=${var.db_password}"
        }
      ]
      
      logConfiguration = {
        logDriver = "awslogs"
        options = {
          "awslogs-group"         = "/ecs/${local.env_prefix}-api"
          "awslogs-region"        = var.region
          "awslogs-stream-prefix" = "ecs"
          "awslogs-create-group"  = "true"
        }
      }
    }
  ])

  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-api-task"
    }
  )
}

resource "aws_ecs_service" "api" {
  name            = "${local.env_prefix}-api-service"
  cluster         = aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.api.arn
  desired_count   = var.api_instance_count
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = var.private_subnet_ids
    security_groups  = [aws_security_group.ecs_tasks.id]
    assign_public_ip = false
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.api.arn
    container_name   = "${local.env_prefix}-api"
    container_port   = 80
  }

  depends_on = [
    aws_lb_listener.http
  ]

  tags = merge(
    local.common_tags,
    {
      Name = "${local.env_prefix}-api-service"
    }
  )
}

# ======= IAM ROLES =======

resource "aws_iam_role" "ecs_execution" {
  name = "${local.env_prefix}-ecs-execution-role"

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

resource "aws_iam_role_policy_attachment" "ecs_execution" {
  role       = aws_iam_role.ecs_execution.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

resource "aws_iam_role" "ecs_task" {
  name = "${local.env_prefix}-ecs-task-role"

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

resource "aws_iam_policy" "dynamodb_access" {
  name        = "${local.env_prefix}-dynamodb-access"
  description = "Policy for DMS Parts Management DynamoDB access"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "dynamodb:BatchGetItem",
          "dynamodb:BatchWriteItem",
          "dynamodb:PutItem",
          "dynamodb:GetItem",
          "dynamodb:Scan",
          "dynamodb:Query",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem"
        ]
        Resource = [
          aws_dynamodb_table.part_catalog.arn,
          "${aws_dynamodb_table.part_catalog.arn}/index/*"
        ]
      }
    ]
  })

  tags = local.common_tags
}

resource "aws_iam_role_policy_attachment" "dynamodb_access" {
  role       = aws_iam_role.ecs_task.name
  policy_arn = aws_iam_policy.dynamodb_access.arn
}
