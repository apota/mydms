using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Services;

namespace DMS.PartsManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the integration service for other DMS modules
    /// </summary>
    public class IntegrationService : IIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<IntegrationService> _logger;
        private readonly IntegrationSettings _settings;

        public IntegrationService(
            HttpClient httpClient,
            IOptions<IntegrationSettings> settings,
            ILogger<IntegrationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _settings = settings.Value;
        }

        #region CRM Integration

        /// <inheritdoc/>
        public async Task<CustomerDto> GetCustomerInfoFromCrmAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.CrmApiBaseUrl}/customers/{customerId}", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<CustomerDto>(cancellationToken: cancellationToken);
                }
                
                _logger.LogWarning("Failed to get customer info from CRM API. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting customer info from CRM API for customer ID: {CustomerId}", customerId);
                return null;
            }
        }

        #endregion

        #region Financial Integration

        /// <inheritdoc/>
        public async Task<PartPricingDto> GetPartPricingFromFinancialAsync(Guid partId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.FinancialApiBaseUrl}/pricing/parts/{partId}", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PartPricingDto>(cancellationToken: cancellationToken);
                }
                
                _logger.LogWarning("Failed to get pricing from Financial API. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pricing from Financial API for part ID: {PartId}", partId);
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<InvoiceDto> CreateInvoiceInFinancialAsync(CreateInvoiceDto createInvoiceDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(createInvoiceDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_settings.FinancialApiBaseUrl}/invoices", content, cancellationToken);
                
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<InvoiceDto>(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating invoice in Financial API");
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<InvoiceDto> GetInvoiceFromFinancialAsync(Guid invoiceId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.FinancialApiBaseUrl}/invoices/{invoiceId}", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<InvoiceDto>(cancellationToken: cancellationToken);
                }
                
                _logger.LogWarning("Failed to get invoice from Financial API. Status: {StatusCode}", response.StatusCode);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting invoice from Financial API for invoice ID: {InvoiceId}", invoiceId);
                return null;
            }
        }

        #endregion

        #region Service Integration

        /// <inheritdoc/>
        public async Task<IEnumerable<ServiceOrderPartDto>> GetPartsForServiceOrderAsync(Guid serviceOrderId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.ServiceApiBaseUrl}/orders/{serviceOrderId}/parts", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ServiceOrderPartDto>>(cancellationToken: cancellationToken);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Service order not found in Service API. ID: {ServiceOrderId}", serviceOrderId);
                    return null;
                }
                
                _logger.LogWarning("Failed to get parts for service order from Service API. Status: {StatusCode}", response.StatusCode);
                return new List<ServiceOrderPartDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting parts for service order from Service API for order ID: {ServiceOrderId}", serviceOrderId);
                return new List<ServiceOrderPartDto>();
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ServiceOrderPartDto>> AssignPartsToServiceOrderAsync(Guid serviceOrderId, AssignServiceOrderPartsDto assignPartsDto, CancellationToken cancellationToken = default)
        {
            try
            {
                var json = JsonSerializer.Serialize(assignPartsDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                var response = await _httpClient.PostAsync($"{_settings.ServiceApiBaseUrl}/orders/{serviceOrderId}/parts", content, cancellationToken);
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Service order not found in Service API. ID: {ServiceOrderId}", serviceOrderId);
                    throw new KeyNotFoundException($"Service order with ID {serviceOrderId} not found");
                }
                
                response.EnsureSuccessStatusCode();
                
                return await response.Content.ReadFromJsonAsync<List<ServiceOrderPartDto>>(cancellationToken: cancellationToken);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Error assigning parts to service order in Service API for order ID: {ServiceOrderId}", serviceOrderId);
                throw;
            }
        }

        #endregion

        #region Sales Integration

        /// <inheritdoc/>
        public async Task<IEnumerable<VehicleAccessoryDto>> GetAccessoriesForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_settings.SalesApiBaseUrl}/vehicles/{vehicleId}/accessories", cancellationToken);
                
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<VehicleAccessoryDto>>(cancellationToken: cancellationToken);
                }
                
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Vehicle not found in Sales API. ID: {VehicleId}", vehicleId);
                    return null;
                }
                
                _logger.LogWarning("Failed to get accessories for vehicle from Sales API. Status: {StatusCode}", response.StatusCode);
                return new List<VehicleAccessoryDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting accessories for vehicle from Sales API for vehicle ID: {VehicleId}", vehicleId);
                return new List<VehicleAccessoryDto>();
            }
        }

        #endregion
    }

    /// <summary>
    /// Settings for integration with other DMS modules
    /// </summary>
    public class IntegrationSettings
    {
        /// <summary>
        /// CRM API base URL
        /// </summary>
        public string CrmApiBaseUrl { get; set; }

        /// <summary>
        /// Financial API base URL
        /// </summary>
        public string FinancialApiBaseUrl { get; set; }

        /// <summary>
        /// Service API base URL
        /// </summary>
        public string ServiceApiBaseUrl { get; set; }

        /// <summary>
        /// Sales API base URL
        /// </summary>
        public string SalesApiBaseUrl { get; set; }
    }
}
