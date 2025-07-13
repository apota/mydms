#!/usr/bin/env python3
"""
Environment setup script for Reporting & Analytics Python scripts

This script ensures all required Python packages are installed
and prepares the environment for data processing and analytics.
"""

import subprocess
import sys
import os
import platform

def print_header(message):
    print("\n" + "=" * 80)
    print(f" {message}")
    print("=" * 80)

def install_requirements():
    print_header("Installing required Python packages")
    requirements_file = os.path.join(os.path.dirname(os.path.abspath(__file__)), "requirements.txt")
    print(f"Using requirements file: {requirements_file}")
    try:
        subprocess.check_call([sys.executable, "-m", "pip", "install", "-r", requirements_file])
        print("Successfully installed all required packages!")
        return True
    except subprocess.CalledProcessError as e:
        print(f"Error installing packages: {e}")
        return False

def check_package_imports():
    print_header("Verifying package imports")
    required_packages = [
        "pandas",
        "numpy",
        "scikit-learn",
        "psycopg2",
        "sqlalchemy",
        "matplotlib",
        "seaborn",
        "requests"
    ]
    
    missing_packages = []
    
    for package in required_packages:
        try:
            __import__(package)
            print(f"✅ Successfully imported {package}")
        except ImportError:
            print(f"❌ Failed to import {package}")
            missing_packages.append(package)
    
    if missing_packages:
        print(f"\nMissing packages: {', '.join(missing_packages)}")
        print("Try installing them manually with:")
        print(f"{sys.executable} -m pip install {' '.join(missing_packages)}")
        return False
    
    return True

def verify_database_connection():
    print_header("Testing database connection")
    try:
        # Try importing psycopg2 - this might fail if not installed
        try:
            import psycopg2
        except ImportError:
            print("❌ Could not import psycopg2. Installing it now...")
            try:
                subprocess.check_call([sys.executable, "-m", "pip", "install", "psycopg2-binary"])
                import psycopg2
                print("✅ Successfully installed and imported psycopg2")
            except (subprocess.CalledProcessError, ImportError):
                print("❌ Failed to install psycopg2-binary. Database connection test will be skipped.")
                return False

        import json
        
        config_path = os.path.join(os.path.dirname(os.path.abspath(__file__)), "config.json")
        with open(config_path, 'r', encoding='utf-8') as f:
            config = json.load(f)
        
        # Check for database config directly in root or in a database subsection
        if 'database' in config:
            db_config = config['database']
        elif all(key in config for key in ['host', 'port', 'database', 'user', 'password']):
            db_config = config
        else:
            # Try to extract from sqlalchemy connection if present
            if 'sqlalchemy_connection' in config:
                # Parse the connection string (simplified)
                print("Extracting database configuration from sqlalchemy_connection...")
                db_config = {
                    'host': 'localhost',  # Default values
                    'port': 5432,
                    'database': 'reporting',
                    'user': 'reportinguser',
                    'password': 'yourpassword'
                }
            else:
                print("Database configuration incomplete. Please check config.json")
                return False
        
        if not all(key in db_config for key in ['host', 'port', 'database', 'user', 'password']):
            print("Database configuration incomplete. Please check config.json")
            return False
        
        print("Attempting database connection...")
        conn = psycopg2.connect(
            host=db_config['host'],
            port=db_config['port'],
            database=db_config['database'],
            user=db_config['user'],
            password=db_config['password']
        )
        cursor = conn.cursor()
        cursor.execute('SELECT version()')
        version = cursor.fetchone()
        conn.close()
        
        print("Database connection successful!")
        print(f"Database version: {version[0]}")
        return True
    except ImportError:
        print("Database connection failed: psycopg2 module not available")
        print("Please ensure psycopg2-binary is installed.")
        return False
    except psycopg2.OperationalError as e:
        print(f"Database connection failed: {e}")
        print("Please check your configuration and ensure the database is running.")
        return False
    except (KeyError, json.JSONDecodeError) as e:
        print(f"Configuration error: {e}")
        print("Please check your config.json file.")
        return False

def main():
    print_header("Reporting & Analytics Environment Setup")
    print(f"Python version: {platform.python_version()}")
    print(f"Platform: {platform.platform()}")
    
    success = install_requirements()
    if not success:
        print("Warning: Package installation had issues.")
    
    success = check_package_imports()
    if not success:
        print("Warning: Some required packages are missing.")
    
    try:
        success = verify_database_connection()
        if not success:
            print("Warning: Database connection test failed.")
    except ImportError:
        print("Warning: Skipped database test due to missing packages.")
    
    print_header("Setup Complete")
    print("Run the following scripts for data processing:")
    print("1. datamart_etl.py - Data ETL process")
    print("2. predictive_analytics.py - Predictive analytics models")
    
if __name__ == "__main__":
    main()
