using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Repositories;
using DMS.ServiceManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.ServiceManagement.Infrastructure.Repositories
{
    public class ServiceInspectionRepository : IServiceInspectionRepository
    {
        private readonly ServiceManagementDbContext _context;
        private readonly ILogger<ServiceInspectionRepository> _logger;
        
        public ServiceInspectionRepository(
            ServiceManagementDbContext context,
            ILogger<ServiceInspectionRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        public async Task<List<ServiceInspection>> GetAllAsync()
        {
            try
            {
                _logger.LogDebug("Getting all service inspections");
                return await _context.ServiceInspections
                    .Include(i => i.RecommendedServices)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all service inspections");
                throw;
            }
        }
        
        public async Task<ServiceInspection> GetByIdAsync(Guid id)
        {
            try
            {
                _logger.LogDebug($"Getting service inspection with ID: {id}");
                return await _context.ServiceInspections
                    .Include(i => i.RecommendedServices)
                    .FirstOrDefaultAsync(i => i.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting service inspection with ID: {id}");
                throw;
            }
        }
        
        public async Task<List<ServiceInspection>> GetByRepairOrderIdAsync(Guid repairOrderId)
        {
            try
            {
                _logger.LogDebug($"Getting service inspections for repair order ID: {repairOrderId}");
                return await _context.ServiceInspections
                    .Include(i => i.RecommendedServices)
                    .Where(i => i.RepairOrderId == repairOrderId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting service inspections for repair order ID: {repairOrderId}");
                throw;
            }
        }
        
        public async Task CreateAsync(ServiceInspection inspection)
        {
            try
            {
                _logger.LogDebug($"Creating service inspection for repair order ID: {inspection.RepairOrderId}");
                await _context.ServiceInspections.AddAsync(inspection);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Created service inspection with ID: {inspection.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service inspection");
                throw;
            }
        }
        
        public async Task UpdateAsync(ServiceInspection inspection)
        {
            try
            {
                _logger.LogDebug($"Updating service inspection with ID: {inspection.Id}");
                
                // First, make sure the inspection exists
                var existingInspection = await _context.ServiceInspections
                    .Include(i => i.RecommendedServices)
                    .FirstOrDefaultAsync(i => i.Id == inspection.Id);
                
                if (existingInspection == null)
                {
                    _logger.LogWarning($"Service inspection with ID: {inspection.Id} not found");
                    throw new InvalidOperationException($"Service inspection with ID: {inspection.Id} not found");
                }
                
                // Update the inspection properties
                _context.Entry(existingInspection).CurrentValues.SetValues(inspection);
                
                // Handle recommended services
                if (inspection.RecommendedServices != null)
                {
                    // Remove services that are no longer present
                    foreach (var existingService in existingInspection.RecommendedServices.ToList())
                    {
                        if (!inspection.RecommendedServices.Any(s => s.Id == existingService.Id))
                        {
                            _context.Remove(existingService);
                        }
                    }
                    
                    // Add/update services
                    foreach (var service in inspection.RecommendedServices)
                    {
                        var existingService = existingInspection.RecommendedServices.FirstOrDefault(s => s.Id == service.Id);
                        if (existingService != null)
                        {
                            // Update existing service
                            _context.Entry(existingService).CurrentValues.SetValues(service);
                        }
                        else
                        {
                            // Add new service
                            existingInspection.RecommendedServices.Add(service);
                        }
                    }
                }
                
                await _context.SaveChangesAsync();
                
                _logger.LogInformation($"Updated service inspection with ID: {inspection.Id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating service inspection with ID: {inspection?.Id}");
                throw;
            }
        }
        
        public async Task DeleteAsync(Guid id)
        {
            try
            {
                _logger.LogDebug($"Deleting service inspection with ID: {id}");
                
                var inspection = await _context.ServiceInspections
                    .FirstOrDefaultAsync(i => i.Id == id);
                
                if (inspection != null)
                {
                    _context.ServiceInspections.Remove(inspection);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation($"Deleted service inspection with ID: {id}");
                }
                else
                {
                    _logger.LogWarning($"Service inspection with ID: {id} not found for deletion");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting service inspection with ID: {id}");
                throw;
            }
        }
    }
}
