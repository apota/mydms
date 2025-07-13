using DMS.InventoryManagement.Core.Models;

namespace DMS.InventoryManagement.API.Models
{
    /// <summary>
    /// Data transfer object for vehicle aging
    /// </summary>
    public class VehicleAgingDto
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
        /// Gets or sets brief vehicle information
        /// </summary>
        public VehicleSummaryDto? Vehicle { get; set; }

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
    }

    /// <summary>
    /// Data transfer object for vehicle summary used in aging reports
    /// </summary>
    public class VehicleSummaryDto
    {
        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the stock number
        /// </summary>
        public string StockNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the VIN
        /// </summary>
        public string VIN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the year
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the make
        /// </summary>
        public string Make { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the model
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the current list price
        /// </summary>
        public decimal ListPrice { get; set; }
    }

    /// <summary>
    /// Data transfer object for updating vehicle aging thresholds
    /// </summary>
    public class UpdateAgingThresholdDto
    {
        /// <summary>
        /// Gets or sets the threshold for aging alerts (in days)
        /// </summary>
        public int AgeThreshold { get; set; }
    }
}
