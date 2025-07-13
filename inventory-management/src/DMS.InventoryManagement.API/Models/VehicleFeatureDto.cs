using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.API.Models
{
    /// <summary>
    /// Data transfer object for vehicle feature information
    /// </summary>
    public class VehicleFeatureDto
    {
        /// <summary>
        /// Gets or sets the unique identifier for the feature
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the feature name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the feature description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the feature category
        /// </summary>
        public string? Category { get; set; }
    }

    /// <summary>
    /// Data transfer object for creating a new vehicle feature
    /// </summary>
    public class CreateVehicleFeatureDto
    {
        /// <summary>
        /// Gets or sets the feature name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the feature description
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the feature category
        /// </summary>
        public string? Category { get; set; }
    }
}
