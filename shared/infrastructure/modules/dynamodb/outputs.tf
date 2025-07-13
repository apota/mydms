output "table_names" {
  description = "Names of the created DynamoDB tables"
  value       = aws_dynamodb_table.table[*].name
}

output "table_arns" {
  description = "ARNs of the created DynamoDB tables"
  value       = aws_dynamodb_table.table[*].arn
}
