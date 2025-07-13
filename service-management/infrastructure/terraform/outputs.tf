output "db_endpoint" {
  description = "The endpoint of the RDS database"
  value       = aws_db_instance.service_management_db.endpoint
}

output "db_name" {
  description = "The name of the database"
  value       = aws_db_instance.service_management_db.db_name
}

output "dynamodb_table_name" {
  description = "The name of the DynamoDB table for service job history"
  value       = aws_dynamodb_table.service_job_history.name
}

output "ecs_cluster_id" {
  description = "The ID of the ECS cluster"
  value       = aws_ecs_cluster.service_management.id
}

output "ecs_cluster_name" {
  description = "The name of the ECS cluster"
  value       = aws_ecs_cluster.service_management.name
}

output "s3_bucket_name" {
  description = "The name of the S3 bucket for static content"
  value       = aws_s3_bucket.service_management_static.bucket
}

output "cloudwatch_log_group_name" {
  description = "The name of the CloudWatch log group"
  value       = aws_cloudwatch_log_group.service_management.name
}
