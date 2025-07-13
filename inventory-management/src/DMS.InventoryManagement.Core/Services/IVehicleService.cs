using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Models;
using Microsoft.AspNetCore.Http;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for vehicle management operations
    /// </summary>
    public interface IVehicleService
    {
        /// <summary>
        /// Gets a vehicle by ID
        /// </summary>
        Task<Vehicle> GetVehicleByIdAsync(Guid id);
        
        /// <summary>
        /// Gets vehicles with optional filtering and pagination
        /// </summary>
        Task<(IEnumerable<Vehicle> Vehicles, int TotalCount)> GetVehiclesAsync(VehicleFilterOptions filterOptions, int page = 1, int pageSize = 10);
        
        /// <summary>
        /// Creates a new vehicle
        /// </summary>
        Task<Vehicle> CreateVehicleAsync(Vehicle vehicle);
        
        /// <summary>
        /// Updates an existing vehicle
        /// </summary>
        Task<Vehicle> UpdateVehicleAsync(Guid id, Vehicle vehicle);
        
        /// <summary>
        /// Deletes a vehicle
        /// </summary>
        Task<bool> DeleteVehicleAsync(Guid id);
        
        /// <summary>
        /// Transfers a vehicle to a new location
        /// </summary>
        Task<VehicleTransferResult> TransferVehicleAsync(Guid vehicleId, Guid destinationLocationId, Guid? destinationZoneId, string transferReason);
        
        /// <summary>
        /// Updates a vehicle's status
        /// </summary>
        Task<VehicleStatusUpdateResult> UpdateVehicleStatusAsync(Guid vehicleId, string newStatus, string statusChangeReason, Dictionary<string, string> additionalInfo = null);
        
        /// <summary>
        /// Starts a bulk import of vehicles
        /// </summary>
        Task<string> StartVehicleImportAsync(IFormFile file, string format, VehicleImportOptions options);
        
        /// <summary>
        /// Gets the status of a vehicle import
        /// </summary>
        Task<VehicleImportStatus> GetImportStatusAsync(string importId);
    }
    
    /// <summary>
    /// Interface for vehicle image operations
    /// </summary>
    public interface IVehicleImageService
    {
        /// <summary>
        /// Uploads images for a vehicle
        /// </summary>
        Task<List<VehicleImageResult>> UploadVehicleImagesAsync(Guid vehicleId, IFormFileCollection images);
        
        /// <summary>
        /// Gets all images for a vehicle
        /// </summary>
        Task<List<VehicleImage>> GetVehicleImagesAsync(Guid vehicleId);
        
        /// <summary>
        /// Deletes an image from a vehicle
        /// </summary>
        Task<bool> DeleteVehicleImageAsync(Guid vehicleId, Guid imageId);
        
        /// <summary>
        /// Sets the primary image for a vehicle
        /// </summary>
        Task<bool> SetPrimaryImageAsync(Guid vehicleId, Guid imageId);
        
        /// <summary>
        /// Updates image metadata
        /// </summary>
        Task<VehicleImage> UpdateImageMetadataAsync(Guid vehicleId, Guid imageId, Dictionary<string, string> metadata);
    }
    
    /// <summary>
    /// Interface for vehicle document operations
    /// </summary>
    public interface IVehicleDocumentService
    {
        /// <summary>
        /// Uploads documents for a vehicle
        /// </summary>
        Task<List<VehicleDocumentResult>> UploadVehicleDocumentsAsync(Guid vehicleId, IFormFileCollection documents, Dictionary<string, string> documentTypes);
        
        /// <summary>
        /// Gets all documents for a vehicle
        /// </summary>
        Task<List<VehicleDocument>> GetVehicleDocumentsAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets a specific document for a vehicle
        /// </summary>
        Task<VehicleDocument> GetVehicleDocumentAsync(Guid vehicleId, Guid documentId);
        
        /// <summary>
        /// Deletes a document from a vehicle
        /// </summary>
        Task<bool> DeleteVehicleDocumentAsync(Guid vehicleId, Guid documentId);
        
        /// <summary>
        /// Updates document metadata
        /// </summary>
        Task<VehicleDocument> UpdateDocumentMetadataAsync(Guid vehicleId, Guid documentId, Dictionary<string, string> metadata);
    }
    
    /// <summary>
    /// Represents the result of a vehicle transfer operation
    /// </summary>
    public class VehicleTransferResult
    {
        public Guid VehicleId { get; set; }
        public Guid DestinationLocationId { get; set; }
        public Guid? DestinationZoneId { get; set; }
        public Guid PreviousLocationId { get; set; }
        public Guid? PreviousZoneId { get; set; }
        public DateTime TransferDate { get; set; }
        public string TransferReason { get; set; }
        public string TransferredBy { get; set; }
    }
    
    /// <summary>
    /// Represents the result of a vehicle status update operation
    /// </summary>
    public class VehicleStatusUpdateResult
    {
        public Guid VehicleId { get; set; }
        public string NewStatus { get; set; }
        public string PreviousStatus { get; set; }
        public DateTime StatusChangeDate { get; set; }
        public string StatusChangeReason { get; set; }
        public string ChangedBy { get; set; }
    }
    
    /// <summary>
    /// Represents the result of a vehicle image upload
    /// </summary>
    public class VehicleImageResult
    {
        public Guid ImageId { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public DateTime UploadDate { get; set; }
        public bool IsPrimary { get; set; }
    }
    
    /// <summary>
    /// Represents the result of a vehicle document upload
    /// </summary>
    public class VehicleDocumentResult
    {
        public Guid DocumentId { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
        public string ContentType { get; set; }
        public string DocumentType { get; set; }
        public DateTime UploadDate { get; set; }
    }
    
    /// <summary>
    /// Represents the status of a vehicle import operation
    /// </summary>
    public class VehicleImportStatus
    {
        public string ImportId { get; set; }
        public string Status { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedRecords { get; set; }
        public List<string> Errors { get; set; }
    }
    
    /// <summary>
    /// Represents options for filtering vehicles
    /// </summary>
    public class VehicleFilterOptions
    {
        public string SearchTerm { get; set; }
        public List<string> Makes { get; set; }
        public List<string> Models { get; set; }
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public List<string> Conditions { get; set; }
        public List<string> Statuses { get; set; }
        public Guid? LocationId { get; set; }
        public int? DaysInInventoryMin { get; set; }
        public int? DaysInInventoryMax { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public bool IncludeImages { get; set; }
        public bool IncludeCosts { get; set; }
        public bool IncludePricing { get; set; }
        public string SortBy { get; set; }
        public bool SortDescending { get; set; }
    }
}
