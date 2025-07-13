variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
  default     = "dev"
}

variable "aws_region" {
  description = "AWS region to deploy resources"
  type        = string
  default     = "us-west-2"
}

variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "availability_zones" {
  description = "List of availability zones to use"
  type        = list(string)
  default     = ["us-west-2a", "us-west-2b", "us-west-2c"]
}

variable "public_subnet_cidrs" {
  description = "CIDR blocks for public subnets"
  type        = list(string)
  default     = ["10.0.1.0/24", "10.0.2.0/24", "10.0.3.0/24"]
}

variable "private_subnet_cidrs" {
  description = "CIDR blocks for private subnets"
  type        = list(string)
  default     = ["10.0.11.0/24", "10.0.12.0/24", "10.0.13.0/24"]
}

variable "database_subnet_cidrs" {
  description = "CIDR blocks for database subnets"
  type        = list(string)
  default     = ["10.0.21.0/24", "10.0.22.0/24", "10.0.23.0/24"]
}

# Database variables
variable "db_master_username" {
  description = "Master username for the RDS instance"
  type        = string
  default     = "dmsadmin"
  sensitive   = true
}

variable "db_master_password" {
  description = "Master password for the RDS instance"
  type        = string
  sensitive   = true
}

variable "db_name" {
  description = "Name of the database to create"
  type        = string
  default     = "dmsdatabase"
}

variable "db_instance_class" {
  description = "RDS instance type"
  type        = string
  default     = "db.t3.medium"
}

variable "db_allocated_storage" {
  description = "Allocated storage in GiB"
  type        = number
  default     = 20
}

# DynamoDB variables
variable "dynamodb_tables" {
  description = "List of DynamoDB tables to create"
  type = list(object({
    name         = string
    billing_mode = string
    hash_key     = string
    range_key    = string
    attributes = list(object({
      name = string
      type = string
    }))
    ttl_enabled = bool
  }))
  default = [
    {
      name         = "VehicleEvents"
      billing_mode = "PAY_PER_REQUEST"
      hash_key     = "VIN"
      range_key    = "EventTimestamp"
      attributes = [
        {
          name = "VIN"
          type = "S"
        },
        {
          name = "EventTimestamp"
          type = "N"
        }
      ]
      ttl_enabled = true
    },
    {
      name         = "CustomerSessions"
      billing_mode = "PAY_PER_REQUEST"
      hash_key     = "SessionId"
      range_key    = "CustomerId"
      attributes = [
        {
          name = "SessionId"
          type = "S"
        },
        {
          name = "CustomerId"
          type = "S"
        }
      ]
      ttl_enabled = true
    }
  ]
}

# S3 Bucket variables
variable "s3_buckets" {
  description = "List of S3 buckets to create"
  type = list(object({
    name        = string
    acl         = string
    versioning  = bool
    lifecycle_rules = list(object({
      enabled = bool
      expiration_days = number
    }))
  }))
  default = [
    {
      name        = "dms-documents"
      acl         = "private"
      versioning  = true
      lifecycle_rules = [
        {
          enabled = true
          expiration_days = 90
        }
      ]
    },
    {
      name        = "dms-vehicle-images"
      acl         = "private"
      versioning  = false
      lifecycle_rules = []
    },
    {
      name        = "dms-website"
      acl         = "private"
      versioning  = true
      lifecycle_rules = []
    }
  ]
}
