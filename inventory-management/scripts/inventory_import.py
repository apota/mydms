#!/usr/bin/env python3
"""
Inventory Batch Import Script

This script facilitates the bulk import of vehicle inventory data from CSV files.
It validates the data, transforms it to match the API requirements, and performs
batch imports via the inventory management API.

Usage:
    python inventory_import.py --file INPUT_FILE.csv [--api-url API_URL] [--dry-run]
"""

import argparse
import csv
import json
import logging
import os
import sys
import time
import traceback
from datetime import datetime
from typing import Dict, List, Optional, Any

import requests

# Configure logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler(f"inventory_import_{datetime.now().strftime('%Y%m%d_%H%M%S')}.log"),
        logging.StreamHandler(sys.stdout)
    ]
)
logger = logging.getLogger("inventory_import")

# Default API URL - should be configurable
DEFAULT_API_URL = "http://localhost:5000/api"

# Field mappings from CSV to API model
FIELD_MAPPINGS = {
    "stock_number": "stockNumber",
    "vin": "vin",
    "year": "year",
    "make": "make",
    "model": "model",
    "trim": "trim",
    "body_style": "bodyStyle",
    "exterior_color": "exteriorColor",
    "interior_color": "interiorColor",
    "mileage": "mileage",
    "engine": "engine",
    "transmission": "transmission",
    "drivetrain": "drivetrain",
    "fuel_type": "fuelType",
    "msrp": "msrp",
    "invoice_price": "invoicePrice",
    "list_price": "listPrice",
    "internet_price": "internetPrice",
    "description": "description",
    "features": "features",
    "status": "status"
}

# Required fields that must be present in the CSV
REQUIRED_FIELDS = ["stock_number", "vin", "year", "make", "model"]

# Numeric fields that need type conversion
NUMERIC_FIELDS = ["year", "mileage", "msrp", "invoice_price", "list_price", "internet_price"]


def validate_csv_headers(headers: List[str]) -> bool:
    """Validate that the CSV contains the required headers"""
    for field in REQUIRED_FIELDS:
        if field not in headers:
            logger.error(f"Missing required field: {field}")
            return False
    return True


def transform_row(row: Dict[str, str]) -> Dict[str, Any]:
    """Transform a row from CSV format to API model format"""
    transformed = {}
    
    # Map fields according to FIELD_MAPPINGS
    for csv_field, api_field in FIELD_MAPPINGS.items():
        if csv_field in row:
            transformed[api_field] = row[csv_field]
    
    # Convert numeric fields
    for field in NUMERIC_FIELDS:
        api_field = FIELD_MAPPINGS.get(field)
        if api_field in transformed and transformed[api_field]:
            try:
                if field in ["msrp", "invoice_price", "list_price", "internet_price"]:
                    # Remove currency symbols and commas
                    value = transformed[api_field].replace("$", "").replace(",", "")
                    transformed[api_field] = float(value)
                else:
                    transformed[api_field] = int(transformed[api_field])
            except ValueError:
                logger.warning(f"Could not convert {field} to number: {transformed[api_field]}")
    
    # Parse features if they're in comma-separated format
    if "features" in transformed:
        try:
            if isinstance(transformed["features"], str) and "," in transformed["features"]:
                transformed["features"] = [
                    {"name": feature.strip()} for feature in transformed["features"].split(",")
                ]
        except Exception as e:
            logger.warning(f"Error parsing features: {e}")
    
    return transformed


def import_vehicle(vehicle_data: Dict[str, Any], api_url: str, dry_run: bool) -> Optional[Dict[str, Any]]:
    """Import a single vehicle via the API"""
    if dry_run:
        logger.info(f"DRY RUN - Would import: {json.dumps(vehicle_data, indent=2)}")
        return {"id": "dry-run-id", "stockNumber": vehicle_data.get("stockNumber")}
    
    try:
        response = requests.post(
            f"{api_url}/vehicles",
            json=vehicle_data,
            headers={"Content-Type": "application/json"}
        )
        
        if response.status_code in [200, 201]:
            return response.json()
        else:
            logger.error(f"API error ({response.status_code}): {response.text}")
            return None
    except Exception as e:
        logger.error(f"Exception during API call: {e}")
        return None


def process_csv_file(file_path: str, api_url: str, dry_run: bool) -> Dict[str, int]:
    """Process the CSV file and import vehicles"""
    stats = {"total": 0, "success": 0, "error": 0, "skipped": 0}
    
    try:
        with open(file_path, 'r', encoding='utf-8-sig') as csvfile:
            reader = csv.DictReader(csvfile)
            
            # Validate headers
            if not validate_csv_headers(reader.fieldnames):
                logger.error("CSV validation failed - missing required headers")
                return stats
            
            for row in reader:
                stats["total"] += 1
                
                try:
                    # Transform the data
                    vehicle_data = transform_row(row)
                    
                    # Import the vehicle
                    result = import_vehicle(vehicle_data, api_url, dry_run)
                    
                    if result:
                        logger.info(f"Imported vehicle: {result.get('stockNumber')} (ID: {result.get('id')})")
                        stats["success"] += 1
                    else:
                        logger.error(f"Failed to import vehicle: {row.get('stock_number')}")
                        stats["error"] += 1
                
                except Exception as e:
                    logger.error(f"Error processing row {stats['total']}: {e}")
                    logger.debug(traceback.format_exc())
                    stats["error"] += 1
                
                # Add a small delay to not overwhelm the API
                if not dry_run and stats["total"] % 10 == 0:
                    time.sleep(1)
    
    except FileNotFoundError:
        logger.error(f"File not found: {file_path}")
    except Exception as e:
        logger.error(f"Error processing CSV file: {e}")
        logger.debug(traceback.format_exc())
    
    return stats


def main():
    parser = argparse.ArgumentParser(description="Import vehicle inventory from CSV file")
    parser.add_argument("--file", required=True, help="Input CSV file")
    parser.add_argument("--api-url", default=DEFAULT_API_URL, help=f"API URL (default: {DEFAULT_API_URL})")
    parser.add_argument("--dry-run", action="store_true", help="Dry run - don't actually import")
    
    args = parser.parse_args()
    
    logger.info(f"Starting inventory import from {args.file}")
    logger.info(f"API URL: {args.api_url}")
    logger.info(f"Dry run: {args.dry_run}")
    
    start_time = time.time()
    stats = process_csv_file(args.file, args.api_url, args.dry_run)
    elapsed_time = time.time() - start_time
    
    logger.info("Import completed")
    logger.info(f"Elapsed time: {elapsed_time:.2f} seconds")
    logger.info(f"Total records: {stats['total']}")
    logger.info(f"Successfully imported: {stats['success']}")
    logger.info(f"Errors: {stats['error']}")
    logger.info(f"Skipped: {stats['skipped']}")


if __name__ == "__main__":
    main()
