using System;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents information about the aging of a vehicle in inventory
    /// </summary>
    public class VehicleAging : IEntity
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
        /// Gets or sets the number of days the vehicle has been in inventory
        /// </summary>
        public int DaysInInventory { get; set; }

        /// <summary>
        /// Gets or sets the threshold for aging alerts (in days)
        /// </summary>
        public int AgeThreshold { get; set; }

        /// <summary>
        /// Gets or sets the aging alert level
        /// </summary>
        public AgingAlertLevel AgingAlertLevel { get; set; }

        /// <summary>
        /// Gets or sets the date of the last price reduction
        /// </summary>
        public DateTime? LastPriceReductionDate { get; set; }

        /// <summary>
        /// Gets or sets the recommended action for this aging vehicle
        /// </summary>
        public string? RecommendedAction { get; set; }

        /// <summary>
        /// Reference to the associated Vehicle
        /// </summary>
        public virtual Vehicle? Vehicle { get; set; }
    }

    /// <summary>
    /// Alert level for aging inventory
    /// </summary>
    public enum AgingAlertLevel
    {
        Normal,
        Warning,
        Critical
    }
}
