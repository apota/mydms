using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for reconditioning workflow services
    /// </summary>
    public interface IReconditioningService
    {
        /// <summary>
        /// Creates a reconditioning workflow for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The created workflow instance</returns>
        Task<WorkflowInstance> CreateReconditioningWorkflowAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets the active reconditioning workflow for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The active workflow instance, or null if none exists</returns>
        Task<WorkflowInstance?> GetActiveReconditioningWorkflowAsync(Guid vehicleId);
        
        /// <summary>
        /// Adds a reconditioning record to a vehicle
        /// </summary>
        /// <param name="record">The reconditioning record</param>
        /// <returns>The created reconditioning record</returns>
        Task<ReconditioningRecord> AddReconditioningRecordAsync(ReconditioningRecord record);
        
        /// <summary>
        /// Updates a reconditioning record
        /// </summary>
        /// <param name="record">The reconditioning record</param>
        /// <returns>The updated reconditioning record</returns>
        Task<ReconditioningRecord> UpdateReconditioningRecordAsync(ReconditioningRecord record);
        
        /// <summary>
        /// Gets reconditioning records for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The list of reconditioning records</returns>
        Task<List<ReconditioningRecord>> GetVehicleReconditioningRecordsAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets reconditioning summary for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The reconditioning summary</returns>
        Task<ReconditioningSummary> GetReconditioningSummaryAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets vehicles currently in reconditioning
        /// </summary>
        /// <returns>The list of vehicles in reconditioning</returns>
        Task<List<Vehicle>> GetVehiclesInReconditioningAsync();
        
        /// <summary>
        /// Marks a vehicle as ready for front line
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The updated vehicle</returns>
        Task<Vehicle> MarkVehicleReadyForFrontLineAsync(Guid vehicleId, string? notes = null);
    }
    
    /// <summary>
    /// Reconditioning summary
    /// </summary>
    public class ReconditioningSummary
    {
        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }
        
        /// <summary>
        /// Gets or sets the current reconditioning status
        /// </summary>
        public string CurrentStatus { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the total reconditioning cost
        /// </summary>
        public decimal TotalCost { get; set; }
        
        /// <summary>
        /// Gets or sets the days in reconditioning
        /// </summary>
        public int DaysInReconditioning { get; set; }
        
        /// <summary>
        /// Gets or sets the estimated completion date
        /// </summary>
        public DateTime? EstimatedCompletionDate { get; set; }
        
        /// <summary>
        /// Gets or sets the current workflow step name
        /// </summary>
        public string CurrentStepName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the assigned technician
        /// </summary>
        public string? AssignedTechnician { get; set; }
    }
}
