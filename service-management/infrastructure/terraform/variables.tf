variable "aws_region" {
  description = "The AWS region to deploy resources to"
  type        = string
  default     = "us-west-2"
}

variable "environment" {
  description = "The deployment environment (e.g. dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "project_name" {
  description = "The name of the project"
  type        = string
  default     = "service-management"
}

variable "dynamodb_read_capacity" {
  description = "The read capacity for the DynamoDB table"
  type        = number
  default     = 5
}

variable "dynamodb_write_capacity" {
  description = "The write capacity for the DynamoDB table"
  type        = number
  default     = 5
}

variable "dynamodb_gsi_read_capacity" {
  description = "The read capacity for the DynamoDB global secondary index"
  type        = number
  default     = 5
}

variable "dynamodb_gsi_write_capacity" {
  description = "The write capacity for the DynamoDB global secondary index"
  type        = number
  default     = 5
}

variable "rds_allocated_storage" {
  description = "The allocated storage size for the RDS instance in GB"
  type        = number
  default     = 20
}

variable "rds_instance_class" {
  description = "The instance class for the RDS instance"
  type        = string
  default     = "db.t3.micro"
}

variable "rds_engine_version" {
  description = "The engine version for PostgreSQL"
  type        = string
  default     = "13.4"
}

variable "db_username" {
  description = "The username for the database"
  type        = string
  default     = "serviceadmin"
  sensitive   = true
}

variable "db_password" {
  description = "The password for the database"
  type        = string
  sensitive   = true
}

variable "subnet_ids" {
  description = "The subnet IDs where the RDS instance will be deployed"
  type        = list(string)
}

variable "vpc_id" {
  description = "The VPC ID where resources will be deployed"
  type        = string
}

variable "db_access_cidr_blocks" {
  description = "CIDR blocks that can access the database"
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "logs_retention_days" {
  description = "Number of days to retain logs"
  type        = number
  default     = 30
}

variable "ecr_repository_url" {
  description = "The URL of the ECR repository for the service management API"
  type        = string
}

variable "crm_api_url" {
  description = "The URL of the CRM API for integration"
  type        = string
  default     = "http://crm-api:8080"
}

variable "inventory_api_url" {
  description = "The URL of the Inventory API for integration"
  type        = string
  default     = "http://inventory-api:8080"
}

variable "parts_api_url" {
  description = "The URL of the Parts Management API for integration"
  type        = string
  default     = "http://parts-api:8080"
}

variable "financial_api_url" {
  description = "The URL of the Financial Management API for integration"
  type        = string
  default     = "http://financial-api:8080"
}

variable "notification_api_url" {
  description = "The URL of the Notification API for integration"
  type        = string
  default     = "http://notification-api:8080"
}

variable "api_task_count" {
  description = "The number of API tasks to run in the ECS service"
  type        = number
  default     = 2
}
