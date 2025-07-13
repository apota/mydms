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
    /// Interface for integration with the CRM module
    /// </summary>
    public interface ICrmIntegrationService
    {
        /// <summary>
        /// Get customer interest in a specific vehicle
        /// </summary>
        Task<List<CustomerInterest>> GetVehicleInterestAsync(string vehicleId);
        
        /// <summary>
        /// Record a new customer interest in a vehicle
        /// </summary>
        Task<bool> RecordCustomerInterestAsync(string vehicleId, CustomerInterestInfo interestInfo);
        
        /// <summary>
        /// Get list of customers who might be interested in a specific vehicle based on their preferences
        /// </summary>
        Task<List<PotentialCustomer>> GetPotentialCustomersAsync(string vehicleId);
        
        /// <summary>
        /// Notify customers about a price change or new vehicle matching their criteria
        /// </summary>
        Task<bool> NotifyInterestedCustomersAsync(string vehicleId, string notificationType, string message);
    }

    /// <summary>
    /// Integration service for the CRM module
    /// </summary>
    public class CrmIntegrationService : ICrmIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CrmIntegrationService> _logger;
        private readonly string _crmApiBaseUrl;

        public CrmIntegrationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<CrmIntegrationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _crmApiBaseUrl = _configuration["ModuleIntegration:CRM:ApiUrl"];
            
            if (string.IsNullOrEmpty(_crmApiBaseUrl))
            {
                _logger.LogWarning("CRM API URL configuration missing");
            }
        }

        /// <inheritdoc />
        public async Task<List<CustomerInterest>> GetVehicleInterestAsync(string vehicleId)
        {
            try
            {
                var endpoint = $"{_crmApiBaseUrl}/vehicles/{vehicleId}/interest";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("CRM API returned status {StatusCode} for vehicle {VehicleId} interest", 
                        response.StatusCode, vehicleId);
                        
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // No interest recorded for this vehicle
                        return new List<CustomerInterest>();
                    }
                    
                    throw new ExternalServiceException($"Error getting vehicle interest: {response.ReasonPhrase}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<List<CustomerInterest>>();
                return result ?? new List<CustomerInterest>();
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error getting customer interest for vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error communicating with CRM module", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> RecordCustomerInterestAsync(string vehicleId, CustomerInterestInfo interestInfo)
        {
            try
            {
                var endpoint = $"{_crmApiBaseUrl}/vehicles/interest";
                var request = new
                {
                    VehicleId = vehicleId,
                    CustomerId = interestInfo.CustomerId,
                    InterestType = interestInfo.InterestType,
                    InterestDate = interestInfo.InterestDate,
                    Source = interestInfo.Source,
                    Notes = interestInfo.Notes,
                    FollowUpDate = interestInfo.FollowUpDate,
                    AssignedToUserId = interestInfo.AssignedToUserId
                };
                
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("CRM API returned status {StatusCode} when recording interest for vehicle {VehicleId}", 
                        response.StatusCode, vehicleId);
                    throw new ExternalServiceException($"Error recording customer interest: {response.ReasonPhrase}");
                }
                
                return true;
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error recording customer interest for vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error communicating with CRM module", ex);
            }
        }

        /// <inheritdoc />
        public async Task<List<PotentialCustomer>> GetPotentialCustomersAsync(string vehicleId)
        {
            try
            {
                var endpoint = $"{_crmApiBaseUrl}/customers/potential/{vehicleId}";
                var response = await _httpClient.GetAsync(endpoint);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("CRM API returned status {StatusCode} for potential customers of vehicle {VehicleId}", 
                        response.StatusCode, vehicleId);
                        
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        // No potential customers found
                        return new List<PotentialCustomer>();
                    }
                    
                    throw new ExternalServiceException($"Error getting potential customers: {response.ReasonPhrase}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<List<PotentialCustomer>>();
                return result ?? new List<PotentialCustomer>();
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error getting potential customers for vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error communicating with CRM module", ex);
            }
        }

        /// <inheritdoc />
        public async Task<bool> NotifyInterestedCustomersAsync(string vehicleId, string notificationType, string message)
        {
            try
            {
                var endpoint = $"{_crmApiBaseUrl}/customers/notify";
                var request = new
                {
                    VehicleId = vehicleId,
                    NotificationType = notificationType,
                    Message = message
                };
                
                var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(endpoint, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("CRM API returned status {StatusCode} when notifying customers about vehicle {VehicleId}", 
                        response.StatusCode, vehicleId);
                    throw new ExternalServiceException($"Error notifying customers: {response.ReasonPhrase}");
                }
                
                var result = await response.Content.ReadFromJsonAsync<NotificationResult>();
                return result?.Success ?? false;
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error notifying customers about vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error communicating with CRM module", ex);
            }
        }
    }

    public class CustomerInterest
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public string InterestType { get; set; }  // Website, Phone, Walk-in, Test Drive, etc.
        public DateTime InterestDate { get; set; }
        public string Source { get; set; }  // Website, 3rd-party listing, Referral, etc.
        public string Notes { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string AssignedToUserId { get; set; }
        public string AssignedToName { get; set; }
        public List<CustomerActivity> RecentActivity { get; set; } = new List<CustomerActivity>();
    }

    public class CustomerInterestInfo
    {
        public string CustomerId { get; set; }
        public string InterestType { get; set; }
        public DateTime InterestDate { get; set; }
        public string Source { get; set; }
        public string Notes { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string AssignedToUserId { get; set; }
    }

    public class CustomerActivity
    {
        public string ActivityType { get; set; }
        public DateTime ActivityDate { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }

    public class PotentialCustomer
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        public decimal MatchScore { get; set; }  // 0-100 score of how well this vehicle matches their preferences
        public VehiclePreference Preferences { get; set; }
        public DateTime LastContactDate { get; set; }
        public string AssignedToUserId { get; set; }
        public string AssignedToName { get; set; }
    }

    public class VehiclePreference
    {
        public List<string> PreferredMakes { get; set; } = new List<string>();
        public List<string> PreferredModels { get; set; } = new List<string>();
        public int? YearFrom { get; set; }
        public int? YearTo { get; set; }
        public decimal? BudgetFrom { get; set; }
        public decimal? BudgetTo { get; set; }
        public List<string> PreferredBodyStyles { get; set; } = new List<string>();
        public List<string> PreferredColors { get; set; } = new List<string>();
        public int? MaximumMileage { get; set; }
        public string PreferredCondition { get; set; }  // New, Used, Certified
        public List<string> MustHaveFeatures { get; set; } = new List<string>();
        public string Notes { get; set; }
    }

    public class NotificationResult
    {
        public bool Success { get; set; }
        public int NotificationsSent { get; set; }
        public List<string> FailedNotifications { get; set; } = new List<string>();
    }
}
