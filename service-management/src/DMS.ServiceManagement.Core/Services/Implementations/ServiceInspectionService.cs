using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace DMS.ServiceManagement.Core.Services.Implementations
{
    public class ServiceInspectionService : IServiceInspectionService
    {
        private readonly IServiceInspectionRepository _repository;
        private readonly IRepairOrderRepository _repairOrderRepository;
        private readonly ILogger<ServiceInspectionService> _logger;
        
        public ServiceInspectionService(
            IServiceInspectionRepository repository,
            IRepairOrderRepository repairOrderRepository,
            ILogger<ServiceInspectionService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _repairOrderRepository = repairOrderRepository ?? throw new ArgumentNullException(nameof(repairOrderRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<List<ServiceInspection>> GetAllInspectionsAsync()
        {
            try
            {
                _logger.LogInformation("Getting all service inspections");
                return await _repository.GetAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all service inspections");
                throw;
            }
        }
        
        public async Task<ServiceInspection> GetInspectionByIdAsync(Guid id)
        {
            try
            {
                _logger.LogInformation($"Getting service inspection with ID: {id}");
                var inspection = await _repository.GetByIdAsync(id);
                
                if (inspection == null)
                {
                    _logger.LogWarning($"Service inspection with ID: {id} not found");
                }
                
                return inspection;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting service inspection with ID: {id}");
                throw;
            }
        }
        
        public async Task<List<ServiceInspection>> GetInspectionsByRepairOrderIdAsync(Guid repairOrderId)
        {
            try
            {
                _logger.LogInformation($"Getting service inspections for repair order ID: {repairOrderId}");
                
                // Validate that the repair order exists
                var repairOrder = await _repairOrderRepository.GetByIdAsync(repairOrderId);
                if (repairOrder == null)
                {
                    _logger.LogWarning($"Repair order with ID: {repairOrderId} not found");
                    throw new ValidationException($"Repair order with ID: {repairOrderId} not found");
                }
                
                return await _repository.GetByRepairOrderIdAsync(repairOrderId);
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting service inspections for repair order ID: {repairOrderId}");
                throw;
            }
        }
        
        public async Task<ServiceInspection> CreateInspectionAsync(ServiceInspection inspection)
        {
            try
            {
                _logger.LogInformation($"Creating service inspection for repair order ID: {inspection.RepairOrderId}");
                
                ValidateInspection(inspection);
                
                // Check if repair order exists
                var repairOrder = await _repairOrderRepository.GetByIdAsync(inspection.RepairOrderId);
                if (repairOrder == null)
                {
                    _logger.LogWarning($"Repair order with ID: {inspection.RepairOrderId} not found");
                    throw new ValidationException($"Repair order with ID: {inspection.RepairOrderId} not found");
                }
                
                // Set metadata
                inspection.Id = Guid.NewGuid();
                inspection.CreatedAt = DateTime.UtcNow;
                inspection.UpdatedAt = DateTime.UtcNow;
                
                // Create the inspection
                await _repository.CreateAsync(inspection);
                
                return inspection;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service inspection");
                throw;
            }
        }
        
        public async Task<ServiceInspection> UpdateInspectionAsync(ServiceInspection inspection)
        {
            try
            {
                _logger.LogInformation($"Updating service inspection with ID: {inspection.Id}");
                
                // Check if inspection exists
                var existingInspection = await _repository.GetByIdAsync(inspection.Id);
                if (existingInspection == null)
                {
                    _logger.LogWarning($"Service inspection with ID: {inspection.Id} not found");
                    return null;
                }
                
                // Validate the inspection data
                ValidateInspection(inspection);
                
                // Update metadata
                inspection.CreatedAt = existingInspection.CreatedAt;
                inspection.UpdatedAt = DateTime.UtcNow;
                
                // Update the inspection
                await _repository.UpdateAsync(inspection);
                
                return inspection;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating service inspection with ID: {inspection?.Id}");
                throw;
            }
        }
        
        public async Task<bool> DeleteInspectionAsync(Guid id)
        {
            try
            {
                _logger.LogInformation($"Deleting service inspection with ID: {id}");
                
                // Check if inspection exists
                var inspection = await _repository.GetByIdAsync(id);
                if (inspection == null)
                {
                    _logger.LogWarning($"Service inspection with ID: {id} not found");
                    return false;
                }
                
                // Delete the inspection
                await _repository.DeleteAsync(id);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting service inspection with ID: {id}");
                throw;
            }
        }
        
        public async Task<ServiceInspection> AddInspectionImageAsync(Guid inspectionId, string pointId, string imageUrl)
        {
            try
            {
                _logger.LogInformation($"Adding image to service inspection with ID: {inspectionId}");
                
                // Check if inspection exists
                var inspection = await _repository.GetByIdAsync(inspectionId);
                if (inspection == null)
                {
                    _logger.LogWarning($"Service inspection with ID: {inspectionId} not found");
                    throw new ValidationException($"Service inspection with ID: {inspectionId} not found");
                }
                
                // Add image URL to the inspection
                if (!string.IsNullOrEmpty(pointId))
                {
                    // Note: InspectionPoints property doesn't exist in ServiceInspection entity
                    // For now, add to general inspection images with a note about the point
                    _logger.LogWarning($"InspectionPoints property not implemented. Adding image for point {pointId} to general images.");
                    inspection.InspectionImages = inspection.InspectionImages ?? new List<string>();
                    inspection.InspectionImages.Add($"Point-{pointId}: {imageUrl}");
                }
                else
                {
                    // Otherwise, add it to the general inspection images
                    inspection.InspectionImages = inspection.InspectionImages ?? new List<string>();
                    inspection.InspectionImages.Add(imageUrl);
                }
                
                // Update the inspection
                inspection.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(inspection);
                
                return inspection;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding image to service inspection with ID: {inspectionId}");
                throw;
            }
        }
        
        public async Task<ServiceInspection> RemoveInspectionImageAsync(Guid inspectionId, string imageUrl)
        {
            try
            {
                _logger.LogInformation($"Removing image from service inspection with ID: {inspectionId}");
                
                // Check if inspection exists
                var inspection = await _repository.GetByIdAsync(inspectionId);
                if (inspection == null)
                {
                    _logger.LogWarning($"Service inspection with ID: {inspectionId} not found");
                    throw new ValidationException($"Service inspection with ID: {inspectionId} not found");
                }
                
                // First try to remove from general inspection images
                bool removed = false;
                if (inspection.InspectionImages != null && inspection.InspectionImages.Contains(imageUrl))
                {
                    inspection.InspectionImages.Remove(imageUrl);
                    removed = true;
                }
                
                // If not found in general images, log that InspectionPoints is not implemented
                if (!removed)
                {
                    _logger.LogWarning($"InspectionPoints property not implemented. Could not remove image from inspection points for inspection {inspectionId}");
                }
                
                if (!removed)
                {
                    _logger.LogWarning($"Image URL: {imageUrl} not found in inspection {inspectionId}");
                    throw new ValidationException($"Image URL not found in inspection");
                }
                
                // Update the inspection
                inspection.UpdatedAt = DateTime.UtcNow;
                await _repository.UpdateAsync(inspection);
                
                return inspection;
            }
            catch (ValidationException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error removing image from service inspection with ID: {inspectionId}");
                throw;
            }
        }
        
        private void ValidateInspection(ServiceInspection inspection)
        {
            if (inspection == null)
            {
                throw new ValidationException("Inspection cannot be null");
            }
            
            if (inspection.RepairOrderId == Guid.Empty)
            {
                throw new ValidationException("RepairOrderId is required");
            }
            
            if (inspection.TechnicianId == Guid.Empty)
            {
                throw new ValidationException("TechnicianId is required");
            }
            
            if (!Enum.IsDefined(typeof(InspectionType), inspection.Type))
            {
                throw new ValidationException("Invalid inspection type");
            }
            
            // Validate start time and end time if provided
            if (inspection.StartTime != DateTime.MinValue && 
                inspection.EndTime != DateTime.MinValue && 
                inspection.EndTime < inspection.StartTime)
            {
                throw new ValidationException("End time cannot be before start time");
            }
            
            // Add more validation as needed
        }
    }
}
