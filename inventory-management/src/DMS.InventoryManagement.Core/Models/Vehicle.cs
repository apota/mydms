using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.InventoryManagement.Core.Models
{
    /// <summary>
    /// Represents a vehicle in inventory
    /// </summary>
    public class Vehicle : IAuditableEntity, ISoftDeleteEntity
    {
        /// <summary>
        /// Gets or sets the unique identifier for the vehicle
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the Vehicle Identification Number
        /// </summary>
        public string VIN { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle stock number
        /// </summary>
        public string StockNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle make
        /// </summary>
        public string Make { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle model
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the vehicle year
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Gets or sets the vehicle trim level
        /// </summary>
        public string? Trim { get; set; }

        /// <summary>
        /// Gets or sets the vehicle exterior color
        /// </summary>
        public string? ExteriorColor { get; set; }

        /// <summary>
        /// Gets or sets the vehicle interior color
        /// </summary>
        public string? InteriorColor { get; set; }

        /// <summary>
        /// Gets or sets the vehicle mileage
        /// </summary>
        public int Mileage { get; set; }

        /// <summary>
        /// Gets or sets the vehicle type (New, Used, Certified Pre-Owned)
        /// </summary>
        public VehicleType VehicleType { get; set; }

        /// <summary>
        /// Gets or sets the vehicle status
        /// </summary>
        public VehicleStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the vehicle acquisition cost
        /// </summary>
        public decimal AcquisitionCost { get; set; }

        /// <summary>
        /// Gets or sets the vehicle listing price
        /// </summary>
        public decimal ListPrice { get; set; }

        /// <summary>
        /// Gets or sets the vehicle invoice price (for new vehicles)
        /// </summary>
        public decimal? InvoicePrice { get; set; }

        /// <summary>
        /// Gets or sets the vehicle MSRP (for new vehicles)
        /// </summary>
        public decimal? MSRP { get; set; }

        /// <summary>
        /// Gets or sets the date when the vehicle was acquired
        /// </summary>
        public DateTime AcquisitionDate { get; set; }

        /// <summary>
        /// Gets or sets the source of the vehicle acquisition
        /// </summary>
        public string? AcquisitionSource { get; set; }

        /// <summary>
        /// Gets or sets the physical location of the vehicle on the lot
        /// </summary>
        public string? LotLocation { get; set; }

        /// <summary>
        /// Gets or sets the list of vehicle features
        /// </summary>
        public List<VehicleFeature> Features { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of vehicle images
        /// </summary>
        public List<VehicleImage> Images { get; set; } = new();        /// <summary>
        /// Gets or sets the list of reconditioning records
        /// </summary>
        public List<ReconditioningRecord> ReconditioningRecords { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of documents
        /// </summary>
        public List<VehicleDocument> Documents { get; set; } = new();

        /// <summary>
        /// Gets or sets the vehicle location ID
        /// </summary>
        public Guid? LocationId { get; set; }

        /// <summary>
        /// Gets or sets the vehicle location
        /// </summary>
        public VehicleLocation? Location { get; set; }

        /// <summary>
        /// Gets or sets the cost details
        /// </summary>
        public VehicleCost? CostDetails { get; set; }

        /// <summary>
        /// Gets or sets the pricing details
        /// </summary>
        public VehiclePricing? PricingDetails { get; set; }

        /// <summary>
        /// Gets or sets the aging information
        /// </summary>
        public VehicleAging? AgingInfo { get; set; }

        /// <summary>
        /// Gets or sets the workflow instances associated with this vehicle
        /// </summary>
        public List<WorkflowInstance> WorkflowInstances { get; set; } = new();

        /// <summary>
        /// Gets or sets the date the vehicle was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who created the vehicle
        /// </summary>
        public string? CreatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date the vehicle was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who last updated the vehicle
        /// </summary>
        public string? UpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the vehicle is deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Gets or sets the date the vehicle was deleted
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Gets or sets the user who deleted the vehicle
        /// </summary>
        public string? DeletedBy { get; set; }
    }

    /// <summary>
    /// Represents the type of vehicle
    /// </summary>
    public enum VehicleType
    {
        New,
        Used,
        CertifiedPreOwned
    }

    /// <summary>
    /// Represents the current status of a vehicle in inventory
    /// </summary>
    public enum VehicleStatus
    {
        InTransit,
        Receiving,
        InStock,
        Reconditioning,
        FrontLine,
        OnHold,
        Sold,
        Delivered,
        Transferred
    }
}
