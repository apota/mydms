using System;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents the type of relationship between a customer and a vehicle
    /// </summary>
    public enum VehicleRelationshipType
    {
        Owner,
        CoOwner,
        Driver
    }

    /// <summary>
    /// Represents the type of vehicle purchase
    /// </summary>
    public enum VehiclePurchaseType
    {
        New,
        Used,
        CPO
    }

    /// <summary>
    /// Represents the type of financing
    /// </summary>
    public enum FinanceType
    {
        Cash,
        Finance,
        Lease
    }

    /// <summary>
    /// Represents the status of a customer vehicle
    /// </summary>
    public enum CustomerVehicleStatus
    {
        Active,
        Sold,
        Traded
    }

    /// <summary>
    /// Represents a vehicle owned by a customer
    /// </summary>
    public class CustomerVehicle : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public VehicleRelationshipType RelationshipType { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public Guid? PurchaseLocationId { get; set; }
        public VehiclePurchaseType PurchaseType { get; set; }
        public FinanceType FinanceType { get; set; }
        public string? FinanceCompany { get; set; }
        public DateTime? EstimatedPayoffDate { get; set; }
        public CustomerVehicleStatus Status { get; set; }
        public bool IsCurrentVehicle { get; set; }
        public bool IsServicedHere { get; set; }
        public string? PrimaryDriver { get; set; }
        public int? AnnualMileage { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
        // Note: VehicleId would reference a vehicle in the inventory management module
    }
}
