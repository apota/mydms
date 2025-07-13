using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Service for vehicle operations
    /// </summary>
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _repository;
        private readonly ILogger<VehicleService> _logger;
        
        public VehicleService(
            IVehicleRepository repository,
            ILogger<VehicleService> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets a list of all vehicles
        /// </summary>
        public async Task<IEnumerable<Vehicle>> GetVehiclesAsync(VehicleSearchParameters searchParams = null)
        {
            try
            {
                return await _repository.GetVehiclesAsync(searchParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicles");
                throw;
            }
        }
        
        /// <summary>
        /// Gets a vehicle by ID
        /// </summary>
        public async Task<Vehicle> GetVehicleByIdAsync(Guid id)
        {
            try
            {
                return await _repository.GetVehicleByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle {VehicleId}", id);
                throw;
            }
        }
        
        /// <summary>
        /// Gets detailed vehicle information by ID
        /// </summary>
        public async Task<VehicleDetails> GetVehicleDetailsByIdAsync(Guid id)
        {
            try
            {
                return await _repository.GetVehicleDetailsByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting vehicle details {VehicleId}", id);
                throw;
            }
        }
        
        /// <summary>
        /// Creates a new vehicle
        /// </summary>
        public async Task<Vehicle> CreateVehicleAsync(VehicleDetails vehicle)
        {
            try
            {
                // Validate vehicle data
                ValidateVehicle(vehicle);
                
                // Set created date
                vehicle.CreatedAt = DateTime.UtcNow;
                vehicle.UpdatedAt = DateTime.UtcNow;
                
                // Set days in inventory
                vehicle.DaysInInventory = 0;
                
                return await _repository.AddVehicleAsync(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating vehicle");
                throw;
            }
        }
        
        /// <summary>
        /// Updates an existing vehicle
        /// </summary>
        public async Task<Vehicle> UpdateVehicleAsync(Guid id, VehicleDetails vehicle)
        {
            try
            {
                // Validate vehicle data
                ValidateVehicle(vehicle);
                
                // Get existing vehicle
                var existingVehicle = await _repository.GetVehicleByIdAsync(id);
                if (existingVehicle == null)
                {
                    throw new KeyNotFoundException($"Vehicle not found with ID: {id}");
                }
                
                // Update timestamp
                vehicle.UpdatedAt = DateTime.UtcNow;
                vehicle.Id = id;
                
                return await _repository.UpdateVehicleAsync(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle {VehicleId}", id);
                throw;
            }
        }
        
        /// <summary>
        /// Deletes a vehicle
        /// </summary>
        public async Task<bool> DeleteVehicleAsync(Guid id)
        {
            try
            {
                var vehicle = await _repository.GetVehicleByIdAsync(id);
                if (vehicle == null)
                {
                    throw new KeyNotFoundException($"Vehicle not found with ID: {id}");
                }
                
                return await _repository.DeleteVehicleAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting vehicle {VehicleId}", id);
                throw;
            }
        }
        
        /// <summary>
        /// Gets vehicles that can be listed on marketplaces
        /// </summary>
        public async Task<IEnumerable<VehicleDetails>> GetListableVehiclesAsync()
        {
            try
            {
                var searchParams = new VehicleSearchParameters
                {
                    Status = new[] { "Available", "InRecon", "InTransit" },
                    MaxResults = 100
                };
                
                var vehicles = await _repository.GetVehiclesWithDetailsAsync(searchParams);
                
                // Filter out vehicles that don't have required marketplace information
                return vehicles.Where(v => 
                    !string.IsNullOrEmpty(v.Vin) &&
                    !string.IsNullOrEmpty(v.StockNumber) &&
                    v.RetailPrice > 0 &&
                    !string.IsNullOrEmpty(v.Make) &&
                    !string.IsNullOrEmpty(v.Model) &&
                    v.Year > 0
                ).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting listable vehicles");
                throw;
            }
        }
        
        /// <summary>
        /// Updates vehicle status
        /// </summary>
        public async Task<Vehicle> UpdateVehicleStatusAsync(Guid id, string status, string reason)
        {
            try
            {
                var vehicle = await _repository.GetVehicleByIdAsync(id);
                if (vehicle == null)
                {
                    throw new KeyNotFoundException($"Vehicle not found with ID: {id}");
                }
                
                // Update status
                vehicle.Status = status;
                vehicle.UpdatedAt = DateTime.UtcNow;
                
                // Log status change
                _logger.LogInformation("Vehicle {VehicleId} status changed to {Status}: {Reason}", 
                    id, status, reason);
                
                return await _repository.UpdateVehicleAsync(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating vehicle status {VehicleId}", id);
                throw;
            }
        }
        
        /// <summary>
        /// Transfers a vehicle to a new location
        /// </summary>
        public async Task<Vehicle> TransferVehicleAsync(Guid id, Guid locationId, string reason)
        {
            try
            {
                var vehicle = await _repository.GetVehicleByIdAsync(id);
                if (vehicle == null)
                {
                    throw new KeyNotFoundException($"Vehicle not found with ID: {id}");
                }
                
                // Update location
                // In a real implementation, we would get the location details from a location repository
                vehicle.LocationId = locationId;
                vehicle.UpdatedAt = DateTime.UtcNow;
                
                // Log transfer
                _logger.LogInformation("Vehicle {VehicleId} transferred to location {LocationId}: {Reason}", 
                    id, locationId, reason);
                
                return await _repository.UpdateVehicleAsync(vehicle);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error transferring vehicle {VehicleId}", id);
                throw;
            }
        }
        
        /// <summary>
        /// Validates vehicle data
        /// </summary>
        private void ValidateVehicle(VehicleDetails vehicle)
        {
            if (vehicle == null)
            {
                throw new ArgumentNullException(nameof(vehicle));
            }
            
            if (string.IsNullOrWhiteSpace(vehicle.Vin))
            {
                throw new ArgumentException("VIN is required");
            }
            
            if (string.IsNullOrWhiteSpace(vehicle.StockNumber))
            {
                throw new ArgumentException("Stock number is required");
            }
            
            if (string.IsNullOrWhiteSpace(vehicle.Make))
            {
                throw new ArgumentException("Make is required");
            }
            
            if (string.IsNullOrWhiteSpace(vehicle.Model))
            {
                throw new ArgumentException("Model is required");
            }
            
            if (vehicle.Year < 1900 || vehicle.Year > DateTime.Now.Year + 1)
            {
                throw new ArgumentException("Invalid year");
            }
        }
    }
}
