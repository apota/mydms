using DMS.InventoryManagement.Core.Models;
using DMS.Shared.Core.Repositories;

namespace DMS.InventoryManagement.Core.Repositories
{
    /// <summary>
    /// Repository for managing vehicle costs
    /// </summary>
    public interface IVehicleCostRepository : IRepository<VehicleCost>
    {
        /// <summary>
        /// Gets the cost record for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The vehicle cost record</returns>
        Task<VehicleCost?> GetByVehicleIdAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets vehicles with costs above a certain threshold
        /// </summary>
        /// <param name="costThreshold">The cost threshold</param>
        /// <returns>A list of vehicle costs</returns>
        Task<IEnumerable<VehicleCost>> GetVehiclesAboveCostThresholdAsync(decimal costThreshold);
        
        /// <summary>
        /// Gets a summary of total inventory investment
        /// </summary>
        /// <returns>The total inventory investment</returns>
        Task<decimal> GetTotalInventoryInvestmentAsync();
    }
}
