using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents a workflow definition that can be applied to vehicles
    /// </summary>
    public class WorkflowDefinition : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the workflow
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the workflow
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the type of workflow
        /// </summary>
        public WorkflowType WorkflowType { get; set; }

        /// <summary>
        /// Gets or sets whether this workflow is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets whether this workflow is the default for its type
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets the list of steps in this workflow
        /// </summary>
        public virtual List<WorkflowStep> Steps { get; set; } = new();

        /// <summary>
        /// Gets or sets the workflow instances created from this definition
        /// </summary>
        public virtual List<WorkflowInstance> Instances { get; set; } = new();

        /// <summary>
        /// Gets or sets the date the workflow was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created the workflow
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date the workflow was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who last updated the workflow
        /// </summary>
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Types of workflows in the inventory system
    /// </summary>
    public enum WorkflowType
    {
        Acquisition,
        Reconditioning,
        AgingManagement
    }
}
