provider "aws" {
  region = var.aws_region
}

terraform {
  backend "s3" {
    # Configure these values in a separate backend.tfvars file
    # bucket = "dms-terraform-state"
    # key    = "sales-management/terraform.tfstate"
    # region = "us-east-1"
  }

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
  }
}

# VPC and Networking
resource "aws_vpc" "sales_management_vpc" {
  cidr_block           = var.vpc_cidr
  enable_dns_hostnames = true
  enable_dns_support   = true

  tags = {
    Name        = "${var.environment}-sales-management-vpc"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# Public subnets
resource "aws_subnet" "public_subnets" {
  count                   = length(var.public_subnet_cidrs)
  vpc_id                  = aws_vpc.sales_management_vpc.id
  cidr_block              = var.public_subnet_cidrs[count.index]
  availability_zone       = var.availability_zones[count.index]
  map_public_ip_on_launch = true

  tags = {
    Name        = "${var.environment}-sales-management-public-subnet-${count.index + 1}"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# Private subnets
resource "aws_subnet" "private_subnets" {
  count             = length(var.private_subnet_cidrs)
  vpc_id            = aws_vpc.sales_management_vpc.id
  cidr_block        = var.private_subnet_cidrs[count.index]
  availability_zone = var.availability_zones[count.index]

  tags = {
    Name        = "${var.environment}-sales-management-private-subnet-${count.index + 1}"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# Internet Gateway for public subnets
resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.sales_management_vpc.id

  tags = {
    Name        = "${var.environment}-sales-management-igw"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# Route table for public subnets
resource "aws_route_table" "public_route_table" {
  vpc_id = aws_vpc.sales_management_vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }

  tags = {
    Name        = "${var.environment}-sales-management-public-rt"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# Route table association for public subnets
resource "aws_route_table_association" "public_subnet_association" {
  count          = length(var.public_subnet_cidrs)
  subnet_id      = aws_subnet.public_subnets[count.index].id
  route_table_id = aws_route_table.public_route_table.id
}

# NAT Gateway for private subnets
resource "aws_eip" "nat_eip" {
  domain = "vpc"

  tags = {
    Name        = "${var.environment}-sales-management-nat-eip"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

resource "aws_nat_gateway" "nat_gateway" {
  allocation_id = aws_eip.nat_eip.id
  subnet_id     = aws_subnet.public_subnets[0].id

  tags = {
    Name        = "${var.environment}-sales-management-nat-gateway"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }

  depends_on = [aws_internet_gateway.igw]
}

# Route table for private subnets
resource "aws_route_table" "private_route_table" {
  vpc_id = aws_vpc.sales_management_vpc.id

  route {
    cidr_block     = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.nat_gateway.id
  }

  tags = {
    Name        = "${var.environment}-sales-management-private-rt"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# Route table association for private subnets
resource "aws_route_table_association" "private_subnet_association" {
  count          = length(var.private_subnet_cidrs)
  subnet_id      = aws_subnet.private_subnets[count.index].id
  route_table_id = aws_route_table.private_route_table.id
}

# RDS PostgreSQL Database
resource "aws_security_group" "rds_sg" {
  name        = "${var.environment}-sales-management-rds-sg"
  description = "Security group for Sales Management PostgreSQL database"
  vpc_id      = aws_vpc.sales_management_vpc.id

  ingress {
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = concat(var.public_subnet_cidrs, var.private_subnet_cidrs)
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name        = "${var.environment}-sales-management-rds-sg"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

resource "aws_db_subnet_group" "rds_subnet_group" {
  name       = "${var.environment}-sales-management-rds-subnet-group"
  subnet_ids = aws_subnet.private_subnets[*].id

  tags = {
    Name        = "${var.environment}-sales-management-rds-subnet-group"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

resource "aws_db_instance" "postgres" {
  identifier             = "${var.environment}-sales-management-db"
  engine                 = "postgres"
  engine_version         = "14.5"
  instance_class         = var.db_instance_class
  allocated_storage      = var.db_allocated_storage
  storage_type           = "gp2"
  db_name                = "salesmanagement"
  username               = var.db_username
  password               = var.db_password
  port                   = 5432
  multi_az               = var.environment == "production" ? true : false
  db_subnet_group_name   = aws_db_subnet_group.rds_subnet_group.name
  vpc_security_group_ids = [aws_security_group.rds_sg.id]
  storage_encrypted      = true
  skip_final_snapshot    = var.environment != "production"
  backup_retention_period = var.environment == "production" ? 7 : 1
  deletion_protection    = var.environment == "production"

  tags = {
    Name        = "${var.environment}-sales-management-postgres"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# DynamoDB Table for document metadata
resource "aws_dynamodb_table" "sales_documents" {
  name           = "${var.environment}-sales-documents"
  billing_mode   = "PAY_PER_REQUEST"
  hash_key       = "id"
  range_key      = "dealId"

  attribute {
    name = "id"
    type = "S"
  }

  attribute {
    name = "dealId"
    type = "S"
  }

  attribute {
    name = "type"
    type = "S"
  }

  global_secondary_index {
    name               = "dealId-type-index"
    hash_key           = "dealId"
    range_key          = "type"
    projection_type    = "ALL"
  }

  tags = {
    Name        = "${var.environment}-sales-documents"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# S3 Bucket for document storage
resource "aws_s3_bucket" "document_storage" {
  bucket = "${var.environment}-dms-sales-documents"

  tags = {
    Name        = "${var.environment}-dms-sales-documents"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

resource "aws_s3_bucket_ownership_controls" "document_storage_ownership" {
  bucket = aws_s3_bucket.document_storage.id

  rule {
    object_ownership = "BucketOwnerPreferred"
  }
}

resource "aws_s3_bucket_public_access_block" "document_storage_public_access" {
  bucket = aws_s3_bucket.document_storage.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_server_side_encryption_configuration" "document_storage_encryption" {
  bucket = aws_s3_bucket.document_storage.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
  }
}

resource "aws_s3_bucket_lifecycle_configuration" "document_storage_lifecycle" {
  bucket = aws_s3_bucket.document_storage.id

  rule {
    id      = "archive-documents"
    status  = "Enabled"

    transition {
      days          = 90
      storage_class = "STANDARD_IA"
    }

    transition {
      days          = 365
      storage_class = "GLACIER"
    }
  }
}

# ECR Repository for the API container
resource "aws_ecr_repository" "api_repository" {
  name                 = "${var.environment}-sales-management-api"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name        = "${var.environment}-sales-management-api"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# ECS Cluster
resource "aws_ecs_cluster" "sales_management_cluster" {
  name = "${var.environment}-sales-management-cluster"

  setting {
    name  = "containerInsights"
    value = "enabled"
  }

  tags = {
    Name        = "${var.environment}-sales-management-cluster"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# ECS Task Execution Role
resource "aws_iam_role" "ecs_task_execution_role" {
  name = "${var.environment}-sales-management-ecs-execution-role"

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

  tags = {
    Name        = "${var.environment}-sales-management-ecs-execution-role"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

resource "aws_iam_role_policy_attachment" "ecs_task_execution_role_policy" {
  role       = aws_iam_role.ecs_task_execution_role.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# Task Role for application permissions
resource "aws_iam_role" "ecs_task_role" {
  name = "${var.environment}-sales-management-ecs-task-role"

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

  tags = {
    Name        = "${var.environment}-sales-management-ecs-task-role"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# Policy for S3 document access
resource "aws_iam_policy" "s3_document_access" {
  name        = "${var.environment}-sales-management-s3-access"
  description = "Allow access to Sales Management document bucket"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:ListBucket",
          "s3:DeleteObject"
        ]
        Effect   = "Allow"
        Resource = [
          aws_s3_bucket.document_storage.arn,
          "${aws_s3_bucket.document_storage.arn}/*"
        ]
      }
    ]
  })
}

# Policy for DynamoDB document metadata access
resource "aws_iam_policy" "dynamodb_document_access" {
  name        = "${var.environment}-sales-management-dynamodb-access"
  description = "Allow access to Sales Management document metadata table"

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = [
          "dynamodb:GetItem",
          "dynamodb:PutItem",
          "dynamodb:UpdateItem",
          "dynamodb:DeleteItem",
          "dynamodb:Query",
          "dynamodb:Scan"
        ]
        Effect   = "Allow"
        Resource = [
          aws_dynamodb_table.sales_documents.arn,
          "${aws_dynamodb_table.sales_documents.arn}/index/*"
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "ecs_task_s3_policy" {
  role       = aws_iam_role.ecs_task_role.name
  policy_arn = aws_iam_policy.s3_document_access.arn
}

resource "aws_iam_role_policy_attachment" "ecs_task_dynamodb_policy" {
  role       = aws_iam_role.ecs_task_role.name
  policy_arn = aws_iam_policy.dynamodb_document_access.arn
}

# Security Group for the ECS Task
resource "aws_security_group" "ecs_task_sg" {
  name        = "${var.environment}-sales-management-ecs-task-sg"
  description = "Security group for Sales Management ECS tasks"
  vpc_id      = aws_vpc.sales_management_vpc.id

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
    Name        = "${var.environment}-sales-management-ecs-task-sg"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

# CloudFront distribution for the web client
resource "aws_cloudfront_distribution" "web_distribution" {
  origin {
    domain_name = aws_s3_bucket.web_client.bucket_regional_domain_name
    origin_id   = "S3-${aws_s3_bucket.web_client.id}"
    
    s3_origin_config {
      origin_access_identity = aws_cloudfront_origin_access_identity.web_oai.cloudfront_access_identity_path
    }
  }

  enabled             = true
  is_ipv6_enabled     = true
  default_root_object = "index.html"
  price_class         = "PriceClass_100"
  
  # Custom error response for SPA routing
  custom_error_response {
    error_code         = 403
    response_code      = 200
    response_page_path = "/index.html"
  }
  
  custom_error_response {
    error_code         = 404
    response_code      = 200
    response_page_path = "/index.html"
  }

  default_cache_behavior {
    allowed_methods  = ["GET", "HEAD", "OPTIONS"]
    cached_methods   = ["GET", "HEAD"]
    target_origin_id = "S3-${aws_s3_bucket.web_client.id}"

    forwarded_values {
      query_string = false
      cookies {
        forward = "none"
      }
    }

    viewer_protocol_policy = "redirect-to-https"
    min_ttl                = 0
    default_ttl            = 3600
    max_ttl                = 86400
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    cloudfront_default_certificate = true
  }

  tags = {
    Name        = "${var.environment}-sales-management-web-distribution"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

resource "aws_cloudfront_origin_access_identity" "web_oai" {
  comment = "OAI for Sales Management Web Client"
}

# S3 Bucket for the web client
resource "aws_s3_bucket" "web_client" {
  bucket = "${var.environment}-dms-sales-management-web"

  tags = {
    Name        = "${var.environment}-dms-sales-management-web"
    Environment = var.environment
    Project     = "DMS-SalesManagement"
  }
}

resource "aws_s3_bucket_ownership_controls" "web_client_ownership" {
  bucket = aws_s3_bucket.web_client.id

  rule {
    object_ownership = "BucketOwnerPreferred"
  }
}

resource "aws_s3_bucket_public_access_block" "web_client_public_access" {
  bucket = aws_s3_bucket.web_client.id

  block_public_acls       = true
  block_public_policy     = true
  ignore_public_acls      = true
  restrict_public_buckets = true
}

resource "aws_s3_bucket_policy" "web_client_policy" {
  bucket = aws_s3_bucket.web_client.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action   = "s3:GetObject"
        Effect   = "Allow"
        Resource = "${aws_s3_bucket.web_client.arn}/*"
        Principal = {
          AWS = aws_cloudfront_origin_access_identity.web_oai.iam_arn
        }
      }
    ]
  })
}

# CloudFront will serve index.html for SPA routing, so we don't need website configuration
