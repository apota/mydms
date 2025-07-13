using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Exceptions;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the Service Integration Service that communicates with the Service Management module
    /// </summary>
    public class ServiceIntegrationService : IServiceIntegrationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ServiceIntegrationService> _logger;
        private readonly string _serviceManagementApiUrl;
        
        public ServiceIntegrationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ServiceIntegrationService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            
            _serviceManagementApiUrl = configuration["IntegrationServices:ServiceManagementApi:Url"]
                ?? throw new ConfigurationException("Service Management API URL is not configured");
            
            _httpClient.BaseAddress = new Uri(_serviceManagementApiUrl);
        }
        
        /// <inheritdoc />
        public async Task<string> CreateReconditioningWorkOrderAsync(
            Guid vehicleId, 
            List<ReconditioningItem> reconditioningItems, 
            ReconditioningUrgency urgency, 
            string notes)
        {
            try
            {
                _logger.LogInformation("Creating reconditioning work order for vehicle {VehicleId}", vehicleId);
                
                var requestData = new
                {
                    VehicleId = vehicleId,
                    ReconditioningItems = reconditioningItems,
                    Urgency = urgency.ToString(),
                    Notes = notes,
                    RequestedDate = DateTime.UtcNow,
                    Source = "InventoryManagement"
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PostAsync(
                    $"{_serviceManagementApiUrl}/api/repair-orders/reconditioning",
                    content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to create reconditioning work order for vehicle {VehicleId}. Status code: {StatusCode}", 
                        vehicleId, 
                        response.StatusCode);
                    
                    throw new Exception($"Failed to create reconditioning work order. Status code: {response.StatusCode}");
                }
                
                var result = await response.Content.ReadAsStringAsync();
                var workOrder = JsonSerializer.Deserialize<ServiceWorkOrderResponse>(result);
                
                _logger.LogInformation(
                    "Created reconditioning work order {WorkOrderId} for vehicle {VehicleId}", 
                    workOrder.WorkOrderId, 
                    vehicleId);
                
                return workOrder.WorkOrderId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating reconditioning work order for vehicle {VehicleId}", vehicleId);
                throw new Exception($"Error creating reconditioning work order: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<ReconditioningStatus> GetReconditioningStatusAsync(string workOrderId)
        {
            try
            {
                _logger.LogInformation("Getting reconditioning status for work order {WorkOrderId}", workOrderId);
                
                var response = await _httpClient.GetAsync(
                    $"{_serviceManagementApiUrl}/api/repair-orders/{workOrderId}/status");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to get reconditioning status for work order {WorkOrderId}. Status code: {StatusCode}", 
                        workOrderId, 
                        response.StatusCode);
                    
                    throw new Exception($"Failed to get reconditioning status. Status code: {response.StatusCode}");
                }
                
                var result = await response.Content.ReadAsStringAsync();
                var statusResponse = JsonSerializer.Deserialize<ServiceStatusResponse>(result);
                
                return MapToReconditioningStatus(statusResponse.Status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting reconditioning status for work order {WorkOrderId}", workOrderId);
                throw new Exception($"Error getting reconditioning status: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<List<ServiceRecord>> GetVehicleServiceHistoryAsync(Guid vehicleId)
        {
            try
            {
                _logger.LogInformation("Getting service history for vehicle {VehicleId}", vehicleId);
                
                var response = await _httpClient.GetAsync(
                    $"{_serviceManagementApiUrl}/api/vehicles/{vehicleId}/service-history");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to get service history for vehicle {VehicleId}. Status code: {StatusCode}", 
                        vehicleId, 
                        response.StatusCode);
                    
                    throw new Exception($"Failed to get service history. Status code: {response.StatusCode}");
                }
                
                var result = await response.Content.ReadAsStringAsync();
                var serviceRecords = JsonSerializer.Deserialize<List<ServiceRecord>>(result);
                
                return serviceRecords;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting service history for vehicle {VehicleId}", vehicleId);
                throw new Exception($"Error getting service history: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<bool> UpdateReconditioningCompletionAsync(
            string workOrderId, 
            ReconditioningCompletionDetails completionDetails)
        {
            try
            {
                _logger.LogInformation("Updating reconditioning completion for work order {WorkOrderId}", workOrderId);
                
                var content = new StringContent(
                    JsonSerializer.Serialize(completionDetails), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PutAsync(
                    $"{_serviceManagementApiUrl}/api/repair-orders/{workOrderId}/completion",
                    content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to update reconditioning completion for work order {WorkOrderId}. Status code: {StatusCode}", 
                        workOrderId, 
                        response.StatusCode);
                    
                    throw new Exception($"Failed to update reconditioning completion. Status code: {response.StatusCode}");
                }
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating reconditioning completion for work order {WorkOrderId}", workOrderId);
                throw new Exception($"Error updating reconditioning completion: {ex.Message}", ex);
            }
        }
        
        /// <inheritdoc />
        public async Task<string> ScheduleServiceAppointmentAsync(
            Guid vehicleId, 
            DateTime serviceDate, 
            ServiceType serviceType, 
            string description)
        {
            try
            {
                _logger.LogInformation(
                    "Scheduling service appointment for vehicle {VehicleId}, type {ServiceType}", 
                    vehicleId, 
                    serviceType);
                
                var requestData = new
                {
                    VehicleId = vehicleId,
                    ServiceDate = serviceDate,
                    ServiceType = serviceType.ToString(),
                    Description = description,
                    Source = "InventoryManagement"
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(requestData), 
                    Encoding.UTF8, 
                    "application/json");
                
                var response = await _httpClient.PostAsync(
                    $"{_serviceManagementApiUrl}/api/appointments",
                    content);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError(
                        "Failed to schedule service appointment for vehicle {VehicleId}. Status code: {StatusCode}", 
                        vehicleId, 
                        response.StatusCode);
                    
                    throw new Exception($"Failed to schedule service appointment. Status code: {response.StatusCode}");
                }
                
                var result = await response.Content.ReadAsStringAsync();
                var appointmentResponse = JsonSerializer.Deserialize<ServiceAppointmentResponse>(result);
                
                _logger.LogInformation(
                    "Scheduled service appointment {AppointmentId} for vehicle {VehicleId}", 
                    appointmentResponse.AppointmentId, 
                    vehicleId);
                
                return appointmentResponse.AppointmentId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error scheduling service appointment for vehicle {VehicleId}", vehicleId);
                throw new Exception($"Error scheduling service appointment: {ex.Message}", ex);
            }
        }
        
        private ReconditioningStatus MapToReconditioningStatus(string serviceStatus)
        {
            return serviceStatus.ToLower() switch
            {
                "scheduled" => ReconditioningStatus.Scheduled,
                "in_progress" or "in progress" or "inprogress" => ReconditioningStatus.InProgress,
                "on_hold" or "on hold" or "onhold" => ReconditioningStatus.OnHold,
                "completed" or "complete" => ReconditioningStatus.Completed,
                "cancelled" or "canceled" => ReconditioningStatus.Cancelled,
                _ => ReconditioningStatus.Scheduled
            };
        }
    }
    
    internal class ServiceWorkOrderResponse
    {
        public string WorkOrderId { get; set; }
        public string Status { get; set; }
    }
    
    internal class ServiceStatusResponse
    {
        public string Status { get; set; }
    }
    
    internal class ServiceAppointmentResponse
    {
        public string AppointmentId { get; set; }
    }
}
