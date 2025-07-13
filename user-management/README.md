# User Management Module

The User Management module provides comprehensive user administration capabilities for the DMS (Dealership Management System). This module handles user accounts, authentication, authorization, and role-based access control.

## Architecture

### Backend (C# .NET 8)
- **DMS.UserManagement.API**: REST API endpoints for user management operations
- **DMS.UserManagement.Core**: Business logic, entities, DTOs, and service interfaces
- **DMS.UserManagement.Infrastructure**: Data access, Entity Framework, and service implementations

### Frontend (React)
- **DMS.UserManagement.Web**: React application with Material-UI for user management interface

## Features

### User Management
- Create, read, update, and delete user accounts
- User profile management (name, email, phone, department)
- Role assignment and management
- User status control (active/inactive)

### Authentication & Authorization
- Password management and hashing (BCrypt)
- JWT token-based authentication
- Role-based access control
- Change password functionality

### User Interface
- Modern React application with Material-UI
- Data grid for user listing with sorting and filtering
- Modal forms for user creation and editing
- Password change dialog
- Search and filter capabilities
- Responsive design

## Technology Stack

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL database
- AutoMapper for object mapping
- BCrypt.Net for password hashing
- JWT authentication
- Swagger/OpenAPI documentation

### Frontend
- React 18
- Material-UI (MUI)
- React Query for data fetching
- Formik for form handling
- Yup for validation
- React Hot Toast for notifications
- Axios for HTTP requests

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- Node.js 18+
- Docker and Docker Compose
- PostgreSQL (or use Docker container)

### Quick Start

1. **Initialize the development environment:**
   ```bash
   initialize-dev.cmd
   ```

2. **Services will be available at:**
   - API: http://localhost:5003
   - Web UI: http://localhost:3003
   - Database: localhost:5434
   - Health Check: http://localhost:5003/health

### Manual Setup

1. **Backend Setup:**
   ```bash
   cd src/DMS.UserManagement.API
   dotnet restore
   dotnet run
   ```

2. **Frontend Setup:**
   ```bash
   cd src/DMS.UserManagement.Web
   npm install
   npm start
   ```

3. **Database Setup:**
   ```bash
   # Update connection string in appsettings.json
   dotnet ef database update
   ```

## API Endpoints

### Users
- `GET /api/users` - Get all users
- `GET /api/users/{id}` - Get user by ID
- `POST /api/users` - Create new user
- `PUT /api/users/{id}` - Update user
- `DELETE /api/users/{id}` - Delete user
- `POST /api/users/{id}/change-password` - Change user password

### Health
- `GET /health` - Health check endpoint

## Environment Variables

### Backend (API)
```bash
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5003
DB_HOST=localhost
DB_PORT=5432
DB_NAME=dms_users
DB_USER=dms_user
DB_PASSWORD=dms_password
```

### Frontend (Web)
```bash
REACT_APP_API_URL=http://localhost:5003/api
```

## Database Schema

### Users Table
- `Id` (Primary Key)
- `FirstName`
- `LastName`
- `Email` (Unique)
- `PasswordHash`
- `Role`
- `Status`
- `Phone`
- `Department`
- `CreatedAt`
- `UpdatedAt`
- `LastLoginAt`

## User Roles

- **admin**: Full system access
- **manager**: Management-level access
- **sales**: Sales department access
- **service**: Service department access
- **parts**: Parts department access
- **user**: Basic user access

## Security

- Passwords are hashed using BCrypt
- JWT tokens for API authentication
- CORS configured for cross-origin requests
- Input validation and sanitization
- Error handling without sensitive information exposure

## Integration

### With Main DMS System
- Integrated in main docker-compose.yml
- Accessible from main dashboard at `/users`
- Redirects to dedicated user management application
- API available for other microservices

### Service Discovery
- API available at: `http://user-management-api:5003`
- Web interface at: `http://localhost:3003`

## Development

### Adding New Features
1. Add DTOs in `Core/DTOs`
2. Add business logic in `Core/Services`
3. Implement in `Infrastructure/Services`
4. Add API endpoints in `API/Controllers`
5. Update frontend components

### Testing
```bash
# Backend tests
dotnet test

# Frontend tests
npm test
```

### Building
```bash
# Build all services
docker-compose build

# Build individual services
docker build -f src/DMS.UserManagement.API/Dockerfile .
docker build -f src/DMS.UserManagement.Web/Dockerfile src/DMS.UserManagement.Web
```

## Deployment

The module is designed for containerized deployment using Docker. It can be deployed as part of the main DMS system or as a standalone service.

### Docker Compose
```bash
docker-compose up -d
```

### Individual Containers
```bash
# API
docker run -p 5003:5003 dms-user-management-api

# Web
docker run -p 3003:3000 dms-user-management-web
```

## Monitoring

- Health checks available at `/health`
- Structured logging
- Error handling and monitoring
- Database connection monitoring
