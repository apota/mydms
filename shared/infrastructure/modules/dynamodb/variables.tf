variable "environment" {
  description = "Deployment environment (dev, staging, prod)"
  type        = string
}

variable "tables" {
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
}
