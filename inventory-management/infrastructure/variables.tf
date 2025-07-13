variable "aws_region" {
  description = "The AWS region to deploy resources"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Environment name (e.g., dev, test, staging, production)"
  type        = string
  default     = "dev"
}

# VPC variables
variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "List of availability zones to use"
  type        = list(string)
  default     = ["us-east-1a", "us-east-1b", "us-east-1c"]
}

variable "private_subnets" {
  description = "List of private subnet CIDR blocks"
  type        = list(string)
  default     = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
}

variable "public_subnets" {
  description = "List of public subnet CIDR blocks"
  type        = list(string)
  default     = ["10.0.101.0/24", "10.0.102.0/24", "10.0.103.0/24"]
}

variable "database_subnets" {
  description = "List of database subnet CIDR blocks"
  type        = list(string)
  default     = ["10.0.201.0/24", "10.0.202.0/24", "10.0.203.0/24"]
}

# Database variables
variable "db_instance_class" {
  description = "RDS instance type"
  type        = string
  default     = "db.t3.medium"
}

variable "db_allocated_storage" {
  description = "Allocated storage for the RDS instance (in GB)"
  type        = number
  default     = 20
}

variable "db_max_allocated_storage" {
  description = "Maximum allocated storage for autoscaling (in GB)"
  type        = number
  default     = 100
}

variable "db_username" {
  description = "Database master username"
  type        = string
  default     = "dmsadmin"
  sensitive   = true
}

variable "db_password" {
  description = "Database master password"
  type        = string
  sensitive   = true
}

# S3 CORS configuration
variable "allowed_origins" {
  description = "List of origins allowed for CORS on the S3 bucket"
  type        = list(string)
  default     = ["*"]
}

# API and worker configuration
variable "api_task_cpu" {
  description = "CPU units for the API task"
  type        = number
  default     = 512
}

variable "api_task_memory" {
  description = "Memory allocation for the API task (in MB)"
  type        = number
  default     = 1024
}

variable "worker_task_cpu" {
  description = "CPU units for the worker task"
  type        = number
  default     = 256
}

variable "worker_task_memory" {
  description = "Memory allocation for the worker task (in MB)"
  type        = number
  default     = 512
}

variable "api_service_count" {
  description = "Number of API task instances to run"
  type        = number
  default     = 2
}

variable "worker_service_count" {
  description = "Number of worker task instances to run"
  type        = number
  default     = 1
}

# ECR and container images
variable "ecr_repository_url" {
  description = "ECR repository URL for container images"
  type        = string
}

variable "api_image_tag" {
  description = "Container image tag for the API service"
  type        = string
  default     = "latest"
}

variable "worker_image_tag" {
  description = "Container image tag for the worker service"
  type        = string
  default     = "latest"
}

# SSL certificate and domain configuration
variable "certificate_arn" {
  description = "ARN of the SSL certificate for the load balancer"
  type        = string
}

# .NET environment configuration
variable "dotnet_environment" {
  description = "ASP.NET Core environment name (Development, Staging, Production)"
  type        = string
  default     = "Production"
}

# CloudWatch logs configuration
variable "log_retention_days" {
  description = "Number of days to retain CloudWatch logs"
  type        = number
  default     = 30
}

# Additional AWS configuration as needed
