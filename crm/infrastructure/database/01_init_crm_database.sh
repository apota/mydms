#!/bin/bash
# CRM Database Initialization Script
# This script creates the CRM database and sets up basic permissions

set -e

echo "🗃️ Initializing CRM Database..."

# Create CRM database
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    -- CRM Service Database
    CREATE DATABASE IF NOT EXISTS dms_crm;
    GRANT ALL PRIVILEGES ON DATABASE dms_crm TO $POSTGRES_USER;
EOSQL

echo "✅ CRM database created successfully"

# Connect to CRM database and create initial schema if needed
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "dms_crm" <<-EOSQL
    -- Enable UUID extension
    CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
    
    -- Create schema for CRM tables
    CREATE SCHEMA IF NOT EXISTS crm;
    GRANT ALL PRIVILEGES ON SCHEMA crm TO $POSTGRES_USER;
EOSQL

echo "✅ CRM database schema initialized successfully"
