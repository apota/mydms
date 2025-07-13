#!/bin/bash
# CRM Sample Data Loading Script
# This script loads sample customer and campaign data for development/testing

set -e

API_BASE_URL="${API_BASE_URL:-http://localhost:7001/api}"
BEARER_TOKEN="${BEARER_TOKEN:-}"

echo "📊 Loading CRM sample data..."

# Function to make API calls with proper error handling
make_api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    local url="${API_BASE_URL}${endpoint}"
    
    echo "Making $method request to $url"
    
    local headers="Content-Type: application/json"
    if [ -n "$BEARER_TOKEN" ]; then
        headers="$headers -H Authorization: Bearer $BEARER_TOKEN"
    fi
    
    local response
    response=$(curl -s -X "$method" "$url" \
        -H "$headers" \
        -d "$data" \
        -w "\nHTTP_STATUS:%{http_code}")
    
    local http_status=$(echo "$response" | grep "HTTP_STATUS:" | cut -d: -f2)
    local body=$(echo "$response" | grep -v "HTTP_STATUS:")
    
    if [ "$http_status" -ge 200 ] && [ "$http_status" -lt 300 ]; then
        echo "✅ Success: $body"
    else
        echo "❌ Failed: HTTP $http_status - $body"
    fi
}

# Wait for API to be ready
echo "⏳ Waiting for CRM API to be ready..."
for i in {1..30}; do
    if curl -s -f "${API_BASE_URL}/health" > /dev/null 2>&1; then
        echo "✅ CRM API is ready"
        break
    fi
    echo "Waiting for API... (attempt $i/30)"
    sleep 2
done

# Create sample customers
echo "👥 Creating sample customers..."

make_api_call "POST" "/customers" '{
    "firstName": "John",
    "lastName": "Smith",
    "email": "john.smith@email.com",
    "phone": "(555) 123-4567",
    "address": "123 Main St, Anytown, ST 12345",
    "dateOfBirth": "1985-06-15T00:00:00Z",
    "customerType": "Individual",
    "businessName": null,
    "preferredContactMethod": "Email",
    "tags": ["VIP", "Regular Customer"]
}'

make_api_call "POST" "/customers" '{
    "firstName": "Sarah",
    "lastName": "Wilson",
    "email": "sarah.wilson@email.com",
    "phone": "(555) 234-5678",
    "address": "456 Oak Ave, Somewhere, ST 23456",
    "dateOfBirth": "1990-03-22T00:00:00Z",
    "customerType": "Individual",
    "businessName": null,
    "preferredContactMethod": "Phone",
    "tags": ["New Customer"]
}'

make_api_call "POST" "/customers" '{
    "firstName": "Mike",
    "lastName": "Johnson",
    "email": "mike.johnson@business.com",
    "phone": "(555) 345-6789",
    "address": "789 Business Blvd, Corporate City, ST 34567",
    "dateOfBirth": "1978-11-10T00:00:00Z",
    "customerType": "Business",
    "businessName": "Johnson Enterprises",
    "preferredContactMethod": "Email",
    "tags": ["Corporate", "High Value"]
}'

make_api_call "POST" "/customers" '{
    "firstName": "Lisa",
    "lastName": "Brown",
    "email": "lisa.brown@email.com",
    "phone": "(555) 456-7890",
    "address": "321 Pine St, Hometown, ST 45678",
    "dateOfBirth": "1992-07-18T00:00:00Z",
    "customerType": "Individual",
    "businessName": null,
    "preferredContactMethod": "SMS",
    "tags": ["Young Professional"]
}'

# Create sample campaigns
echo "📢 Creating sample campaigns..."

make_api_call "POST" "/campaigns" '{
    "name": "Summer Sales Event",
    "description": "Special summer promotions on vehicles and services",
    "type": "Sales",
    "status": "Active",
    "startDate": "2024-06-01T00:00:00Z",
    "endDate": "2024-08-31T23:59:59Z",
    "budget": 15000.00,
    "targetAudience": ["Individual", "Business"],
    "channels": ["Email", "SMS", "Social Media"]
}'

make_api_call "POST" "/campaigns" '{
    "name": "Service Reminder Campaign",
    "description": "Automated reminders for scheduled maintenance",
    "type": "Service",
    "status": "Active",
    "startDate": "2024-01-01T00:00:00Z",
    "endDate": "2024-12-31T23:59:59Z",
    "budget": 5000.00,
    "targetAudience": ["Individual", "Business"],
    "channels": ["Email", "SMS"]
}'

make_api_call "POST" "/campaigns" '{
    "name": "New Year Promotion",
    "description": "Special New Year deals and financing options",
    "type": "Promotional",
    "status": "Planned",
    "startDate": "2025-01-01T00:00:00Z",
    "endDate": "2025-01-31T23:59:59Z",
    "budget": 20000.00,
    "targetAudience": ["Individual"],
    "channels": ["Email", "Direct Mail", "Social Media"]
}'

echo "✅ CRM sample data loading completed!"
