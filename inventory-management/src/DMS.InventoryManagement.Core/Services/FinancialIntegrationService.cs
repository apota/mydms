using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text;

namespace DMS.InventoryManagement.Core.Services
{
    public interface IFinancialIntegrationService
    {
        /// <summary>
        /// Syncs inventory cost data with the financial management system
        /// </summary>
        /// <param name="vehicleId">ID of the vehicle to sync</param>
        /// <returns>True if sync was successful</returns>
        Task<bool> SyncVehicleCostToFinancialAsync(Guid vehicleId);
        
        /// <summary>
        /// Retrieves financial valuation data for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">ID of the vehicle to get valuation for</param>
        /// <returns>Financial valuation data</returns>
        Task<FinancialValuationData> GetVehicleFinancialValuationAsync(Guid vehicleId);
        
        /// <summary>
        /// Submits a request for a financial appraisal of a vehicle
        /// </summary>
        /// <param name="vehicleId">ID of the vehicle to appraise</param>
        /// <param name="appraisalRequestData">Data for the appraisal request</param>
        /// <returns>Appraisal request result</returns>
        Task<AppraisalRequestResult> RequestVehicleAppraisalAsync(Guid vehicleId, AppraisalRequestData appraisalRequestData);
    }
    
    public class FinancialIntegrationService : IFinancialIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<FinancialIntegrationService> _logger;
        
        public FinancialIntegrationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<FinancialIntegrationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // Configure the HTTP client
            string baseUrl = _configuration["Integration:FinancialManagement:BaseUrl"];
            if (string.IsNullOrEmpty(baseUrl))
            {
                throw new ConfigurationException("Financial Management integration base URL is not configured");
            }
            
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _configuration["Integration:FinancialManagement:ApiKey"]);
            _httpClient.DefaultRequestHeaders.Add("x-module-source", "inventory-management");
        }
        
        public async Task<bool> SyncVehicleCostToFinancialAsync(Guid vehicleId)
        {
            try
            {
                _logger.LogInformation("Syncing vehicle cost to financial system for vehicle {VehicleId}", vehicleId);
                
                // Create the sync request
                var syncRequest = new 
                {
                    VehicleId = vehicleId,
                    InventoryModuleId = vehicleId,
                    Timestamp = DateTime.UtcNow
                };
                
                // Send the request to the financial system
                var response = await _httpClient.PostAsJsonAsync("/api/financial/inventory/sync", syncRequest);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to sync vehicle cost to financial system. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    return false;
                }
                
                _logger.LogInformation("Successfully synced vehicle cost to financial system for vehicle {VehicleId}", vehicleId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing vehicle cost to financial system for vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error communicating with Financial Management module", ex);
            }
        }
        
        public async Task<FinancialValuationData> GetVehicleFinancialValuationAsync(Guid vehicleId)
        {
            try
            {
                _logger.LogInformation("Getting financial valuation data for vehicle {VehicleId}", vehicleId);
                
                // Send the request to the financial system
                var response = await _httpClient.GetAsync($"/api/financial/inventory/valuation/{vehicleId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to get financial valuation data. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    throw new ExternalServiceException($"Failed to get financial valuation data: {response.StatusCode}", 
                        errorContent, response.StatusCode);
                }
                
                var result = await response.Content.ReadFromJsonAsync<FinancialValuationData>();
                
                if (result == null)
                {
                    throw new ExternalServiceException("Invalid response format from Financial Management module");
                }
                
                return result;
            }
            catch (ExternalServiceException)
            {
                throw; // Re-throw already captured external service exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting financial valuation data for vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error communicating with Financial Management module", ex);
            }
        }
        
        public async Task<AppraisalRequestResult> RequestVehicleAppraisalAsync(Guid vehicleId, AppraisalRequestData appraisalRequestData)
        {
            try
            {
                _logger.LogInformation("Requesting financial appraisal for vehicle {VehicleId}", vehicleId);
                
                // Validate input
                if (appraisalRequestData == null)
                {
                    throw new ArgumentNullException(nameof(appraisalRequestData));
                }
                
                // Add vehicle ID to the request data
                var requestData = new
                {
                    VehicleId = vehicleId,
                    appraisalRequestData.RequestedBy,
                    appraisalRequestData.AppraisalType,
                    appraisalRequestData.AppraisalDate,
                    appraisalRequestData.EstimatedValue,
                    appraisalRequestData.Comments,
                    appraisalRequestData.AdditionalData
                };
                
                // Send the request to the financial system
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PostAsync($"/api/financial/appraisals", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    string errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to request vehicle appraisal. Status: {Status}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    throw new ExternalServiceException($"Failed to request vehicle appraisal: {response.StatusCode}", 
                        errorContent, response.StatusCode);
                }
                
                var result = await response.Content.ReadFromJsonAsync<AppraisalRequestResult>();
                
                if (result == null)
                {
                    throw new ExternalServiceException("Invalid response format from Financial Management module");
                }
                
                _logger.LogInformation("Successfully requested appraisal for vehicle {VehicleId}, appraisal ID: {AppraisalId}", 
                    vehicleId, result.AppraisalId);
                
                return result;
            }
            catch (ExternalServiceException)
            {
                throw; // Re-throw already captured external service exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error requesting financial appraisal for vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error communicating with Financial Management module", ex);
            }
        }
    }
    
    public class FinancialValuationData
    {
        public Guid VehicleId { get; set; }
        public decimal BookValue { get; set; }
        public decimal MarketValue { get; set; }
        public decimal WholesaleValue { get; set; }
        public decimal RetailValue { get; set; }
        public decimal DepreciationRate { get; set; }
        public decimal ResidualValue { get; set; }
        public DateTime ValuationDate { get; set; }
        public string ValuationMethod { get; set; } = string.Empty;
        public int DaysToFloorplanLimit { get; set; }
        public decimal TotalFinancingCost { get; set; }
        public decimal DailyHoldingCost { get; set; }
        public Dictionary<string, decimal> AdditionalValuations { get; set; } = new();
        public Dictionary<string, object> FinancialNotes { get; set; } = new();
    }
    
    public class AppraisalRequestData
    {
        public string RequestedBy { get; set; } = string.Empty;
        public string AppraisalType { get; set; } = string.Empty; // Trade-In, Auction, Private Purchase, etc.
        public DateTime AppraisalDate { get; set; } = DateTime.UtcNow;
        public decimal EstimatedValue { get; set; }
        public string Comments { get; set; } = string.Empty;
        public Dictionary<string, object>? AdditionalData { get; set; }
    }
    
    public class AppraisalRequestResult
    {
        public Guid AppraisalId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestTimestamp { get; set; }
        public DateTime? CompletionTimestamp { get; set; }
        public string? AssignedTo { get; set; }
        public int EstimatedCompletionMinutes { get; set; }
    }
}
