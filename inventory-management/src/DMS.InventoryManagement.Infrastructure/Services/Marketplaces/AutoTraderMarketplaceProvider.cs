using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DMS.InventoryManagement.Core.Services;

namespace DMS.InventoryManagement.Infrastructure.Services.Marketplaces
{
    /// <summary>
    /// AutoTrader integration provider for marketplace listings
    /// </summary>
    public class AutoTraderMarketplaceProvider : IMarketplaceProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AutoTraderMarketplaceProvider> _logger;
        private readonly MarketplaceProviderSettings _settings;
        
        public string MarketplaceId => "autotrader";
        
        public AutoTraderMarketplaceProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<AutoTraderMarketplaceProvider> logger,
            IOptions<MarketplaceOptions> options)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            if (options?.Value?.Providers == null || 
                !options.Value.Providers.TryGetValue(MarketplaceId, out var settings))
            {
                throw new ArgumentException($"Settings for {MarketplaceId} marketplace not found");
            }
            
            _settings = settings;
        }
        
        /// <inheritdoc/>
        public async Task<MarketplaceListingResult> CreateListingAsync(object vehicleData, IEnumerable<object> images)
        {
            _logger.LogInformation("Creating AutoTrader listing for vehicle");
            
            try
            {
                // Convert the vehicle data to AutoTrader format
                var payload = ConvertToAutoTraderFormat(vehicleData, images);
                
                // Send the request to AutoTrader API
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Post, $"{_settings.ApiBaseUrl}/inventory");
                
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                AddAuthHeaders(request);
                
                var json = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AutoTraderApiResponse>(content);
                    
                    return new MarketplaceListingResult
                    {
                        Success = true,
                        ListingId = result.ListingId,
                        ListingUrl = result.ListingUrl,
                        ProcessedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AutoTrader API error: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    
                    return new MarketplaceListingResult
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode} - {error}",
                        ProcessedAt = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating AutoTrader listing");
                return new MarketplaceListingResult
                {
                    Success = false,
                    ErrorMessage = $"Exception: {ex.Message}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }
        
        /// <inheritdoc/>
        public async Task<MarketplaceListingResult> UpdateListingAsync(string listingId, object vehicleData, IEnumerable<object> images)
        {
            _logger.LogInformation("Updating AutoTrader listing {ListingId}", listingId);
            
            try
            {
                // Convert the vehicle data to AutoTrader format
                var payload = ConvertToAutoTraderFormat(vehicleData, images);
                
                // Send the request to AutoTrader API
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Put, $"{_settings.ApiBaseUrl}/inventory/{listingId}");
                
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                AddAuthHeaders(request);
                
                var json = JsonSerializer.Serialize(payload);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AutoTraderApiResponse>(content);
                    
                    return new MarketplaceListingResult
                    {
                        Success = true,
                        ListingId = result.ListingId,
                        ListingUrl = result.ListingUrl,
                        ProcessedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AutoTrader API error: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    
                    return new MarketplaceListingResult
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode} - {error}",
                        ProcessedAt = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating AutoTrader listing {ListingId}", listingId);
                return new MarketplaceListingResult
                {
                    Success = false,
                    ErrorMessage = $"Exception: {ex.Message}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }
        
        /// <inheritdoc/>
        public async Task<MarketplaceListingResult> RemoveListingAsync(string listingId)
        {
            _logger.LogInformation("Removing AutoTrader listing {ListingId}", listingId);
            
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Delete, $"{_settings.ApiBaseUrl}/inventory/{listingId}");
                
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                AddAuthHeaders(request);
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return new MarketplaceListingResult
                    {
                        Success = true,
                        ListingId = listingId,
                        ProcessedAt = DateTime.UtcNow
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AutoTrader API error: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    
                    return new MarketplaceListingResult
                    {
                        Success = false,
                        ErrorMessage = $"API Error: {response.StatusCode} - {error}",
                        ProcessedAt = DateTime.UtcNow
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing AutoTrader listing {ListingId}", listingId);
                return new MarketplaceListingResult
                {
                    Success = false,
                    ErrorMessage = $"Exception: {ex.Message}",
                    ProcessedAt = DateTime.UtcNow
                };
            }
        }
        
        /// <inheritdoc/>
        public async Task<MarketplaceListingStatus> GetListingStatusAsync(string listingId)
        {
            _logger.LogInformation("Getting status for AutoTrader listing {ListingId}", listingId);
            
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.ApiBaseUrl}/inventory/{listingId}/status");
                
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                AddAuthHeaders(request);
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var statusResponse = JsonSerializer.Deserialize<AutoTraderStatusResponse>(content);
                    
                    return new MarketplaceListingStatus
                    {
                        ListingId = listingId,
                        ListingUrl = statusResponse.ListingUrl,
                        State = ConvertAutoTraderStatus(statusResponse.Status),
                        LastUpdated = statusResponse.LastUpdated,
                        StatusMessage = statusResponse.Message,
                        ListingPrice = statusResponse.Price,
                        IsFeatured = statusResponse.IsFeatured,
                        MarketplaceSpecificData = new Dictionary<string, string>
                        {
                            { "package", statusResponse.Package },
                            { "enhancementCount", statusResponse.Enhancements.ToString() }
                        }
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AutoTrader API error: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    
                    return new MarketplaceListingStatus
                    {
                        ListingId = listingId,
                        State = MarketplaceListingState.Unknown,
                        LastUpdated = DateTime.UtcNow,
                        StatusMessage = $"API Error: {response.StatusCode} - {error}"
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting status for AutoTrader listing {ListingId}", listingId);
                return new MarketplaceListingStatus
                {
                    ListingId = listingId,
                    State = MarketplaceListingState.Unknown,
                    LastUpdated = DateTime.UtcNow,
                    StatusMessage = $"Exception: {ex.Message}"
                };
            }
        }
        
        /// <inheritdoc/>
        public async Task<MarketplaceListingStats> GetListingStatsAsync(string listingId)
        {
            _logger.LogInformation("Getting stats for AutoTrader listing {ListingId}", listingId);
            
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.ApiBaseUrl}/inventory/{listingId}/stats");
                
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                AddAuthHeaders(request);
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var statsResponse = JsonSerializer.Deserialize<AutoTraderStatsResponse>(content);
                    
                    return new MarketplaceListingStats
                    {
                        Views = statsResponse.TotalViews,
                        Saves = statsResponse.Saves,
                        Inquiries = statsResponse.Inquiries,
                        Shares = statsResponse.Shares,
                        DailyViews = statsResponse.DailyViews,
                        ComparisonRank = statsResponse.ComparisonRank,
                        EngagementScore = statsResponse.EngagementScore
                    };
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("AutoTrader API error: {StatusCode} - {Error}", 
                        response.StatusCode, error);
                    
                    return new MarketplaceListingStats
                    {
                        Views = 0,
                        Saves = 0,
                        Inquiries = 0,
                        Shares = 0,
                        DailyViews = new Dictionary<string, int>(),
                        ComparisonRank = 0,
                        EngagementScore = 0
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting stats for AutoTrader listing {ListingId}", listingId);
                return new MarketplaceListingStats
                {
                    Views = 0,
                    Saves = 0,
                    Inquiries = 0,
                    Shares = 0,
                    DailyViews = new Dictionary<string, int>(),
                    ComparisonRank = 0,
                    EngagementScore = 0
                };
            }
        }
        
        /// <inheritdoc/>
        public async Task<MarketplaceDefinition> GetMarketplaceDefinitionAsync()
        {
            return new MarketplaceDefinition
            {
                Id = MarketplaceId,
                Name = "AutoTrader",
                LogoUrl = "https://assets.autotrader.com/at-logo.svg",
                IsConnected = !string.IsNullOrEmpty(_settings.ApiKey),
                SupportsRealTimeUpdates = true,
                SupportsBulkOperations = true,
                SupportedFeatures = new[]
                {
                    "premium_listings",
                    "featured_listings",
                    "video_integration",
                    "lead_management",
                    "performance_reporting"
                },
                ConnectionStatus = await TestConnectionAsync()
            };
        }
        
        /// <inheritdoc/>
        public async Task<bool> VerifyCredentialsAsync(MarketplaceCredentials credentials)
        {
            _logger.LogInformation("Verifying AutoTrader credentials");
            
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.ApiBaseUrl}/auth/verify");
                
                // Add temporary credentials for verification
                request.Headers.Add("X-API-Key", credentials.ApiKey);
                request.Headers.Add("X-Dealer-ID", credentials.DealerId);
                
                var response = await client.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying AutoTrader credentials");
                return false;
            }
        }
        
        #region Helper Methods
        
        private object ConvertToAutoTraderFormat(object vehicleData, IEnumerable<object> images)
        {
            // In a real implementation, this would convert the vehicle data to AutoTrader's expected format
            // For this example, we'll just create a simple mock payload
            
            dynamic vehicle = vehicleData;
            
            var payload = new
            {
                dealerId = _settings.DealerId,
                vehicle = new
                {
                    vin = vehicle.vin,
                    stockNumber = vehicle.stockNumber,
                    year = vehicle.year,
                    make = vehicle.make,
                    model = vehicle.model,
                    trim = vehicle.trim,
                    bodyStyle = vehicle.bodyStyle,
                    exteriorColor = vehicle.exteriorColor,
                    interiorColor = vehicle.interiorColor,
                    mileage = vehicle.mileage,
                    price = vehicle.retailPrice,
                    condition = vehicle.condition,
                    description = vehicle.description,
                    engine = vehicle.engine,
                    transmission = vehicle.transmission,
                    fuelType = vehicle.fuelType,
                    driveType = vehicle.driveType
                },
                features = vehicle.features,
                images = ConvertImagesToAutoTraderFormat(images),
                pricing = new
                {
                    listPrice = vehicle.listPrice,
                    msrp = vehicle.msrp,
                    internetPrice = vehicle.retailPrice,
                    showPriceAsCall = false
                },
                seller = new
                {
                    name = _settings.AdditionalSettings.ContainsKey("dealerName") 
                        ? _settings.AdditionalSettings["dealerName"] 
                        : "Your Dealership",
                    phone = _settings.AdditionalSettings.ContainsKey("dealerPhone") 
                        ? _settings.AdditionalSettings["dealerPhone"] 
                        : "555-123-4567",
                    email = _settings.AdditionalSettings.ContainsKey("dealerEmail") 
                        ? _settings.AdditionalSettings["dealerEmail"] 
                        : "sales@yourdealership.com"
                },
                options = new
                {
                    package = _settings.AdditionalSettings.ContainsKey("listingPackage") 
                        ? _settings.AdditionalSettings["listingPackage"] 
                        : "standard",
                    featured = _settings.AdditionalSettings.ContainsKey("featuredListing") 
                        && _settings.AdditionalSettings["featuredListing"] == "true"
                }
            };
            
            return payload;
        }
        
        private object ConvertImagesToAutoTraderFormat(IEnumerable<object> images)
        {
            var imageList = new List<object>();
            int position = 1;
            
            foreach (dynamic image in images)
            {
                imageList.Add(new
                {
                    url = image.url,
                    position = position++,
                    primaryImage = position == 2, // First image is primary
                });
            }
            
            return imageList;
        }
        
        private MarketplaceListingState ConvertAutoTraderStatus(string status)
        {
            return status?.ToLowerInvariant() switch
            {
                "active" => MarketplaceListingState.Active,
                "pending" => MarketplaceListingState.Pending,
                "rejected" => MarketplaceListingState.Rejected,
                "expired" => MarketplaceListingState.Expired,
                "featured" => MarketplaceListingState.Featured,
                "removed" => MarketplaceListingState.Removed,
                _ => MarketplaceListingState.Unknown
            };
        }
        
        private void AddAuthHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("X-API-Key", _settings.ApiKey);
            request.Headers.Add("X-Dealer-ID", _settings.DealerId);
        }
        
        private async Task<MarketplaceConnectionStatus> TestConnectionAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.ApiBaseUrl}/auth/status");
                
                AddAuthHeaders(request);
                
                var response = await client.SendAsync(request);
                
                if (response.IsSuccessStatusCode)
                {
                    return MarketplaceConnectionStatus.Connected;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                         response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    return MarketplaceConnectionStatus.AuthError;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    return MarketplaceConnectionStatus.RateLimited;
                }
                else if ((int)response.StatusCode >= 500)
                {
                    return MarketplaceConnectionStatus.ServiceUnavailable;
                }
                else
                {
                    return MarketplaceConnectionStatus.ConfigurationError;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing AutoTrader connection");
                return MarketplaceConnectionStatus.Disconnected;
            }
        }
        
        #endregion
        
        #region AutoTrader API Response Models
        
        private class AutoTraderApiResponse
        {
            public string ListingId { get; set; }
            public string ListingUrl { get; set; }
            public string Status { get; set; }
        }
        
        private class AutoTraderStatusResponse
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public string ListingUrl { get; set; }
            public DateTime LastUpdated { get; set; }
            public decimal Price { get; set; }
            public bool IsFeatured { get; set; }
            public string Package { get; set; }
            public int Enhancements { get; set; }
        }
        
        private class AutoTraderStatsResponse
        {
            public int TotalViews { get; set; }
            public int Saves { get; set; }
            public int Inquiries { get; set; }
            public int Shares { get; set; }
            public Dictionary<string, int> DailyViews { get; set; }
            public int ComparisonRank { get; set; }
            public double EngagementScore { get; set; }
        }
        
        #endregion
    }
}
