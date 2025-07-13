using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for integration with Service Management module
    /// </summary>
    public interface IServiceIntegrationService
    {
        /// <summary>
        /// Creates a reconditioning work order in the Service Management module
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle requiring reconditioning</param>
        /// <param name="reconditioningItems">List of reconditioning tasks to be performed</param>
        /// <param name="urgency">Priority level of the reconditioning</param>
        /// <param name="notes">Additional notes for the service department</param>
        /// <returns>The ID of the created service work order</returns>
        Task<string> CreateReconditioningWorkOrderAsync(Guid vehicleId, List<ReconditioningItem> reconditioningItems, ReconditioningUrgency urgency, string notes);
        
        /// <summary>
        /// Retrieves the status of a reconditioning work order
        /// </summary>
        /// <param name="workOrderId">The ID of the work order</param>
        /// <returns>The current status of the work order</returns>
        Task<ReconditioningStatus> GetReconditioningStatusAsync(string workOrderId);
        
        /// <summary>
        /// Retrieves the complete history of service records for a vehicle
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>List of service records for the vehicle</returns>
        Task<List<ServiceRecord>> GetVehicleServiceHistoryAsync(Guid vehicleId);
        
        /// <summary>
        /// Updates the reconditioning record when service work is completed
        /// </summary>
        /// <param name="workOrderId">The ID of the work order</param>
        /// <param name="completionDetails">Details about completed reconditioning work</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateReconditioningCompletionAsync(string workOrderId, ReconditioningCompletionDetails completionDetails);
        
        /// <summary>
        /// Schedules a service appointment for warranty or repair work
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <param name="serviceDate">The requested date for service</param>
        /// <param name="serviceType">The type of service needed</param>
        /// <param name="description">Description of the service required</param>
        /// <returns>The ID of the scheduled appointment</returns>
        Task<string> ScheduleServiceAppointmentAsync(Guid vehicleId, DateTime serviceDate, ServiceType serviceType, string description);
    }
    
    /// <summary>
    /// Represents a reconditioning item to be performed on a vehicle
    /// </summary>
    public class ReconditioningItem
    {
        public string Description { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Category { get; set; }
        public bool IsRequired { get; set; }
    }
    
    /// <summary>
    /// Represents the urgency level of reconditioning work
    /// </summary>
    public enum ReconditioningUrgency
    {
        Low,
        Medium,
        High,
        Critical
    }
    
    /// <summary>
    /// Represents the current status of reconditioning work
    /// </summary>
    public enum ReconditioningStatus
    {
        Scheduled,
        InProgress,
        OnHold,
        Completed,
        Cancelled
    }
    
    /// <summary>
    /// Represents details about completed reconditioning work
    /// </summary>
    public class ReconditioningCompletionDetails
    {
        public DateTime CompletionDate { get; set; }
        public decimal ActualCost { get; set; }
        public List<string> CompletedItems { get; set; }
        public List<string> SkippedItems { get; set; }
        public string TechnicianNotes { get; set; }
        public string QualityCheckStatus { get; set; }
    }
    
    /// <summary>
    /// Represents a service record for a vehicle
    /// </summary>
    public class ServiceRecord
    {
        public string ServiceId { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceType { get; set; }
        public decimal Cost { get; set; }
        public string TechnicianId { get; set; }
        public string Description { get; set; }
        public int Mileage { get; set; }
        public string Status { get; set; }
    }
    
    /// <summary>
    /// Represents the type of service needed
    /// </summary>
    public enum ServiceType
    {
        Maintenance,
        Repair,
        Recall,
        Warranty,
        Other
    }
}
