using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for acquisition workflow services
    /// </summary>
    public interface IAcquisitionService
    {
        /// <summary>
        /// Creates an acquisition workflow for a newly acquired vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The created workflow instance</returns>
        Task<WorkflowInstance> CreateAcquisitionWorkflowAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets the active acquisition workflow for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The active workflow instance, or null if none exists</returns>
        Task<WorkflowInstance?> GetActiveAcquisitionWorkflowAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets vehicles currently in the acquisition process
        /// </summary>
        /// <returns>The list of vehicles in acquisition</returns>
        Task<List<Vehicle>> GetVehiclesInAcquisitionAsync();
        
        /// <summary>
        /// Records an inspection for a vehicle in acquisition
        /// </summary>
        /// <param name="inspection">The inspection details</param>
        /// <returns>The recorded inspection</returns>
        Task<VehicleInspection> RecordVehicleInspectionAsync(VehicleInspection inspection);
        
        /// <summary>
        /// Gets the inspection for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The vehicle inspection, or null if none exists</returns>
        Task<VehicleInspection?> GetVehicleInspectionAsync(Guid vehicleId);
        
        /// <summary>
        /// Updates acquisition documents for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="documentIds">List of document IDs</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateAcquisitionDocumentsAsync(Guid vehicleId, List<Guid> documentIds);
        
        /// <summary>
        /// Completes the intake process for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="userId">The user ID</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The updated vehicle</returns>
        Task<Vehicle> CompleteVehicleIntakeAsync(Guid vehicleId, string userId, string? notes = null);
        
        /// <summary>
        /// Gets acquisition statistics
        /// </summary>
        /// <param name="startDate">Optional start date</param>
        /// <param name="endDate">Optional end date</param>
        /// <returns>Acquisition statistics</returns>
        Task<AcquisitionStatistics> GetAcquisitionStatisticsAsync(DateTime? startDate = null, DateTime? endDate = null);
    }
    
    /// <summary>
    /// Vehicle inspection details
    /// </summary>
    public class VehicleInspection : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }
        
        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }
        
        /// <summary>
        /// Gets or sets the inspection date
        /// </summary>
        public DateTime InspectionDate { get; set; }
        
        /// <summary>
        /// Gets or sets the inspector
        /// </summary>
        public string Inspector { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the inspection notes
        /// </summary>
        public string Notes { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the inspection status
        /// </summary>
        public InspectionStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets the inspection checklist items
        /// </summary>
        public Dictionary<string, bool> ChecklistItems { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the issues found
        /// </summary>
        public List<InspectionIssue> Issues { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the reference to the vehicle
        /// </summary>
        public Vehicle? Vehicle { get; set; }
    }
    
    /// <summary>
    /// Inspection issue
    /// </summary>
    public class InspectionIssue
    {
        /// <summary>
        /// Gets or sets the category
        /// </summary>
        public string Category { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the severity
        /// </summary>
        public IssueSeverity Severity { get; set; }
        
        /// <summary>
        /// Gets or sets the estimated repair cost
        /// </summary>
        public decimal? EstimatedRepairCost { get; set; }
    }
    
    /// <summary>
    /// Issue severity
    /// </summary>
    public enum IssueSeverity
    {
        Minor,
        Moderate,
        Major,
        Critical
    }
    
    /// <summary>
    /// Inspection status
    /// </summary>
    public enum InspectionStatus
    {
        Passed,
        PassedWithIssues,
        Failed
    }
    
    /// <summary>
    /// Acquisition statistics
    /// </summary>
    public class AcquisitionStatistics
    {
        /// <summary>
        /// Gets or sets the total acquired vehicles
        /// </summary>
        public int TotalAcquired { get; set; }
        
        /// <summary>
        /// Gets or sets the average acquisition cost
        /// </summary>
        public decimal AverageAcquisitionCost { get; set; }
        
        /// <summary>
        /// Gets or sets the average time to frontline (days)
        /// </summary>
        public double AverageTimeToFrontline { get; set; }
        
        /// <summary>
        /// Gets or sets the acquisition sources breakdown
        /// </summary>
        public Dictionary<string, int> AcquisitionSources { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the vehicles by status
        /// </summary>
        public Dictionary<string, int> VehiclesByStatus { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the pending inspections
        /// </summary>
        public int PendingInspections { get; set; }
    }
}
