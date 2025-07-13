using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for aging management services
    /// </summary>
    public interface IAgingManagementService
    {
        /// <summary>
        /// Updates aging data for all vehicles
        /// </summary>
        /// <returns>Number of vehicles processed</returns>
        Task<int> UpdateAllVehicleAgingDataAsync();
        
        /// <summary>
        /// Updates aging data for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The updated vehicle aging info</returns>
        Task<VehicleAging> UpdateVehicleAgingDataAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets vehicles requiring age-based price adjustments
        /// </summary>
        /// <param name="daysThreshold">Optional days threshold</param>
        /// <returns>List of vehicles needing price adjustments</returns>
        Task<List<Vehicle>> GetVehiclesForPriceAdjustmentAsync(int? daysThreshold = null);
        
        /// <summary>
        /// Gets vehicles that have reached critical aging status
        /// </summary>
        /// <returns>List of vehicles with critical aging</returns>
        Task<List<Vehicle>> GetCriticalAgingVehiclesAsync();
        
        /// <summary>
        /// Creates an aging management workflow for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The created workflow instance</returns>
        Task<WorkflowInstance> CreateAgingManagementWorkflowAsync(Guid vehicleId);
        
        /// <summary>
        /// Recommends a price adjustment based on vehicle aging
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>Recommended price and percentage adjustment</returns>
        Task<PriceAdjustmentRecommendation> GetPriceAdjustmentRecommendationAsync(Guid vehicleId);
        
        /// <summary>
        /// Applies a recommended price adjustment to a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="newPrice">The new price</param>
        /// <param name="reason">The reason for the price adjustment</param>
        /// <param name="userId">The user making the adjustment</param>
        /// <returns>The updated price history entry</returns>
        Task<PriceHistoryEntry> ApplyPriceAdjustmentAsync(
            Guid vehicleId, 
            decimal newPrice, 
            string reason, 
            string userId);
    }
    
    /// <summary>
    /// Price adjustment recommendation
    /// </summary>
    public class PriceAdjustmentRecommendation
    {
        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }
        
        /// <summary>
        /// Gets or sets the current price
        /// </summary>
        public decimal CurrentPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the recommended price
        /// </summary>
        public decimal RecommendedPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the adjustment percentage
        /// </summary>
        public decimal AdjustmentPercentage { get; set; }
        
        /// <summary>
        /// Gets or sets the days in inventory
        /// </summary>
        public int DaysInInventory { get; set; }
        
        /// <summary>
        /// Gets or sets the reason for adjustment
        /// </summary>
        public string ReasonForAdjustment { get; set; } = string.Empty;
    }
}
