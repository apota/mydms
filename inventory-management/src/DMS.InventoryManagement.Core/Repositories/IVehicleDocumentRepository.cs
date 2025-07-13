using DMS.InventoryManagement.Core.Models;
using DMS.Shared.Core.Repositories;

namespace DMS.InventoryManagement.Core.Repositories
{
    /// <summary>
    /// Repository for managing vehicle documents
    /// </summary>
    public interface IVehicleDocumentRepository : IRepository<VehicleDocument>
    {
        /// <summary>
        /// Gets documents for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>A list of vehicle documents</returns>
        Task<IEnumerable<VehicleDocument>> GetByVehicleIdAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets documents by type for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="documentType">The document type</param>
        /// <returns>A list of vehicle documents</returns>
        Task<IEnumerable<VehicleDocument>> GetByVehicleIdAndTypeAsync(Guid vehicleId, DocumentType documentType);
    }
}
