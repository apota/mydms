using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Exceptions;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for integration with the Sales Management module
    /// </summary>
    public interface ISalesIntegrationService
    {
        /// <summary>
        /// Check if a vehicle is currently involved in any active deals
        /// </summary>
        Task<VehicleDealStatus> GetVehicleDealStatusAsync(string vehicleId);
        
        /// <summary>
        /// Place a temporary hold on a vehicle for a specific deal
        /// </summary>
        Task<bool> PlaceDealHoldAsync(string vehicleId, string dealId, string customerId, TimeSpan duration);
        
        /// <summary>
        /// Release a deal hold on a vehicle
        /// </summary>
        Task<bool> ReleaseDealHoldAsync(string vehicleId, string dealId);
        
        /// <summary>
        /// Mark a vehicle as sold in a completed deal
        /// </summary>
        Task<bool> MarkVehicleSoldAsync(string vehicleId, VehicleSaleInfo saleInfo);
        
        /// <summary>
        /// Get sales history for a vehicle
        /// </summary>
        Task<List<SalesHistoryEvent>> GetVehicleSalesHistoryAsync(string vehicleId);
    }

    /// <summary>
    /// Integration service for the Sales Management module
    /// </summary>
    public class SalesIntegrationService : ISalesIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<SalesIntegrationService> _logger;
        private readonly string _salesApiBaseUrl;

        public SalesIntegrationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<SalesIntegrationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _salesApiBaseUrl = _configuration["ModuleIntegration:SalesManagement:ApiUrl"];
            
            if (string.IsNullOrEmpty(_salesApiBaseUrl))
            {
                _logger.LogWarning("Sales Management API URL configuration missing");
            }
        }

        /// <inheritdoc />
        public async Task<VehicleDealStatus> GetVehicleDealStatusAsync(string vehicleId)
        {
            try
            {
                var endpoint = $"{_salesApiBaseUrl}/deals/vehicle/{vehicleId}/status";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Sales API returned status {StatusCode} for vehicle {VehicleId}", 
                        response.StatusCode, vehicleId);
                        
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // No deals found for this vehicle
                        return new VehicleDealStatus { VehicleId = vehicleId, HasActiveDeals = false };
                    }
                    
                    throw new ExternalServiceException($"Error getting vehicle deal status: {response.ReasonPhrase}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<VehicleDealStatus>();
                return result;
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error getting deal status for vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error communicating with Sales module", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> PlaceDealHoldAsync(string vehicleId, string dealId, string customerId, TimeSpan duration)
        {
            try
            {
                var endpoint = $"{_salesApiBaseUrl}/deals/vehicle/hold";
                var request = new
                {
                    VehicleId = vehicleId,
                    DealId = dealId,
                    CustomerId = customerId,
                    DurationMinutes = (int)duration.TotalMinutes
                };
                
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Sales API returned status {StatusCode} when placing hold on vehicle {VehicleId}", 
                        response.StatusCode, vehicleId);
                    throw new ExternalServiceException($"Error placing deal hold: {response.ReasonPhrase}");
                }
                
                return true;
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error placing deal hold for vehicle {VehicleId}, deal {DealId}", vehicleId, dealId);
                throw new ExternalServiceException("Error communicating with Sales module", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> ReleaseDealHoldAsync(string vehicleId, string dealId)
        {
            try
            {
                var endpoint = $"{_salesApiBaseUrl}/deals/vehicle/hold/release";
                var request = new
                {
                    VehicleId = vehicleId,
                    DealId = dealId
                };
                
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Sales API returned status {StatusCode} when releasing hold on vehicle {VehicleId}", 
                        response.StatusCode, vehicleId);
                    throw new ExternalServiceException($"Error releasing deal hold: {response.ReasonPhrase}");
                }
                
                return true;
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error releasing deal hold for vehicle {VehicleId}, deal {DealId}", vehicleId, dealId);
                throw new ExternalServiceException("Error communicating with Sales module", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> MarkVehicleSoldAsync(string vehicleId, VehicleSaleInfo saleInfo)
        {
            try
            {
                var endpoint = $"{_salesApiBaseUrl}/deals/vehicle/sold";
                var request = new
                {
                    VehicleId = vehicleId,
                    DealId = saleInfo.DealId,
                    CustomerId = saleInfo.CustomerId,
                    SaleDate = saleInfo.SaleDate,
                    SalePrice = saleInfo.SalePrice,
                    SalesPersonId = saleInfo.SalesPersonId,
                    FinanceManagerId = saleInfo.FinanceManagerId,
                    PaymentType = saleInfo.PaymentType,
                    FinanceTermMonths = saleInfo.FinanceTermMonths,
                    TradeInVehicleId = saleInfo.TradeInVehicleId
                };
                
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Sales API returned status {StatusCode} when marking vehicle {VehicleId} as sold", 
                        response.StatusCode, vehicleId);
                    throw new ExternalServiceException($"Error marking vehicle as sold: {response.ReasonPhrase}");
                }
                
                return true;
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error marking vehicle {VehicleId} as sold", vehicleId);
                throw new ExternalServiceException("Error communicating with Sales module", ex);
            }
        }

        /// <inheritdoc />
        public async Task<List<SalesHistoryEvent>> GetVehicleSalesHistoryAsync(string vehicleId)
        {
            try
            {
                var endpoint = $"{_salesApiBaseUrl}/deals/vehicle/{vehicleId}/history";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Sales API returned status {StatusCode} for vehicle {VehicleId} history", 
                        response.StatusCode, vehicleId);
                        
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // No sales history for this vehicle
                        return new List<SalesHistoryEvent>();
                    }
                    
                    throw new ExternalServiceException($"Error getting vehicle sales history: {response.ReasonPhrase}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<List<SalesHistoryEvent>>();
                return result ?? new List<SalesHistoryEvent>();
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error getting sales history for vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error communicating with Sales module", ex);
            }
        }
    }

    public class VehicleDealStatus
    {
        public string VehicleId { get; set; }
        public bool HasActiveDeals { get; set; }
        public List<DealInfo> ActiveDeals { get; set; } = new List<DealInfo>();
        public bool HasCurrentHold { get; set; }
        public DealHoldInfo CurrentHold { get; set; }
    }

    public class DealInfo
    {
        public string DealId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DealStatus { get; set; }
        public decimal ProposedPrice { get; set; }
    }

    public class DealHoldInfo
    {
        public string DealId { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public DateTime HoldStartTime { get; set; }
        public DateTime HoldEndTime { get; set; }
        public string PlacedById { get; set; }
        public string PlacedByName { get; set; }
    }

    public class VehicleSaleInfo
    {
        public string DealId { get; set; }
        public string CustomerId { get; set; }
        public DateTime SaleDate { get; set; }
        public decimal SalePrice { get; set; }
        public string SalesPersonId { get; set; }
        public string FinanceManagerId { get; set; }
        public string PaymentType { get; set; }  // Cash, Finance, Lease
        public int? FinanceTermMonths { get; set; }
        public decimal? DownPayment { get; set; }
        public string TradeInVehicleId { get; set; }
    }

    public class SalesHistoryEvent
    {
        public string EventType { get; set; }  // Inquiry, TestDrive, Offer, Hold, Sale
        public DateTime EventDate { get; set; }
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string DealId { get; set; }
        public decimal? Amount { get; set; }
        public string Notes { get; set; }
    }
}
