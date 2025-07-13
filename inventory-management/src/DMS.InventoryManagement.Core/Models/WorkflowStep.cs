using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents a step within a workflow definition
    /// </summary>
    public class WorkflowStep : IAuditableEntity
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
        /// Gets or sets the name of the step
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the step
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the sequence number of this step
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets the expected duration for this step (in hours)
        /// </summary>
        public int ExpectedDurationHours { get; set; }

        /// <summary>
        /// Gets or sets the department or role responsible for this step
        /// </summary>
        public string? ResponsibleParty { get; set; }

        /// <summary>
        /// Gets or sets whether this step requires approval to proceed
        /// </summary>
        public bool RequiresApproval { get; set; }

        /// <summary>
        /// Gets or sets the list of required documents for this step
        /// </summary>
        public List<string>? RequiredDocuments { get; set; }

        /// <summary>
        /// Gets or sets the vehicle status associated with this step
        /// </summary>
        public VehicleStatus? AssociatedVehicleStatus { get; set; }

        /// <summary>
        /// Gets or sets the reference to the parent workflow definition
        /// </summary>
        public virtual WorkflowDefinition? WorkflowDefinition { get; set; }

        /// <summary>
        /// Gets or sets the date the step was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created the step
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date the step was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who last updated the step
        /// </summary>
        public string? UpdatedBy { get; set; }
    }
}
