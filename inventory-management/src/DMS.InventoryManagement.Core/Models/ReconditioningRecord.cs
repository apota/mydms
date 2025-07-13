using System;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents a reconditioning record for a vehicle
    /// </summary>
    public class ReconditioningRecord : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the record
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the description of work performed
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cost of reconditioning
        /// </summary>
        public decimal Cost { get; set; }

        /// <summary>
        /// Gets or sets the vendor who performed the work
        /// </summary>
        public string? Vendor { get; set; }

        /// <summary>
        /// Gets or sets the date the work was performed
        /// </summary>
        public DateTime WorkDate { get; set; }

        /// <summary>
        /// Gets or sets the status of the reconditioning work
        /// </summary>
        public ReconditioningStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the date the record was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created the record
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date the record was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who last updated the record
        /// </summary>
        public string? UpdatedBy { get; set; }
    }

    /// <summary>
    /// Status of reconditioning work
    /// </summary>
    public enum ReconditioningStatus
    {
        Scheduled,
        InProgress,
        Completed,
        Cancelled
    }
}
