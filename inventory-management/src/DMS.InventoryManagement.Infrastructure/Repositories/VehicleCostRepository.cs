using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Infrastructure.Data;
using DMS.Shared.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DMS.InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for vehicle costs
    /// </summary>
    public class VehicleCostRepository : Repository<VehicleCost, InventoryDbContext>, IVehicleCostRepository
    {
        public VehicleCostRepository(InventoryDbContext dbContext) : base(dbContext)
        {
        }

        /// <inheritdoc />
        public async Task<VehicleCost?> GetByVehicleIdAsync(Guid vehicleId)
        {
            return await DbContext.VehicleCosts
                .Include(vc => vc.AdditionalCosts)
                .FirstOrDefaultAsync(vc => vc.VehicleId == vehicleId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VehicleCost>> GetVehiclesAboveCostThresholdAsync(decimal costThreshold)
        {
            return await DbContext.VehicleCosts
                .Include(vc => vc.AdditionalCosts)
                .Include(vc => vc.Vehicle)
                .Where(vc => vc.TotalCost > costThreshold)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<decimal> GetTotalInventoryInvestmentAsync()
        {
            return await DbContext.VehicleCosts
                .Where(vc => vc.Vehicle != null && vc.Vehicle.Status != VehicleStatus.Sold && vc.Vehicle.Status != VehicleStatus.Delivered)
                .SumAsync(vc => vc.TotalCost);
        }
    }
}
