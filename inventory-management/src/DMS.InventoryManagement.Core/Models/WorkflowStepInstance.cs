using System;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents an instance of a workflow step for a specific vehicle
    /// </summary>
    public class WorkflowStepInstance : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the workflow instance ID
        /// </summary>
        public Guid WorkflowInstanceId { get; set; }

        /// <summary>
        /// Gets or sets the workflow step ID
        /// </summary>
        public Guid WorkflowStepId { get; set; }

        /// <summary>
        /// Gets or sets the status of this step
        /// </summary>
        public WorkflowStepStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the start date of this step
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the completion date of this step
        /// </summary>
        public DateTime? CompletionDate { get; set; }

        /// <summary>
        /// Gets or sets the actual duration in hours
        /// </summary>
        public double? ActualDurationHours { get; set; }

        /// <summary>
        /// Gets or sets notes about this step instance
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Gets or sets the user assigned to this step
        /// </summary>
        public string? AssignedTo { get; set; }

        /// <summary>
        /// Gets or sets the approver of this step (if approval is required)
        /// </summary>
        public string? ApprovedBy { get; set; }

        /// <summary>
        /// Gets or sets the approval date
        /// </summary>
        public DateTime? ApprovalDate { get; set; }

        /// <summary>
        /// Gets or sets the reference to the parent workflow instance
        /// </summary>
        public virtual WorkflowInstance? WorkflowInstance { get; set; }

        /// <summary>
        /// Gets or sets the reference to the workflow step definition
        /// </summary>
        public virtual WorkflowStep? WorkflowStep { get; set; }

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
    /// Status of a workflow step instance
    /// </summary>
    public enum WorkflowStepStatus
    {
        NotStarted,
        InProgress,
        WaitingForApproval,
        Approved,
        Rejected,
        Completed,
        Skipped
    }
}
