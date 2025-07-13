using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Infrastructure.Data;
using DMS.Shared.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DMS.InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for vehicle pricing
    /// </summary>
    public class VehiclePricingRepository : Repository<VehiclePricing, InventoryDbContext>, IVehiclePricingRepository
    {
        public VehiclePricingRepository(InventoryDbContext dbContext) : base(dbContext)
        {
        }

        /// <inheritdoc />
        public async Task<VehiclePricing?> GetByVehicleIdAsync(Guid vehicleId)
        {
            return await DbContext.VehiclePricings
                .Include(vp => vp.PriceHistory)
                .FirstOrDefaultAsync(vp => vp.VehicleId == vehicleId);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VehiclePricing>> GetVehiclesWithSpecialPricingAsync()
        {
            var today = DateTime.UtcNow.Date;
            
            return await DbContext.VehiclePricings
                .Include(vp => vp.Vehicle)
                .Include(vp => vp.PriceHistory)
                .Where(vp => vp.SpecialPrice != null && 
                            vp.SpecialStartDate <= today && 
                            vp.SpecialEndDate >= today)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VehiclePricing>> GetVehiclesWithPriceChangesAsync(DateTime startDate, DateTime endDate)
        {
            return await DbContext.VehiclePricings
                .Include(vp => vp.Vehicle)
                .Include(vp => vp.PriceHistory.Where(ph => ph.Date >= startDate && ph.Date <= endDate))
                .Where(vp => vp.PriceHistory.Any(ph => ph.Date >= startDate && ph.Date <= endDate))
                .ToListAsync();
        }
    }
}
