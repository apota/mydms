using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http.Json;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DMS.ServiceManagement.Infrastructure.Integrations
{
    /// <summary>
    /// Service for integrating with other DMS modules like CRM, Inventory, Financial, etc.
    /// </summary>
    public class ModuleIntegrationService : IModuleIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ModuleIntegrationService> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public ModuleIntegrationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ModuleIntegrationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #region CRM Module Integration
        
        public async Task<T> GetCustomerInfoAsync<T>(Guid customerId)
        {
            try
            {
                var crmApiUrl = _configuration["IntegrationUrls:CrmApi"];
                var url = $"{crmApiUrl}/api/customers/{customerId}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting customer info for ID: {customerId}");
                throw new IntegrationException($"Failed to retrieve customer information: {ex.Message}", ex);
            }
        }

        public async Task<T> GetVehicleInfoAsync<T>(Guid vehicleId)
        {
            try
            {
                var inventoryApiUrl = _configuration["IntegrationUrls:InventoryApi"];
                var url = $"{inventoryApiUrl}/api/vehicles/{vehicleId}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting vehicle info for ID: {vehicleId}");
                throw new IntegrationException($"Failed to retrieve vehicle information: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Get customer service history from CRM module
        /// </summary>
        public async Task<T> GetCustomerServiceHistoryAsync<T>(Guid customerId)
        {
            try
            {
                var crmApiUrl = _configuration["IntegrationUrls:CrmApi"];
                var url = $"{crmApiUrl}/api/customers/{customerId}/service-history";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting service history for customer ID: {customerId}");
                throw new IntegrationException($"Failed to retrieve customer service history: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Updates customer information in CRM when contact details change during service
        /// </summary>
        public async Task<bool> UpdateCustomerInfoAsync(Guid customerId, object customerData)
        {
            try
            {
                var crmApiUrl = _configuration["IntegrationUrls:CrmApi"];
                var url = $"{crmApiUrl}/api/customers/{customerId}";
                
                var json = JsonSerializer.Serialize(customerData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PutAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating customer info for ID: {customerId}");
                throw new IntegrationException($"Failed to update customer information: {ex.Message}", ex);
            }
        }
        
        public async Task<bool> NotifyCustomerAsync(Guid customerId, string messageType, object payload)
        {
            try
            {
                var crmApiUrl = _configuration["IntegrationUrls:CrmApi"];
                var url = $"{crmApiUrl}/api/notifications/customer/{customerId}";
                
                var notificationData = new
                {
                    MessageType = messageType,
                    Payload = payload,
                    Timestamp = DateTime.UtcNow
                };
                
                var json = JsonSerializer.Serialize(notificationData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error notifying customer: {customerId}, message type: {messageType}");
                throw new IntegrationException($"Failed to send customer notification: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Parts Management Integration
        
        public async Task<T> GetPartsInfoAsync<T>(string partNumber)
        {
            try
            {
                var partsApiUrl = _configuration["IntegrationUrls:PartsApi"];
                var url = $"{partsApiUrl}/api/parts/{partNumber}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting part info for part number: {partNumber}");
                throw new IntegrationException($"Failed to retrieve part information: {ex.Message}", ex);
            }
        }

        public async Task<T> CheckPartsInventoryAsync<T>(string[] partNumbers)
        {
            try
            {
                var partsApiUrl = _configuration["IntegrationUrls:PartsApi"];
                var url = $"{partsApiUrl}/api/parts/inventory-check";
                
                var json = JsonSerializer.Serialize(partNumbers);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking parts inventory");
                throw new IntegrationException($"Failed to check parts inventory: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Order parts from Parts Management module
        /// </summary>
        public async Task<T> OrderPartsAsync<T>(object partsOrder)
        {
            try
            {
                var partsApiUrl = _configuration["IntegrationUrls:PartsApi"];
                var url = $"{partsApiUrl}/api/parts/orders";
                
                var json = JsonSerializer.Serialize(partsOrder);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ordering parts");
                throw new IntegrationException($"Failed to order parts: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Check status of a parts order
        /// </summary>
        public async Task<T> CheckPartsOrderStatusAsync<T>(Guid orderId)
        {
            try
            {
                var partsApiUrl = _configuration["IntegrationUrls:PartsApi"];
                var url = $"{partsApiUrl}/api/parts/orders/{orderId}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking parts order status for ID: {orderId}");
                throw new IntegrationException($"Failed to check parts order status: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Financial Integration
        
        public async Task<T> CreateInvoiceAsync<T>(object invoiceData)
        {
            try
            {
                var financialApiUrl = _configuration["IntegrationUrls:FinancialApi"];
                var url = $"{financialApiUrl}/api/invoices";
                
                var json = JsonSerializer.Serialize(invoiceData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice");
                throw new IntegrationException($"Failed to create invoice: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Process payment for a service invoice
        /// </summary>
        public async Task<T> ProcessPaymentAsync<T>(object paymentData)
        {
            try
            {
                var financialApiUrl = _configuration["IntegrationUrls:FinancialApi"];
                var url = $"{financialApiUrl}/api/payments";
                
                var json = JsonSerializer.Serialize(paymentData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                throw new IntegrationException($"Failed to process payment: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Get invoice status
        /// </summary>
        public async Task<T> GetInvoiceStatusAsync<T>(Guid invoiceId)
        {
            try
            {
                var financialApiUrl = _configuration["IntegrationUrls:FinancialApi"];
                var url = $"{financialApiUrl}/api/invoices/{invoiceId}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting invoice status for ID: {invoiceId}");
                throw new IntegrationException($"Failed to get invoice status: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Technician Management
        
        public async Task<T> GetAvailableTechniciansAsync<T>(DateTime startTime, DateTime endTime, string specialization = null)
        {
            try
            {
                var hrApiUrl = _configuration["IntegrationUrls:HrApi"];
                var queryString = $"startTime={startTime:o}&endTime={endTime:o}";
                
                if (!string.IsNullOrEmpty(specialization))
                {
                    queryString += $"&specialization={Uri.EscapeDataString(specialization)}";
                }
                
                var url = $"{hrApiUrl}/api/technicians/available?{queryString}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available technicians");
                throw new IntegrationException($"Failed to retrieve available technicians: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Assign a technician to a service job
        /// </summary>
        public async Task<bool> AssignTechnicianAsync(Guid technicianId, Guid serviceJobId, DateTime startTime, DateTime endTime)
        {
            try
            {
                var hrApiUrl = _configuration["IntegrationUrls:HrApi"];
                var url = $"{hrApiUrl}/api/technicians/{technicianId}/assignments";
                
                var assignmentData = new
                {
                    ServiceJobId = serviceJobId,
                    StartTime = startTime,
                    EndTime = endTime
                };
                
                var json = JsonSerializer.Serialize(assignmentData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning technician {technicianId} to job {serviceJobId}");
                throw new IntegrationException($"Failed to assign technician: {ex.Message}", ex);
            }
        }
        
        #endregion
        
        #region Service Bay Management
        
        /// <summary>
        /// Reserve a service bay for a job
        /// </summary>
        public async Task<T> ReserveServiceBayAsync<T>(Guid bayId, DateTime startTime, DateTime endTime, Guid serviceJobId)
        {
            try
            {
                var inventoryApiUrl = _configuration["IntegrationUrls:InventoryApi"];
                var url = $"{inventoryApiUrl}/api/service-bays/{bayId}/reservations";
                
                var reservationData = new
                {
                    ServiceJobId = serviceJobId,
                    StartTime = startTime,
                    EndTime = endTime
                };
                
                var json = JsonSerializer.Serialize(reservationData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync(url, content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reserving service bay {bayId}");
                throw new IntegrationException($"Failed to reserve service bay: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Get availability of all service bays
        /// </summary>
        public async Task<T> GetServiceBayAvailabilityAsync<T>(DateTime date)
        {
            try
            {
                var inventoryApiUrl = _configuration["IntegrationUrls:InventoryApi"];
                var url = $"{inventoryApiUrl}/api/service-bays/availability?date={date:yyyy-MM-dd}";
                
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting service bay availability for date: {date:yyyy-MM-dd}");
                throw new IntegrationException($"Failed to get service bay availability: {ex.Message}", ex);
            }
        }
        
        #endregion
    }
    
    public class IntegrationException : Exception
    {
        public IntegrationException(string message) : base(message) { }
        public IntegrationException(string message, Exception innerException) : base(message, innerException) { }
    }
}
