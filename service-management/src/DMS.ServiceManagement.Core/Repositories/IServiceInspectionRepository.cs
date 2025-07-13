using DMS.ServiceManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.ServiceManagement.Core.Repositories
{
    public interface IServiceInspectionRepository
    {
        /// <summary>
        /// Get all service inspections
        /// </summary>
        /// <returns>A list of service inspections</returns>
        Task<List<ServiceInspection>> GetAllAsync();
        
        /// <summary>
        /// Get a service inspection by ID
        /// </summary>
        /// <param name="id">The inspection ID</param>
        /// <returns>The service inspection if found, null otherwise</returns>
        Task<ServiceInspection> GetByIdAsync(Guid id);
        
        /// <summary>
        /// Get all service inspections for a repair order
        /// </summary>
        /// <param name="repairOrderId">The repair order ID</param>
        /// <returns>A list of service inspections for the repair order</returns>
        Task<List<ServiceInspection>> GetByRepairOrderIdAsync(Guid repairOrderId);
        
        /// <summary>
        /// Create a new service inspection
        /// </summary>
        /// <param name="inspection">The service inspection to create</param>
        Task CreateAsync(ServiceInspection inspection);
        
        /// <summary>
        /// Update an existing service inspection
        /// </summary>
        /// <param name="inspection">The updated service inspection</param>
        Task UpdateAsync(ServiceInspection inspection);
        
        /// <summary>
        /// Delete a service inspection
        /// </summary>
        /// <param name="id">The inspection ID to delete</param>
        Task DeleteAsync(Guid id);
    }
}
