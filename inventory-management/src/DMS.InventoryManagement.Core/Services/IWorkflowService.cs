using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for workflow management services
    /// </summary>
    public interface IWorkflowService
    {
        /// <summary>
        /// Creates a new workflow definition
        /// </summary>
        /// <param name="workflowDefinition">The workflow definition to create</param>
        /// <returns>The created workflow definition</returns>
        Task<WorkflowDefinition> CreateWorkflowDefinitionAsync(WorkflowDefinition workflowDefinition);
        
        /// <summary>
        /// Updates an existing workflow definition
        /// </summary>
        /// <param name="workflowDefinition">The workflow definition to update</param>
        /// <returns>The updated workflow definition</returns>
        Task<WorkflowDefinition> UpdateWorkflowDefinitionAsync(WorkflowDefinition workflowDefinition);
        
        /// <summary>
        /// Gets a workflow definition by ID
        /// </summary>
        /// <param name="id">The workflow definition ID</param>
        /// <returns>The workflow definition</returns>
        Task<WorkflowDefinition?> GetWorkflowDefinitionAsync(Guid id);
        
        /// <summary>
        /// Gets all workflow definitions
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive workflow definitions</param>
        /// <returns>A list of workflow definitions</returns>
        Task<List<WorkflowDefinition>> GetAllWorkflowDefinitionsAsync(bool includeInactive = false);
        
        /// <summary>
        /// Gets workflow definitions by type
        /// </summary>
        /// <param name="workflowType">The workflow type</param>
        /// <param name="includeInactive">Whether to include inactive workflow definitions</param>
        /// <returns>A list of workflow definitions</returns>
        Task<List<WorkflowDefinition>> GetWorkflowDefinitionsByTypeAsync(WorkflowType workflowType, bool includeInactive = false);
        
        /// <summary>
        /// Gets the default workflow definition for a workflow type
        /// </summary>
        /// <param name="workflowType">The workflow type</param>
        /// <returns>The default workflow definition</returns>
        Task<WorkflowDefinition?> GetDefaultWorkflowDefinitionAsync(WorkflowType workflowType);
        
        /// <summary>
        /// Deletes a workflow definition
        /// </summary>
        /// <param name="id">The workflow definition ID</param>
        /// <returns>True if deleted, false otherwise</returns>
        Task<bool> DeleteWorkflowDefinitionAsync(Guid id);
        
        /// <summary>
        /// Creates a new workflow step
        /// </summary>
        /// <param name="workflowStep">The workflow step to create</param>
        /// <returns>The created workflow step</returns>
        Task<WorkflowStep> CreateWorkflowStepAsync(WorkflowStep workflowStep);
        
        /// <summary>
        /// Updates an existing workflow step
        /// </summary>
        /// <param name="workflowStep">The workflow step to update</param>
        /// <returns>The updated workflow step</returns>
        Task<WorkflowStep> UpdateWorkflowStepAsync(WorkflowStep workflowStep);
        
        /// <summary>
        /// Creates a new workflow instance for a vehicle
        /// </summary>
        /// <param name="workflowDefinitionId">The workflow definition ID</param>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="priority">The priority (optional)</param>
        /// <returns>The created workflow instance</returns>
        Task<WorkflowInstance> CreateWorkflowInstanceAsync(Guid workflowDefinitionId, Guid vehicleId, int priority = 3);
        
        /// <summary>
        /// Gets a workflow instance by ID
        /// </summary>
        /// <param name="id">The workflow instance ID</param>
        /// <returns>The workflow instance</returns>
        Task<WorkflowInstance?> GetWorkflowInstanceAsync(Guid id);
        
        /// <summary>
        /// Gets all workflow instances for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>A list of workflow instances</returns>
        Task<List<WorkflowInstance>> GetVehicleWorkflowInstancesAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets all active workflow instances for a vehicle by workflow type
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="workflowType">The workflow type</param>
        /// <returns>A list of workflow instances</returns>
        Task<List<WorkflowInstance>> GetActiveVehicleWorkflowInstancesByTypeAsync(Guid vehicleId, WorkflowType workflowType);
        
        /// <summary>
        /// Updates the status of a workflow instance
        /// </summary>
        /// <param name="workflowInstanceId">The workflow instance ID</param>
        /// <param name="status">The new status</param>
        /// <returns>The updated workflow instance</returns>
        Task<WorkflowInstance> UpdateWorkflowInstanceStatusAsync(Guid workflowInstanceId, WorkflowStatus status);
        
        /// <summary>
        /// Updates the status of a workflow step instance
        /// </summary>
        /// <param name="stepInstanceId">The step instance ID</param>
        /// <param name="status">The new status</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The updated workflow step instance</returns>
        Task<WorkflowStepInstance> UpdateWorkflowStepInstanceStatusAsync(
            Guid stepInstanceId, 
            WorkflowStepStatus status, 
            string? notes = null);
        
        /// <summary>
        /// Approves a workflow step instance
        /// </summary>
        /// <param name="stepInstanceId">The step instance ID</param>
        /// <param name="approverUserId">The ID of the user approving the step</param>
        /// <param name="notes">Optional notes</param>
        /// <returns>The updated workflow step instance</returns>
        Task<WorkflowStepInstance> ApproveWorkflowStepInstanceAsync(
            Guid stepInstanceId, 
            string approverUserId, 
            string? notes = null);
        
        /// <summary>
        /// Rejects a workflow step instance
        /// </summary>
        /// <param name="stepInstanceId">The step instance ID</param>
        /// <param name="approverUserId">The ID of the user rejecting the step</param>
        /// <param name="notes">Notes explaining the rejection (required)</param>
        /// <returns>The updated workflow step instance</returns>
        Task<WorkflowStepInstance> RejectWorkflowStepInstanceAsync(
            Guid stepInstanceId, 
            string approverUserId, 
            string notes);
        
        /// <summary>
        /// Assigns a workflow step instance to a user
        /// </summary>
        /// <param name="stepInstanceId">The step instance ID</param>
        /// <param name="userId">The user ID to assign to</param>
        /// <returns>The updated workflow step instance</returns>
        Task<WorkflowStepInstance> AssignWorkflowStepInstanceAsync(Guid stepInstanceId, string userId);
        
        /// <summary>
        /// Moves a workflow instance to the next step
        /// </summary>
        /// <param name="workflowInstanceId">The workflow instance ID</param>
        /// <returns>The next workflow step instance, or null if the workflow is complete</returns>
        Task<WorkflowStepInstance?> AdvanceWorkflowAsync(Guid workflowInstanceId);
    }
}
