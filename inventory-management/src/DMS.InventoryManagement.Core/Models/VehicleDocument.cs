using System;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents a document associated with a vehicle
    /// </summary>
    public class VehicleDocument : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the document title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        public DocumentType DocumentType { get; set; }

        /// <summary>
        /// Gets or sets the S3 file path
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the file size in bytes
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the MIME type
        /// </summary>
        public string MimeType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the upload date
        /// </summary>
        public DateTime UploadDate { get; set; }

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

        /// <summary>
        /// Reference to the associated Vehicle
        /// </summary>
        public virtual Vehicle? Vehicle { get; set; }
    }

    /// <summary>
    /// Type of vehicle document
    /// </summary>
    public enum DocumentType
    {
        Title,
        Registration,
        ServiceRecord,
        WarrantyInformation,
        PurchaseAgreement,
        InspectionReport,
        Odometer,
        EmissionTest,
        CarfaxReport,
        Other
    }
}
