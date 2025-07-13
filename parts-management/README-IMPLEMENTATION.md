# DMS Parts Management Module

## Overview

The Parts Management module for the Automotive Dealership Management System (DMS) provides comprehensive functionality for managing automobile parts inventory, ordering, and tracking. This module integrates with other DMS modules to provide a complete dealership management solution.

## Features

- Parts inventory management
- Parts catalog and search capabilities
- Order management for parts
- Transaction tracking for parts movements
- Core part tracking
- Supplier management
- Pricing management

## Technology Stack

- **Backend**: .NET 8 / C# with ASP.NET Core Web API
- **Frontend**: React with Material UI
- **Database**: PostgreSQL for relational data
- **NoSQL**: DynamoDB for parts catalog search
- **Infrastructure**: AWS (via Terraform)
- **Containerization**: Docker with Docker Compose

## Project Structure

```
parts-management/
├── infrastructure/         # Terraform files for AWS deployment
├── src/
│   ├── DMS.PartsManagement.API/           # ASP.NET Core Web API
│   ├── DMS.PartsManagement.Core/          # Domain models and interfaces
│   ├── DMS.PartsManagement.Infrastructure/ # Implementation of repositories and services
│   └── DMS.PartsManagement.Web/           # React frontend application
└── tests/                  # Unit and integration tests
```

## Getting Started

### Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm
- Docker and Docker Compose
- PostgreSQL (or use the Docker container)
- AWS CLI and Terraform (for deployment)

### Local Development

1. Clone the repository:

```bash
git clone <repository-url>
cd parts-management
```

2. Initialize the development database:

```bash
initialize-dev.cmd
# or
sh initialize-dev.sh
```

3. Start the application using Docker Compose:

```bash
docker-compose up -d
```

4. Access the API at: http://localhost:5001
5. Access the Web app at: http://localhost:3000
6. Access Swagger documentation at: http://localhost:5001/swagger

### Running Tests

```bash
dotnet test tests/DMS.PartsManagement.Tests.sln
```

## Infrastructure Deployment

1. Navigate to the infrastructure directory:

```bash
cd infrastructure
```

2. Initialize Terraform:

```bash
terraform init
```

3. Create a `terraform.tfvars` file with your AWS configuration (see `terraform.tfvars.example`).

4. Plan the deployment:

```bash
terraform plan -out=tfplan
```

5. Apply the deployment:

```bash
terraform apply tfplan
```

## API Endpoints

The API provides the following main endpoints:

- `/api/parts` - CRUD operations for parts
- `/api/inventory` - Inventory management
- `/api/suppliers` - Supplier management
- `/api/orders` - Order management for parts
- `/api/transactions` - Transaction history and management
- `/api/core-tracking` - Core tracking functionality

For detailed API documentation, refer to the Swagger documentation.

## Integration Points

This module integrates with:

- CRM Module - For customer information
- Financial Management Module - For pricing and invoicing
- Service Management Module - For parts usage in service operations
- Sales Management Module - For parts usage in sales operations

## License

Proprietary - All rights reserved

## Support

For support, contact the DMS support team at support@dms.example.com
