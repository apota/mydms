using System;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents an entry in the vehicle price history
    /// </summary>
    public class PriceHistoryEntry : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the vehicle pricing ID
        /// </summary>
        public Guid VehiclePricingId { get; set; }

        /// <summary>
        /// Gets or sets the price
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the date of the price change
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the reason for the price change
        /// </summary>
        public string? Reason { get; set; }

        /// <summary>
        /// Gets or sets the ID of the user who made the change
        /// </summary>
        public string? UserId { get; set; }
        
        /// <summary>
        /// Gets or sets the type of price action
        /// </summary>
        public PriceActionType ActionType { get; set; }
    }
}
