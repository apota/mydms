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
    /// Implementation of the workflow service
    /// </summary>
    public class WorkflowService : IWorkflowService
    {
        private readonly InventoryDbContext _dbContext;
        private readonly ILogger<WorkflowService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowService"/> class
        /// </summary>
        public WorkflowService(InventoryDbContext dbContext, ILogger<WorkflowService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(WorkflowDefinition workflowDefinition)
        {
            _dbContext.WorkflowDefinitions.Add(workflowDefinition);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Created workflow definition {WorkflowName} with ID {WorkflowId}", 
                workflowDefinition.Name, workflowDefinition.Id);
                
            return workflowDefinition;
        }

        /// <inheritdoc/>
        public async Task<WorkflowDefinition> UpdateWorkflowDefinitionAsync(WorkflowDefinition workflowDefinition)
        {
            var existing = await _dbContext.WorkflowDefinitions
                .Include(w => w.Steps)
                .FirstOrDefaultAsync(w => w.Id == workflowDefinition.Id);
                
            if (existing == null)
            {
                throw new ArgumentException($"Workflow definition with ID {workflowDefinition.Id} not found");
            }

            // Update properties
            existing.Name = workflowDefinition.Name;
            existing.Description = workflowDefinition.Description;
            existing.IsActive = workflowDefinition.IsActive;
            existing.IsDefault = workflowDefinition.IsDefault;
            
            // If this is set as default, unset any other defaults for this type
            if (workflowDefinition.IsDefault)
            {
                var otherDefaults = await _dbContext.WorkflowDefinitions
                    .Where(w => w.WorkflowType == workflowDefinition.WorkflowType && w.IsDefault && w.Id != workflowDefinition.Id)
                    .ToListAsync();
                    
                foreach (var other in otherDefaults)
                {
                    other.IsDefault = false;
                }
            }
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Updated workflow definition {WorkflowName} with ID {WorkflowId}", 
                existing.Name, existing.Id);
                
            return existing;
        }

        /// <inheritdoc/>
        public async Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(Guid id)
        {
            return await _dbContext.WorkflowDefinitions
                .Include(w => w.Steps.OrderBy(s => s.SequenceNumber))
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        /// <inheritdoc/>
        public async Task<List<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync(bool includeInactive = false)
        {
            var query = _dbContext.WorkflowDefinitions.AsQueryable();
            
            if (!includeInactive)
            {
                query = query.Where(w => w.IsActive);
            }
            
            return await query
                .Include(w => w.Steps.OrderBy(s => s.SequenceNumber))
                .OrderBy(w => w.WorkflowType)
                .ThenBy(w => w.Name)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<WorkflowDefinition>> GetWorkflowDefinitionsByTypeAsync(WorkflowType workflowType, bool includeInactive = false)
        {
            var query = _dbContext.WorkflowDefinitions
                .Where(w => w.WorkflowType == workflowType);
                
            if (!includeInactive)
            {
                query = query.Where(w => w.IsActive);
            }
            
            return await query
                .Include(w => w.Steps.OrderBy(s => s.SequenceNumber))
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<WorkflowDefinition?> GetDefaultWorkflowDefinitionAsync(WorkflowType workflowType)
        {
            return await _dbContext.WorkflowDefinitions
                .Include(w => w.Steps.OrderBy(s => s.SequenceNumber))
                .FirstOrDefaultAsync(w => w.WorkflowType == workflowType && w.IsDefault && w.IsActive);
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteWorkflowDefinitionAsync(Guid id)
        {
            // Check if there are any active workflow instances using this definition
            var hasActiveInstances = await _dbContext.WorkflowInstances
                .AnyAsync(w => w.WorkflowDefinitionId == id && 
                              (w.Status == WorkflowStatus.NotStarted || 
                               w.Status == WorkflowStatus.InProgress || 
                               w.Status == WorkflowStatus.OnHold));
                               
            if (hasActiveInstances)
            {
                _logger.LogWarning("Cannot delete workflow definition {WorkflowId} as it has active instances", id);
                return false;
            }
            
            var workflowDef = await _dbContext.WorkflowDefinitions
                .Include(w => w.Steps)
                .FirstOrDefaultAsync(w => w.Id == id);
                
            if (workflowDef == null)
            {
                return false;
            }
            
            _dbContext.WorkflowSteps.RemoveRange(workflowDef.Steps);
            _dbContext.WorkflowDefinitions.Remove(workflowDef);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Deleted workflow definition {WorkflowName} with ID {WorkflowId}", 
                workflowDef.Name, workflowDef.Id);
                
            return true;
        }

        /// <inheritdoc/>
        public async Task<WorkflowStep> CreateWorkflowStepAsync(WorkflowStep workflowStep)
        {
            _dbContext.WorkflowSteps.Add(workflowStep);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Created workflow step {StepName} with ID {StepId} for workflow {WorkflowId}", 
                workflowStep.Name, workflowStep.Id, workflowStep.WorkflowDefinitionId);
                
            return workflowStep;
        }

        /// <inheritdoc/>
        public async Task<WorkflowStep> UpdateWorkflowStepAsync(WorkflowStep workflowStep)
        {
            var existing = await _dbContext.WorkflowSteps
                .FirstOrDefaultAsync(s => s.Id == workflowStep.Id);
                
            if (existing == null)
            {
                throw new ArgumentException($"Workflow step with ID {workflowStep.Id} not found");
            }
            
            // Update properties
            existing.Name = workflowStep.Name;
            existing.Description = workflowStep.Description;
            existing.SequenceNumber = workflowStep.SequenceNumber;
            existing.ExpectedDurationHours = workflowStep.ExpectedDurationHours;
            existing.ResponsibleParty = workflowStep.ResponsibleParty;
            existing.RequiresApproval = workflowStep.RequiresApproval;
            existing.RequiredDocuments = workflowStep.RequiredDocuments;
            existing.AssociatedVehicleStatus = workflowStep.AssociatedVehicleStatus;
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Updated workflow step {StepName} with ID {StepId}", 
                existing.Name, existing.Id);
                
            return existing;
        }

        /// <inheritdoc/>
        public async Task<WorkflowInstance> CreateWorkflowInstanceAsync(Guid workflowDefinitionId, Guid vehicleId, int priority = 3)
        {
            var workflowDefinition = await _dbContext.WorkflowDefinitions
                .Include(w => w.Steps.OrderBy(s => s.SequenceNumber))
                .FirstOrDefaultAsync(w => w.Id == workflowDefinitionId);
                
            if (workflowDefinition == null)
            {
                throw new ArgumentException($"Workflow definition with ID {workflowDefinitionId} not found");
            }
            
            var vehicle = await _dbContext.Vehicles.FindAsync(vehicleId);
            if (vehicle == null)
            {
                throw new ArgumentException($"Vehicle with ID {vehicleId} not found");
            }
            
            // Create the workflow instance
            var workflowInstance = new WorkflowInstance
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = workflowDefinitionId,
                VehicleId = vehicleId,
                Status = WorkflowStatus.NotStarted,
                StartDate = DateTime.UtcNow,
                Priority = priority
            };
            
            _dbContext.WorkflowInstances.Add(workflowInstance);
            
            // Create step instances for each step in the workflow definition
            foreach (var step in workflowDefinition.Steps.OrderBy(s => s.SequenceNumber))
            {
                var stepInstance = new WorkflowStepInstance
                {
                    Id = Guid.NewGuid(),
                    WorkflowInstanceId = workflowInstance.Id,
                    WorkflowStepId = step.Id,
                    Status = WorkflowStepStatus.NotStarted
                };
                
                _dbContext.WorkflowStepInstances.Add(stepInstance);
            }
            
            await _dbContext.SaveChangesAsync();
            
            // Update vehicle status if first step has an associated status
            var firstStep = workflowDefinition.Steps.OrderBy(s => s.SequenceNumber).FirstOrDefault();
            if (firstStep?.AssociatedVehicleStatus != null)
            {
                vehicle.Status = firstStep.AssociatedVehicleStatus.Value;
                await _dbContext.SaveChangesAsync();
            }
            
            _logger.LogInformation("Created workflow instance {WorkflowId} for vehicle {VehicleId}", 
                workflowInstance.Id, vehicleId);
                
            return workflowInstance;
        }

        /// <inheritdoc/>
        public async Task<WorkflowInstance?> GetWorkflowInstanceAsync(Guid id)
        {
            return await _dbContext.WorkflowInstances
                .Include(w => w.WorkflowDefinition)
                .Include(w => w.Vehicle)
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.WorkflowStep)
                .FirstOrDefaultAsync(w => w.Id == id);
        }

        /// <inheritdoc/>
        public async Task<List<WorkflowInstance>> GetVehicleWorkflowInstancesAsync(Guid vehicleId)
        {
            return await _dbContext.WorkflowInstances
                .Include(w => w.WorkflowDefinition)
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.WorkflowStep)
                .Where(w => w.VehicleId == vehicleId)
                .OrderByDescending(w => w.StartDate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<List<WorkflowInstance>> GetActiveVehicleWorkflowInstancesByTypeAsync(Guid vehicleId, WorkflowType workflowType)
        {
            return await _dbContext.WorkflowInstances
                .Include(w => w.WorkflowDefinition)
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.WorkflowStep)
                .Where(w => w.VehicleId == vehicleId && 
                          w.WorkflowDefinition.WorkflowType == workflowType &&
                          (w.Status == WorkflowStatus.NotStarted || w.Status == WorkflowStatus.InProgress || w.Status == WorkflowStatus.OnHold))
                .OrderByDescending(w => w.StartDate)
                .ToListAsync();
        }

        /// <inheritdoc/>
        public async Task<WorkflowInstance> UpdateWorkflowInstanceStatusAsync(Guid workflowInstanceId, WorkflowStatus status)
        {
            var workflowInstance = await _dbContext.WorkflowInstances
                .Include(w => w.StepInstances)
                .FirstOrDefaultAsync(w => w.Id == workflowInstanceId);
                
            if (workflowInstance == null)
            {
                throw new ArgumentException($"Workflow instance with ID {workflowInstanceId} not found");
            }
            
            workflowInstance.Status = status;
            
            if (status == WorkflowStatus.Completed)
            {
                workflowInstance.CompletionDate = DateTime.UtcNow;
            }
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Updated workflow instance {WorkflowId} status to {Status}", 
                workflowInstance.Id, status);
                
            return workflowInstance;
        }

        /// <inheritdoc/>
        public async Task<WorkflowStepInstance> UpdateWorkflowStepInstanceStatusAsync(Guid stepInstanceId, WorkflowStepStatus status, string? notes = null)
        {
            var stepInstance = await _dbContext.WorkflowStepInstances
                .Include(s => s.WorkflowInstance)
                .Include(s => s.WorkflowStep)
                .FirstOrDefaultAsync(s => s.Id == stepInstanceId);
                
            if (stepInstance == null)
            {
                throw new ArgumentException($"Workflow step instance with ID {stepInstanceId} not found");
            }
            
            stepInstance.Status = status;
            
            if (notes != null)
            {
                stepInstance.Notes = notes;
            }
            
            switch (status)
            {
                case WorkflowStepStatus.InProgress:
                    if (stepInstance.StartDate == null)
                    {
                        stepInstance.StartDate = DateTime.UtcNow;
                    }
                    
                    // If this is the first step, update the workflow instance status
                    if (stepInstance.WorkflowInstance.Status == WorkflowStatus.NotStarted)
                    {
                        stepInstance.WorkflowInstance.Status = WorkflowStatus.InProgress;
                    }
                    break;
                    
                case WorkflowStepStatus.Completed:
                    stepInstance.CompletionDate = DateTime.UtcNow;
                    if (stepInstance.StartDate.HasValue)
                    {
                        stepInstance.ActualDurationHours = (DateTime.UtcNow - stepInstance.StartDate.Value).TotalHours;
                    }
                    break;
            }
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Updated workflow step instance {StepInstanceId} status to {Status}", 
                stepInstance.Id, status);
                
            return stepInstance;
        }

        /// <inheritdoc/>
        public async Task<WorkflowStepInstance> ApproveWorkflowStepInstanceAsync(Guid stepInstanceId, string approverUserId, string? notes = null)
        {
            var stepInstance = await _dbContext.WorkflowStepInstances
                .Include(s => s.WorkflowStep)
                .FirstOrDefaultAsync(s => s.Id == stepInstanceId);
                
            if (stepInstance == null)
            {
                throw new ArgumentException($"Workflow step instance with ID {stepInstanceId} not found");
            }
            
            if (!stepInstance.WorkflowStep.RequiresApproval)
            {
                throw new InvalidOperationException($"Workflow step {stepInstance.WorkflowStep.Name} does not require approval");
            }
            
            if (stepInstance.Status != WorkflowStepStatus.WaitingForApproval)
            {
                throw new InvalidOperationException($"Cannot approve workflow step in status {stepInstance.Status}");
            }
            
            stepInstance.Status = WorkflowStepStatus.Approved;
            stepInstance.ApprovedBy = approverUserId;
            stepInstance.ApprovalDate = DateTime.UtcNow;
            
            if (notes != null)
            {
                stepInstance.Notes = notes;
            }
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Approved workflow step instance {StepInstanceId} by user {UserId}", 
                stepInstance.Id, approverUserId);
                
            return stepInstance;
        }

        /// <inheritdoc/>
        public async Task<WorkflowStepInstance> RejectWorkflowStepInstanceAsync(Guid stepInstanceId, string approverUserId, string notes)
        {
            var stepInstance = await _dbContext.WorkflowStepInstances
                .Include(s => s.WorkflowStep)
                .FirstOrDefaultAsync(s => s.Id == stepInstanceId);
                
            if (stepInstance == null)
            {
                throw new ArgumentException($"Workflow step instance with ID {stepInstanceId} not found");
            }
            
            if (!stepInstance.WorkflowStep.RequiresApproval)
            {
                throw new InvalidOperationException($"Workflow step {stepInstance.WorkflowStep.Name} does not require approval");
            }
            
            if (stepInstance.Status != WorkflowStepStatus.WaitingForApproval)
            {
                throw new InvalidOperationException($"Cannot reject workflow step in status {stepInstance.Status}");
            }
            
            if (string.IsNullOrWhiteSpace(notes))
            {
                throw new ArgumentException("Rejection notes are required");
            }
            
            stepInstance.Status = WorkflowStepStatus.Rejected;
            stepInstance.ApprovedBy = approverUserId;
            stepInstance.ApprovalDate = DateTime.UtcNow;
            stepInstance.Notes = notes;
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Rejected workflow step instance {StepInstanceId} by user {UserId}", 
                stepInstance.Id, approverUserId);
                
            return stepInstance;
        }

        /// <inheritdoc/>
        public async Task<WorkflowStepInstance> AssignWorkflowStepInstanceAsync(Guid stepInstanceId, string userId)
        {
            var stepInstance = await _dbContext.WorkflowStepInstances
                .FirstOrDefaultAsync(s => s.Id == stepInstanceId);
                
            if (stepInstance == null)
            {
                throw new ArgumentException($"Workflow step instance with ID {stepInstanceId} not found");
            }
            
            stepInstance.AssignedTo = userId;
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Assigned workflow step instance {StepInstanceId} to user {UserId}", 
                stepInstance.Id, userId);
                
            return stepInstance;
        }

        /// <inheritdoc/>
        public async Task<WorkflowStepInstance?> AdvanceWorkflowAsync(Guid workflowInstanceId)
        {
            var workflow = await _dbContext.WorkflowInstances
                .Include(w => w.StepInstances)
                    .ThenInclude(s => s.WorkflowStep)
                .Include(w => w.Vehicle)
                .Include(w => w.WorkflowDefinition)
                    .ThenInclude(wd => wd.Steps)
                .FirstOrDefaultAsync(w => w.Id == workflowInstanceId);
                
            if (workflow == null)
            {
                throw new ArgumentException($"Workflow instance with ID {workflowInstanceId} not found");
            }
            
            if (workflow.Status != WorkflowStatus.InProgress && workflow.Status != WorkflowStatus.NotStarted)
            {
                throw new InvalidOperationException($"Cannot advance workflow in status {workflow.Status}");
            }
            
            // Find current step
            var currentStep = workflow.StepInstances
                .Where(s => s.Status == WorkflowStepStatus.InProgress || 
                           s.Status == WorkflowStepStatus.WaitingForApproval)
                .OrderBy(s => s.WorkflowStep.SequenceNumber)
                .FirstOrDefault();
            
            if (currentStep == null)
            {
                // Find the first not started step
                currentStep = workflow.StepInstances
                    .Where(s => s.Status == WorkflowStepStatus.NotStarted)
                    .OrderBy(s => s.WorkflowStep.SequenceNumber)
                    .FirstOrDefault();
                    
                if (currentStep == null)
                {
                    // All steps are completed, complete the workflow
                    workflow.Status = WorkflowStatus.Completed;
                    workflow.CompletionDate = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                    
                    _logger.LogInformation("Completed workflow instance {WorkflowId}", workflow.Id);
                    
                    return null;
                }
                
                // Start the first step
                currentStep.Status = WorkflowStepStatus.InProgress;
                currentStep.StartDate = DateTime.UtcNow;
                
                // Update the workflow status
                workflow.Status = WorkflowStatus.InProgress;
                
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Started workflow step {StepName} for workflow {WorkflowId}", 
                    currentStep.WorkflowStep.Name, workflow.Id);
                    
                return currentStep;
            }
            
            // Current step exists, mark it as completed and start the next step
            if (currentStep.Status == WorkflowStepStatus.WaitingForApproval)
            {
                throw new InvalidOperationException("Cannot advance workflow when current step is waiting for approval");
            }
            
            if (currentStep.WorkflowStep.RequiresApproval)
            {
                // Set status to waiting for approval
                currentStep.Status = WorkflowStepStatus.WaitingForApproval;
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Step {StepName} is waiting for approval in workflow {WorkflowId}", 
                    currentStep.WorkflowStep.Name, workflow.Id);
                    
                return currentStep;
            }
            
            // Complete the current step
            currentStep.Status = WorkflowStepStatus.Completed;
            currentStep.CompletionDate = DateTime.UtcNow;
            if (currentStep.StartDate.HasValue)
            {
                currentStep.ActualDurationHours = (DateTime.UtcNow - currentStep.StartDate.Value).TotalHours;
            }
            
            // Find the next step
            var nextStep = workflow.StepInstances
                .Where(s => s.Status == WorkflowStepStatus.NotStarted && 
                          s.WorkflowStep.SequenceNumber > currentStep.WorkflowStep.SequenceNumber)
                .OrderBy(s => s.WorkflowStep.SequenceNumber)
                .FirstOrDefault();
                
            if (nextStep == null)
            {
                // No more steps, complete the workflow
                workflow.Status = WorkflowStatus.Completed;
                workflow.CompletionDate = DateTime.UtcNow;
                
                // If the last step has an associated vehicle status, update the vehicle
                if (currentStep.WorkflowStep.AssociatedVehicleStatus.HasValue && workflow.Vehicle != null)
                {
                    workflow.Vehicle.Status = currentStep.WorkflowStep.AssociatedVehicleStatus.Value;
                }
                
                await _dbContext.SaveChangesAsync();
                
                _logger.LogInformation("Completed workflow instance {WorkflowId}", workflow.Id);
                
                return null;
            }
            
            // Start the next step
            nextStep.Status = WorkflowStepStatus.InProgress;
            nextStep.StartDate = DateTime.UtcNow;
            
            // If the next step has an associated vehicle status, update the vehicle
            if (nextStep.WorkflowStep.AssociatedVehicleStatus.HasValue && workflow.Vehicle != null)
            {
                workflow.Vehicle.Status = nextStep.WorkflowStep.AssociatedVehicleStatus.Value;
            }
            
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Advanced workflow {WorkflowId} to step {StepName}", 
                workflow.Id, nextStep.WorkflowStep.Name);
                
            return nextStep;
        }
    }
}
