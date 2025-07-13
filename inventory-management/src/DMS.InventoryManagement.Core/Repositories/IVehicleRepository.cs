using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Services;
using DMS.Shared.Core.Data;

namespace DMS.InventoryManagement.Core.Repositories
{
    /// <summary>
    /// Interface for vehicle repository operations
    /// </summary>
    public interface IVehicleRepository : IRepository<Vehicle>
    {
        /// <summary>
        /// Gets a vehicle by its VIN
        /// </summary>
        /// <param name="vin">The vehicle identification number</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The vehicle if found, otherwise null</returns>
        Task<Vehicle?> GetByVinAsync(string vin, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a vehicle by its stock number
        /// </summary>
        /// <param name="stockNumber">The stock number</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The vehicle if found, otherwise null</returns>
        Task<Vehicle?> GetByStockNumberAsync(string stockNumber, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets vehicles by their status
        /// </summary>
        /// <param name="status">The vehicle status</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of vehicles with the specified status</returns>
        Task<IEnumerable<Vehicle>> GetByStatusAsync(VehicleStatus status, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets vehicles that have been in inventory longer than the specified number of days
        /// </summary>
        /// <param name="days">The number of days</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of vehicles that have been in inventory longer than the specified number of days</returns>
        Task<IEnumerable<Vehicle>> GetAgingInventoryAsync(int days, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets vehicles by their type
        /// </summary>
        /// <param name="vehicleType">The vehicle type</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of vehicles with the specified type</returns>
        Task<IEnumerable<Vehicle>> GetByTypeAsync(VehicleType vehicleType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a vehicle inspection by vehicle ID
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The vehicle inspection if found, otherwise null</returns>
        Task<VehicleInspection?> GetVehicleInspection(Guid vehicleId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new vehicle inspection
        /// </summary>
        /// <param name="inspection">The inspection to add</param>
        void AddInspection(VehicleInspection inspection);
        
        /// <summary>
        /// Gets vehicle documents by ID
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="documentIds">List of document IDs to retrieve</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A list of vehicle documents</returns>
        Task<List<VehicleDocument>> GetVehicleDocuments(Guid vehicleId, List<Guid> documentIds, CancellationToken cancellationToken = default);
    }
}
