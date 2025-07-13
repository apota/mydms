using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Infrastructure.Data;
using DMS.Shared.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DMS.InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for vehicle aging information
    /// </summary>
    public class VehicleAgingRepository : Repository<VehicleAging, InventoryDbContext>, IVehicleAgingRepository
    {
        public VehicleAgingRepository(InventoryDbContext dbContext) : base(dbContext)
        {
        }

        /// <inheritdoc />
        public async Task<VehicleAging?> GetByVehicleIdAsync(Guid vehicleId)
        {
            return await DbContext.VehicleAgings
                .FirstOrDefaultAsync(va => va.VehicleId == vehicleId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VehicleAging>> GetByAlertLevelAsync(AgingAlertLevel alertLevel)
        {
            return await DbContext.VehicleAgings
                .Include(va => va.Vehicle)
                .Where(va => va.AgingAlertLevel == alertLevel)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VehicleAging>> GetVehiclesNeedingPriceReductionAsync()
        {
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            
            return await DbContext.VehicleAgings
                .Include(va => va.Vehicle)
                .Where(va => va.AgingAlertLevel >= AgingAlertLevel.Warning &&
                            (va.LastPriceReductionDate == null || va.LastPriceReductionDate < thirtyDaysAgo))
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task UpdateDaysInInventoryAsync()
        {
            var today = DateTime.UtcNow.Date;
            var vehicles = await DbContext.Vehicles
                .Include(v => v.AgingInfo)
                .Where(v => !v.IsDeleted && 
                           v.Status != VehicleStatus.Sold && 
                           v.Status != VehicleStatus.Delivered)
                .ToListAsync();

            foreach (var vehicle in vehicles)
            {
                // Create aging record if it doesn't exist
                if (vehicle.AgingInfo == null)
                {
                    vehicle.AgingInfo = new VehicleAging
                    {
                        VehicleId = vehicle.Id,
                        AgeThreshold = 60, // Default threshold
                        AgingAlertLevel = AgingAlertLevel.Normal
                    };
                }

                // Calculate days in inventory
                var daysInInventory = (int)(today - vehicle.AcquisitionDate.Date).TotalDays;
                vehicle.AgingInfo.DaysInInventory = daysInInventory;

                // Update alert level based on days in inventory
                if (daysInInventory > vehicle.AgingInfo.AgeThreshold * 1.5)
                {
                    vehicle.AgingInfo.AgingAlertLevel = AgingAlertLevel.Critical;
                    vehicle.AgingInfo.RecommendedAction = "Immediate price reduction or wholesale consideration";
                }
                else if (daysInInventory > vehicle.AgingInfo.AgeThreshold)
                {
                    vehicle.AgingInfo.AgingAlertLevel = AgingAlertLevel.Warning;
                    vehicle.AgingInfo.RecommendedAction = "Consider price reduction";
                }
                else
                {
                    vehicle.AgingInfo.AgingAlertLevel = AgingAlertLevel.Normal;
                    vehicle.AgingInfo.RecommendedAction = null;
                }
            }

            await DbContext.SaveChangesAsync();
        }
    }
}
