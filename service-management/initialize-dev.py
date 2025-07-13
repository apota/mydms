#!/usr/bin/env python3

import os
import subprocess
import sys
import time

def print_banner(message):
    print("\n" + "=" * 60)
    print(f"  {message}")
    print("=" * 60 + "\n")

def run_command(command, error_message):
    try:
        subprocess.run(command, check=True, shell=True)
        return True
    except subprocess.CalledProcessError:
        print(f"\nError: {error_message}")
        return False

def main():
    print_banner("DMS Service Management Dev Environment Setup")
    
    # Ensure we're in the right directory
    repo_root = os.path.dirname(os.path.abspath(__file__))
    os.chdir(repo_root)
    print(f"Working in: {repo_root}")
    
    # Check if Docker is running
    print("Checking Docker status...")
    if not run_command("docker info > nul 2>&1", "Docker is not running. Please start Docker and try again."):
        return False
    print("Docker is running.")
    
    # Check if .NET SDK is installed
    print("Checking .NET SDK...")
    if not run_command("dotnet --version > nul 2>&1", ".NET SDK is not installed. Please install .NET SDK and try again."):
        return False
    print(".NET SDK is installed.")
    
    # Check if Node.js is installed for web development
    print("Checking Node.js...")
    if not run_command("node --version > nul 2>&1", "Node.js is not installed. Please install Node.js and try again."):
        return False
    print("Node.js is installed.")
    
    # Start Docker services
    print_banner("Starting Docker services")
    if not run_command("docker-compose up -d postgres dynamodb-local", "Failed to start Docker services."):
        return False
    
    print("Waiting for services to initialize...")
    time.sleep(5)  # Give services some time to initialize
    
    # Run EF Core migrations if needed
    ef_core_command = (
        "dotnet ef database update "
        "--project src/DMS.ServiceManagement.Infrastructure "
        "--startup-project src/DMS.ServiceManagement.API"
    )
    
    print_banner("Setting up database")
    print("Running EF Core migrations...")
    if not run_command(ef_core_command, "Failed to run database migrations."):
        choice = input("Do you want to continue without running migrations? (y/n): ")
        if choice.lower() != 'y':
            return False
    else:
        print("Database migrations applied successfully.")
    
    # Set up dynamodb tables
    print("Setting up DynamoDB tables...")
    # Add AWS CLI commands to create tables if needed
    
    print_banner("Setting up development environment")
    
    # Restore .NET packages
    print("Restoring .NET packages...")
    if not run_command("dotnet restore", "Failed to restore .NET packages."):
        return False
    
    # Install Node.js dependencies for Web project
    print("Installing Node.js dependencies...")
    if not run_command("cd src/DMS.ServiceManagement.Web && npm install", "Failed to install Node.js dependencies."):
        return False
    
    print_banner("Development environment setup complete!")
    print("You can now:")
    print("1. Run the API with: dotnet run --project src/DMS.ServiceManagement.API")
    print("2. Run the Web app with: cd src/DMS.ServiceManagement.Web && npm start")
    print("3. Access PostgreSQL at localhost:5432")
    print("4. Access DynamoDB local at localhost:8000")
    print("5. Access DynamoDB Admin UI at http://localhost:8001")
    print("\nOr use docker-compose to run the full stack:")
    print("   docker-compose up")
    
    return True

if __name__ == "__main__":
    success = main()
    sys.exit(0 if success else 1)
