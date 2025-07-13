using DMS.InventoryManagement.Core.Models;
using DMS.Shared.Core.Repositories;

namespace DMS.InventoryManagement.Core.Repositories
{
    /// <summary>
    /// Repository for managing vehicle pricing
    /// </summary>
    public interface IVehiclePricingRepository : IRepository<VehiclePricing>
    {
        /// <summary>
        /// Gets the pricing record for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The vehicle pricing record</returns>
        Task<VehiclePricing?> GetByVehicleIdAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets vehicles with active special pricing
        /// </summary>
        /// <returns>A list of vehicles with special pricing</returns>
        Task<IEnumerable<VehiclePricing>> GetVehiclesWithSpecialPricingAsync();
        
        /// <summary>
        /// Gets vehicles with price changes in the specified date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <returns>A list of vehicles with price changes</returns>
        Task<IEnumerable<VehiclePricing>> GetVehiclesWithPriceChangesAsync(DateTime startDate, DateTime endDate);
    }
}
