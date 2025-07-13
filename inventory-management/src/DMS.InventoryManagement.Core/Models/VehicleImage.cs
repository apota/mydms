using System;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents an image of a vehicle
    /// </summary>
    public class VehicleImage : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the image
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }

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
    /// Type of vehicle image
    /// </summary>
    public enum VehicleImageType
    {
        Exterior,
        Interior,
        Damage,
        Feature,
        Other
    }
}
