using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for marketplace integration services
    /// Handles integration with external marketplaces like AutoTrader, Cars.com, etc.
    /// </summary>
    public interface IMarketplaceIntegrationService
    {
        /// <summary>
        /// Lists a vehicle on external marketplaces
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle to list</param>
        /// <param name="marketplaceIds">IDs of the marketplaces to list on</param>
        /// <returns>A dictionary with marketplace ID and listing result</returns>
        Task<Dictionary<string, MarketplaceListingResult>> ListVehicleAsync(Guid vehicleId, IEnumerable<string> marketplaceIds);
        
        /// <summary>
        /// Updates an existing marketplace listing
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <param name="marketplaceIds">IDs of the marketplaces to update</param>
        /// <returns>A dictionary with marketplace ID and update result</returns>
        Task<Dictionary<string, MarketplaceListingResult>> UpdateVehicleListingAsync(Guid vehicleId, IEnumerable<string> marketplaceIds);
        
        /// <summary>
        /// Removes a vehicle listing from external marketplaces
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <param name="marketplaceIds">IDs of the marketplaces to remove from</param>
        /// <returns>A dictionary with marketplace ID and removal result</returns>
        Task<Dictionary<string, MarketplaceListingResult>> RemoveVehicleListingAsync(Guid vehicleId, IEnumerable<string> marketplaceIds);
        
        /// <summary>
        /// Gets listing status for a vehicle across all marketplaces
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>A dictionary with marketplace ID and listing status</returns>
        Task<Dictionary<string, MarketplaceListingStatus>> GetVehicleListingStatusAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets all available marketplaces for integration
        /// </summary>
        /// <returns>A list of marketplace definitions</returns>
        Task<IEnumerable<MarketplaceDefinition>> GetAvailableMarketplacesAsync();
        
        /// <summary>
        /// Gets listing statistics for a vehicle across all marketplaces
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>A dictionary with marketplace ID and listing statistics</returns>
        Task<Dictionary<string, MarketplaceListingStats>> GetVehicleListingStatsAsync(Guid vehicleId);
        
        /// <summary>
        /// Verifies marketplace account credentials
        /// </summary>
        /// <param name="marketplaceId">ID of the marketplace</param>
        /// <param name="credentials">Authentication credentials for the marketplace</param>
        /// <returns>True if credentials are valid, otherwise false</returns>
        Task<bool> VerifyMarketplaceCredentialsAsync(string marketplaceId, MarketplaceCredentials credentials);
        
        /// <summary>
        /// Synchronizes the inventory with all configured marketplaces
        /// </summary>
        /// <returns>A summary of the synchronization process</returns>
        Task<MarketplaceSyncSummary> SynchronizeInventoryAsync();
    }
    
    /// <summary>
    /// Result of a marketplace listing operation
    /// </summary>
    public class MarketplaceListingResult
    {
        public bool Success { get; set; }
        public string ListingId { get; set; }
        public string ListingUrl { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime ProcessedAt { get; set; }
    }
    
    /// <summary>
    /// Status of a marketplace listing
    /// </summary>
    public class MarketplaceListingStatus
    {
        public string ListingId { get; set; }
        public string ListingUrl { get; set; }
        public MarketplaceListingState State { get; set; }
        public DateTime LastUpdated { get; set; }
        public string StatusMessage { get; set; }
        public decimal ListingPrice { get; set; }
        public bool IsFeatured { get; set; }
        public Dictionary<string, string> MarketplaceSpecificData { get; set; }
    }
    
    /// <summary>
    /// Statistics for a marketplace listing
    /// </summary>
    public class MarketplaceListingStats
    {
        public int Views { get; set; }
        public int Saves { get; set; }
        public int Inquiries { get; set; }
        public int Shares { get; set; }
        public Dictionary<string, int> DailyViews { get; set; }
        public int ComparisonRank { get; set; }
        public double EngagementScore { get; set; }
    }
    
    /// <summary>
    /// Definition of a marketplace
    /// </summary>
    public class MarketplaceDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string LogoUrl { get; set; }
        public bool IsConnected { get; set; }
        public bool SupportsRealTimeUpdates { get; set; }
        public bool SupportsBulkOperations { get; set; }
        public IEnumerable<string> SupportedFeatures { get; set; }
        public MarketplaceConnectionStatus ConnectionStatus { get; set; }
    }
    
    /// <summary>
    /// Credentials for marketplace authentication
    /// </summary>
    public class MarketplaceCredentials
    {
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DealerId { get; set; }
        public Dictionary<string, string> AdditionalParameters { get; set; }
    }
    
    /// <summary>
    /// Summary of a marketplace synchronization operation
    /// </summary>
    public class MarketplaceSyncSummary
    {
        public DateTime SyncStartedAt { get; set; }
        public DateTime SyncCompletedAt { get; set; }
        public int TotalVehicles { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public int NewListings { get; set; }
        public int UpdatedListings { get; set; }
        public int RemovedListings { get; set; }
        public Dictionary<string, int> MarketplaceSummary { get; set; }
        public IEnumerable<MarketplaceSyncError> Errors { get; set; }
    }
    
    /// <summary>
    /// Error during marketplace synchronization
    /// </summary>
    public class MarketplaceSyncError
    {
        public Guid VehicleId { get; set; }
        public string MarketplaceId { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// State of a marketplace listing
    /// </summary>
    public enum MarketplaceListingState
    {
        Active,
        Pending,
        Rejected,
        Expired,
        Removed,
        Featured,
        Unknown
    }
    
    /// <summary>
    /// Status of a marketplace connection
    /// </summary>
    public enum MarketplaceConnectionStatus
    {
        Connected,
        Disconnected,
        AuthError,
        ConfigurationError,
        RateLimited,
        ServiceUnavailable
    }
}
