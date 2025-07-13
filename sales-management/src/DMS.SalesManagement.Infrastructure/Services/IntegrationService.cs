using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DMS.SalesManagement.Core.DTOs;
using DMS.SalesManagement.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DMS.SalesManagement.Infrastructure.Services
{
    public class IntegrationService : IIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IntegrationService> _logger;
        private readonly string _partsManagementApiUrl;
        private readonly string _financialManagementApiUrl;
        private readonly string _serviceManagementApiUrl;
        private readonly string _crmApiUrl;
        private readonly JsonSerializerOptions _jsonOptions;

        public IntegrationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<IntegrationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            // Load API URLs from configuration
            _partsManagementApiUrl = configuration["IntegrationUrls:PartsManagementApi"];
            _financialManagementApiUrl = configuration["IntegrationUrls:FinancialManagementApi"];
            _serviceManagementApiUrl = configuration["IntegrationUrls:ServiceManagementApi"];
            _crmApiUrl = configuration["IntegrationUrls:CrmApi"];
            
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<IEnumerable<AccessoryDto>> GetVehicleAccessoriesAsync(string vehicleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_partsManagementApiUrl}/integration/sales/vehicles/{vehicleId}/accessories");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<AccessoryDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching accessories for vehicle {VehicleId}", vehicleId);
                return new List<AccessoryDto>();
            }
        }

        public async Task<IEnumerable<PartDto>> GetCompatiblePartsAsync(string vehicleId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_partsManagementApiUrl}/integration/sales/vehicles/{vehicleId}/compatible-parts");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<PartDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching compatible parts for vehicle {VehicleId}", vehicleId);
                return new List<PartDto>();
            }
        }

        public async Task<PartsReservationDto> ReservePartsForDealAsync(string dealId, ReservePartsRequestDto request)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");
                    
                var response = await _httpClient.PostAsync(
                    $"{_partsManagementApiUrl}/integration/sales/deals/{dealId}/reserve-parts",
                    content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<PartsReservationDto>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving parts for deal {DealId}", dealId);
                throw new ApplicationException($"Failed to reserve parts for deal: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<ServiceHistoryDto>> GetCustomerServiceHistoryAsync(string customerId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_serviceManagementApiUrl}/integration/sales/customers/{customerId}/service-history");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<ServiceHistoryDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching service history for customer {CustomerId}", customerId);
                return new List<ServiceHistoryDto>();
            }
        }

        public async Task<IEnumerable<FinancialQuoteDto>> GetFinancialQuotesForDealAsync(string dealId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_financialManagementApiUrl}/integration/sales/deals/{dealId}/quotes");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<FinancialQuoteDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching financial quotes for deal {DealId}", dealId);
                return new List<FinancialQuoteDto>();
            }
        }

        public async Task<FinancingApplicationResultDto> SubmitDealForFinancingAsync(string dealId, FinancingRequestDto request)
        {
            try
            {
                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");
                    
                var response = await _httpClient.PostAsync(
                    $"{_financialManagementApiUrl}/integration/sales/deals/{dealId}/apply-financing",
                    content);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<FinancingApplicationResultDto>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting financing for deal {DealId}", dealId);
                throw new ApplicationException($"Failed to submit financing: {ex.Message}", ex);
            }
        }

        public async Task<IEnumerable<InsuranceQuoteDto>> GetInsuranceQuotesAsync(string dealId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_financialManagementApiUrl}/integration/sales/deals/{dealId}/insurance-quotes");
                response.EnsureSuccessStatusCode();
                
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<IEnumerable<InsuranceQuoteDto>>(content, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching insurance quotes for deal {DealId}", dealId);
                return new List<InsuranceQuoteDto>();
            }
        }

        public async Task<DmvRegistrationResultDto> RegisterVehicleWithDmvAsync(string dealId)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"{_financialManagementApiUrl}/integration/sales/deals/{dealId}/dmv-registration",
                    null);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<DmvRegistrationResultDto>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering vehicle with DMV for deal {DealId}", dealId);
                throw new ApplicationException($"Failed to register vehicle with DMV: {ex.Message}", ex);
            }
        }

        public async Task<InvoiceDto> CreateDealInvoiceAsync(string dealId)
        {
            try
            {
                var response = await _httpClient.PostAsync(
                    $"{_financialManagementApiUrl}/integration/sales/deals/{dealId}/invoice",
                    null);
                response.EnsureSuccessStatusCode();
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<InvoiceDto>(responseContent, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice for deal {DealId}", dealId);
                throw new ApplicationException($"Failed to create invoice: {ex.Message}", ex);
            }
        }
    }
}
