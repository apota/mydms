using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.API.Models
{
    /// <summary>
    /// Data transfer object for vehicle image information
    /// </summary>
    public class VehicleImageDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the image
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the S3 file path
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the image caption
        /// </summary>
        public string? Caption { get; set; }

        /// <summary>
        /// Gets or sets the image sequence number for ordering
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the primary image
        /// </summary>
        public bool IsPrimary { get; set; }

        /// <summary>
        /// Gets or sets the upload date
        /// </summary>
        public DateTime UploadDate { get; set; }
        
        /// <summary>
        /// Gets or sets the image type
        /// </summary>
        public VehicleImageType ImageType { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a new vehicle image
    /// </summary>
    public class CreateVehicleImageDto
    {
        /// <summary>
        /// Gets or sets the image caption
        /// </summary>
        public string? Caption { get; set; }

        /// <summary>
        /// Gets or sets the image sequence number for ordering
        /// </summary>
        public int SequenceNumber { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this is the primary image
        /// </summary>
        public bool IsPrimary { get; set; }
        
        /// <summary>
        /// Gets or sets the image type
        /// </summary>
        public VehicleImageType ImageType { get; set; }
    }
}
