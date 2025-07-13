using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents an instance of a workflow applied to a specific vehicle
    /// </summary>
    public class WorkflowInstance : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the workflow definition ID
        /// </summary>
        public Guid WorkflowDefinitionId { get; set; }

        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the current status of the workflow
        /// </summary>
        public WorkflowStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the start date of the workflow
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// Gets or sets the completion date of the workflow
        /// </summary>
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        /// Gets or sets the priority of this workflow (1-5, with 1 being highest)
        /// </summary>
        public int Priority { get; set; } = 3;

        /// <summary>
        /// Gets or sets the list of workflow step instances in this workflow
        /// </summary>
        public virtual List<WorkflowStepInstance> StepInstances { get; set; } = new();

        /// <summary>
        /// Gets or sets the reference to the workflow definition
        /// </summary>
        public virtual WorkflowDefinition? WorkflowDefinition { get; set; }

        /// <summary>
        /// Gets or sets the reference to the vehicle
        /// </summary>
        public virtual Vehicle? Vehicle { get; set; }

        /// <summary>
        /// Gets or sets the date the instance was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created the instance
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date the instance was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who last updated the instance
        /// </summary>
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Status of a workflow instance
    /// </summary>
    public enum WorkflowStatus
    {
        NotStarted,
        InProgress,
        OnHold,
        Completed,
        Cancelled
    }
}
