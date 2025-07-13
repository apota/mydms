# DMS Reporting & Analytics - Usage Guide

## Getting Started

This guide explains how to run and use the Reporting & Analytics module of the Dealership Management System.

### Prerequisites

- Node.js 16+ for the frontend application
- .NET 7.0+ for the backend API
- PostgreSQL for the database
- Python 3.8+ for advanced analytics features

### Running the Application

#### Development Mode

1. **Initialize the Development Environment**

   Run the initialization script to set up the database and required services:

   ```
   initialize-dev.cmd
   ```

2. **Start the Backend API**

   Open a new terminal and run:

   ```
   cd src\DMS.ReportingAnalytics.API
   dotnet run
   ```

   The API will be available at https://localhost:7127 or http://localhost:5000

3. **Start the Frontend Application**

   Open a new terminal and run:

   ```
   start-web-dev-server.cmd
   ```

   Or manually:

   ```
   cd src\DMS.ReportingAnalytics.Web
   npm start
   ```

   The web application will be available at http://localhost:3000

#### Production Mode

For production deployment, follow these steps:

1. Build the backend API:
   ```
   dotnet publish src\DMS.ReportingAnalytics.API -c Release
   ```

2. Build the frontend application:
   ```
   cd src\DMS.ReportingAnalytics.Web
   npm run build
   ```

3. Deploy using Docker:
   ```
   docker-compose up -d
   ```

## Using the Analytics Dashboard

The Analytics Dashboard provides various features to analyze dealership performance:

### Key Metrics Tab

Displays key performance indicators (KPIs) across different departments:
- Sales metrics (total sales, new/used vehicle sales)
- Service department metrics (service revenue, RO count)
- Parts metrics (parts sales, inventory turnover)
- Financial metrics (gross profit, expenses)

You can filter KPIs by department using the dropdown menu.

### Forecasts Tab

Shows sales forecasting projections based on historical data:
- Select different metrics (total sales, new/used vehicle sales, etc.)
- Choose time granularity (daily, weekly, monthly, quarterly)
- Select forecast periods (3, 6, 12, or 24 periods)

The forecast includes prediction intervals showing the upper and lower bounds of the forecast with the confidence level displayed.

### Inventory Tab

Provides inventory optimization recommendations:
- Shows current stock levels vs. recommended levels
- Highlights vehicles that need stock adjustment (increase, decrease, maintain)
- Displays sales velocity and days supply metrics
- Export recommendations to CSV for inventory planning

### Customer Insights Tab

Shows customer churn risk predictions:
- Identifies customers at risk of leaving
- Displays risk score and risk category
- Shows customer lifetime value
- Provides insights into churn factors
- Suggests recommended actions to retain customers

## Running Advanced Analytics

The module includes Python scripts for more advanced analytics:

1. **Data Mart ETL**:
   ```
   cd scripts
   python datamart_etl.py
   ```

2. **Predictive Analytics**:
   ```
   cd scripts
   python predictive_analytics.py
   ```

## Generating Reports

1. Navigate to the Reports section in the sidebar
2. Select a report template or create a new one
3. Set parameters and filters
4. Generate the report
5. Export to PDF, Excel, or CSV as needed

## Troubleshooting

If you encounter issues, refer to the [TROUBLESHOOTING.md](TROUBLESHOOTING.md) file for common problems and solutions.

For more details on implementation, see [README-IMPLEMENTATION.md](README-IMPLEMENTATION.md).
