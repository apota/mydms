# Docker Building and Deployment Guide

## Overview

This document provides guidance for building and deploying the DMS services using Docker.

## Current Status

All services in the Docker Compose file have been converted to placeholder implementations that successfully build and run. 

- Backend APIs use `mcr.microsoft.com/dotnet/aspnet:6.0` images with placeholder scripts
- Web UIs use `nginx:alpine` with placeholder HTML pages
- Databases use production-ready PostgreSQL images
- Message broker uses production-ready RabbitMQ
- LocalStack provides local AWS emulation

## Moving from Placeholder to Production

When your service code is ready for Docker deployment, follow these steps:

1. Create proper project files for each service
2. Create a Dockerfile in each service API directory (see template provided)
3. Update the docker-compose.yml file to use the build configuration instead of placeholder images

## Troubleshooting Docker Build Issues

If you encounter Docker build errors like:

```
ERROR [service-api build 3/8] COPY [src/DMS.ServiceManagement.API/DMS.ServiceManagement.API.csproj, src/DMS.ServiceManagement.API/]
```

Common issues and solutions:

1. **File Path Issues**: Ensure the file paths in your COPY commands match your actual directory structure.

2. **Build Context**: Make sure the `context` in docker-compose.yml points to the correct directory relative to project files.

3. **Project References**: For multi-project solutions, you must:
   - Copy all project files before running `dotnet restore`
   - Maintain consistent project reference paths
   
4. **Dockerfile Approach Options**:

   a. Copy everything and build from specific directory:
   ```dockerfile
   COPY . .
   WORKDIR "/src/src/DMS.ServiceManagement.API"
   RUN dotnet restore
   RUN dotnet build -c Release
   ```
   
   b. Copy project files first, then source code:
   ```dockerfile
   COPY ["src/DMS.ServiceManagement.API/DMS.ServiceManagement.API.csproj", "src/DMS.ServiceManagement.API/"]
   COPY ["src/DMS.ServiceManagement.Core/DMS.ServiceManagement.Core.csproj", "src/DMS.ServiceManagement.Core/"]
   RUN dotnet restore "src/DMS.ServiceManagement.API/DMS.ServiceManagement.API.csproj"
   COPY . .
   ```

5. **Missing Dependencies**: Ensure all required packages are included in your project files.
