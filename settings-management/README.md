# DMS Settings Management Service

A comprehensive user and system settings management service for the Dealership Management System (DMS).

## 🚀 Quick Start

### Option 1: Start All Services (Recommended)
```bash
# Run this script to start everything
start-all-services.cmd
```

### Option 2: Manual Start
```bash
# 1. Start backend services
docker-compose up postgres settings-api -d

# 2. Start React frontend
start-react.cmd
```

### Access Points
- **Settings Manager**: http://localhost:3001 (React Frontend)
- **API Documentation**: http://localhost:8090 (API Testing)
- **Demo App**: http://localhost:3000 (Navigate to Settings menu)

## Overview

The Settings Management Service provides:
- **User Settings**: Personal preferences, themes, notifications, security settings
- **System Settings**: Global configuration, feature flags, regional defaults
- **Category-based Settings**: Organized settings by functionality
- **Real-time Persistence**: Immediate database storage
- **Flexible Storage**: JSONB for extensible settings schema

## Architecture

```
settings-management/
├── src/
│   ├── DMS.SettingsManagement.API/     # Backend API Service
│   │   ├── settings-server.js          # Express server
│   │   ├── package.json                # Dependencies
│   │   └── Dockerfile                  # Container setup
│   └── DMS.SettingsManagement.Web/     # Frontend Components
│       └── src/
│           ├── contexts/SettingsContext.js  # React context
│           └── pages/Settings.js            # Settings UI
├── infrastructure/                     # Database & deployment
├── tests/                             # Test suites
└── docker-compose.yml                 # Service orchestration
```

## Features

### User Settings
- **Notifications**: Email, SMS, browser, mobile push notifications
- **Security**: Two-factor authentication, session timeout, privacy controls
- **Display**: Dark mode, themes, dashboard layout, items per page
- **Regional**: Language, timezone, date format, currency

### System Settings
- **Global Configuration**: Feature flags, system-wide defaults
- **Public Settings**: Settings visible to all users
- **Admin Controls**: System-level configuration management

### API Endpoints

#### User Settings
- `GET /settings/user/{userId}` - Get user settings
- `PUT /settings/user/{userId}` - Update user settings
- `PATCH /settings/user/{userId}` - Bulk update settings
- `DELETE /settings/user/{userId}` - Delete user settings
- `GET /settings/user/{userId}/category/{category}` - Get category settings

#### System Settings
- `GET /settings/system` - Get public system settings
- `PUT /settings/system/{key}` - Update system setting (admin)

## Database Schema

### user_settings
```sql
CREATE TABLE user_settings (
  id SERIAL PRIMARY KEY,
  user_id INTEGER NOT NULL,
  settings JSONB NOT NULL,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  UNIQUE(user_id)
);
```

### system_settings
```sql
CREATE TABLE system_settings (
  id SERIAL PRIMARY KEY,
  setting_key VARCHAR(255) NOT NULL UNIQUE,
  setting_value JSONB NOT NULL,
  description TEXT,
  is_public BOOLEAN DEFAULT false,
  created_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
  updated_at TIMESTAMP WITH TIME ZONE DEFAULT NOW()
);
```

## Installation & Setup

### Prerequisites
- Node.js 18+
- PostgreSQL 15+
- Docker & Docker Compose

### Development Setup

1. **Clone and navigate:**
   ```bash
   cd settings-management
   ```

2. **Install dependencies:**
   ```bash
   cd src/DMS.SettingsManagement.API
   npm install
   ```

3. **Set environment variables:**
   ```bash
   export DB_HOST=localhost
   export DB_PORT=5432
   export DB_NAME=dms_settings
   export DB_USER=dms_user
   export DB_PASSWORD=dms_password
   ```

4. **Start the service:**
   ```bash
   npm run dev
   ```

### Docker Setup

1. **Start services:**
   ```bash
   docker-compose up -d
   ```

2. **Check health:**
   ```bash
   curl http://localhost:8090/health
   ```

## Integration

### Frontend Integration

```javascript
import { SettingsProvider, useSettings } from './contexts/SettingsContext';

// Wrap your app
<SettingsProvider user={user}>
  <App />
</SettingsProvider>

// Use in components
const { settings, updateSetting } = useSettings();
```

### API Gateway Integration

Add to your API gateway routes:
```javascript
'/settings': 'http://localhost:8090'
```

## Configuration

### Environment Variables

| Variable | Default | Description |
|----------|---------|-------------|
| `PORT` | 8090 | API server port |
| `DB_HOST` | postgres | Database host |
| `DB_PORT` | 5432 | Database port |
| `DB_NAME` | dms_settings | Database name |
| `DB_USER` | dms_user | Database user |
| `DB_PASSWORD` | dms_password | Database password |

### Default Settings

```javascript
{
  emailNotifications: true,
  smsNotifications: true,
  darkMode: false,
  twoFactorAuth: true,
  loginAlerts: true,
  sessionTimeout: false,
  language: 'en',
  timezone: 'UTC',
  dateFormat: 'MM/DD/YYYY',
  currency: 'USD',
  theme: 'light',
  dashboardLayout: 'grid',
  itemsPerPage: 25,
  autoSave: true,
  notifications: {
    email: true,
    browser: true,
    mobile: false
  },
  privacy: {
    profileVisible: true,
    activityVisible: false,
    statusVisible: true
  }
}
```

## Testing

### Run Tests
```bash
npm test
```

### Test Coverage
```bash
npm run test:coverage
```

### API Testing
```bash
# Health check
curl http://localhost:8090/health

# Get user settings
curl http://localhost:8090/settings/user/1

# Update setting
curl -X PUT http://localhost:8090/settings/user/1 \
  -H "Content-Type: application/json" \
  -d '{"settings": {"darkMode": true}}'
```

## Security

- **Authentication**: Bearer token validation
- **Authorization**: User-specific settings access
- **Input Validation**: JSON schema validation
- **SQL Injection**: Parameterized queries
- **CORS**: Configurable origin policies

## Performance

- **Database Indexing**: Optimized queries
- **Connection Pooling**: PostgreSQL connection management
- **Caching**: Redis integration (optional)
- **Compression**: Response compression

## Monitoring

- **Health Checks**: `/health` endpoint
- **Logging**: Winston structured logging
- **Metrics**: Performance monitoring
- **Alerts**: Error notification system

## Contributing

1. **Fork the repository**
2. **Create feature branch**: `git checkout -b feature/settings-enhancement`
3. **Commit changes**: `git commit -am 'Add new setting category'`
4. **Push to branch**: `git push origin feature/settings-enhancement`
5. **Create Pull Request**

## License

MIT License - see LICENSE file for details

## Support

For issues and questions:
- **GitHub Issues**: [settings-management/issues](link)
- **Documentation**: [settings-docs](link)
- **Team Contact**: dms-team@company.com
