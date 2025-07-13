using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DMS.InventoryManagement.Core.Services;
using DMS.InventoryManagement.Core.Exceptions;
using DMS.InventoryManagement.Core.Data;
using System.Text.Json;
using System.Linq;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of IMarketplaceIntegrationService for managing vehicle listings on external marketplaces
    /// </summary>
    public class MarketplaceIntegrationService : IMarketplaceIntegrationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<MarketplaceIntegrationService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly MarketplaceOptions _options;
        private readonly IVehicleService _vehicleService;
        private readonly IVehicleImageService _vehicleImageService;
        
        private readonly Dictionary<string, IMarketplaceProvider> _providers;
        
        public MarketplaceIntegrationService(
            IHttpClientFactory httpClientFactory,
            ILogger<MarketplaceIntegrationService> logger,
            IUnitOfWork unitOfWork,
            IOptions<MarketplaceOptions> options,
            IVehicleService vehicleService,
            IVehicleImageService vehicleImageService,
            IEnumerable<IMarketplaceProvider> marketplaceProviders)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _vehicleService = vehicleService ?? throw new ArgumentNullException(nameof(vehicleService));
            _vehicleImageService = vehicleImageService ?? throw new ArgumentNullException(nameof(vehicleImageService));
            
            // Initialize marketplace providers
            _providers = marketplaceProviders?.ToDictionary(p => p.MarketplaceId) ?? 
                         new Dictionary<string, IMarketplaceProvider>();
        }
        
        /// <inheritdoc/>
        public async Task<Dictionary<string, MarketplaceListingResult>> ListVehicleAsync(Guid vehicleId, IEnumerable<string> marketplaceIds)
        {
            _logger.LogInformation("Listing vehicle {VehicleId} on marketplaces: {Marketplaces}", 
                vehicleId, string.Join(", ", marketplaceIds));
            
            var result = new Dictionary<string, MarketplaceListingResult>();
            var vehicle = await _vehicleService.GetVehicleDetailsByIdAsync(vehicleId);
            
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle not found: {VehicleId}", vehicleId);
                throw new KeyNotFoundException($"Vehicle not found with ID: {vehicleId}");
            }
            
            foreach (var marketplaceId in marketplaceIds)
            {
                if (!_providers.TryGetValue(marketplaceId, out var provider))
                {
                    _logger.LogWarning("Marketplace provider not found: {MarketplaceId}", marketplaceId);
                    result[marketplaceId] = new MarketplaceListingResult
                    {
                        Success = false,
                        ErrorMessage = $"Provider not configured for marketplace: {marketplaceId}",
                        ProcessedAt = DateTime.UtcNow
                    };
                    continue;
                }
                
                try
                {
                    var images = await _vehicleImageService.GetVehicleImagesAsync(vehicleId);
                    var listingData = await provider.CreateListingAsync(vehicle, images);
                    
                    // Store the listing reference
                    await SaveMarketplaceReference(vehicleId, marketplaceId, listingData.ListingId, listingData.ListingUrl);
                    
                    result[marketplaceId] = listingData;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error listing vehicle {VehicleId} on {MarketplaceId}", vehicleId, marketplaceId);
                    result[marketplaceId] = new MarketplaceListingResult
                    {
                        Success = false,
                        ErrorMessage = $"Failed to list on {marketplaceId}: {ex.Message}",
                        ProcessedAt = DateTime.UtcNow
                    };
                }
            }
            
            return result;
        }
        
        /// <inheritdoc/>
        public async Task<Dictionary<string, MarketplaceListingResult>> UpdateVehicleListingAsync(Guid vehicleId, IEnumerable<string> marketplaceIds)
        {
            _logger.LogInformation("Updating vehicle {VehicleId} on marketplaces: {Marketplaces}", 
                vehicleId, string.Join(", ", marketplaceIds));
            
            var result = new Dictionary<string, MarketplaceListingResult>();
            var vehicle = await _vehicleService.GetVehicleDetailsByIdAsync(vehicleId);
            
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle not found: {VehicleId}", vehicleId);
                throw new KeyNotFoundException($"Vehicle not found with ID: {vehicleId}");
            }
            
            // Get existing listing references
            var existingReferences = await GetMarketplaceReferences(vehicleId);
            
            foreach (var marketplaceId in marketplaceIds)
            {
                if (!_providers.TryGetValue(marketplaceId, out var provider))
                {
                    _logger.LogWarning("Marketplace provider not found: {MarketplaceId}", marketplaceId);
                    result[marketplaceId] = new MarketplaceListingResult
                    {
                        Success = false,
                        ErrorMessage = $"Provider not configured for marketplace: {marketplaceId}",
                        ProcessedAt = DateTime.UtcNow
                    };
                    continue;
                }
                
                try
                {
                    // Check if we have an existing listing ID
                    string listingId = null;
                    if (existingReferences.TryGetValue(marketplaceId, out var reference))
                    {
                        listingId = reference.ListingId;
                    }
                    
                    var images = await _vehicleImageService.GetVehicleImagesAsync(vehicleId);
                    MarketplaceListingResult updateResult;
                    
                    if (string.IsNullOrEmpty(listingId))
                    {
                        // No existing listing, create a new one
                        updateResult = await provider.CreateListingAsync(vehicle, images);
                    }
                    else
                    {
                        // Update existing listing
                        updateResult = await provider.UpdateListingAsync(listingId, vehicle, images);
                    }
                    
                    // Update the listing reference
                    await SaveMarketplaceReference(vehicleId, marketplaceId, updateResult.ListingId, updateResult.ListingUrl);
                    
                    result[marketplaceId] = updateResult;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating vehicle {VehicleId} on {MarketplaceId}", vehicleId, marketplaceId);
                    result[marketplaceId] = new MarketplaceListingResult
                    {
                        Success = false,
                        ErrorMessage = $"Failed to update on {marketplaceId}: {ex.Message}",
                        ProcessedAt = DateTime.UtcNow
                    };
                }
            }
            
            return result;
        }
        
        /// <inheritdoc/>
        public async Task<Dictionary<string, MarketplaceListingResult>> RemoveVehicleListingAsync(Guid vehicleId, IEnumerable<string> marketplaceIds)
        {
            _logger.LogInformation("Removing vehicle {VehicleId} from marketplaces: {Marketplaces}", 
                vehicleId, string.Join(", ", marketplaceIds));
            
            var result = new Dictionary<string, MarketplaceListingResult>();
            
            // Get existing listing references
            var existingReferences = await GetMarketplaceReferences(vehicleId);
            
            foreach (var marketplaceId in marketplaceIds)
            {
                if (!_providers.TryGetValue(marketplaceId, out var provider))
                {
                    _logger.LogWarning("Marketplace provider not found: {MarketplaceId}", marketplaceId);
                    result[marketplaceId] = new MarketplaceListingResult
                    {
                        Success = false,
                        ErrorMessage = $"Provider not configured for marketplace: {marketplaceId}",
                        ProcessedAt = DateTime.UtcNow
                    };
                    continue;
                }
                
                try
                {
                    // Check if we have an existing listing ID
                    if (!existingReferences.TryGetValue(marketplaceId, out var reference) || string.IsNullOrEmpty(reference.ListingId))
                    {
                        result[marketplaceId] = new MarketplaceListingResult
                        {
                            Success = true,
                            ErrorMessage = "No listing found to remove",
                            ProcessedAt = DateTime.UtcNow
                        };
                        continue;
                    }
                    
                    var removeResult = await provider.RemoveListingAsync(reference.ListingId);
                    
                    // Remove the listing reference
                    await RemoveMarketplaceReference(vehicleId, marketplaceId);
                    
                    result[marketplaceId] = removeResult;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error removing vehicle {VehicleId} from {MarketplaceId}", vehicleId, marketplaceId);
                    result[marketplaceId] = new MarketplaceListingResult
                    {
                        Success = false,
                        ErrorMessage = $"Failed to remove from {marketplaceId}: {ex.Message}",
                        ProcessedAt = DateTime.UtcNow
                    };
                }
            }
            
            return result;
        }
        
        /// <inheritdoc/>
        public async Task<Dictionary<string, MarketplaceListingStatus>> GetVehicleListingStatusAsync(Guid vehicleId)
        {
            _logger.LogInformation("Getting listing status for vehicle {VehicleId}", vehicleId);
            
            var result = new Dictionary<string, MarketplaceListingStatus>();
            
            // Get existing listing references
            var existingReferences = await GetMarketplaceReferences(vehicleId);
            
            foreach (var reference in existingReferences)
            {
                var marketplaceId = reference.Key;
                var listingRef = reference.Value;
                
                if (!_providers.TryGetValue(marketplaceId, out var provider))
                {
                    _logger.LogWarning("Marketplace provider not found: {MarketplaceId}", marketplaceId);
                    result[marketplaceId] = new MarketplaceListingStatus
                    {
                        ListingId = listingRef.ListingId,
                        ListingUrl = listingRef.ListingUrl,
                        State = MarketplaceListingState.Unknown,
                        LastUpdated = DateTime.UtcNow,
                        StatusMessage = "Provider not configured"
                    };
                    continue;
                }
                
                try
                {
                    if (string.IsNullOrEmpty(listingRef.ListingId))
                    {
                        result[marketplaceId] = new MarketplaceListingStatus
                        {
                            State = MarketplaceListingState.Unknown,
                            LastUpdated = DateTime.UtcNow,
                            StatusMessage = "No listing ID available"
                        };
                        continue;
                    }
                    
                    var status = await provider.GetListingStatusAsync(listingRef.ListingId);
                    result[marketplaceId] = status;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting status for vehicle {VehicleId} on {MarketplaceId}", 
                        vehicleId, marketplaceId);
                    result[marketplaceId] = new MarketplaceListingStatus
                    {
                        ListingId = listingRef.ListingId,
                        ListingUrl = listingRef.ListingUrl,
                        State = MarketplaceListingState.Unknown,
                        LastUpdated = DateTime.UtcNow,
                        StatusMessage = $"Failed to get status: {ex.Message}"
                    };
                }
            }
            
            return result;
        }
        
        /// <inheritdoc/>
        public async Task<IEnumerable<MarketplaceDefinition>> GetAvailableMarketplacesAsync()
        {
            var marketplaces = new List<MarketplaceDefinition>();
            
            foreach (var provider in _providers.Values)
            {
                try
                {
                    var definition = await provider.GetMarketplaceDefinitionAsync();
                    marketplaces.Add(definition);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting marketplace definition for {MarketplaceId}", 
                        provider.MarketplaceId);
                }
            }
            
            return marketplaces;
        }
        
        /// <inheritdoc/>
        public async Task<Dictionary<string, MarketplaceListingStats>> GetVehicleListingStatsAsync(Guid vehicleId)
        {
            _logger.LogInformation("Getting listing stats for vehicle {VehicleId}", vehicleId);
            
            var result = new Dictionary<string, MarketplaceListingStats>();
            
            // Get existing listing references
            var existingReferences = await GetMarketplaceReferences(vehicleId);
            
            foreach (var reference in existingReferences)
            {
                var marketplaceId = reference.Key;
                var listingRef = reference.Value;
                
                if (!_providers.TryGetValue(marketplaceId, out var provider))
                {
                    _logger.LogWarning("Marketplace provider not found: {MarketplaceId}", marketplaceId);
                    continue;
                }
                
                try
                {
                    if (string.IsNullOrEmpty(listingRef.ListingId))
                    {
                        continue;
                    }
                    
                    var stats = await provider.GetListingStatsAsync(listingRef.ListingId);
                    result[marketplaceId] = stats;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting stats for vehicle {VehicleId} on {MarketplaceId}", 
                        vehicleId, marketplaceId);
                }
            }
            
            return result;
        }
        
        /// <inheritdoc/>
        public async Task<bool> VerifyMarketplaceCredentialsAsync(string marketplaceId, MarketplaceCredentials credentials)
        {
            if (!_providers.TryGetValue(marketplaceId, out var provider))
            {
                _logger.LogWarning("Marketplace provider not found: {MarketplaceId}", marketplaceId);
                return false;
            }
            
            try
            {
                return await provider.VerifyCredentialsAsync(credentials);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying credentials for {MarketplaceId}", marketplaceId);
                return false;
            }
        }
        
        /// <inheritdoc/>
        public async Task<MarketplaceSyncSummary> SynchronizeInventoryAsync()
        {
            _logger.LogInformation("Starting inventory synchronization with marketplaces");
            
            var summary = new MarketplaceSyncSummary
            {
                SyncStartedAt = DateTime.UtcNow,
                MarketplaceSummary = new Dictionary<string, int>(),
                Errors = new List<MarketplaceSyncError>()
            };
            
            try
            {
                // Get all available vehicles that should be listed on marketplaces
                var vehicles = await _vehicleService.GetListableVehiclesAsync();
                summary.TotalVehicles = vehicles.Count;
                
                foreach (var vehicle in vehicles)
                {
                    foreach (var provider in _providers.Values)
                    {
                        var marketplaceId = provider.MarketplaceId;
                        
                        try
                        {
                            var references = await GetMarketplaceReferences(vehicle.Id);
                            bool hasExistingListing = references.TryGetValue(marketplaceId, out var reference) && 
                                                      !string.IsNullOrEmpty(reference.ListingId);
                            
                            if (vehicle.Status == "Sold" || vehicle.Status == "Transferred" || vehicle.Status == "OnHold")
                            {
                                // Vehicle should be removed from marketplaces
                                if (hasExistingListing)
                                {
                                    await provider.RemoveListingAsync(reference.ListingId);
                                    await RemoveMarketplaceReference(vehicle.Id, marketplaceId);
                                    
                                    IncrementSummaryCounter(summary.MarketplaceSummary, marketplaceId);
                                    summary.RemovedListings++;
                                }
                            }
                            else if (vehicle.Status == "Available")
                            {
                                // Vehicle should be listed on marketplaces
                                var images = await _vehicleImageService.GetVehicleImagesAsync(vehicle.Id);
                                
                                if (hasExistingListing)
                                {
                                    // Update existing listing
                                    var result = await provider.UpdateListingAsync(reference.ListingId, vehicle, images);
                                    
                                    if (result.Success)
                                    {
                                        await SaveMarketplaceReference(vehicle.Id, marketplaceId, result.ListingId, result.ListingUrl);
                                        IncrementSummaryCounter(summary.MarketplaceSummary, marketplaceId);
                                        summary.UpdatedListings++;
                                    }
                                    else
                                    {
                                        AddSyncError(summary, vehicle.Id, marketplaceId, "UPDATE_FAILED", result.ErrorMessage);
                                        summary.FailureCount++;
                                    }
                                }
                                else
                                {
                                    // Create new listing
                                    var result = await provider.CreateListingAsync(vehicle, images);
                                    
                                    if (result.Success)
                                    {
                                        await SaveMarketplaceReference(vehicle.Id, marketplaceId, result.ListingId, result.ListingUrl);
                                        IncrementSummaryCounter(summary.MarketplaceSummary, marketplaceId);
                                        summary.NewListings++;
                                    }
                                    else
                                    {
                                        AddSyncError(summary, vehicle.Id, marketplaceId, "CREATE_FAILED", result.ErrorMessage);
                                        summary.FailureCount++;
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error synchronizing vehicle {VehicleId} with marketplace {MarketplaceId}", 
                                vehicle.Id, marketplaceId);
                            
                            AddSyncError(summary, vehicle.Id, marketplaceId, "EXCEPTION", ex.Message);
                            summary.FailureCount++;
                        }
                    }
                }
                
                summary.SuccessCount = (summary.NewListings + summary.UpdatedListings + summary.RemovedListings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during inventory synchronization");
            }
            
            summary.SyncCompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Inventory synchronization completed. Total: {Total}, Success: {Success}, Failures: {Failures}", 
                summary.TotalVehicles, summary.SuccessCount, summary.FailureCount);
            
            return summary;
        }
        
        #region Helper Methods
        
        private async Task<Dictionary<string, MarketplaceListingReference>> GetMarketplaceReferences(Guid vehicleId)
        {
            try
            {
                // In a real implementation, this would fetch from a database
                // Using a mock implementation for now
                var repository = _unitOfWork.GetRepository<MarketplaceListingReference>();
                var references = await repository.FindAsync(r => r.VehicleId == vehicleId);
                return references.ToDictionary(r => r.MarketplaceId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting marketplace references for vehicle {VehicleId}", vehicleId);
                return new Dictionary<string, MarketplaceListingReference>();
            }
        }
        
        private async Task SaveMarketplaceReference(Guid vehicleId, string marketplaceId, string listingId, string listingUrl)
        {
            try
            {
                // In a real implementation, this would save to a database
                var repository = _unitOfWork.GetRepository<MarketplaceListingReference>();
                var existing = await repository.FindOneAsync(r => r.VehicleId == vehicleId && r.MarketplaceId == marketplaceId);
                
                if (existing != null)
                {
                    existing.ListingId = listingId;
                    existing.ListingUrl = listingUrl;
                    existing.LastUpdated = DateTime.UtcNow;
                    await repository.UpdateAsync(existing);
                }
                else
                {
                    var reference = new MarketplaceListingReference
                    {
                        Id = Guid.NewGuid(),
                        VehicleId = vehicleId,
                        MarketplaceId = marketplaceId,
                        ListingId = listingId,
                        ListingUrl = listingUrl,
                        CreatedAt = DateTime.UtcNow,
                        LastUpdated = DateTime.UtcNow
                    };
                    
                    await repository.AddAsync(reference);
                }
                
                await _unitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving marketplace reference for vehicle {VehicleId}, marketplace {MarketplaceId}", 
                    vehicleId, marketplaceId);
            }
        }
        
        private async Task RemoveMarketplaceReference(Guid vehicleId, string marketplaceId)
        {
            try
            {
                // In a real implementation, this would delete from a database
                var repository = _unitOfWork.GetRepository<MarketplaceListingReference>();
                var existing = await repository.FindOneAsync(r => r.VehicleId == vehicleId && r.MarketplaceId == marketplaceId);
                
                if (existing != null)
                {
                    await repository.DeleteAsync(existing);
                    await _unitOfWork.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing marketplace reference for vehicle {VehicleId}, marketplace {MarketplaceId}", 
                    vehicleId, marketplaceId);
            }
        }
        
        private void IncrementSummaryCounter(Dictionary<string, int> summary, string marketplaceId)
        {
            if (summary.ContainsKey(marketplaceId))
            {
                summary[marketplaceId]++;
            }
            else
            {
                summary[marketplaceId] = 1;
            }
        }
        
        private void AddSyncError(MarketplaceSyncSummary summary, Guid vehicleId, string marketplaceId, string errorCode, string errorMessage)
        {
            ((List<MarketplaceSyncError>)summary.Errors).Add(new MarketplaceSyncError
            {
                VehicleId = vehicleId,
                MarketplaceId = marketplaceId,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage
            });
        }
        
        #endregion
    }
    
    /// <summary>
    /// Interface for marketplace providers
    /// </summary>
    public interface IMarketplaceProvider
    {
        string MarketplaceId { get; }
        
        Task<MarketplaceListingResult> CreateListingAsync(object vehicleData, IEnumerable<object> images);
        Task<MarketplaceListingResult> UpdateListingAsync(string listingId, object vehicleData, IEnumerable<object> images);
        Task<MarketplaceListingResult> RemoveListingAsync(string listingId);
        Task<MarketplaceListingStatus> GetListingStatusAsync(string listingId);
        Task<MarketplaceListingStats> GetListingStatsAsync(string listingId);
        Task<MarketplaceDefinition> GetMarketplaceDefinitionAsync();
        Task<bool> VerifyCredentialsAsync(MarketplaceCredentials credentials);
    }
    
    /// <summary>
    /// Configuration options for marketplaces
    /// </summary>
    public class MarketplaceOptions
    {
        public Dictionary<string, MarketplaceProviderSettings> Providers { get; set; } = new();
        public bool AutoSyncEnabled { get; set; }
        public int AutoSyncIntervalMinutes { get; set; } = 60;
    }
    
    /// <summary>
    /// Settings for a marketplace provider
    /// </summary>
    public class MarketplaceProviderSettings
    {
        public string ApiBaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string ApiSecret { get; set; }
        public string DealerId { get; set; }
        public Dictionary<string, string> AdditionalSettings { get; set; } = new();
    }
    
    /// <summary>
    /// Reference to a marketplace listing for a vehicle
    /// </summary>
    public class MarketplaceListingReference
    {
        public Guid Id { get; set; }
        public Guid VehicleId { get; set; }
        public string MarketplaceId { get; set; }
        public string ListingId { get; set; }
        public string ListingUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
