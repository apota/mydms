#!/usr/bin/env python3
"""
ETL Script for Reporting & Analytics Data Mart Population

This script extracts data from various DMS modules, transforms it according to
the data mart schema definitions, and loads it into the reporting database.

Usage:
    python datamart_etl.py [--config CONFIG_FILE] [--mart MART_NAME] [--full-refresh]

Options:
    --config CONFIG_FILE    Path to configuration file (default: config.json)
    --mart MART_NAME        Name of specific data mart to refresh (default: all)
    --full-refresh          Perform full refresh instead of incremental
"""

import argparse
import json
import logging
import os
import sys
import time
from datetime import datetime, timedelta

import pandas as pd
import requests

# Set up logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler("datamart_etl.log"),
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger("DataMartETL")

# Handle potentially missing database dependencies
try:
    import psycopg2
    from psycopg2.extras import execute_values
except ImportError:
    logger.error("psycopg2 is required. Please install it with: pip install psycopg2-binary")
    psycopg2 = None

try:
    from sqlalchemy import create_engine
except ImportError:
    logger.error("sqlalchemy is required. Please install it with: pip install sqlalchemy")
    create_engine = None

# Set up logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler("datamart_etl.log"),
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger("DataMartETL")

class DataMartETL:
    def __init__(self, config_path='config.json'):
        """Initialize the ETL process"""
        logger.info("Initializing Data Mart ETL")
        
        # Load configuration
        with open(config_path, 'r', encoding='utf-8') as f:
            self.config = json.load(f)
        
        # Connect to PostgreSQL
        if psycopg2 is None:
            raise ImportError("psycopg2 is required for this script")
        if create_engine is None:
            raise ImportError("sqlalchemy is required for this script")
            
        self.conn = psycopg2.connect(self.config['db_connection'])
        self.engine = create_engine(self.config['sqlalchemy_connection'])
        
        # Initialize API session
        self.session = requests.Session()
        
    def close(self):
        """Close database connections"""
        if self.conn:
            self.conn.close()
        
    def extract_module_data(self, module, entity, last_extract_time=None):
        """Extract data from a module API"""
        logger.info("Extracting %s data from %s module", entity, module)
        
        try:
            # Build API URL
            base_url = self.config['module_apis'][module]
            url = f"{base_url}/api/reporting/{entity}"
            
            params = {}
            if last_extract_time:
                params['changedSince'] = last_extract_time.isoformat()
            
            # Make API request
            response = self.session.get(url, params=params)
            response.raise_for_status()
            
            # Convert to dataframe
            data = response.json()
            df = pd.DataFrame(data)
            
            logger.info("Extracted %d records from %s.%s", len(df), module, entity)
            return df
            
        except Exception as e:
            logger.error("Error extracting data from %s.%s: %s", module, entity, str(e))
            raise
    
    def transform_sales_data(self, sales_df, vehicles_df, customers_df):
        """Transform sales data for the sales analytics data mart"""
        logger.info("Transforming sales data")
        
        try:
            # Merge sales with vehicle and customer data
            df = sales_df.merge(vehicles_df, on='VehicleId', how='left')
            df = df.merge(customers_df, on='CustomerId', how='left')
            
            # Add date dimensions
            df['SaleDate'] = pd.to_datetime(df['SaleDate'])
            df['SaleYear'] = df['SaleDate'].dt.year
            df['SaleQuarter'] = df['SaleDate'].dt.quarter
            df['SaleMonth'] = df['SaleDate'].dt.month
            df['SaleDay'] = df['SaleDate'].dt.day
            df['SaleDayOfWeek'] = df['SaleDate'].dt.dayofweek
            
            # Calculate profit
            df['GrossProfit'] = df['SalePrice'] - df['DealerCost']
            
            # Return transformed dataframe
            return df
            
        except Exception as e:
            logger.error("Error transforming sales data: %s", str(e))
            raise
    
    def transform_service_data(self, service_df, technicians_df, vehicles_df):
        """Transform service data for the service analytics data mart"""
        logger.info("Transforming service data")
        
        try:
            # Merge service data with technician and vehicle data
            df = service_df.merge(technicians_df, on='TechnicianId', how='left')
            df = df.merge(vehicles_df, on='VehicleId', how='left')
            
            # Add date dimensions
            df['ServiceDate'] = pd.to_datetime(df['CompletedDate'])
            df['ServiceYear'] = df['ServiceDate'].dt.year
            df['ServiceQuarter'] = df['ServiceDate'].dt.quarter
            df['ServiceMonth'] = df['ServiceDate'].dt.month
            
            # Calculate KPIs
            df['ServiceEfficiency'] = df['LaborHours'] / df['EstimatedHours']
            
            # Return transformed dataframe
            return df
            
        except Exception as e:
            logger.error("Error transforming service data: %s", str(e))
            raise
    
    def transform_inventory_data(self, inventory_df, vehicles_df):
        """Transform inventory data for the inventory analytics data mart"""
        logger.info("Transforming inventory data")
        
        try:
            # Merge inventory with vehicle data
            df = inventory_df.merge(vehicles_df, on='VehicleId', how='left')
            
            # Calculate days in inventory
            df['ReceivedDate'] = pd.to_datetime(df['ReceivedDate'])
            today = datetime.now().date()
            df['DaysInInventory'] = (today - df['ReceivedDate'].dt.date).dt.days
            
            # Set inventory age buckets
            df['AgeBucket'] = pd.cut(
                df['DaysInInventory'],
                bins=[0, 30, 60, 90, float('inf')],
                labels=['0-30', '31-60', '61-90', '90+']
            )
            
            # Return transformed dataframe
            return df
            
        except Exception as e:
            logger.error("Error transforming inventory data: %s", str(e))
            raise
    
    def transform_customer_data(self, customers_df, interactions_df, sales_df, service_df):
        """Transform customer data for the customer analytics data mart"""
        logger.info("Transforming customer data")
        
        try:
            # Start with customer base data
            df = customers_df.copy()
            
            # Calculate customer metrics
            # Sales count and total spent
            sales_by_customer = sales_df.groupby('CustomerId').agg({
                'SaleId': 'count',
                'SalePrice': 'sum'
            }).rename(columns={
                'SaleId': 'TotalPurchases',
                'SalePrice': 'TotalSpent'
            }).reset_index()
            
            # Service count and total spent
            service_by_customer = service_df.groupby('CustomerId').agg({
                'ServiceOrderId': 'count',
                'TotalCost': 'sum'
            }).rename(columns={
                'ServiceOrderId': 'TotalServiceVisits',
                'TotalCost': 'TotalServiceSpent'
            }).reset_index()
            
            # Interaction count
            interaction_by_customer = interactions_df.groupby('CustomerId').size().reset_index(name='InteractionCount')
            
            # Merge all customer metrics
            df = df.merge(sales_by_customer, on='CustomerId', how='left')
            df = df.merge(service_by_customer, on='CustomerId', how='left')
            df = df.merge(interaction_by_customer, on='CustomerId', how='left')
            
            # Fill NaN values with 0 for numerical columns
            for col in ['TotalPurchases', 'TotalSpent', 'TotalServiceVisits', 'TotalServiceSpent', 'InteractionCount']:
                if col in df.columns:
                    df[col] = df[col].fillna(0)
            
            # Calculate customer lifetime value (simple version)
            df['LifetimeValue'] = df['TotalSpent'] + df['TotalServiceSpent']
            
            # Create customer segments based on RFM (Recency, Frequency, Monetary)
            # This is a simplified version
            df['RFM_Score'] = (
                df['TotalPurchases'] * 0.3 +
                df['TotalServiceVisits'] * 0.3 +
                df['LifetimeValue'] * 0.4
            )
            
            # Assign segments based on RFM score quantiles
            df['CustomerSegment'] = pd.qcut(
                df['RFM_Score'],
                q=[0, 0.25, 0.5, 0.75, 1],
                labels=['Low Value', 'Medium Value', 'High Value', 'Premium']
            )
            
            return df
            
        except Exception as e:
            logger.error("Error transforming customer data: %s", str(e))
            raise
    
    def load_data_mart(self, df, mart_name, schema_name='marts'):
        """Load transformed data into a data mart table"""
        logger.info("Loading data into %s data mart", mart_name)
        
        try:
            # Write to PostgreSQL
            table_name = f"{schema_name}.{mart_name}"
            
            # First drop temp table if it exists
            with self.conn.cursor() as cursor:
                cursor.execute(f"DROP TABLE IF EXISTS {table_name}_temp")
            
            # Write dataframe to temp table
            df.to_sql(
                f"{mart_name}_temp",
                self.engine,
                schema=schema_name,
                if_exists='replace',
                index=False
            )
            
            # Replace production table with temp table
            with self.conn.cursor() as cursor:
                # Drop production table and rename temp
                cursor.execute(f"DROP TABLE IF EXISTS {table_name}")
                cursor.execute(f"ALTER TABLE {table_name}_temp RENAME TO {mart_name}")
            
            # Commit the transaction
            self.conn.commit()
            
            logger.info("Successfully loaded %d records into %s", len(df), mart_name)
            
        except Exception as e:
            self.conn.rollback()
            logger.error("Error loading data into %s: %s", mart_name, str(e))
            raise
    
    def refresh_sales_mart(self, full_refresh=False):
        """Refresh the sales analytics data mart"""
        logger.info("Refreshing sales analytics data mart")
        
        try:
            # Get last extract time unless doing full refresh
            last_extract_time = None
            if not full_refresh:
                with self.conn.cursor() as cursor:
                    cursor.execute("SELECT MAX(last_refresh_date) FROM marts.data_mart_metadata WHERE mart_name = 'sales_analytics'")
                    result = cursor.fetchone()
                    if result and result[0]:
                        last_extract_time = result[0]
            
            # Extract data
            sales_df = self.extract_module_data('sales', 'sales', last_extract_time)
            vehicles_df = self.extract_module_data('inventory', 'vehicles', last_extract_time)
            customers_df = self.extract_module_data('crm', 'customers', last_extract_time)
            
            # Transform data
            transformed_df = self.transform_sales_data(sales_df, vehicles_df, customers_df)
            
            # Load data mart
            self.load_data_mart(transformed_df, 'sales_analytics')
            
            # Update metadata
            with self.conn.cursor() as cursor:
                cursor.execute("""
                    INSERT INTO marts.data_mart_metadata (mart_name, last_refresh_date, record_count)
                    VALUES ('sales_analytics', NOW(), %s)
                    ON CONFLICT (mart_name) DO UPDATE
                    SET last_refresh_date = NOW(), record_count = %s
                """, (len(transformed_df), len(transformed_df)))
            self.conn.commit()
            
        except Exception as e:
            logger.error("Error refreshing sales analytics data mart: %s", str(e))
            raise
    
    def refresh_service_mart(self, full_refresh=False):
        """Refresh the service analytics data mart"""
        logger.info("Refreshing service analytics data mart")
        
        try:
            # Get last extract time unless doing full refresh
            last_extract_time = None
            if not full_refresh:
                with self.conn.cursor() as cursor:
                    cursor.execute("SELECT MAX(last_refresh_date) FROM marts.data_mart_metadata WHERE mart_name = 'service_analytics'")
                    result = cursor.fetchone()
                    if result and result[0]:
                        last_extract_time = result[0]
            
            # Extract data
            service_df = self.extract_module_data('service', 'ServiceOrders', last_extract_time)
            technicians_df = self.extract_module_data('service', 'TechnicianPerformance', last_extract_time)
            vehicles_df = self.extract_module_data('inventory', 'vehicles', last_extract_time)
            
            # Transform data
            transformed_df = self.transform_service_data(service_df, technicians_df, vehicles_df)
            
            # Load data mart
            self.load_data_mart(transformed_df, 'service_analytics')
            
            # Update metadata
            with self.conn.cursor() as cursor:
                cursor.execute("""
                    INSERT INTO marts.data_mart_metadata (mart_name, last_refresh_date, record_count)
                    VALUES ('service_analytics', NOW(), %s)
                    ON CONFLICT (mart_name) DO UPDATE
                    SET last_refresh_date = NOW(), record_count = %s
                """, (len(transformed_df), len(transformed_df)))
            self.conn.commit()
            
        except Exception as e:
            logger.error("Error refreshing service analytics data mart: %s", str(e))
            raise
    
    def refresh_inventory_mart(self, full_refresh=False):
        """Refresh the inventory analytics data mart"""
        logger.info("Refreshing inventory analytics data mart")
        
        try:
            # Get last extract time unless doing full refresh
            last_extract_time = None
            if not full_refresh:
                with self.conn.cursor() as cursor:
                    cursor.execute("SELECT MAX(last_refresh_date) FROM marts.data_mart_metadata WHERE mart_name = 'inventory_analytics'")
                    result = cursor.fetchone()
                    if result and result[0]:
                        last_extract_time = result[0]
            
            # Extract data
            inventory_df = self.extract_module_data('inventory', 'inventory', last_extract_time)
            vehicles_df = self.extract_module_data('inventory', 'vehicles', last_extract_time)
            
            # Transform data
            transformed_df = self.transform_inventory_data(inventory_df, vehicles_df)
            
            # Load data mart
            self.load_data_mart(transformed_df, 'inventory_analytics')
            
            # Update metadata
            with self.conn.cursor() as cursor:
                cursor.execute("""
                    INSERT INTO marts.data_mart_metadata (mart_name, last_refresh_date, record_count)
                    VALUES ('inventory_analytics', NOW(), %s)
                    ON CONFLICT (mart_name) DO UPDATE
                    SET last_refresh_date = NOW(), record_count = %s
                """, (len(transformed_df), len(transformed_df)))
            self.conn.commit()
            
        except Exception as e:
            logger.error("Error refreshing inventory analytics data mart: %s", str(e))
            raise
    
    def refresh_customer_mart(self, full_refresh=False):
        """Refresh the customer analytics data mart"""
        logger.info("Refreshing customer analytics data mart")
        
        try:
            # Get last extract time unless doing full refresh
            last_extract_time = None
            if not full_refresh:
                with self.conn.cursor() as cursor:
                    cursor.execute("SELECT MAX(last_refresh_date) FROM marts.data_mart_metadata WHERE mart_name = 'customer_analytics'")
                    result = cursor.fetchone()
                    if result and result[0]:
                        last_extract_time = result[0]
            
            # Extract data
            customers_df = self.extract_module_data('crm', 'customers', last_extract_time)
            interactions_df = self.extract_module_data('crm', 'CustomerInteractions', last_extract_time)
            sales_df = self.extract_module_data('sales', 'sales', last_extract_time)
            service_df = self.extract_module_data('service', 'ServiceOrders', last_extract_time)
            
            # Transform data
            transformed_df = self.transform_customer_data(customers_df, interactions_df, sales_df, service_df)
            
            # Load data mart
            self.load_data_mart(transformed_df, 'customer_analytics')
            
            # Update metadata
            with self.conn.cursor() as cursor:
                cursor.execute("""
                    INSERT INTO marts.data_mart_metadata (mart_name, last_refresh_date, record_count)
                    VALUES ('customer_analytics', NOW(), %s)
                    ON CONFLICT (mart_name) DO UPDATE
                    SET last_refresh_date = NOW(), record_count = %s
                """, (len(transformed_df), len(transformed_df)))
            self.conn.commit()
            
        except Exception as e:
            logger.error("Error refreshing customer analytics data mart: %s", str(e))
            raise
    
    def refresh_all_marts(self, full_refresh=False):
        """Refresh all data marts"""
        refresh_type = 'full' if full_refresh else 'incremental'
        logger.info("Starting %s refresh of all data marts", refresh_type)
        
        try:
            self.refresh_sales_mart(full_refresh)
            self.refresh_service_mart(full_refresh)
            self.refresh_inventory_mart(full_refresh)
            self.refresh_customer_mart(full_refresh)
            logger.info("All data marts refreshed successfully")
            
        except Exception as e:
            logger.error("Error refreshing data marts: %s", str(e))
            raise

def main():
    """Main entry point for the ETL script"""
    parser = argparse.ArgumentParser(description="Data Mart ETL Process")
    parser.add_argument("--config", default="config.json", help="Path to configuration file")
    parser.add_argument("--mart", default=None, help="Specific data mart to refresh")
    parser.add_argument("--full-refresh", action="store_true", help="Perform full refresh instead of incremental")
    
    args = parser.parse_args()
    
    try:
        etl = DataMartETL(args.config)
        
        if args.mart:
            if args.mart == 'sales':
                etl.refresh_sales_mart(args.full_refresh)
            elif args.mart == 'service':
                etl.refresh_service_mart(args.full_refresh)
            elif args.mart == 'inventory':
                etl.refresh_inventory_mart(args.full_refresh)
            elif args.mart == 'customer':
                etl.refresh_customer_mart(args.full_refresh)
            else:
                logger.error("Unknown data mart: %s", args.mart)
                sys.exit(1)
        else:
            etl.refresh_all_marts(args.full_refresh)
            
    except ImportError as e:
        logger.error("ETL process failed due to missing dependencies: %s", str(e))
        sys.exit(1)
    except ValueError as e:
        logger.error("ETL process failed due to invalid configuration: %s", str(e))
        sys.exit(1)
    except Exception as e:
        logger.error("ETL process failed with unexpected error: %s", str(e))
        sys.exit(1)
    finally:
        if 'etl' in locals():
            etl.close()
    
    logger.info("ETL process completed successfully")

if __name__ == "__main__":
    main()
