using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.API.Models
{
    /// <summary>
    /// Data transfer object for reconditioning record information
    /// </summary>
    public class ReconditioningRecordDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the record
        /// </summary>
        public Guid Id { get; set; }

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
    }

    /// <summary>
    /// Data transfer object for creating a new reconditioning record
    /// </summary>
    public class CreateReconditioningRecordDto
    {
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
    }
}
