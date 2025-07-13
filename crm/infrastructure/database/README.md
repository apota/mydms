# CRM Database Infrastructure

This directory contains database initialization and sample data scripts for the CRM module.

## Scripts

### 01_init_crm_database.sh
- **Purpose**: Initializes the PostgreSQL database for the CRM module
- **Creates**: `dms_crm` database with proper permissions and schema
- **Usage**: Automatically run when starting the CRM docker-compose stack

### 02_load_sample_data.sh
- **Purpose**: Loads sample customers and campaigns for development/testing
- **Dependencies**: Requires the CRM API to be running
- **Usage**: 
  ```bash
  # Make sure CRM API is running first
  cd crm
  docker-compose up -d
  
  # Wait for API to be ready, then load sample data
  cd infrastructure/database
  chmod +x 02_load_sample_data.sh
  ./02_load_sample_data.sh
  ```

## Environment Variables

The sample data script uses these environment variables:

- `API_BASE_URL`: Base URL for CRM API (default: http://localhost:7001/api)
- `BEARER_TOKEN`: Optional JWT token for authenticated requests

## Sample Data Included

### Customers
- John Smith (Individual, VIP)
- Sarah Wilson (Individual, New Customer) 
- Mike Johnson (Business, Corporate)
- Lisa Brown (Individual, Young Professional)

### Campaigns
- Summer Sales Event (Active)
- Service Reminder Campaign (Active)
- New Year Promotion (Planned)

## Integration

These scripts are designed to work with:
- PostgreSQL 14+
- .NET 8 CRM API
- Entity Framework Core migrations
- Docker Compose environment
