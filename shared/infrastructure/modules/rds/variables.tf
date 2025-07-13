variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "vpc_id" {
  description = "ID of the VPC"
  type        = string
}

variable "database_subnet_ids" {
  description = "List of database subnet IDs"
  type        = list(string)
}

variable "db_name" {
  description = "Name of the database to create"
  type        = string
}

variable "master_username" {
  description = "Master username for the RDS instance"
  type        = string
  sensitive   = true
}

variable "master_password" {
  description = "Master password for the RDS instance"
  type        = string
  sensitive   = true
}

variable "instance_class" {
  description = "RDS instance type"
  type        = string
}

variable "allocated_storage" {
  description = "Allocated storage in GiB"
  type        = number
}

variable "subnet_group_name" {
  description = "Name of the DB subnet group"
  type        = string
}

variable "parameter_group_name" {
  description = "Name of the DB parameter group"
  type        = string
}
