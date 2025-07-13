# Python Environment Setup Guide

This guide explains how to set up the Python environment needed for the Reporting & Analytics module.

## Prerequisites

- Python 3.8 or higher
- pip (Python package manager)
- virtualenv (recommended for isolated environments)

## Setup Steps

1. **Create a Virtual Environment (optional but recommended)**

   ```bash
   # Windows
   python -m venv venv
   venv\Scripts\activate

   # macOS/Linux  
   python3 -m venv venv
   source venv/bin/activate
   ```

2. **Install Required Packages**

   ```bash
   # Navigate to the scripts directory
   cd reporting-analytics/scripts

   # Install all required packages
   pip install -r requirements.txt
   ```

   If you encounter issues with some packages, try installing them individually:

   ```bash
   pip install pandas numpy
   pip install scikit-learn
   pip install psycopg2-binary
   pip install sqlalchemy
   ```

3. **Run the Environment Setup Script**

   ```bash
   python initialize-env.py
   ```

   This script will:
   - Verify all required packages are installed
   - Test database connectivity
   - Ensure the environment is properly configured

## Troubleshooting

### Common Issues:

1. **Missing compiler for psycopg2**
   - Solution: Install psycopg2-binary instead of psycopg2:
     ```
     pip install psycopg2-binary
     ```

2. **Prophet installation issues**
   - Solution: Install Prophet dependencies first:
     ```
     # Windows
     pip install pystan==2.19.1.1
     
     # Then install prophet
     pip install prophet
     ```

3. **Database connectivity issues**
   - Ensure PostgreSQL is running
   - Check that config.json has the correct credentials
   - Verify that the database has been created

## Running the Analytics Scripts

After setup is complete, you can run:

```bash
# ETL process to populate data marts
python datamart_etl.py

# Predictive analytics models
python predictive_analytics.py
```

For additional parameters and options, use the `--help` flag with each script.
