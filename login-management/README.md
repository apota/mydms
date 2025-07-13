# Login Management Module

A comprehensive authentication and user management system for the DMS (Dealership Management System). This module provides secure user authentication, registration, password management, and user profile management with JWT token-based authentication.

## Features

### Backend (.NET 8 API)
- **User Authentication**: JWT token-based authentication with refresh tokens
- **User Registration**: Account creation with email verification
- **Password Management**: Secure password hashing, reset, and change functionality
- **Email Verification**: Account email verification system
- **User Profile Management**: CRUD operations for user profiles
- **Security**: BCrypt password hashing, secure token generation
- **Database**: Entity Framework Core with SQL Server

### Frontend (React)
- **Modern UI**: Material-UI components with responsive design
- **Authentication Flow**: Login, register, forgot password, reset password
- **Form Validation**: Formik with Yup validation schemas
- **State Management**: React Context API for authentication state
- **Routing**: React Router with protected routes
- **API Integration**: Axios with interceptors for token management

## Architecture

```
login-management/
├── src/
│   ├── DMS.LoginManagement.API/          # ASP.NET Core Web API
│   ├── DMS.LoginManagement.Core/         # Domain entities and interfaces
│   ├── DMS.LoginManagement.Infrastructure/ # Data access and services
│   └── DMS.LoginManagement.Web/          # React frontend
├── docker-compose.yml                    # Docker services configuration
├── initialize-dev.cmd                    # Development setup script
└── README.md                            # This file
```

## Quick Start

### Prerequisites
- Docker Desktop
- .NET 8 SDK
- Node.js 18+

### Development Setup

1. **Clone and Navigate**
   ```bash
   cd login-management
   ```

2. **Run Initialization Script**
   ```bash
   initialize-dev.cmd
   ```

3. **Access the Application**
   - **Web App**: http://localhost:3004
   - **API**: http://localhost:5004
   - **API Documentation**: http://localhost:5004/swagger

### Demo Credentials

The system comes with pre-configured demo accounts for testing:

- **Admin**: admin@dms-demo.com / Admin123!
- **Sales**: sales@dms-demo.com / Demo123!
- **Service**: service@dms-demo.com / Demo123!

## Manual Setup

### Backend Setup

1. **Restore Dependencies**
   ```bash
   cd src/DMS.LoginManagement.API
   dotnet restore
   ```

2. **Update Database**
   ```bash
   dotnet ef database update
   ```

3. **Run API**
   ```bash
   dotnet run
   ```

### Frontend Setup

1. **Install Dependencies**
   ```bash
   cd src/DMS.LoginManagement.Web
   npm install
   ```

2. **Start Development Server**
   ```bash
   npm start
   ```

## API Endpoints

### Authentication
- `POST /api/auth/login` - User login
- `POST /api/auth/register` - User registration
- `POST /api/auth/refresh` - Refresh access token
- `POST /api/auth/logout` - User logout
- `GET /api/auth/profile` - Get user profile

### Password Management
- `POST /api/auth/forgot-password` - Request password reset
- `POST /api/auth/reset-password` - Reset password with token
- `POST /api/auth/change-password` - Change password (authenticated)

### Email Verification
- `POST /api/auth/verify-email` - Verify email with token
- `POST /api/auth/resend-verification` - Resend verification email

## Configuration

### Backend Configuration (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DMS_LoginManagement;Trusted_Connection=true;"
  },
  "JwtSettings": {
    "SecretKey": "your-super-secret-jwt-key-that-is-at-least-32-characters-long",
    "Issuer": "DMS.LoginManagement",
    "Audience": "DMS.Users",
    "AccessTokenExpiryMinutes": 15,
    "RefreshTokenExpiryDays": 7
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "noreply@dms.com",
    "SenderName": "DMS System",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

### Frontend Configuration (.env)

```bash
REACT_APP_API_URL=http://localhost:5004/api
```

## Docker Services

The docker-compose.yml defines the following services:

- **login-management-api**: .NET 8 Web API (Port 5004)
- **login-management-web**: React frontend with Nginx (Port 3004)
- **login-db**: SQL Server database (Port 1433)
- **login-db-init**: Database initialization service

## Development

### Building the Backend

```bash
cd src/DMS.LoginManagement.API
dotnet build
```

### Running Tests

```bash
cd tests
dotnet test
```

### Database Migrations

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

### Frontend Development

```bash
cd src/DMS.LoginManagement.Web

# Start development server
npm start

# Run tests
npm test

# Build for production
npm run build
```

## Security Features

- **Password Hashing**: BCrypt with salt
- **JWT Tokens**: Secure token generation with expiry
- **Refresh Tokens**: Automatic token refresh
- **Email Verification**: Account verification workflow
- **Password Reset**: Secure password reset with tokens
- **CORS Configuration**: Configured for cross-origin requests
- **Input Validation**: Comprehensive validation on both client and server

## Troubleshooting

### Common Issues

1. **Database Connection Failed**
   - Ensure SQL Server container is running
   - Check connection string in appsettings.json

2. **CORS Errors**
   - Verify CORS configuration in API
   - Check if frontend URL is in allowed origins

3. **Authentication Issues**
   - Verify JWT configuration
   - Check token expiry settings

4. **Email Not Sending**
   - Configure SMTP settings correctly
   - Use app-specific passwords for Gmail

### Logs

```bash
# View all logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f login-management-api
docker-compose logs -f login-management-web
```

## Integration with Main DMS

To integrate this login module with the main DMS system:

1. **Update Main Docker Compose**
   - Include login-management services
   - Configure network connectivity

2. **Configure API Gateway**
   - Route authentication requests to login API
   - Set up authentication middleware

3. **Frontend Integration**
   - Import authentication components
   - Configure authentication context

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is part of the DMS system and follows the same licensing terms.
