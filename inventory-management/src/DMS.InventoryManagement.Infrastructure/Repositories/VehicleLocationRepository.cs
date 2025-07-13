using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Infrastructure.Data;
using DMS.Shared.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DMS.InventoryManagement.Infrastructure.Repositories
{
    /// <summary>
    /// Repository implementation for vehicle locations
    /// </summary>
    public class VehicleLocationRepository : Repository<VehicleLocation, InventoryDbContext>, IVehicleLocationRepository
    {
        public VehicleLocationRepository(InventoryDbContext dbContext) : base(dbContext)
        {
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VehicleLocation>> GetByTypeAsync(LocationType type)
        {
            return await DbContext.VehicleLocations
                .Include(vl => vl.Zones)
                .Where(vl => vl.Type == type)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<VehicleLocation>> GetLocationsWithAvailableCapacityAsync()
        {
            var locations = await DbContext.VehicleLocations
                .Include(vl => vl.Zones)
                .Include(vl => vl.Vehicles.Where(v => !v.IsDeleted))
                .ToListAsync();
                
            return locations.Where(location => 
                location.Zones.Sum(z => z.Capacity) > location.Vehicles.Count);
        }

        /// <inheritdoc />
        public async Task<Dictionary<Guid, int>> GetVehicleCountsByLocationAsync()
        {
            var vehicleCounts = await DbContext.Vehicles
                .Where(v => !v.IsDeleted && v.LocationId.HasValue)
                .GroupBy(v => v.LocationId.Value)
                .Select(group => new 
                {
                    LocationId = group.Key, 
                    Count = group.Count()
                })
                .ToListAsync();
                
            return vehicleCounts.ToDictionary(vc => vc.LocationId, vc => vc.Count);
        }
    }
}
