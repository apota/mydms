using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.API.Models.Workflows
{
    /// <summary>
    /// Request model for creating a workflow instance
    /// </summary>
    public class CreateWorkflowInstanceRequest
    {
        /// <summary>
        /// Gets or sets the workflow definition ID
        /// </summary>
        [Required]
        public Guid WorkflowDefinitionId { get; set; }
        
        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        [Required]
        public Guid VehicleId { get; set; }
        
        /// <summary>
        /// Gets or sets the priority (1-5, with 1 being highest)
        /// </summary>
        [Range(1, 5)]
        public int Priority { get; set; } = 3;
    }

    /// <summary>
    /// Request model for updating workflow status
    /// </summary>
    public class UpdateWorkflowStatusRequest
    {
        /// <summary>
        /// Gets or sets the new workflow status
        /// </summary>
        [Required]
        public WorkflowStatus Status { get; set; }
    }

    /// <summary>
    /// Request model for updating step status
    /// </summary>
    public class UpdateStepStatusRequest
    {
        /// <summary>
        /// Gets or sets the new step status
        /// </summary>
        [Required]
        public WorkflowStepStatus Status { get; set; }
        
        /// <summary>
        /// Gets or sets optional notes
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request model for assigning a step to a user
    /// </summary>
    public class AssignStepRequest
    {
        /// <summary>
        /// Gets or sets the user ID to assign to
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for approving a step
    /// </summary>
    public class ApproveStepRequest
    {
        /// <summary>
        /// Gets or sets the user ID of the approver
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets optional notes
        /// </summary>
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Request model for rejecting a step
    /// </summary>
    public class RejectStepRequest
    {
        /// <summary>
        /// Gets or sets the user ID of the rejector
        /// </summary>
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        /// <summary>
        /// Gets or sets notes explaining the rejection
        /// </summary>
        [Required]
        public string Notes { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for applying a price adjustment
    /// </summary>
    public class ApplyPriceAdjustmentRequest
    {
        /// <summary>
        /// Gets or sets the new price
        /// </summary>
        [Required]
        [Range(0.01, double.MaxValue)]
        public decimal NewPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the reason for the adjustment
        /// </summary>
        [Required]
        public string Reason { get; set; } = string.Empty;
    }

    /// <summary>
    /// Request model for completing reconditioning
    /// </summary>
    public class CompleteReconditioningRequest
    {
        /// <summary>
        /// Gets or sets optional notes
        /// </summary>
        public string? Notes { get; set; }
    }
    
    /// <summary>
    /// Request model for updating acquisition documents
    /// </summary>
    public class UpdateDocumentsRequest
    {
        /// <summary>
        /// Gets or sets the document IDs
        /// </summary>
        [Required]
        public List<Guid> DocumentIds { get; set; } = new();
    }
    
    /// <summary>
    /// Request model for completing vehicle intake
    /// </summary>
    public class CompleteIntakeRequest
    {
        /// <summary>
        /// Gets or sets optional notes
        /// </summary>
        public string? Notes { get; set; }
    }
}
