using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Service interface for importing vehicles from various sources
    /// </summary>
    public interface IVehicleImportService
    {
        /// <summary>
        /// Imports vehicles from a CSV file
        /// </summary>
        /// <param name="filePath">Path to the CSV file</param>
        /// <param name="mappingTemplate">Name of the column mapping template to use</param>
        /// <returns>Import result containing success, error count, and imported vehicles</returns>
        Task<VehicleImportResult> ImportFromCsvAsync(string filePath, string mappingTemplate);
        
        /// <summary>
        /// Imports vehicles from a manufacturer feed
        /// </summary>
        /// <param name="manufacturerCode">Manufacturer code (e.g., "TOYOTA", "FORD")</param>
        /// <param name="options">Import options</param>
        /// <returns>Import result containing success, error count, and imported vehicles</returns>
        Task<VehicleImportResult> ImportFromManufacturerFeedAsync(string manufacturerCode, ManufacturerImportOptions options);
        
        /// <summary>
        /// Imports vehicles from an auction platform
        /// </summary>
        /// <param name="auctionCode">Auction platform code (e.g., "MANHEIM", "ADESA")</param>
        /// <param name="options">Import options</param>
        /// <returns>Import result containing success, error count, and imported vehicles</returns>
        Task<VehicleImportResult> ImportFromAuctionAsync(string auctionCode, AuctionImportOptions options);
        
        /// <summary>
        /// Gets available mapping templates for CSV import
        /// </summary>
        /// <returns>List of available mapping templates</returns>
        Task<IEnumerable<MappingTemplate>> GetAvailableMappingTemplatesAsync();
        
        /// <summary>
        /// Gets available manufacturer feed codes
        /// </summary>
        /// <returns>List of available manufacturer feed codes</returns>
        Task<IEnumerable<string>> GetAvailableManufacturerCodesAsync();
        
        /// <summary>
        /// Gets available auction platform codes
        /// </summary>
        /// <returns>List of available auction platform codes</returns>
        Task<IEnumerable<string>> GetAvailableAuctionCodesAsync();
    }
    
    /// <summary>
    /// Result of a vehicle import operation
    /// </summary>
    public class VehicleImportResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the import was successful
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// Gets or sets the total number of records processed
        /// </summary>
        public int TotalRecords { get; set; }
        
        /// <summary>
        /// Gets or sets the number of records successfully imported
        /// </summary>
        public int SuccessCount { get; set; }
        
        /// <summary>
        /// Gets or sets the number of records that failed to import
        /// </summary>
        public int ErrorCount { get; set; }
        
        /// <summary>
        /// Gets or sets the list of errors encountered during import
        /// </summary>
        public List<string> Errors { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the list of imported vehicles
        /// </summary>
        public List<Vehicle> ImportedVehicles { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the list of validation warnings
        /// </summary>
        public List<ValidationWarning> Warnings { get; set; } = new();
    }
    
    /// <summary>
    /// Validation warning for vehicle import
    /// </summary>
    public class ValidationWarning
    {
        /// <summary>
        /// Gets or sets the row number in the source data
        /// </summary>
        public int RowNumber { get; set; }
        
        /// <summary>
        /// Gets or sets the field name with the warning
        /// </summary>
        public string FieldName { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the warning message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// CSV column mapping template
    /// </summary>
    public class MappingTemplate
    {
        /// <summary>
        /// Gets or sets the template name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the template description
        /// </summary>
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets the column mappings
        /// </summary>
        public Dictionary<string, string> ColumnMappings { get; set; } = new();
    }
    
    /// <summary>
    /// Options for manufacturer feed import
    /// </summary>
    public class ManufacturerImportOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to import only new inventory
        /// </summary>
        public bool NewInventoryOnly { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to update existing inventory
        /// </summary>
        public bool UpdateExisting { get; set; }
        
        /// <summary>
        /// Gets or sets dealer codes to filter by
        /// </summary>
        public List<string> DealerCodes { get; set; } = new();
        
        /// <summary>
        /// Gets or sets the date range to filter by arrival date
        /// </summary>
        public DateRangeFilter? ArrivalDateFilter { get; set; }
    }
    
    /// <summary>
    /// Options for auction import
    /// </summary>
    public class AuctionImportOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether to include only won auctions
        /// </summary>
        public bool WonAuctionsOnly { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to include only active auctions
        /// </summary>
        public bool ActiveAuctionsOnly { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether to include only auctions the dealership is watching
        /// </summary>
        public bool WatchedAuctionsOnly { get; set; }
        
        /// <summary>
        /// Gets or sets the date range to filter by auction date
        /// </summary>
        public DateRangeFilter? AuctionDateFilter { get; set; }
        
        /// <summary>
        /// Gets or sets the makes to filter by
        /// </summary>
        public List<string> Makes { get; set; } = new();
    }
    
    /// <summary>
    /// Date range filter
    /// </summary>
    public class DateRangeFilter
    {
        /// <summary>
        /// Gets or sets the start date
        /// </summary>
        public DateTime? StartDate { get; set; }
        
        /// <summary>
        /// Gets or sets the end date
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
