using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.API.Models
{
    /// <summary>
    /// Data transfer object for vehicle document
    /// </summary>
    public class VehicleDocumentDto
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
    }

    /// <summary>
    /// Data transfer object for creating a vehicle document
    /// </summary>
    public class CreateVehicleDocumentDto
    {
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
    }

    /// <summary>
    /// Data transfer object for updating a vehicle document
    /// </summary>
    public class UpdateVehicleDocumentDto
    {
        /// <summary>
        /// Gets or sets the document title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the document type
        /// </summary>
        public DocumentType DocumentType { get; set; }
    }
}
