using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Services;
using DMS.InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the aging management service
    /// </summary>
    public class AgingManagementService : IAgingManagementService
    {
        private readonly InventoryDbContext _dbContext;
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<AgingManagementService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AgingManagementService"/> class
        /// </summary>
        public AgingManagementService(
            InventoryDbContext dbContext,
            IWorkflowService workflowService,
            ILogger<AgingManagementService> logger)
        {
            _dbContext = dbContext;
            _workflowService = workflowService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<int> UpdateAllVehicleAgingDataAsync()
        {
            // Get all vehicles that are not sold or delivered
            var vehicles = await _dbContext.Vehicles
                .Where(v => v.Status != VehicleStatus.Sold && 
                           v.Status != VehicleStatus.Delivered && 
                           v.Status != VehicleStatus.Transferred &&
                           !v.IsDeleted)
                .ToListAsync();
                
            int count = 0;
            
            foreach (var vehicle in vehicles)
            {
                await UpdateVehicleAgingDataAsync(vehicle.Id);
                count++;
            }
            
            _logger.LogInformation("Updated aging data for {Count} vehicles", count);
            
            return count;
        }

        /// <inheritdoc/>
        public async Task<VehicleAging> UpdateVehicleAgingDataAsync(Guid vehicleId)
        {
            var vehicle = await _dbContext.Vehicles
                .Include(v => v.AgingInfo)
                .Include(v => v.PricingDetails)
                    .ThenInclude(p => p.PriceHistory.OrderByDescending(ph => ph.Date))
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
                
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found");
            }
            
            // Create aging info if it doesn't exist
            if (vehicle.AgingInfo == null)
            {
                vehicle.AgingInfo = new VehicleAging
                {
                    Id = Guid.NewGuid(),
                    VehicleId = vehicle.Id,
                    AgeThreshold = GetAgeThresholdByVehicleType(vehicle.VehicleType),
                    AgingAlertLevel = AgingAlertLevel.Normal
                };
            }
            
            // Calculate days in inventory
            vehicle.AgingInfo.DaysInInventory = (int)(DateTime.UtcNow - vehicle.AcquisitionDate).TotalDays;
            
            // Get the last price reduction date
            if (vehicle.PricingDetails?.PriceHistory != null && vehicle.PricingDetails.PriceHistory.Any())
            {
                var lastPriceReduction = vehicle.PricingDetails.PriceHistory
                    .Where(ph => ph.ActionType == PriceActionType.Reduction)
                    .OrderByDescending(ph => ph.Date)
                    .FirstOrDefault();
                    
                if (lastPriceReduction != null)
                {
                    vehicle.AgingInfo.LastPriceReductionDate = lastPriceReduction.Date;
                }
            }
            
            // Determine alert level and recommended action
            UpdateAgingAlertLevelAndRecommendation(vehicle);
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Updated aging data for vehicle {VIN}, Days in inventory: {Days}, Alert level: {AlertLevel}", 
                vehicle.VIN, vehicle.AgingInfo.DaysInInventory, vehicle.AgingInfo.AgingAlertLevel);
                
            return vehicle.AgingInfo;
        }

        /// <inheritdoc/>
        public async Task<List<Vehicle>> GetVehiclesForPriceAdjustmentAsync(int? daysThreshold = null)
        {
            var query = _dbContext.Vehicles
                .Include(v => v.AgingInfo)
                .Include(v => v.PricingDetails)
                .Where(v => v.Status != VehicleStatus.Sold && 
                           v.Status != VehicleStatus.Delivered && 
                           v.Status != VehicleStatus.Transferred &&
                           !v.IsDeleted &&
                           v.AgingInfo != null &&
                           (v.AgingInfo.AgingAlertLevel == AgingAlertLevel.Warning || 
                            v.AgingInfo.AgingAlertLevel == AgingAlertLevel.Critical));
                            
            if (daysThreshold.HasValue)
            {
                query = query.Where(v => v.AgingInfo.DaysInInventory >= daysThreshold.Value);
            }
            
            return await query.OrderByDescending(v => v.AgingInfo.DaysInInventory).ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<Vehicle>> GetCriticalAgingVehiclesAsync()
        {
            return await _dbContext.Vehicles
                .Include(v => v.AgingInfo)
                .Include(v => v.PricingDetails)
                .Where(v => v.Status != VehicleStatus.Sold && 
                           v.Status != VehicleStatus.Delivered && 
                           v.Status != VehicleStatus.Transferred &&
                           !v.IsDeleted &&
                           v.AgingInfo != null &&
                           v.AgingInfo.AgingAlertLevel == AgingAlertLevel.Critical)
                .OrderByDescending(v => v.AgingInfo.DaysInInventory)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<WorkflowInstance> CreateAgingManagementWorkflowAsync(Guid vehicleId)
        {
            // Get the default aging management workflow definition
            var workflowDefinition = await _workflowService.GetDefaultWorkflowDefinitionAsync(WorkflowType.AgingManagement);
            
            if (workflowDefinition == null)
            {
                throw new InvalidOperationException("No default aging management workflow definition found");
            }
            
            // Check if there's already an active aging management workflow for this vehicle
            var activeWorkflows = await _workflowService.GetActiveVehicleWorkflowInstancesByTypeAsync(vehicleId, WorkflowType.AgingManagement);
            
            if (activeWorkflows.Any())
            {
                throw new InvalidOperationException("Vehicle already has an active aging management workflow");
            }
            
            // Create the workflow instance
            var workflowInstance = await _workflowService.CreateWorkflowInstanceAsync(
                workflowDefinition.Id, 
                vehicleId, 
                priority: 2); // High priority for aging management
                
            _logger.LogInformation("Created aging management workflow for vehicle {VehicleId}", vehicleId);
            
            return workflowInstance;
        }

        /// <inheritdoc/>
        public async Task<PriceAdjustmentRecommendation> GetPriceAdjustmentRecommendationAsync(Guid vehicleId)
        {
            var vehicle = await _dbContext.Vehicles
                .Include(v => v.AgingInfo)
                .Include(v => v.PricingDetails)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
                
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found");
            }
            
            if (vehicle.AgingInfo == null)
            {
                await UpdateVehicleAgingDataAsync(vehicleId);
                vehicle = await _dbContext.Vehicles
                    .Include(v => v.AgingInfo)
                    .Include(v => v.PricingDetails)
                    .FirstOrDefaultAsync(v => v.Id == vehicleId);
            }
            
            var currentPrice = vehicle.PricingDetails?.InternetPrice ?? vehicle.ListPrice;
            decimal adjustmentPercentage = 0;
            
            // Calculate adjustment percentage based on days in inventory and alert level
            if (vehicle.AgingInfo.AgingAlertLevel == AgingAlertLevel.Warning)
            {
                adjustmentPercentage = 0.03m; // 3% reduction for Warning level
            }
            else if (vehicle.AgingInfo.AgingAlertLevel == AgingAlertLevel.Critical)
            {
                // For critical level, scale from 5% to 12% based on days over threshold
                var daysOverThreshold = vehicle.AgingInfo.DaysInInventory - vehicle.AgingInfo.AgeThreshold;
                adjustmentPercentage = Math.Min(0.12m, 0.05m + (daysOverThreshold / 30.0m) * 0.01m);
            }
            
            var recommendedPrice = Math.Round(currentPrice * (1 - adjustmentPercentage), 2);
            
            // Ensure price doesn't go below a certain threshold (implementation-specific)
            // This could be based on vehicle cost, market data, etc.
            
            var recommendation = new PriceAdjustmentRecommendation
            {
                VehicleId = vehicleId,
                CurrentPrice = currentPrice,
                RecommendedPrice = recommendedPrice,
                AdjustmentPercentage = adjustmentPercentage,
                DaysInInventory = vehicle.AgingInfo.DaysInInventory,
                ReasonForAdjustment = GetReasonForAdjustment(vehicle)
            };
            
            return recommendation;
        }

        /// <inheritdoc/>
        public async Task<PriceHistoryEntry> ApplyPriceAdjustmentAsync(
            Guid vehicleId, 
            decimal newPrice, 
            string reason, 
            string userId)
        {
            var vehicle = await _dbContext.Vehicles
                .Include(v => v.AgingInfo)
                .Include(v => v.PricingDetails)
                    .ThenInclude(p => p.PriceHistory)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
                
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found");
            }
            
            var oldPrice = vehicle.PricingDetails?.InternetPrice ?? vehicle.ListPrice;
            
            // Create pricing details if it doesn't exist
            if (vehicle.PricingDetails == null)
            {
                vehicle.PricingDetails = new VehiclePricing
                {
                    Id = Guid.NewGuid(),
                    VehicleId = vehicle.Id,
                    MSRP = vehicle.MSRP,
                    InternetPrice = vehicle.ListPrice,
                    StickingPrice = vehicle.ListPrice,
                    PriceHistory = new List<PriceHistoryEntry>()
                };
            }
            
            // Update the price
            vehicle.PricingDetails.InternetPrice = newPrice;
            vehicle.ListPrice = newPrice;
            
            // Create a price history entry
            var priceAction = newPrice < oldPrice 
                ? PriceActionType.Reduction 
                : (newPrice > oldPrice ? PriceActionType.Increase : PriceActionType.Other);
                
            var priceHistoryEntry = new PriceHistoryEntry
            {
                Id = Guid.NewGuid(),
                VehiclePricingId = vehicle.PricingDetails.Id,
                Price = newPrice,
                Date = DateTime.UtcNow,
                Reason = reason,
                UserId = userId,
                ActionType = priceAction
            };
            
            vehicle.PricingDetails.PriceHistory.Add(priceHistoryEntry);
            
            // Update the last price reduction date if this was a reduction
            if (priceAction == PriceActionType.Reduction)
            {
                vehicle.AgingInfo.LastPriceReductionDate = DateTime.UtcNow;
                
                // Recalculate alert level after price reduction
                UpdateAgingAlertLevelAndRecommendation(vehicle);
            }
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Applied price adjustment for vehicle {VIN}, New price: {Price}, Reason: {Reason}", 
                vehicle.VIN, newPrice, reason);
                
            return priceHistoryEntry;
        }

        #region Private Methods
        
        private int GetAgeThresholdByVehicleType(VehicleType vehicleType)
        {
            // Default thresholds by vehicle type
            switch (vehicleType)
            {
                case VehicleType.New:
                    return 60; // 60 days for new vehicles
                case VehicleType.CertifiedPreOwned:
                    return 45; // 45 days for CPO vehicles
                case VehicleType.Used:
                default:
                    return 30; // 30 days for used vehicles
            }
        }
        
        private void UpdateAgingAlertLevelAndRecommendation(Vehicle vehicle)
        {
            if (vehicle.AgingInfo == null)
                return;
                
            var daysInInventory = vehicle.AgingInfo.DaysInInventory;
            var ageThreshold = vehicle.AgingInfo.AgeThreshold;
            
            AgingAlertLevel alertLevel;
            string recommendation;
            
            if (daysInInventory <= ageThreshold)
            {
                alertLevel = AgingAlertLevel.Normal;
                recommendation = "No action needed at this time.";
            }
            else if (daysInInventory <= ageThreshold * 1.5)
            {
                alertLevel = AgingAlertLevel.Warning;
                recommendation = "Consider price reduction of 3-5%. Review vehicle presentation and online listing.";
            }
            else
            {
                alertLevel = AgingAlertLevel.Critical;
                
                // Check if price reduction has been done recently
                var hasRecentPriceReduction = vehicle.AgingInfo.LastPriceReductionDate.HasValue && 
                    (DateTime.UtcNow - vehicle.AgingInfo.LastPriceReductionDate.Value).TotalDays <= 7;
                    
                if (hasRecentPriceReduction)
                {
                    recommendation = "Recent price reduction applied. Consider wholesale options if no retail interest in next 7 days.";
                }
                else
                {
                    recommendation = "Immediate action required. Apply significant price reduction (8-12%) or consider wholesale/auction options.";
                }
            }
            
            vehicle.AgingInfo.AgingAlertLevel = alertLevel;
            vehicle.AgingInfo.RecommendedAction = recommendation;
        }
        
        private string GetReasonForAdjustment(Vehicle vehicle)
        {
            if (vehicle.AgingInfo == null)
                return "Price adjustment due to market conditions";
                
            var daysInInventory = vehicle.AgingInfo.DaysInInventory;
            var ageThreshold = vehicle.AgingInfo.AgeThreshold;
            
            if (daysInInventory <= ageThreshold * 1.25)
            {
                return $"Proactive price adjustment after {daysInInventory} days in inventory";
            }
            else if (daysInInventory <= ageThreshold * 1.75)
            {
                return $"Standard aging inventory adjustment ({daysInInventory} days in inventory)";
            }
            else
            {
                return $"Critical aging inventory adjustment ({daysInInventory} days in inventory, {daysInInventory - ageThreshold} days over threshold)";
            }
        }
        
        #endregion
    }
}

// Add this to support price action types
namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Type of price action
    /// </summary>
    public enum PriceActionType
    {
        Increase,
        Reduction,
        Other
    }
}
