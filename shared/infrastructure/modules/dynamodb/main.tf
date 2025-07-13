resource "aws_dynamodb_table" "table" {
  count          = length(var.tables)
  name           = "${var.environment}-${var.tables[count.index].name}"
  billing_mode   = var.tables[count.index].billing_mode
  hash_key       = var.tables[count.index].hash_key
  range_key      = var.tables[count.index].range_key
  
  dynamic "attribute" {
    for_each = var.tables[count.index].attributes
    content {
      name = attribute.value.name
      type = attribute.value.type
    }
  }

  dynamic "ttl" {
    for_each = var.tables[count.index].ttl_enabled ? [1] : []
    content {
      enabled        = true
      attribute_name = "TimeToLive"
    }
  }

  point_in_time_recovery {
    enabled = var.environment == "prod" ? true : false
  }

  tags = {
    Name        = "${var.environment}-${var.tables[count.index].name}"
    Environment = var.environment
  }
}
