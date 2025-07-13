#!/usr/bin/env python3
"""
Predictive Analytics for Dealership Management System

This script implements predictive models for various dealership processes, including:
1. Sales forecasting
2. Inventory optimization
3. Customer churn prediction
4. Service demand forecasting
5. Parts demand forecasting

Usage:
    python predictive_analytics.py [--config CONFIG_FILE] [--model MODEL_NAME] [--retrain]

Options:
    --config CONFIG_FILE    Path to configuration file (default: config.json)
    --model MODEL_NAME      Name of specific model to run (default: all)
    --retrain               Force retraining of models instead of using cached versions
"""

import argparse
import json
import logging
import os
import pickle
import sys
from datetime import datetime, timedelta

import numpy as np
import pandas as pd
from sklearn.ensemble import RandomForestRegressor, RandomForestClassifier
from sklearn.metrics import mean_squared_error, mean_absolute_error, r2_score, accuracy_score, precision_score, recall_score
from sklearn.model_selection import train_test_split

# Import sqlalchemy safely
try:
    from sqlalchemy import create_engine
except ImportError:
    logging.error("sqlalchemy package is required. Please install it with: pip install sqlalchemy")
    create_engine = None

# Set up logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler("predictive_analytics.log"),
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger("PredictiveAnalytics")

class PredictiveAnalytics:
    def __init__(self, config_path='config.json'):
        """Initialize predictive analytics"""
        logger.info("Initializing Predictive Analytics")
        
        # Load configuration
        with open(config_path, 'r', encoding='utf-8') as f:
            self.config = json.load(f)
        
        # Connect to PostgreSQL
        if create_engine is not None:
            self.engine = create_engine(self.config['sqlalchemy_connection'])
        else:
            logger.error("Could not create database engine - sqlalchemy not installed")
            raise ImportError("sqlalchemy is required for this script")
        
        # Create models directory if it doesn't exist
        os.makedirs('models', exist_ok=True)
    
    def load_data_from_mart(self, mart_name, schema='marts'):
        """Load data from a data mart"""
        logger.info("Loading data from %s data mart", mart_name)
        
        query = f"SELECT * FROM {schema}.{mart_name}"
        df = pd.read_sql(query, self.engine)
        
        logger.info("Loaded %d records from %s", len(df), mart_name)
        return df
    
    def save_model(self, model, model_name):
        """Save a trained model to disk"""
        model_path = os.path.join('models', f"{model_name}.pkl")
        
        with open(model_path, 'wb') as f:
            pickle.dump(model, f)
        
        logger.info("Model saved to %s", model_path)
    
    def load_model(self, model_name):
        """Load a trained model from disk"""
        model_path = os.path.join('models', f"{model_name}.pkl")
        
        if not os.path.exists(model_path):
            logger.error("Model file not found: %s", model_path)
            return None
            
        with open(model_path, 'rb') as f:
            model = pickle.load(f)
        
        logger.info("Model loaded from %s", model_path)
        return model
    
    def save_predictions(self, predictions_df, model_name):
        """Save predictions to database"""
        table_name = f"predictions_{model_name}"
        schema = 'analytics'
        
        predictions_df['prediction_date'] = datetime.now()
        predictions_df.to_sql(table_name, self.engine, schema=schema, if_exists='replace', index=False)
        
        logger.info("Saved %d predictions to %s.%s", len(predictions_df), schema, table_name)
    
    def train_sales_forecast_model(self, retrain=False):
        """Train a sales forecasting model"""
        logger.info("Training sales forecast model")
        
        model_name = 'sales_forecast'
        
        # Check if we should use existing model
        if not retrain:
            existing_model = self.load_model(model_name)
            if existing_model:
                return existing_model
        
        try:
            # Load sales data
            sales_df = self.load_data_from_mart('sales_analytics')
            
            # Feature engineering
            sales_df['SaleDate'] = pd.to_datetime(sales_df['SaleDate'])
            
            # Aggregate by day
            daily_sales = sales_df.groupby(sales_df['SaleDate'].dt.date).agg({
                'SaleId': 'count',
                'SalePrice': 'sum'
            }).reset_index()
            daily_sales.rename(columns={'SaleId': 'SalesCount'}, inplace=True)
            
            # Create features for day of week, month, etc.
            daily_sales['DayOfWeek'] = pd.to_datetime(daily_sales['SaleDate']).dt.dayofweek
            daily_sales['Month'] = pd.to_datetime(daily_sales['SaleDate']).dt.month
            daily_sales['Year'] = pd.to_datetime(daily_sales['SaleDate']).dt.year
            daily_sales['DayOfMonth'] = pd.to_datetime(daily_sales['SaleDate']).dt.day
            
            # Add lag features (previous day, previous week)
            daily_sales['SalesCount_Lag1'] = daily_sales['SalesCount'].shift(1)
            daily_sales['SalesCount_Lag7'] = daily_sales['SalesCount'].shift(7)
            
            # Add rolling average features
            daily_sales['SalesCount_Rolling7'] = daily_sales['SalesCount'].rolling(window=7).mean()
            daily_sales['SalesCount_Rolling30'] = daily_sales['SalesCount'].rolling(window=30).mean()
            
            # Drop rows with NaN values after adding lag features
            daily_sales = daily_sales.dropna()
            
            # Prepare features and target
            X = daily_sales[['DayOfWeek', 'Month', 'Year', 'DayOfMonth', 
                             'SalesCount_Lag1', 'SalesCount_Lag7', 
                             'SalesCount_Rolling7', 'SalesCount_Rolling30']]
            y = daily_sales['SalesCount']
            
            # Split into training and testing sets
            X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
            
            # Train model
            model = RandomForestRegressor(n_estimators=100, random_state=42)
            model.fit(X_train, y_train)
            
            # Evaluate model
            y_pred = model.predict(X_test)
            mse = mean_squared_error(y_test, y_pred)
            mae = mean_absolute_error(y_test, y_pred)
            r2 = r2_score(y_test, y_pred)
            
            logger.info("Sales forecast model performance: MSE=%.2f, MAE=%.2f, R²=%.2f", mse, mae, r2)
            
            # Save model
            self.save_model(model, model_name)
            
            return model
            
        except Exception as e:
            logger.error("Error training sales forecast model: %s", str(e))
            raise
    
    def generate_sales_forecast(self, days_ahead=30, retrain=False):
        """Generate sales forecast for the next N days"""
        logger.info("Generating %d-day sales forecast", days_ahead)
        
        try:
            # Train or load model
            model = self.train_sales_forecast_model(retrain)
            
            # Load recent sales data for forecasting
            sales_df = self.load_data_from_mart('sales_analytics')
            
            # Aggregate by day
            daily_sales = sales_df.groupby(pd.to_datetime(sales_df['SaleDate']).dt.date).agg({
                'SaleId': 'count',
                'SalePrice': 'sum'
            }).reset_index()
            daily_sales.rename(columns={'SaleId': 'SalesCount'}, inplace=True)
            
            # Create features
            daily_sales['DayOfWeek'] = pd.to_datetime(daily_sales['SaleDate']).dt.dayofweek
            daily_sales['Month'] = pd.to_datetime(daily_sales['SaleDate']).dt.month
            daily_sales['Year'] = pd.to_datetime(daily_sales['SaleDate']).dt.year
            daily_sales['DayOfMonth'] = pd.to_datetime(daily_sales['SaleDate']).dt.day
            
            # Add lag features
            daily_sales['SalesCount_Lag1'] = daily_sales['SalesCount'].shift(1)
            daily_sales['SalesCount_Lag7'] = daily_sales['SalesCount'].shift(7)
            daily_sales['SalesCount_Rolling7'] = daily_sales['SalesCount'].rolling(window=7).mean()
            daily_sales['SalesCount_Rolling30'] = daily_sales['SalesCount'].rolling(window=30).mean()
            
            # Get the latest data
            latest_data = daily_sales.iloc[-1]
            
            # Generate forecast
            forecast_dates = []
            forecast_values = []
            
            # Start with the most recent values
            last_count = latest_data['SalesCount']
            # We'll use lag7 for predictions but not lag1 directly
            last_lag7 = latest_data['SalesCount_Lag7']
            rolling7_values = list(daily_sales['SalesCount'].iloc[-7:])
            rolling30_values = list(daily_sales['SalesCount'].iloc[-30:])
            
            last_date = latest_data['SaleDate']
            
            # Predict for each future day
            for i in range(1, days_ahead + 1):
                # Create next date
                next_date = last_date + timedelta(days=i)
                forecast_dates.append(next_date)
                
                # Create features for this date
                features = {
                    'DayOfWeek': next_date.weekday(),
                    'Month': next_date.month,
                    'Year': next_date.year,
                    'DayOfMonth': next_date.day,
                    'SalesCount_Lag1': last_count,
                    'SalesCount_Lag7': last_lag7,
                    'SalesCount_Rolling7': np.mean(rolling7_values),
                    'SalesCount_Rolling30': np.mean(rolling30_values)
                }
                
                # Convert to DataFrame for prediction
                features_df = pd.DataFrame([features])
                
                # Make prediction
                prediction = model.predict(features_df)[0]
                forecast_values.append(prediction)
                
                # Update for next iteration
                last_lag7 = last_count if i == 7 else last_lag7
                # Update the counts for next iteration
                last_count = prediction
                
                # Update rolling values
                rolling7_values.append(prediction)
                rolling7_values = rolling7_values[-7:]
                
                rolling30_values.append(prediction)
                rolling30_values = rolling30_values[-30:]
            
            # Create forecast DataFrame
            forecast_df = pd.DataFrame({
                'ForecastDate': forecast_dates,
                'PredictedSales': forecast_values
            })
            
            # Save predictions
            self.save_predictions(forecast_df, 'sales_forecast')
            
            logger.info("Generated %d-day sales forecast", days_ahead)
            return forecast_df
            
        except Exception as e:
            logger.error("Error generating sales forecast: %s", str(e))
            raise
    
    def train_inventory_optimization_model(self, retrain=False):
        """Train a model for inventory optimization"""
        logger.info("Training inventory optimization model")
        
        model_name = 'inventory_optimization'
        
        # Check if we should use existing model
        if not retrain:
            existing_model = self.load_model(model_name)
            if existing_model:
                return existing_model
        
        try:
            # Load sales and inventory data
            sales_df = self.load_data_from_mart('sales_analytics')
            inventory_df = self.load_data_from_mart('inventory_analytics')
            
            # Feature engineering - group by vehicle attributes
            sales_grouped = sales_df.groupby(['Make', 'Model', 'Year']).agg({
                'SaleId': 'count',
                'DaysInInventory': 'mean'
            }).reset_index()
            sales_grouped.rename(columns={'SaleId': 'SalesCount'}, inplace=True)
            
            # Calculate inventory levels
            inventory_grouped = inventory_df.groupby(['Make', 'Model', 'Year']).size().reset_index(name='InventoryCount')
            
            # Merge sales and inventory data
            merged_df = pd.merge(sales_grouped, inventory_grouped, on=['Make', 'Model', 'Year'], how='inner')
            
            # Calculate inventory-to-sales ratio
            merged_df['InventoryToSalesRatio'] = merged_df['InventoryCount'] / merged_df['SalesCount']
            
            # Create features and target
            X = merged_df[['SalesCount', 'DaysInInventory']]
            y = merged_df['InventoryToSalesRatio']
            
            # Split data
            X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
            
            # Train model
            model = RandomForestRegressor(n_estimators=100, random_state=42)
            model.fit(X_train, y_train)
            
            # Evaluate model
            y_pred = model.predict(X_test)
            mse = mean_squared_error(y_test, y_pred)
            mae = mean_absolute_error(y_test, y_pred)
            r2 = r2_score(y_test, y_pred)
            
            logger.info("Inventory optimization model performance: MSE=%.2f, MAE=%.2f, R²=%.2f", mse, mae, r2)
            
            # Save model
            self.save_model(model, model_name)
            
            return model
            
        except Exception as e:
            logger.error("Error training inventory optimization model: %s", str(e))
            raise
    
    def generate_inventory_recommendations(self, retrain=False):
        """Generate inventory stocking recommendations"""
        logger.info("Generating inventory recommendations")
        
        try:
            # Train or load model
            model = self.train_inventory_optimization_model(retrain)
            
            # Load current inventory and sales data
            sales_df = self.load_data_from_mart('sales_analytics')
            inventory_df = self.load_data_from_mart('inventory_analytics')
            
            # Group sales by vehicle attributes
            sales_grouped = sales_df.groupby(['Make', 'Model', 'Year']).agg({
                'SaleId': 'count',
                'DaysInInventory': 'mean'
            }).reset_index()
            sales_grouped.rename(columns={'SaleId': 'SalesCount'}, inplace=True)
            
            # Calculate current inventory levels
            inventory_grouped = inventory_df.groupby(['Make', 'Model', 'Year']).size().reset_index(name='CurrentInventory')
            
            # Merge data
            vehicle_data = pd.merge(sales_grouped, inventory_grouped, on=['Make', 'Model', 'Year'], how='outer').fillna(0)
            
            # Make predictions for optimal inventory levels
            X_pred = vehicle_data[['SalesCount', 'DaysInInventory']]
            predicted_ratio = model.predict(X_pred)
            
            # Calculate recommended inventory
            vehicle_data['OptimalInventory'] = np.round(vehicle_data['SalesCount'] * predicted_ratio)
            vehicle_data['InventoryDelta'] = vehicle_data['OptimalInventory'] - vehicle_data['CurrentInventory']
            
            # Create recommendations dataframe
            recommendations = vehicle_data[['Make', 'Model', 'Year', 'SalesCount', 'CurrentInventory', 
                                           'OptimalInventory', 'InventoryDelta']].copy()
            
            # Categorize recommendations
            recommendations['Action'] = 'Hold'
            recommendations.loc[recommendations['InventoryDelta'] > 2, 'Action'] = 'Increase'
            recommendations.loc[recommendations['InventoryDelta'] < -2, 'Action'] = 'Reduce'
            
            # Save recommendations
            self.save_predictions(recommendations, 'inventory_recommendations')
            
            logger.info("Generated inventory recommendations for %d vehicle types", len(recommendations))
            return recommendations
            
        except Exception as e:
            logger.error("Error generating inventory recommendations: %s", str(e))
            raise
    
    def train_customer_churn_model(self, retrain=False):
        """Train a model to predict customer churn"""
        logger.info("Training customer churn prediction model")
        
        model_name = 'customer_churn'
        
        # Check if we should use existing model
        if not retrain:
            existing_model = self.load_model(model_name)
            if existing_model:
                return existing_model
        
        try:
            # Load customer data
            customer_df = self.load_data_from_mart('customer_analytics')
            
            # Define churn (simplified - customers who haven't made a purchase or service visit in last 12 months)
            current_date = datetime.now().date()
            customer_df['LastInteraction'] = pd.to_datetime(customer_df['LastInteraction']).dt.date
            customer_df['DaysSinceLastInteraction'] = (current_date - customer_df['LastInteraction']).dt.days
            customer_df['IsChurned'] = customer_df['DaysSinceLastInteraction'] > 365
            
            # Feature engineering
            features = ['TotalPurchases', 'TotalSpent', 'TotalServiceVisits', 
                        'TotalServiceSpent', 'InteractionCount', 'LifetimeValue']
            
            X = customer_df[features]
            y = customer_df['IsChurned']
            
            # Split data
            X_train, X_test, y_train, y_test = train_test_split(X, y, test_size=0.2, random_state=42)
            
            # Train model
            model = RandomForestClassifier(n_estimators=100, random_state=42)
            model.fit(X_train, y_train)
            
            # Evaluate model
            y_pred = model.predict(X_test)
            accuracy = accuracy_score(y_test, y_pred)
            precision = precision_score(y_test, y_pred)
            recall = recall_score(y_test, y_pred)
            
            logger.info("Customer churn model performance: Accuracy=%.2f, Precision=%.2f, Recall=%.2f", 
                      accuracy, precision, recall)
            
            # Save model
            self.save_model(model, model_name)
            
            return model
            
        except Exception as e:
            logger.error("Error training customer churn model: %s", str(e))
            raise
    
    def predict_customer_churn(self, retrain=False):
        """Predict which customers are at risk of churning"""
        logger.info("Predicting customer churn risk")
        
        try:
            # Train or load model
            model = self.train_customer_churn_model(retrain)
            
            # Load customer data
            customer_df = self.load_data_from_mart('customer_analytics')
            
            # Prepare features
            features = ['TotalPurchases', 'TotalSpent', 'TotalServiceVisits', 
                        'TotalServiceSpent', 'InteractionCount', 'LifetimeValue']
            
            X = customer_df[features]
            
            # Make predictions
            y_pred = model.predict(X)
            y_pred_proba = model.predict_proba(X)[:, 1]  # Probability of churn
            
            # Add predictions to customer data
            churn_predictions = customer_df[['CustomerId', 'FirstName', 'LastName', 'Email', 'LifetimeValue']].copy()
            churn_predictions['ChurnProbability'] = y_pred_proba
            churn_predictions['IsChurnRisk'] = y_pred
            
            # Categorize churn risk
            churn_predictions['RiskCategory'] = pd.cut(
                churn_predictions['ChurnProbability'],
                bins=[0, 0.3, 0.7, 1],
                labels=['Low', 'Medium', 'High']
            )
            
            # Save predictions
            self.save_predictions(churn_predictions, 'customer_churn')
            
            logger.info("Predicted churn for %d customers", len(churn_predictions))
            return churn_predictions
            
        except Exception as e:
            logger.error("Error predicting customer churn: %s", str(e))
            raise
    
    def run_all_models(self, retrain=False):
        """Run all predictive models"""
        logger.info("Running all predictive models (retrain=%s)", retrain)
        
        try:
            # Sales forecasting
            self.generate_sales_forecast(days_ahead=30, retrain=retrain)
            
            # Inventory optimization
            self.generate_inventory_recommendations(retrain=retrain)
            
            # Customer churn prediction
            self.predict_customer_churn(retrain=retrain)
            
            logger.info("All predictive models completed successfully")
            
        except Exception as e:
            logger.error("Error running predictive models: %s", str(e))
            raise

def main():
    """Main entry point for predictive analytics"""
    parser = argparse.ArgumentParser(description="Predictive Analytics for DMS")
    parser.add_argument("--config", default="config.json", help="Path to configuration file")
    parser.add_argument("--model", default=None, help="Specific model to run (sales, inventory, customer)")
    parser.add_argument("--retrain", action="store_true", help="Force retraining of models")
    
    args = parser.parse_args()
    
    try:
        analytics = PredictiveAnalytics(args.config)
        
        if args.model:
            if args.model == 'sales':
                analytics.generate_sales_forecast(retrain=args.retrain)
            elif args.model == 'inventory':
                analytics.generate_inventory_recommendations(retrain=args.retrain)
            elif args.model == 'customer':
                analytics.predict_customer_churn(retrain=args.retrain)
            else:
                logger.error("Unknown model: %s", args.model)
                sys.exit(1)
        else:
            analytics.run_all_models(retrain=args.retrain)
            
    except (ImportError, ValueError) as e:
        # Handle specific exceptions with known causes
        logger.error("Configuration error: %s", str(e))
        sys.exit(1)
    except Exception as e:
        # Fall back for unexpected errors
        logger.error("Predictive analytics failed: %s", str(e))
        sys.exit(1)
    
    logger.info("Predictive analytics completed successfully")

if __name__ == "__main__":
    main()
