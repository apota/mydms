using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Services;
using DMS.InventoryManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of the reconditioning service
    /// </summary>
    public class ReconditioningService : IReconditioningService
    {
        private readonly InventoryDbContext _dbContext;
        private readonly IWorkflowService _workflowService;
        private readonly ILogger<ReconditioningService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReconditioningService"/> class
        /// </summary>
        public ReconditioningService(
            InventoryDbContext dbContext,
            IWorkflowService workflowService,
            ILogger<ReconditioningService> logger)
        {
            _dbContext = dbContext;
            _workflowService = workflowService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<WorkflowInstance> CreateReconditioningWorkflowAsync(Guid vehicleId)
        {
            // Get the default reconditioning workflow definition
            var workflowDefinition = await _workflowService.GetDefaultWorkflowDefinitionAsync(WorkflowType.Reconditioning);
            
            if (workflowDefinition == null)
            {
                throw new InvalidOperationException("No default reconditioning workflow definition found");
            }
            
            // Check if there's already an active reconditioning workflow for this vehicle
            var activeWorkflows = await _workflowService.GetActiveVehicleWorkflowInstancesByTypeAsync(vehicleId, WorkflowType.Reconditioning);
            
            if (activeWorkflows.Any())
            {
                throw new InvalidOperationException("Vehicle already has an active reconditioning workflow");
            }
            
            var vehicle = await _dbContext.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found");
            }
            
            // Update vehicle status to Reconditioning
            vehicle.Status = VehicleStatus.Reconditioning;
            await _dbContext.SaveChangesAsync();
            
            // Create the workflow instance
            var workflowInstance = await _workflowService.CreateWorkflowInstanceAsync(
                workflowDefinition.Id, 
                vehicleId, 
                priority: 3); // Normal priority for reconditioning
                
            _logger.LogInformation("Created reconditioning workflow for vehicle {VehicleId}", vehicleId);
            
            return workflowInstance;
        }

        /// <inheritdoc/>
        public async Task<WorkflowInstance?> GetActiveReconditioningWorkflowAsync(Guid vehicleId)
        {
            var activeWorkflows = await _workflowService.GetActiveVehicleWorkflowInstancesByTypeAsync(vehicleId, WorkflowType.Reconditioning);
            
            return activeWorkflows.FirstOrDefault();
        }

        /// <inheritdoc/>
        public async Task<ReconditioningRecord> AddReconditioningRecordAsync(ReconditioningRecord record)
        {
            // Validate that the vehicle exists
            var vehicle = await _dbContext.Vehicles
                .Include(v => v.CostDetails)
                .FirstOrDefaultAsync(v => v.Id == record.VehicleId);
                
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {record.VehicleId} not found");
            }
            
            // Generate ID if not provided
            if (record.Id == Guid.Empty)
            {
                record.Id = Guid.NewGuid();
            }
            
            // Set created date if not provided
            if (record.CreatedAt == default)
            {
                record.CreatedAt = DateTime.UtcNow;
            }
            
            _dbContext.ReconditioningRecords.Add(record);
            
            // Update the vehicle reconditioning cost
            if (vehicle.CostDetails != null)
            {
                vehicle.CostDetails.ReconditioningCost += record.Cost;
                vehicle.CostDetails.TotalCost += record.Cost;
            }
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Added reconditioning record {RecordId} to vehicle {VehicleId}, Cost: {Cost}", 
                record.Id, record.VehicleId, record.Cost);
                
            return record;
        }

        /// <inheritdoc/>
        public async Task<ReconditioningRecord> UpdateReconditioningRecordAsync(ReconditioningRecord record)
        {
            var existingRecord = await _dbContext.ReconditioningRecords
                .FindAsync(record.Id);
                
            if (existingRecord == null)
            {
                throw new ArgumentException($"Reconditioning record with ID {record.Id} not found");
            }
            
            // Update the vehicle reconditioning cost
            var vehicle = await _dbContext.Vehicles
                .Include(v => v.CostDetails)
                .FirstOrDefaultAsync(v => v.Id == existingRecord.VehicleId);
                
            if (vehicle?.CostDetails != null)
            {
                // Adjust the reconditioning cost by the difference
                var costDifference = record.Cost - existingRecord.Cost;
                vehicle.CostDetails.ReconditioningCost += costDifference;
                vehicle.CostDetails.TotalCost += costDifference;
            }
            
            // Update properties
            existingRecord.Description = record.Description;
            existingRecord.Cost = record.Cost;
            existingRecord.Vendor = record.Vendor;
            existingRecord.WorkDate = record.WorkDate;
            existingRecord.Status = record.Status;
            existingRecord.UpdatedAt = DateTime.UtcNow;
            existingRecord.UpdatedBy = record.UpdatedBy;
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Updated reconditioning record {RecordId}, Cost: {Cost}", 
                existingRecord.Id, existingRecord.Cost);
                
            return existingRecord;
        }

        /// <inheritdoc/>
        public async Task<List<ReconditioningRecord>> GetVehicleReconditioningRecordsAsync(Guid vehicleId)
        {
            return await _dbContext.ReconditioningRecords
                .Where(r => r.VehicleId == vehicleId)
                .OrderByDescending(r => r.WorkDate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<ReconditioningSummary> GetReconditioningSummaryAsync(Guid vehicleId)
        {
            var vehicle = await _dbContext.Vehicles
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
                
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found");
            }
            
            // Get reconditioning records
            var records = await _dbContext.ReconditioningRecords
                .Where(r => r.VehicleId == vehicleId)
                .ToListAsync();
                
            // Get active workflow
            var activeWorkflow = await GetActiveReconditioningWorkflowAsync(vehicleId);
            
            string currentStatus = "No active reconditioning";
            string currentStepName = "N/A";
            string? assignedTechnician = null;
            DateTime? estimatedCompletionDate = null;
            
            if (activeWorkflow != null)
            {
                // Get the current step
                var currentStep = activeWorkflow.StepInstances
                    .Where(s => s.Status == WorkflowStepStatus.InProgress || 
                               s.Status == WorkflowStepStatus.WaitingForApproval)
                    .OrderBy(s => s.WorkflowStep.SequenceNumber)
                    .FirstOrDefault();
                    
                if (currentStep != null)
                {
                    currentStatus = $"In Progress - {currentStep.Status}";
                    currentStepName = currentStep.WorkflowStep.Name;
                    assignedTechnician = currentStep.AssignedTo;
                    
                    // Calculate estimated completion date
                    if (currentStep.StartDate.HasValue)
                    {
                        estimatedCompletionDate = currentStep.StartDate.Value.AddHours(currentStep.WorkflowStep.ExpectedDurationHours);
                    }
                }
                else
                {
                    currentStatus = $"Workflow Status: {activeWorkflow.Status}";
                }
            }
            
            // Calculate days in reconditioning
            var firstRecord = records.OrderBy(r => r.WorkDate).FirstOrDefault();
            var daysInReconditioning = firstRecord != null ? 
                (int)(DateTime.UtcNow - firstRecord.WorkDate).TotalDays : 
                0;
                
            var summary = new ReconditioningSummary
            {
                VehicleId = vehicleId,
                CurrentStatus = currentStatus,
                TotalCost = records.Sum(r => r.Cost),
                DaysInReconditioning = daysInReconditioning,
                EstimatedCompletionDate = estimatedCompletionDate,
                CurrentStepName = currentStepName,
                AssignedTechnician = assignedTechnician
            };
            
            return summary;
        }

        /// <inheritdoc/>
        public async Task<List<Vehicle>> GetVehiclesInReconditioningAsync()
        {
            return await _dbContext.Vehicles
                .Include(v => v.ReconditioningRecords)
                .Where(v => v.Status == VehicleStatus.Reconditioning && !v.IsDeleted)
                .OrderByDescending(v => v.UpdatedAt)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<Vehicle> MarkVehicleReadyForFrontLineAsync(Guid vehicleId, string? notes = null)
        {
            var vehicle = await _dbContext.Vehicles.FindAsync(vehicleId);
            
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found");
            }
            
            // Complete any active reconditioning workflows
            var activeWorkflow = await GetActiveReconditioningWorkflowAsync(vehicleId);
            if (activeWorkflow != null)
            {
                await _workflowService.UpdateWorkflowInstanceStatusAsync(activeWorkflow.Id, WorkflowStatus.Completed);
            }
            
            // Update vehicle status
            vehicle.Status = VehicleStatus.FrontLine;
            vehicle.UpdatedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Marked vehicle {VehicleId} as ready for front line", vehicleId);
            
            return vehicle;
        }
    }
}
