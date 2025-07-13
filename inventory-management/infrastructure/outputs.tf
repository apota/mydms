output "vpc_id" {
  description = "ID of the VPC"
  value       = module.vpc.vpc_id
}

output "private_subnets" {
  description = "List of private subnet IDs"
  value       = module.vpc.private_subnets
}

output "public_subnets" {
  description = "List of public subnet IDs"
  value       = module.vpc.public_subnets
}

output "db_instance_address" {
  description = "Address of the RDS instance"
  value       = module.postgres.db_instance_address
}

output "db_instance_endpoint" {
  description = "Connection endpoint of the RDS instance"
  value       = module.postgres.db_instance_endpoint
}

output "api_load_balancer_dns" {
  description = "DNS name of the API load balancer"
  value       = aws_lb.api.dns_name
}

output "s3_bucket_name" {
  description = "Name of the S3 bucket for vehicle media"
  value       = aws_s3_bucket.vehicle_media.bucket
}

output "dynamodb_table_name" {
  description = "Name of the DynamoDB table for vehicle media"
  value       = aws_dynamodb_table.vehicle_media.name
}

output "sqs_queue_url" {
  description = "URL of the SQS queue for inventory processing"
  value       = aws_sqs_queue.inventory_queue.url
}

output "ecs_cluster_name" {
  description = "Name of the ECS cluster"
  value       = module.ecs.cluster_name
}
