# 🚀 DMS Local Demo - Complete Dealership Management System

Get the complete Automotive Dealership Management System running locally in under 10 minutes!

## 📋 System Overview

This demo provides a fully functional DMS with all core modules:

- **🔐 Authentication** - SSO/MFA/Captcha login system
- **📊 Dashboard** - Executive overview with KPIs
- **🚗 Inventory Management** - Vehicle tracking and management
- **💰 Sales Management** - Lead tracking and deal management
- **🔧 Service Department** - Appointment scheduling and service history
- **🔩 Parts Management** - Inventory control and ordering
- **👥 CRM** - Customer relationship management
- **💼 Financial Management** - Accounting and reporting
- **🤖 AI Assistant (Rudy)** - Global search and intelligent assistance
- **⚙️ User Management** - Role-based access control
- **⚙️ Settings Management** - Separate microservice for user and system settings (see settings-management folder)

## ⚡ Prerequisites

- **Docker Desktop** installed and running
- **12GB+ RAM** available for containers
- **Ports available**: 3000, 3001, 4566, 5432, 6379, 8080-8090
- **Storage**: 5GB free space for demo data
- **Windows PowerShell** or **Command Prompt**

## 🎯 One-Command Start

```bash
# Navigate to the demo directory
cd demo

# Start everything with a single command
docker-compose up

# This will automatically:
# - Start all infrastructure (PostgreSQL, Redis, LocalStack)
# - Initialize databases and AWS resources
# - Build and start all microservices
# - Build and start frontend applications
# - Load demo data
# - Display ready message when complete

# To run in background (detached mode):
docker-compose up -d

# Monitor startup progress
docker-compose logs -f

# Check status of all services
docker-compose ps
```

Wait for the initialization to complete. You'll see a message like:
```
========================================
DMS Demo System Ready!
========================================
Frontend:     http://localhost:3000
Admin Panel:  http://localhost:3001
API Gateway:  http://localhost:8080

Demo Credentials:
Username: admin
Password: admin123
========================================
```

## 🛑 Stopping the Demo

```bash
# Stop all services
docker-compose down

# Stop and remove all data volumes (fresh start next time)
docker-compose down -v

# Stop and remove all images (complete cleanup)
docker-compose down -v --rmi all
```

## 🌐 Access Your DMS

| Service | URL | Purpose |
|---------|-----|---------|
| **Main DMS App** | http://localhost:3000 | Primary application interface |
| **Admin Portal** | http://localhost:3001 | System administration |
| **API Gateway** | http://localhost:8080 | Main API endpoints |
| **LocalStack Console** | http://localhost:4566 | AWS services status |

## 🔑 Demo Login Credentials

### Primary Demo Account
```
Username: admin
Password: admin123
```

This account has full system access and can be used to explore all features.

### Additional Test Accounts (Created after first login)
- **Sales Manager**: Create via User Management
- **Service Advisor**: Create via User Management  
- **Parts Manager**: Create via User Management
- **Customer Service**: Create via User Management
```
Email: parts@dms-demo.com
Password: Demo123!
```

### System Administrator
```
Email: admin@dms-demo.com
Password: Admin123!
```

## 🎮 Demo Features to Try

### 1. Dashboard Overview
1. Login with manager credentials
2. View real-time KPIs and metrics
3. Interact with charts and reports
4. Test global search functionality
5. Chat with AI Assistant Rudy

### 2. Inventory Management
1. Navigate to Inventory module
2. Browse vehicle inventory
3. Add a new vehicle
4. Update vehicle status
5. Generate aging reports

### 3. Sales Management
1. Go to Sales module
2. Create a new lead
3. Work through deal pipeline
4. Generate sales reports
5. Test commission calculations

### 4. Service Department
1. Access Service module
2. Schedule an appointment
3. Create service order
4. Update work status
5. Process warranty claims

### 5. AI Assistant Integration
1. Click Rudy icon (anywhere in app)
2. Ask questions like:
   - "Show me vehicles over 60 days old"
   - "What's my sales performance this month?"
   - "Schedule a service appointment for VIN 123"
   - "Find customer John Smith's history"

## 🏗️ System Architecture

```
┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐
│   Frontend      │  │  Admin Portal   │  │   API Gateway   │
│   React SPA     │  │   React Admin   │  │   Node.js       │
│   Port: 3000    │  │   Port: 3001    │  │   Port: 8080    │
└─────────────────┘  └─────────────────┘  └─────────────────┘
         │                     │                     │
         └─────────────────────┼─────────────────────┘
                               │
┌─────────────────┬─────────────┴─────────┬─────────────────┐
│                 │                       │                 │
│  Microservices Layer (Ports 8081-8090)                   │
├─────────────────┼─────────────────────┼─────────────────┤
│   Auth Service  │   Inventory Service │   Sales Service │
│   Port: 8081    │   Port: 8082        │   Port: 8083    │
├─────────────────┼─────────────────────┼─────────────────┤
│ Service Service │   Parts Service     │   CRM Service   │
│   Port: 8084    │   Port: 8085        │   Port: 8086    │
├─────────────────┼─────────────────────┼─────────────────┤
│Financial Service│   AI Assistant      │   User Service  │
│   Port: 8087    │   Port: 8088        │   Port: 8089    │
└─────────────────┴─────────────────────┴─────────────────┘
         │                     │                     │
┌─────────────────┐  ┌─────────┴─────────┐  ┌─────────────────┐
│   PostgreSQL    │  │     Redis         │  │   LocalStack    │
│   Database      │  │     Cache         │  │   AWS Services  │
│   Port: 5432    │  │     Port: 6379    │  │   Port: 4566    │
└─────────────────┘  └───────────────────┘  └─────────────────┘
```

## 🛠️ Development Commands

```bash
# View all logs
docker-compose logs -f

# View specific service logs
docker-compose logs -f frontend
docker-compose logs -f api-gateway
docker-compose logs -f auth-service

# Restart a service
docker-compose restart frontend

# Stop everything
docker-compose down

# Reset everything (removes all data)
docker-compose down -v
docker-compose up

# Check container status and health
docker-compose ps
docker stats

# Access database directly
docker-compose exec postgres psql -U dms_user -d dms_main

# Check LocalStack S3 buckets
docker-compose exec localstack awslocal s3 ls

# View system resource usage
docker-compose top

# Run health check script
# Windows:
.\health-check.ps1
# Linux/Mac:
chmod +x health-check.sh && ./health-check.sh
```

# Stop everything
docker-compose down

# Reset everything (removes all data)
docker-compose down -v
docker-compose up -d

# Check container status
docker-compose ps

# Access database directly
docker-compose exec postgres psql -U dms_user -d dms_main

# Check LocalStack S3 buckets
docker-compose exec localstack awslocal s3 ls
```

## 🔧 Troubleshooting

### Common Issues

**Port Conflicts:**
```bash
# Check what's using a port (e.g., 3000)
# Windows:
netstat -ano | findstr :3000
# Linux/Mac:
lsof -i :3000

# Kill process by PID
# Windows:
taskkill /PID <PID> /F
# Linux/Mac:
kill -9 <PID>
```

**Services Not Starting:**
```bash
# Check individual service logs
docker-compose logs [service-name]

# Examples:
docker-compose logs auth-service
docker-compose logs frontend
docker-compose logs postgres

# Restart specific service
docker-compose restart [service-name]

# Rebuild and restart
docker-compose up -d --build [service-name]
```

**Database Connection Issues:**
```bash
# Check database status
docker-compose exec postgres psql -U dms_user -d dms_main -c "\l"

# Reset database
docker-compose down -v
docker-compose up
```

**LocalStack AWS Services:**
```bash
# Check LocalStack health
curl http://localhost:4566/_localstack/health

# List S3 buckets
docker-compose exec localstack awslocal s3 ls

# Check SQS queues
docker-compose exec localstack awslocal sqs list-queues
```

**Complete System Reset:**
```bash
# Stop everything and remove all data
docker-compose down -v --rmi all

# Remove any leftover containers
docker system prune -a

# Start fresh
docker-compose up
```

### Performance Tips

**System Requirements:**
- Minimum 8GB RAM (12GB+ recommended)
- 4+ CPU cores recommended
- SSD storage for better performance

**Docker Settings:**
- Increase Docker Desktop memory to 8GB+
- Enable WSL2 backend on Windows
- Disable unnecessary services to free up ports

### Legacy Scripts (Deprecated)

The PowerShell scripts (`start-demo.ps1`, `stop-demo.ps1`, `test-system.ps1`) are no longer needed. All functionality is now handled directly by `docker-compose up`. These scripts are kept for reference but may be removed in future versions.
docker-compose up -d
```

**Memory Issues:**
```powershell
# Stop all services
docker-compose down

# Clean up Docker resources
docker system prune -f

# Restart with limited services
docker-compose up -d postgres redis localstack api-gateway frontend
```

### Performance Tips

- **Allocate more memory to Docker Desktop (8GB+ recommended)**
- **Enable WSL2 backend on Windows for better performance**
- **Close unnecessary applications while running the demo**
- **Use SSD storage for better I/O performance**

### Getting Help

If you encounter issues:

1. Check the logs: `docker-compose logs -f`
2. Verify system requirements: `.\test-system.ps1`
3. Try a clean restart: `docker-compose down -v && docker-compose up -d`
4. Check Docker Desktop settings and available resources

## 📊 What You're Demoing

✅ **Complete DMS Solution** - Full automotive dealership management system  
✅ **Modern Authentication** - SSO/MFA/Captcha security  
✅ **Microservices Architecture** - Scalable, distributed services  
✅ **Real-time Data** - Live updates across all modules  
✅ **AI Integration** - Intelligent assistant with natural language processing  
✅ **Cloud-Native** - AWS services via LocalStack  
✅ **Role-Based Security** - Granular permissions and access control  
✅ **Mobile Responsive** - Works on all devices  
✅ **Local Development** - Everything runs locally, no cloud dependencies  

## 🎉 Success Indicators

When everything is working, you should see:
- ✅ All containers running (`docker-compose ps`)
- ✅ Frontend loads at http://localhost:3000
- ✅ Login system with MFA working
- ✅ Dashboard displays real data
- ✅ All modules accessible from navigation
- ✅ AI Assistant responds to queries
- ✅ Database persists changes
- ✅ Search functionality works across modules

## 🆘 Need Help?

1. **Check logs**: `docker-compose logs -f`
2. **Reset demo**: `docker-compose down -v && docker-compose up -d`
3. **Check requirements**: Docker Desktop running, sufficient RAM
4. **Wait longer**: Initial startup takes 3-5 minutes
5. **Port conflicts**: Ensure ports 3000, 8080-8090 are available

---

**🎊 You now have a complete Automotive Dealership Management System running locally!**

## 📁 Project Structure

```
demo/
├── docker-compose.yml           # Main orchestration
├── .env                        # Environment configuration
├── apps/                       # Application services
│   ├── frontend/              # React SPA (Port 3000)
│   ├── admin/                 # Admin Portal (Port 3001)
│   ├── api-gateway/           # API Gateway (Port 8080)
│   └── services/              # Microservices (8081-8089)
│       ├── auth-service      # Authentication & Authorization
│       ├── inventory-service/ # Vehicle Inventory Management
│       ├── sales-service/     # Sales & Lead Management
│       ├── service-service/   # Service Department Management
│       ├── parts-service/     # Parts Inventory Management
│       ├── crm-service/       # Customer Relationship Management
│       ├── financial-service/ # Financial & Accounting
│       ├── ai-service/        # AI Assistant (Rudy)
│       └── user-service/      # User Management & Settings
├── infrastructure/            # Infrastructure setup
│   ├── database/             # Database schemas & migrations
│   ├── localstack/           # AWS LocalStack configuration
│   └── nginx/                # Load balancer configuration
├── config/                   # Configuration files
└── scripts/                  # Utility scripts
```
