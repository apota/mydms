using DMS.InventoryManagement.Core.Models;
using DMS.Shared.Core.Repositories;

namespace DMS.InventoryManagement.Core.Repositories
{
    /// <summary>
    /// Repository for managing vehicle aging information
    /// </summary>
    public interface IVehicleAgingRepository : IRepository<VehicleAging>
    {
        /// <summary>
        /// Gets the aging record for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The vehicle aging record</returns>
        Task<VehicleAging?> GetByVehicleIdAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets vehicles with aging alert of a specific level
        /// </summary>
        /// <param name="alertLevel">The alert level</param>
        /// <returns>A list of vehicle aging records</returns>
        Task<IEnumerable<VehicleAging>> GetByAlertLevelAsync(AgingAlertLevel alertLevel);
        
        /// <summary>
        /// Gets vehicles that need price reductions based on aging thresholds
        /// </summary>
        /// <returns>A list of vehicle aging records</returns>
        Task<IEnumerable<VehicleAging>> GetVehiclesNeedingPriceReductionAsync();
        
        /// <summary>
        /// Updates the days in inventory for all vehicles
        /// </summary>
        /// <returns>A task representing the asynchronous operation</returns>
        Task UpdateDaysInInventoryAsync();
    }
}
