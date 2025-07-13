using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.SalesManagement.Core.Models
{
    /// <summary>
    /// Represents the status of a deal in the sales process
    /// </summary>
    public enum DealStatus
    {
        Draft,
        Pending,
        Approved,
        Completed,
        Cancelled
    }

    /// <summary>
    /// Represents the type of deal financing
    /// </summary>
    public enum DealType
    {
        Cash,
        Finance,
        Lease
    }

    /// <summary>
    /// Represents a fee in a deal
    /// </summary>
    public class Fee
    {
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Represents a status change in the deal history
    /// </summary>
    public class DealStatusHistory
    {
        public Guid Id { get; set; }
        public DealStatus Status { get; set; }
        public DateTime Date { get; set; }
        public string? UserId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Represents a vehicle sales deal in the system
    /// </summary>
    public class Deal : IAuditableEntity, ISoftDeleteEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public string? SalesRepId { get; set; }
        public DealStatus Status { get; set; }
        public DealType DealType { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid? TradeInVehicleId { get; set; }
        public decimal TradeInValue { get; set; }
        public decimal DownPayment { get; set; }
        public int? FinancingTermMonths { get; set; }
        public decimal? FinancingRate { get; set; }
        public decimal? MonthlyPayment { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public List<Fee> Fees { get; set; } = new List<Fee>();
        public decimal TotalPrice { get; set; }
        public List<DealStatusHistory> StatusHistory { get; set; } = new List<DealStatusHistory>();
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        
        // ISoftDeleteEntity implementation
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Navigation properties
        public virtual ICollection<DealAddOn> AddOns { get; set; } = new List<DealAddOn>();
        public virtual ICollection<Commission> Commissions { get; set; } = new List<Commission>();
        public virtual ICollection<SalesDocument> Documents { get; set; } = new List<SalesDocument>();
    }
}
