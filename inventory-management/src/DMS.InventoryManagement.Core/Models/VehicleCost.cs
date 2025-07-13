using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents the cost breakdown for a vehicle
    /// </summary>
    public class VehicleCost : IAuditableEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the cost record
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the vehicle ID
        /// </summary>
        public Guid VehicleId { get; set; }

        /// <summary>
        /// Gets or sets the vehicle acquisition cost
        /// </summary>
        public decimal AcquisitionCost { get; set; }

        /// <summary>
        /// Gets or sets the transport cost
        /// </summary>
        public decimal TransportCost { get; set; }

        /// <summary>
        /// Gets or sets the reconditioning cost
        /// </summary>
        public decimal ReconditioningCost { get; set; }

        /// <summary>
        /// Gets or sets the certification cost (for CPO vehicles)
        /// </summary>
        public decimal CertificationCost { get; set; }

        /// <summary>
        /// Gets or sets the list of additional costs
        /// </summary>
        public List<AdditionalCost> AdditionalCosts { get; set; } = new();

        /// <summary>
        /// Gets or sets the total cost
        /// </summary>
        public decimal TotalCost { get; set; }

        /// <summary>
        /// Gets or sets the target gross profit
        /// </summary>
        public decimal TargetGrossProfit { get; set; }

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
    /// Represents an additional cost for a vehicle
    /// </summary>
    public class AdditionalCost : IEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the cost description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the cost amount
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// Gets or sets the date the cost was incurred
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the ID of the vehicle cost record this belongs to
        /// </summary>
        public Guid VehicleCostId { get; set; }
    }
}
