using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace DMS.InventoryManagement.API.Models
{
    /// <summary>
    /// Represents a request to transfer a vehicle between locations
    /// </summary>
    public class VehicleTransferRequest
    {
        /// <summary>
        /// The ID of the destination location
        /// </summary>
        public Guid DestinationLocationId { get; set; }
        
        /// <summary>
        /// The ID of the specific zone within the destination location (optional)
        /// </summary>
        public Guid? DestinationZoneId { get; set; }
        
        /// <summary>
        /// The reason for the transfer
        /// </summary>
        public string TransferReason { get; set; }
    }
    
    /// <summary>
    /// Represents a request to update a vehicle's status
    /// </summary>
    public class VehicleStatusUpdateRequest
    {
        /// <summary>
        /// The new status for the vehicle
        /// </summary>
        public string NewStatus { get; set; }
        
        /// <summary>
        /// The reason for the status change
        /// </summary>
        public string StatusChangeReason { get; set; }
        
        /// <summary>
        /// Additional information related to the status change
        /// </summary>
        public Dictionary<string, string> AdditionalInfo { get; set; }
    }
    
    /// <summary>
    /// Represents a request to upload documents for a vehicle
    /// </summary>
    public class VehicleDocumentUploadRequest
    {
        /// <summary>
        /// The documents to upload
        /// </summary>
        public IFormFileCollection Documents { get; set; }
        
        /// <summary>
        /// The types of the documents being uploaded, mapped by filename
        /// </summary>
        public Dictionary<string, string> DocumentTypes { get; set; }
    }
    
    /// <summary>
    /// Represents a request to import vehicles from a file
    /// </summary>
    public class VehicleImportRequest
    {
        /// <summary>
        /// The file containing vehicle data
        /// </summary>
        public IFormFile File { get; set; }
        
        /// <summary>
        /// The format of the import file (CSV, Excel, XML, etc.)
        /// </summary>
        public string Format { get; set; } = "CSV";
        
        /// <summary>
        /// Additional import options
        /// </summary>
        public VehicleImportOptions Options { get; set; } = new VehicleImportOptions();
    }
    
    /// <summary>
    /// Represents options for importing vehicles
    /// </summary>
    public class VehicleImportOptions
    {
        /// <summary>
        /// Whether to skip the header row
        /// </summary>
        public bool SkipHeaderRow { get; set; } = true;
        
        /// <summary>
        /// Whether to validate the data before importing
        /// </summary>
        public bool ValidateBeforeImport { get; set; } = true;
        
        /// <summary>
        /// The column delimiter for CSV files
        /// </summary>
        public string Delimiter { get; set; } = ",";
        
        /// <summary>
        /// Custom field mappings (column name -> property name)
        /// </summary>
        public Dictionary<string, string> FieldMappings { get; set; }
        
        /// <summary>
        /// The default location ID to use for imported vehicles
        /// </summary>
        public Guid? DefaultLocationId { get; set; }
        
        /// <summary>
        /// The default status to use for imported vehicles
        /// </summary>
        public string DefaultStatus { get; set; } = "In-Transit";
    }
}
