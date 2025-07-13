# Sales Management Module - Implementation Guide

## Overview

The Sales Management module is a comprehensive solution for managing the entire vehicle sales process in an automotive dealership. This implementation guide provides technical details on how the module is structured and how to work with its components.

## Architecture

The Sales Management module follows a clean architecture approach with the following components:

- **API Layer**: ASP.NET Core Web API providing RESTful endpoints for client applications
- **Core Layer**: Contains business domain models, interfaces, and business logic
- **Infrastructure Layer**: Implements data access, external integrations, and cross-cutting concerns
- **Web Client**: React-based single-page application for user interface

## Technology Stack

### Backend
- **Framework**: .NET 7.0
- **API**: ASP.NET Core Web API
- **ORM**: Entity Framework Core
- **Data Store**: PostgreSQL (relational data), DynamoDB (document metadata)
- **Object Storage**: Amazon S3 (document files)
- **Authentication**: JWT bearer tokens (integrated with identity service)
- **Documentation**: Swagger/OpenAPI

### Frontend
- **Framework**: React 18
- **State Management**: React Query
- **UI Components**: Material-UI (MUI)
- **Forms**: Formik with Yup validation
- **Routing**: React Router
- **Charts**: Chart.js with react-chartjs-2
- **HTTP Client**: Axios

### Infrastructure
- **Containerization**: Docker
- **Container Orchestration**: AWS ECS
- **Cloud Provider**: AWS
- **Infrastructure as Code**: Terraform
- **CI/CD**: GitHub Actions (or configured AWS CodePipeline)

## Project Structure

```
sales-management/
├── infrastructure/
│   └── terraform/          # Terraform IaC configuration
├── scripts/                # Development and deployment scripts
├── src/
│   ├── DMS.SalesManagement.API/            # API project
│   │   ├── Controllers/    # API endpoints
│   │   ├── Extensions/     # Service configuration extensions
│   │   └── Dockerfile      # Containerization
│   ├── DMS.SalesManagement.Core/           # Core domain project
│   │   ├── DTOs/           # Data transfer objects
│   │   ├── Models/         # Domain models
│   │   ├── Repositories/   # Repository interfaces
│   │   └── Services/       # Business logic services
│   ├── DMS.SalesManagement.Infrastructure/ # Infrastructure project
│   │   ├── Data/           # Database context and migrations
│   │   ├── External/       # External service integrations
│   │   └── Services/       # Implementations of core services
│   └── DMS.SalesManagement.Web/            # React web client
│       ├── public/         # Static assets
│       ├── src/            # React components and logic
│       └── Dockerfile      # Containerization
└── tests/
    ├── DMS.SalesManagement.API.Tests/      # API tests
    └── DMS.SalesManagement.Core.Tests/     # Core domain tests
```

## Setup and Deployment

### Local Development

1. **Prerequisites**:
   - Docker and Docker Compose
   - .NET SDK 7.0 or later
   - Node.js 16 or later
   - AWS CLI configured with appropriate credentials

2. **Initialize Development Environment**:
   ```
   cd sales-management
   ./initialize-dev.cmd
   ```

3. **Start Services**:
   ```
   docker-compose up -d
   ```

4. **Access Development Environment**:
   - Web UI: http://localhost:3000
   - API: http://localhost:5000
   - Swagger: http://localhost:5000/swagger
   - LocalStack (AWS services): http://localhost:4566

### Cloud Deployment

1. **Configure Terraform backend**:
   - Create an S3 bucket for state storage
   - Update `terraform/backend.tfvars`

2. **Configure deployment variables**:
   - Copy `terraform/terraform.tfvars.example` to `terraform/terraform.tfvars`
   - Update with your environment-specific values

3. **Deploy infrastructure**:
   ```
   cd infrastructure/terraform
   terraform init -backend-config=backend.tfvars
   terraform apply -var-file=terraform.tfvars
   ```

4. **Deploy application**:
   - Build and push Docker images to ECR
   - Update ECS services to use the latest images

## Data Flow and Key Processes

### Lead Management Flow

1. Leads are captured from various sources (website, phone, walk-in)
2. Leads are assigned to sales representatives
3. Activities and follow-ups are tracked
4. Qualified leads are converted to deals

### Deal Processing Flow

1. Customer and vehicle information is collected
2. Deal structure is configured (finance, lease, cash)
3. Trade-in valuation is calculated if applicable
4. F&I products are added
5. Documents are generated and signed
6. Deal is approved by management
7. Delivery is scheduled and completed

### Commission Calculation Flow

1. Deal is completed
2. Commission rules are applied based on role and deal parameters
3. Commissions are calculated for all participating staff
4. Commissions are approved by management
5. Commission payments are processed

## API Endpoints

See the [API documentation](README.md#api-endpoints) for a complete list of endpoints and their descriptions.

## Security Considerations

- All API endpoints are protected with JWT authentication
- Role-based authorization is implemented for sensitive operations
- Document storage uses server-side encryption
- Sensitive data is encrypted at rest in the database
- HTTPS is enforced for all communications

## Integration Points

This module integrates with several other DMS modules:

- **Inventory Management**: Vehicle information and status
- **Customer Management**: Customer records and history
- **Financial Management**: Payment processing and financial reporting
- **Documents**: Document generation and management
- **Reporting**: Analytics and business intelligence

## Extending the Module

To extend the module with additional features:

1. Add new models to the Core project
2. Implement repository interfaces in the Infrastructure project
3. Add migrations for database schema changes
4. Create new API endpoints in the Controllers directory
5. Update the React client with new components and services

## Troubleshooting

- **Database connection issues**: Check connection strings in appsettings.json
- **AWS service errors**: Verify credentials and endpoint configuration
- **API errors**: Check API logs in the Docker container
- **Web client issues**: Check browser console for JavaScript errors

For more detailed guidance, see the troubleshooting section in the README.md file.
