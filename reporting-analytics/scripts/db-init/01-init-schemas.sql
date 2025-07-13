-- Initialize PostgreSQL schemas for Reporting & Analytics
CREATE SCHEMA IF NOT EXISTS reports;
CREATE SCHEMA IF NOT EXISTS analytics;
CREATE SCHEMA IF NOT EXISTS marts;
CREATE SCHEMA IF NOT EXISTS metadata;

-- Create the quartz tables for scheduling jobs
-- This is a simplified version; a complete script would be created by Quartz.NET

CREATE TABLE QRTZ_JOB_DETAILS (
  SCHED_NAME VARCHAR(120) NOT NULL,
  JOB_NAME VARCHAR(200) NOT NULL,
  JOB_GROUP VARCHAR(200) NOT NULL,
  DESCRIPTION VARCHAR(250) NULL,
  JOB_CLASS_NAME VARCHAR(250) NOT NULL,
  IS_DURABLE BOOL NOT NULL,
  IS_NONCONCURRENT BOOL NOT NULL,
  IS_UPDATE_DATA BOOL NOT NULL,
  REQUESTS_RECOVERY BOOL NOT NULL,
  JOB_DATA BYTEA NULL,
  PRIMARY KEY (SCHED_NAME,JOB_NAME,JOB_GROUP)
);

CREATE TABLE QRTZ_TRIGGERS (
  SCHED_NAME VARCHAR(120) NOT NULL,
  TRIGGER_NAME VARCHAR(200) NOT NULL,
  TRIGGER_GROUP VARCHAR(200) NOT NULL,
  JOB_NAME VARCHAR(200) NOT NULL,
  JOB_GROUP VARCHAR(200) NOT NULL,
  DESCRIPTION VARCHAR(250) NULL,
  NEXT_FIRE_TIME BIGINT NULL,
  PREV_FIRE_TIME BIGINT NULL,
  PRIORITY INTEGER NULL,
  TRIGGER_STATE VARCHAR(16) NOT NULL,
  TRIGGER_TYPE VARCHAR(8) NOT NULL,
  START_TIME BIGINT NOT NULL,
  END_TIME BIGINT NULL,
  CALENDAR_NAME VARCHAR(200) NULL,
  MISFIRE_INSTR SMALLINT NULL,
  JOB_DATA BYTEA NULL,
  PRIMARY KEY (SCHED_NAME,TRIGGER_NAME,TRIGGER_GROUP),
  FOREIGN KEY (SCHED_NAME,JOB_NAME,JOB_GROUP)
      REFERENCES QRTZ_JOB_DETAILS(SCHED_NAME,JOB_NAME,JOB_GROUP)
);

-- Reports schema tables
CREATE TABLE reports.report_definition (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    query TEXT NOT NULL,
    parameters JSONB,
    created_by VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_public BOOLEAN NOT NULL DEFAULT FALSE,
    category VARCHAR(50)
);

CREATE TABLE reports.report_execution (
    id UUID PRIMARY KEY,
    report_id UUID NOT NULL REFERENCES reports.report_definition(id),
    executed_by VARCHAR(100) NOT NULL,
    executed_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    parameters JSONB,
    status VARCHAR(20) NOT NULL, -- RUNNING, COMPLETED, FAILED
    results_key VARCHAR(200), -- Reference to cached results in DynamoDB or S3
    error_message TEXT,
    execution_time_ms INTEGER
);

-- Dashboards
CREATE TABLE reports.dashboard (
    id UUID PRIMARY KEY,
    name VARCHAR(200) NOT NULL,
    description TEXT,
    layout JSONB,
    created_by VARCHAR(100) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    is_public BOOLEAN NOT NULL DEFAULT FALSE,
    category VARCHAR(50)
);

CREATE TABLE reports.dashboard_widget (
    id UUID PRIMARY KEY,
    dashboard_id UUID NOT NULL REFERENCES reports.dashboard(id) ON DELETE CASCADE,
    widget_type VARCHAR(50) NOT NULL,
    name VARCHAR(200) NOT NULL,
    config JSONB NOT NULL,
    position JSONB NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Sample data mart tables
CREATE TABLE marts.sales_fact (
    id SERIAL PRIMARY KEY,
    sale_date DATE NOT NULL,
    vehicle_id INTEGER NOT NULL,
    customer_id INTEGER NOT NULL,
    salesperson_id INTEGER NOT NULL,
    sale_amount NUMERIC(12, 2) NOT NULL,
    cost_amount NUMERIC(12, 2) NOT NULL,
    profit_amount NUMERIC(12, 2) NOT NULL,
    vehicle_make VARCHAR(50) NOT NULL,
    vehicle_model VARCHAR(100) NOT NULL,
    vehicle_year INTEGER NOT NULL,
    vehicle_type VARCHAR(50) NOT NULL
);

CREATE TABLE marts.service_fact (
    id SERIAL PRIMARY KEY,
    service_date DATE NOT NULL,
    repair_order_id INTEGER NOT NULL,
    customer_id INTEGER NOT NULL,
    technician_id INTEGER NOT NULL,
    service_amount NUMERIC(12, 2) NOT NULL,
    parts_amount NUMERIC(12, 2) NOT NULL,
    labor_amount NUMERIC(12, 2) NOT NULL,
    labor_hours NUMERIC(5, 2) NOT NULL,
    vehicle_make VARCHAR(50) NOT NULL,
    vehicle_model VARCHAR(100) NOT NULL,
    vehicle_year INTEGER NOT NULL
);

CREATE TABLE analytics.inventory_recommendations (
    id SERIAL PRIMARY KEY,
    make VARCHAR(50) NOT NULL,
    model VARCHAR(100) NOT NULL,
    year INTEGER NOT NULL,
    current_stock INTEGER NOT NULL,
    recommended_stock INTEGER NOT NULL,
    sales_velocity NUMERIC(5, 2) NOT NULL,
    days_supply INTEGER NOT NULL,
    calculation_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE analytics.customer_churn_predictions (
    id SERIAL PRIMARY KEY,
    customer_id VARCHAR(20) NOT NULL,
    first_name VARCHAR(50) NOT NULL,
    last_name VARCHAR(50) NOT NULL,
    churn_probability NUMERIC(5, 4) NOT NULL,
    risk_category VARCHAR(10) NOT NULL,
    lifetime_value NUMERIC(12, 2) NOT NULL,
    days_since_last_purchase INTEGER NOT NULL,
    churn_factors TEXT[] NOT NULL,
    recommended_actions TEXT[] NOT NULL,
    calculation_date TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Add some sample data for testing
INSERT INTO marts.sales_fact (sale_date, vehicle_id, customer_id, salesperson_id, 
    sale_amount, cost_amount, profit_amount, vehicle_make, vehicle_model, vehicle_year, vehicle_type)
VALUES
    ('2023-01-05', 1001, 5001, 101, 35000, 31500, 3500, 'Toyota', 'RAV4', 2023, 'SUV'),
    ('2023-01-08', 1002, 5002, 102, 28000, 25000, 3000, 'Honda', 'Civic', 2023, 'Sedan'),
    ('2023-01-12', 1003, 5003, 103, 55000, 49500, 5500, 'Ford', 'F-150', 2022, 'Truck'),
    ('2023-01-15', 1004, 5004, 101, 38000, 34000, 4000, 'Toyota', 'Camry', 2023, 'Sedan'),
    ('2023-01-20', 1005, 5005, 102, 65000, 59000, 6000, 'Chevrolet', 'Tahoe', 2023, 'SUV'),
    ('2023-01-25', 1006, 5006, 103, 32000, 28500, 3500, 'Honda', 'Accord', 2022, 'Sedan'),
    ('2023-01-30', 1007, 5007, 101, 42000, 37500, 4500, 'Toyota', 'Highlander', 2022, 'SUV');

INSERT INTO marts.service_fact (service_date, repair_order_id, customer_id, technician_id,
    service_amount, parts_amount, labor_amount, labor_hours, vehicle_make, vehicle_model, vehicle_year)
VALUES
    ('2023-01-10', 2001, 5001, 201, 350, 150, 200, 2.0, 'Toyota', 'RAV4', 2021),
    ('2023-01-12', 2002, 5008, 202, 650, 350, 300, 3.0, 'Honda', 'Accord', 2020),
    ('2023-01-15', 2003, 5003, 203, 180, 80, 100, 1.0, 'Ford', 'F-150', 2019),
    ('2023-01-18', 2004, 5009, 201, 420, 220, 200, 2.0, 'Toyota', 'Corolla', 2022),
    ('2023-01-22', 2005, 5005, 202, 980, 580, 400, 4.0, 'Chevrolet', 'Malibu', 2020);

INSERT INTO analytics.inventory_recommendations (make, model, year, current_stock, recommended_stock, sales_velocity, days_supply)
VALUES
    ('Toyota', 'RAV4', 2023, 12, 18, 0.9, 13),
    ('Honda', 'Civic', 2023, 15, 10, 0.5, 30),
    ('Ford', 'F-150', 2022, 8, 12, 0.6, 13),
    ('Toyota', 'Camry', 2023, 10, 10, 0.4, 25),
    ('Chevrolet', 'Tahoe', 2023, 5, 3, 0.1, 50),
    ('Honda', 'Accord', 2022, 7, 8, 0.3, 23),
    ('Toyota', 'Highlander', 2022, 6, 9, 0.4, 15);

INSERT INTO analytics.customer_churn_predictions (customer_id, first_name, last_name, churn_probability, risk_category, 
    lifetime_value, days_since_last_purchase, churn_factors, recommended_actions)
VALUES
    ('C1001', 'John', 'Smith', 0.87, 'High', 45000, 180, 
     ARRAY['Limited service visits', 'No response to promotions', 'New vehicle purchase due'], 
     ARRAY['Personal call from manager', 'Special trade-in offer', 'Service discount']),
    ('C1254', 'Jane', 'Doe', 0.72, 'High', 32000, 145, 
     ARRAY['Bad service experience', 'Multiple vehicle issues', 'Long service wait times'], 
     ARRAY['Service recovery plan', 'Complimentary service', 'Express service option']),
    ('C2187', 'Robert', 'Johnson', 0.54, 'Medium', 28000, 90, 
     ARRAY['Infrequent service visits', 'Declining service satisfaction'], 
     ARRAY['Service reminder', 'Loyalty rewards promotion']),
    ('C3042', 'Maria', 'Garcia', 0.35, 'Low', 52000, 60, 
     ARRAY['Recent vehicle purchase', 'Regular service visits'], 
     ARRAY['Ownership anniversary recognition', 'Referral program information']),
    ('C4201', 'James', 'Williams', 0.68, 'High', 37500, 120, 
     ARRAY['Missed service appointments', 'Competitor service visits', 'Price sensitivity'], 
     ARRAY['Price match guarantee', 'Service convenience options', 'Customer appreciation offer']);
