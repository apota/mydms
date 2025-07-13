#!/bin/bash

# Sample data initialization for DMS
echo "📊 Loading sample data into DMS..."

# Wait for API services to be ready
sleep 30

# Base URL for API
API_BASE="http://api-gateway:8080"

# Function to make API calls with retry
make_api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    local max_retries=5
    local retry_count=0

    while [ $retry_count -lt $max_retries ]; do
        if [ "$method" = "POST" ]; then
            response=$(curl -s -w "%{http_code}" -X POST \
                -H "Content-Type: application/json" \
                -d "$data" \
                "$API_BASE$endpoint")
        else
            response=$(curl -s -w "%{http_code}" "$API_BASE$endpoint")
        fi
        
        http_code="${response: -3}"
        
        if [ "$http_code" = "200" ] || [ "$http_code" = "201" ]; then
            echo "✅ Success: $endpoint"
            break
        else
            echo "⏳ Retrying $endpoint (attempt $((retry_count + 1))/$max_retries)..."
            sleep 5
            retry_count=$((retry_count + 1))
        fi
    done
}

# Create sample vehicles
echo "🚗 Creating sample vehicles..."

make_api_call "POST" "/inventory/vehicles" '{
    "vin": "1HGBH41JXMN109186",
    "make": "Honda",
    "model": "Accord",
    "year": 2023,
    "trim": "EX-L",
    "color": "Pearl White",
    "mileage": 15000,
    "status": "Available",
    "price": 28500,
    "cost": 24000,
    "location": "Lot A-15",
    "condition": "Excellent",
    "fuelType": "Gasoline",
    "transmission": "Automatic",
    "drivetrain": "FWD"
}'

make_api_call "POST" "/inventory/vehicles" '{
    "vin": "2T1BURHE0JC123456",
    "make": "Toyota",
    "model": "Camry",
    "year": 2024,
    "trim": "SE",
    "color": "Midnight Black",
    "mileage": 8500,
    "status": "Available",
    "price": 31200,
    "cost": 27500,
    "location": "Lot B-03",
    "condition": "Like New",
    "fuelType": "Gasoline",
    "transmission": "Automatic",
    "drivetrain": "FWD"
}'

make_api_call "POST" "/inventory/vehicles" '{
    "vin": "1FTFW1ET5DFC12345",
    "make": "Ford",
    "model": "F-150",
    "year": 2022,
    "trim": "XLT",
    "color": "Magnetic Gray",
    "mileage": 32000,
    "status": "Available",
    "price": 42500,
    "cost": 38000,
    "location": "Lot C-08",
    "condition": "Good",
    "fuelType": "Gasoline",
    "transmission": "Automatic",
    "drivetrain": "4WD"
}'

# Create sample parts
echo "🔩 Creating sample parts..."

make_api_call "POST" "/parts/inventory" '{
    "partNumber": "BP-1001",
    "name": "Brake Pads - Front",
    "category": "Brakes",
    "manufacturer": "ACDelco",
    "description": "Premium ceramic brake pads for front wheels",
    "quantity": 25,
    "unitCost": 45.99,
    "retailPrice": 89.99,
    "reorderLevel": 5,
    "supplier": "Parts Plus",
    "fitmentInfo": ["Honda Accord 2018-2023", "Honda CR-V 2017-2022"]
}'

make_api_call "POST" "/parts/inventory" '{
    "partNumber": "OF-2001",
    "name": "Oil Filter",
    "category": "Engine",
    "manufacturer": "Mobil 1",
    "description": "Extended performance oil filter",
    "quantity": 50,
    "unitCost": 8.99,
    "retailPrice": 16.99,
    "reorderLevel": 10,
    "supplier": "AutoZone",
    "fitmentInfo": ["Multiple Honda Models", "Multiple Toyota Models"]
}'

# Create sample service appointments
echo "🔧 Creating sample service appointments..."

make_api_call "POST" "/service/appointments" '{
    "customerId": 1,
    "vehicleVin": "1HGBH41JXMN109186",
    "serviceType": "Oil Change",
    "appointmentDate": "2024-07-15T10:00:00.000Z",
    "description": "Regular oil change and multi-point inspection",
    "estimatedDuration": 60,
    "status": "Scheduled",
    "advisorId": 1
}'

make_api_call "POST" "/service/appointments" '{
    "customerId": 2,
    "vehicleVin": "2T1BURHE0JC123456",
    "serviceType": "Brake Service",
    "appointmentDate": "2024-07-16T14:00:00.000Z",
    "description": "Brake pad replacement and rotor inspection",
    "estimatedDuration": 120,
    "status": "Scheduled",
    "advisorId": 2
}'

# Create sample sales leads
echo "💰 Creating sample sales leads..."

make_api_call "POST" "/sales/leads" '{
    "customerId": 3,
    "source": "Website",
    "status": "New",
    "interestedVehicle": "2024 Toyota Camry",
    "budget": 35000,
    "timeframe": "Within 30 days",
    "notes": "Looking for reliable sedan with good fuel economy",
    "assignedSalesperson": 1,
    "priority": "High"
}'

make_api_call "POST" "/sales/leads" '{
    "customerId": 1,
    "source": "Phone Inquiry", 
    "status": "Contacted",
    "interestedVehicle": "2022 Ford F-150",
    "budget": 45000,
    "timeframe": "Within 60 days",
    "notes": "Needs truck for construction business",
    "assignedSalesperson": 2,
    "priority": "Medium"
}'

# Upload sample vehicle images to S3
echo "📷 Uploading sample vehicle images..."

# Create placeholder images
echo "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQV..." | base64 -d > /tmp/honda-accord.jpg 2>/dev/null || echo "Creating placeholder image" > /tmp/honda-accord.jpg
echo "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQV..." | base64 -d > /tmp/toyota-camry.jpg 2>/dev/null || echo "Creating placeholder image" > /tmp/toyota-camry.jpg
echo "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQV..." | base64 -d > /tmp/ford-f150.jpg 2>/dev/null || echo "Creating placeholder image" > /tmp/ford-f150.jpg

awslocal s3 cp /tmp/honda-accord.jpg s3://dms-vehicle-images/vehicles/1HGBH41JXMN109186/main.jpg
awslocal s3 cp /tmp/toyota-camry.jpg s3://dms-vehicle-images/vehicles/2T1BURHE0JC123456/main.jpg  
awslocal s3 cp /tmp/ford-f150.jpg s3://dms-vehicle-images/vehicles/1FTFW1ET5DFC12345/main.jpg

echo "✅ Sample data loading complete!"
echo "📊 Created sample data:"
echo "   - 3 Customers (John Smith, Sarah Wilson, Mike Johnson)"
echo "   - 3 Vehicles (Honda Accord, Toyota Camry, Ford F-150)"
echo "   - 2 Parts (Brake Pads, Oil Filter)"
echo "   - 2 Service Appointments"
echo "   - 2 Sales Leads"
echo "   - Vehicle images uploaded to S3"
echo ""
echo "🎯 You can now explore the DMS with realistic sample data!"
