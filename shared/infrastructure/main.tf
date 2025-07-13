terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 4.0"
    }
  }
  backend "s3" {
    bucket = "mydms-terraform-state"
    key    = "global/s3/terraform.tfstate"
    region = "us-west-2"

    dynamodb_table = "terraform-locks"
    encrypt        = true
  }
}

provider "aws" {
  region = var.aws_region
}

# VPC and Networking
module "vpc" {
  source = "./modules/vpc"
  
  environment       = var.environment
  vpc_cidr          = var.vpc_cidr
  availability_zones = var.availability_zones
  public_subnet_cidrs = var.public_subnet_cidrs
  private_subnet_cidrs = var.private_subnet_cidrs
  database_subnet_cidrs = var.database_subnet_cidrs
}

# RDS PostgreSQL instances
module "rds" {
  source = "./modules/rds"
  
  environment          = var.environment
  vpc_id               = module.vpc.vpc_id
  database_subnet_ids  = module.vpc.database_subnet_ids
  master_username      = var.db_master_username
  master_password      = var.db_master_password
  db_name              = var.db_name
  instance_class       = var.db_instance_class
  allocated_storage    = var.db_allocated_storage
  subnet_group_name    = "${var.environment}-rds-subnet-group"
  parameter_group_name = "${var.environment}-rds-pg"
}

# DynamoDB Tables
module "dynamodb" {
  source = "./modules/dynamodb"
  
  environment = var.environment
  tables = var.dynamodb_tables
}

# S3 Buckets
module "s3" {
  source = "./modules/s3"
  
  environment = var.environment
  buckets = var.s3_buckets
}

# ECS Clusters
module "ecs" {
  source = "./modules/ecs"
  
  environment       = var.environment
  vpc_id            = module.vpc.vpc_id
  private_subnet_ids = module.vpc.private_subnet_ids
  public_subnet_ids = module.vpc.public_subnet_ids
  cluster_name      = "${var.environment}-dms-cluster"
  container_insights = true
}

# Cognito User Pool
module "cognito" {
  source = "./modules/cognito"
  
  environment = var.environment
  user_pool_name = "${var.environment}-dms-users"
  auto_verified_attributes = ["email"]
}

# API Gateway
module "api_gateway" {
  source = "./modules/api_gateway"
  
  environment = var.environment
  name        = "${var.environment}-dms-api"
  description = "API Gateway for DMS ${var.environment} environment"
  cognito_user_pool_arn = module.cognito.user_pool_arn
}

# CloudFront Distribution
module "cloudfront" {
  source = "./modules/cloudfront"
  
  environment          = var.environment
  s3_bucket_domain_name = module.s3.website_bucket_domain_name
  s3_bucket_origin_id  = "S3-${var.environment}-dms-website"
}
