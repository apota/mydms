using DMS.ServiceManagement.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.ServiceManagement.Core.Services
{
    public interface IServiceInspectionService
    {
        /// <summary>
        /// Get all service inspections
        /// </summary>
        /// <returns>A list of service inspections</returns>
        Task<List<ServiceInspection>> GetAllInspectionsAsync();
        
        /// <summary>
        /// Get service inspection by ID
        /// </summary>
        /// <param name="id">Service inspection ID</param>
        /// <returns>The service inspection if found, null otherwise</returns>
        Task<ServiceInspection> GetInspectionByIdAsync(Guid id);
        
        /// <summary>
        /// Get all service inspections for a specific repair order
        /// </summary>
        /// <param name="repairOrderId">Repair order ID</param>
        /// <returns>A list of service inspections for the repair order</returns>
        Task<List<ServiceInspection>> GetInspectionsByRepairOrderIdAsync(Guid repairOrderId);
        
        /// <summary>
        /// Create a new service inspection
        /// </summary>
        /// <param name="inspection">Service inspection to create</param>
        /// <returns>The created service inspection</returns>
        Task<ServiceInspection> CreateInspectionAsync(ServiceInspection inspection);
        
        /// <summary>
        /// Update an existing service inspection
        /// </summary>
        /// <param name="inspection">Updated service inspection data</param>
        /// <returns>The updated service inspection if found, null otherwise</returns>
        Task<ServiceInspection> UpdateInspectionAsync(ServiceInspection inspection);
        
        /// <summary>
        /// Delete a service inspection
        /// </summary>
        /// <param name="id">Service inspection ID to delete</param>
        /// <returns>True if deleted, false if not found</returns>
        Task<bool> DeleteInspectionAsync(Guid id);
        
        /// <summary>
        /// Add an image to a service inspection
        /// </summary>
        /// <param name="inspectionId">Service inspection ID</param>
        /// <param name="pointId">Inspection point ID if applicable</param>
        /// <param name="imageUrl">URL of the uploaded image</param>
        /// <returns>The updated service inspection</returns>
        Task<ServiceInspection> AddInspectionImageAsync(Guid inspectionId, string pointId, string imageUrl);
        
        /// <summary>
        /// Remove an image from a service inspection
        /// </summary>
        /// <param name="inspectionId">Service inspection ID</param>
        /// <param name="imageUrl">URL of the image to remove</param>
        /// <returns>The updated service inspection</returns>
        Task<ServiceInspection> RemoveInspectionImageAsync(Guid inspectionId, string imageUrl);
    }
}
