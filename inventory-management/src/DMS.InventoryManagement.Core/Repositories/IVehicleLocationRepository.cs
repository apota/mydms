using DMS.InventoryManagement.Core.Models;
using DMS.Shared.Core.Repositories;

namespace DMS.InventoryManagement.Core.Repositories
{
    /// <summary>
    /// Repository for managing vehicle locations
    /// </summary>
    public interface IVehicleLocationRepository : IRepository<VehicleLocation>
    {
        /// <summary>
        /// Gets locations by type
        /// </summary>
        /// <param name="type">The location type</param>
        /// <returns>A list of locations</returns>
        Task<IEnumerable<VehicleLocation>> GetByTypeAsync(LocationType type);
        
        /// <summary>
        /// Gets locations with available capacity
        /// </summary>
        /// <returns>A list of locations with available capacity</returns>
        Task<IEnumerable<VehicleLocation>> GetLocationsWithAvailableCapacityAsync();
        
        /// <summary>
        /// Gets vehicle count by location
        /// </summary>
        /// <returns>A dictionary of location IDs and vehicle counts</returns>
        Task<Dictionary<Guid, int>> GetVehicleCountsByLocationAsync();
    }
}
