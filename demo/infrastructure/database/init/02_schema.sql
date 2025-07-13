-- Main DMS Database Schema
-- This script creates the core tables used across the DMS system

-- Enable UUID extension
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

-- Users and Authentication
CREATE TABLE IF NOT EXISTS users (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    phone VARCHAR(20),
    role VARCHAR(50) NOT NULL DEFAULT 'user',
    department VARCHAR(100),
    employee_id VARCHAR(50) UNIQUE,
    is_active BOOLEAN DEFAULT true,
    mfa_enabled BOOLEAN DEFAULT false,
    mfa_secret VARCHAR(255),
    last_login_at TIMESTAMP,
    password_changed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Vehicles Inventory
CREATE TABLE IF NOT EXISTS vehicles (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    vin VARCHAR(17) UNIQUE NOT NULL,
    stock_number VARCHAR(50) UNIQUE NOT NULL,
    make VARCHAR(100) NOT NULL,
    model VARCHAR(100) NOT NULL,
    year INTEGER NOT NULL,
    trim VARCHAR(100),
    exterior_color VARCHAR(50),
    interior_color VARCHAR(50),
    mileage INTEGER DEFAULT 0,
    condition VARCHAR(20) DEFAULT 'new', -- new, used, certified
    status VARCHAR(50) DEFAULT 'available', -- available, sold, pending, service
    purchase_price DECIMAL(10,2),
    asking_price DECIMAL(10,2),
    invoice_price DECIMAL(10,2),
    msrp DECIMAL(10,2),
    acquisition_date DATE,
    days_in_inventory INTEGER DEFAULT 0,
    location VARCHAR(100),
    fuel_type VARCHAR(30),
    transmission VARCHAR(50),
    engine VARCHAR(100),
    drivetrain VARCHAR(30),
    features TEXT,
    description TEXT,
    images JSONB,
    documents JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Customers
CREATE TABLE IF NOT EXISTS customers (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    customer_number VARCHAR(50) UNIQUE NOT NULL,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    email VARCHAR(255),
    phone VARCHAR(20),
    mobile VARCHAR(20),
    date_of_birth DATE,
    drivers_license VARCHAR(50),
    ssn_last_4 VARCHAR(4),
    address_line1 VARCHAR(255),
    address_line2 VARCHAR(255),
    city VARCHAR(100),
    state VARCHAR(50),
    zip_code VARCHAR(20),
    country VARCHAR(100) DEFAULT 'USA',
    credit_score INTEGER,
    customer_type VARCHAR(50) DEFAULT 'retail', -- retail, wholesale, employee
    referral_source VARCHAR(100),
    marketing_opt_in BOOLEAN DEFAULT false,
    notes TEXT,
    tags JSONB,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Sales Leads
CREATE TABLE IF NOT EXISTS leads (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    lead_number VARCHAR(50) UNIQUE NOT NULL,
    customer_id UUID REFERENCES customers(id),
    assigned_salesperson_id UUID REFERENCES users(id),
    source VARCHAR(100), -- website, walk-in, phone, referral, etc.
    status VARCHAR(50) DEFAULT 'new', -- new, contacted, qualified, proposal, negotiation, closed-won, closed-lost
    interest_level VARCHAR(20) DEFAULT 'medium', -- low, medium, high, hot
    vehicle_interest JSONB, -- array of vehicle preferences
    budget_min DECIMAL(10,2),
    budget_max DECIMAL(10,2),
    trade_in_vehicle JSONB,
    financing_needed BOOLEAN DEFAULT true,
    timeline VARCHAR(50), -- immediate, 30_days, 60_days, 90_days+
    notes TEXT,
    next_follow_up DATE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Sales Deals
CREATE TABLE IF NOT EXISTS deals (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    deal_number VARCHAR(50) UNIQUE NOT NULL,
    customer_id UUID REFERENCES customers(id) NOT NULL,
    vehicle_id UUID REFERENCES vehicles(id),
    salesperson_id UUID REFERENCES users(id) NOT NULL,
    manager_id UUID REFERENCES users(id),
    status VARCHAR(50) DEFAULT 'pending', -- pending, approved, funded, delivered, cancelled
    deal_type VARCHAR(50) DEFAULT 'retail', -- retail, lease, wholesale
    vehicle_price DECIMAL(10,2),
    trade_in_value DECIMAL(10,2) DEFAULT 0,
    down_payment DECIMAL(10,2) DEFAULT 0,
    loan_amount DECIMAL(10,2),
    monthly_payment DECIMAL(10,2),
    loan_term INTEGER, -- months
    apr DECIMAL(5,4),
    finance_company VARCHAR(100),
    total_accessories DECIMAL(10,2) DEFAULT 0,
    total_fi_products DECIMAL(10,2) DEFAULT 0, -- Finance & Insurance products
    doc_fee DECIMAL(10,2) DEFAULT 0,
    sales_tax DECIMAL(10,2) DEFAULT 0,
    total_amount DECIMAL(10,2),
    gross_profit DECIMAL(10,2),
    commission_amount DECIMAL(10,2),
    delivery_date DATE,
    contract_signed_at TIMESTAMP,
    funded_at TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Service Appointments
CREATE TABLE IF NOT EXISTS service_appointments (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    appointment_number VARCHAR(50) UNIQUE NOT NULL,
    customer_id UUID REFERENCES customers(id) NOT NULL,
    vehicle_id UUID REFERENCES vehicles(id),
    advisor_id UUID REFERENCES users(id),
    technician_id UUID REFERENCES users(id),
    appointment_date TIMESTAMP NOT NULL,
    status VARCHAR(50) DEFAULT 'scheduled', -- scheduled, in_progress, completed, cancelled
    service_type VARCHAR(100), -- maintenance, repair, warranty, recall
    description TEXT,
    mileage_in INTEGER,
    estimated_completion TIMESTAMP,
    actual_completion TIMESTAMP,
    labor_hours DECIMAL(4,2),
    labor_rate DECIMAL(6,2),
    parts_total DECIMAL(10,2) DEFAULT 0,
    labor_total DECIMAL(10,2) DEFAULT 0,
    total_amount DECIMAL(10,2) DEFAULT 0,
    warranty_claim BOOLEAN DEFAULT false,
    customer_approved BOOLEAN DEFAULT false,
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Parts Inventory
CREATE TABLE IF NOT EXISTS parts (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    part_number VARCHAR(100) UNIQUE NOT NULL,
    description VARCHAR(255) NOT NULL,
    category VARCHAR(100),
    subcategory VARCHAR(100),
    manufacturer VARCHAR(100),
    oem_part_number VARCHAR(100),
    cost DECIMAL(10,2),
    list_price DECIMAL(10,2),
    retail_price DECIMAL(10,2),
    quantity_on_hand INTEGER DEFAULT 0,
    quantity_on_order INTEGER DEFAULT 0,
    reorder_point INTEGER DEFAULT 0,
    reorder_quantity INTEGER DEFAULT 0,
    bin_location VARCHAR(50),
    supplier VARCHAR(100),
    weight DECIMAL(8,2),
    dimensions VARCHAR(100),
    is_core_part BOOLEAN DEFAULT false,
    is_hazmat BOOLEAN DEFAULT false,
    superseded_by VARCHAR(100),
    supersedes VARCHAR(100),
    notes TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Financial Transactions
CREATE TABLE IF NOT EXISTS financial_transactions (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    transaction_number VARCHAR(50) UNIQUE NOT NULL,
    transaction_type VARCHAR(50) NOT NULL, -- sale, purchase, payment, refund, adjustment
    department VARCHAR(50), -- sales, service, parts, fi
    reference_id UUID, -- reference to deal, service_appointment, etc.
    amount DECIMAL(12,2) NOT NULL,
    description TEXT,
    account_code VARCHAR(50),
    posted_date DATE DEFAULT CURRENT_DATE,
    created_by UUID REFERENCES users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Audit Log
CREATE TABLE IF NOT EXISTS audit_log (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    table_name VARCHAR(100) NOT NULL,
    record_id UUID,
    action VARCHAR(20) NOT NULL, -- INSERT, UPDATE, DELETE
    old_values JSONB,
    new_values JSONB,
    changed_by UUID REFERENCES users(id),
    ip_address INET,
    user_agent TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- System Settings
CREATE TABLE IF NOT EXISTS system_settings (
    id UUID PRIMARY KEY DEFAULT uuid_generate_v4(),
    setting_key VARCHAR(100) UNIQUE NOT NULL,
    setting_value TEXT,
    setting_type VARCHAR(50) DEFAULT 'string', -- string, number, boolean, json
    description TEXT,
    category VARCHAR(100),
    is_system BOOLEAN DEFAULT false,
    updated_by UUID REFERENCES users(id),
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes for performance
CREATE INDEX IF NOT EXISTS idx_vehicles_vin ON vehicles(vin);
CREATE INDEX IF NOT EXISTS idx_vehicles_status ON vehicles(status);
CREATE INDEX IF NOT EXISTS idx_vehicles_make_model ON vehicles(make, model);
CREATE INDEX IF NOT EXISTS idx_customers_email ON customers(email);
CREATE INDEX IF NOT EXISTS idx_customers_phone ON customers(phone);
CREATE INDEX IF NOT EXISTS idx_leads_status ON leads(status);
CREATE INDEX IF NOT EXISTS idx_deals_status ON deals(status);
CREATE INDEX IF NOT EXISTS idx_deals_delivery_date ON deals(delivery_date);
CREATE INDEX IF NOT EXISTS idx_appointments_date ON service_appointments(appointment_date);
CREATE INDEX IF NOT EXISTS idx_appointments_status ON service_appointments(status);
CREATE INDEX IF NOT EXISTS idx_parts_number ON parts(part_number);
CREATE INDEX IF NOT EXISTS idx_transactions_type ON financial_transactions(transaction_type);
CREATE INDEX IF NOT EXISTS idx_transactions_date ON financial_transactions(posted_date);
CREATE INDEX IF NOT EXISTS idx_audit_table_action ON audit_log(table_name, action);

-- Update timestamp triggers
CREATE OR REPLACE FUNCTION update_updated_at_column()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = CURRENT_TIMESTAMP;
    RETURN NEW;
END;
$$ language 'plpgsql';

-- Apply update triggers to relevant tables
CREATE TRIGGER update_users_updated_at BEFORE UPDATE ON users FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_vehicles_updated_at BEFORE UPDATE ON vehicles FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_customers_updated_at BEFORE UPDATE ON customers FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_leads_updated_at BEFORE UPDATE ON leads FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_deals_updated_at BEFORE UPDATE ON deals FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_appointments_updated_at BEFORE UPDATE ON service_appointments FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();
CREATE TRIGGER update_parts_updated_at BEFORE UPDATE ON parts FOR EACH ROW EXECUTE FUNCTION update_updated_at_column();

COMMIT;
