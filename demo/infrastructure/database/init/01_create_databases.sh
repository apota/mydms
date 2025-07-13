#!/bin/bash
set -e

# Create additional databases for microservices
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- Authentication Service Database
    CREATE DATABASE dms_auth;
    GRANT ALL PRIVILEGES ON DATABASE dms_auth TO $POSTGRES_USER;

    -- Inventory Service Database
    CREATE DATABASE dms_inventory;
    GRANT ALL PRIVILEGES ON DATABASE dms_inventory TO $POSTGRES_USER;

    -- Sales Service Database
    CREATE DATABASE dms_sales;
    GRANT ALL PRIVILEGES ON DATABASE dms_sales TO $POSTGRES_USER;

    -- Service Department Database
    CREATE DATABASE dms_service;
    GRANT ALL PRIVILEGES ON DATABASE dms_service TO $POSTGRES_USER;

    -- Parts Service Database
    CREATE DATABASE dms_parts;
    GRANT ALL PRIVILEGES ON DATABASE dms_parts TO $POSTGRES_USER;

    -- Financial Service Database
    CREATE DATABASE dms_financial;
    GRANT ALL PRIVILEGES ON DATABASE dms_financial TO $POSTGRES_USER;

    -- User Management Database
    CREATE DATABASE dms_users;
    GRANT ALL PRIVILEGES ON DATABASE dms_users TO $POSTGRES_USER;

    -- Settings Management Database
    CREATE DATABASE dms_settings;
    GRANT ALL PRIVILEGES ON DATABASE dms_settings TO $POSTGRES_USER;

    -- CRM Database
    CREATE DATABASE dms_crm;
    GRANT ALL PRIVILEGES ON DATABASE dms_crm TO $POSTGRES_USER;
EOSQL

echo "✅ All microservice databases created successfully"
