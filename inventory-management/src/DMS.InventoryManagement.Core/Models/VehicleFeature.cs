using System;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents a feature of a vehicle
    /// </summary>
    public class VehicleFeature : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the feature
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }

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
