using System;
using DMS.Shared.Core.Models;

namespace DMS.SalesManagement.Core.Models
{
    /// <summary>
    /// Represents the role of a person receiving commission
    /// </summary>
    public enum CommissionRole
    {
        SalesRep,
        Manager,
        Finance,
        Other
    }

    /// <summary>
    /// Represents the status of a commission payment
    /// </summary>
    public enum CommissionStatus
    {
        Pending,
        Approved,
        Paid,
        Disputed,
        Cancelled
    }

    /// <summary>
    /// Represents a commission payment for a sales transaction
    /// </summary>
    public class Commission : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid DealId { get; set; }
        public string? UserId { get; set; }
        public CommissionRole Role { get; set; }
        public decimal BaseAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? CalculationMethod { get; set; }
        public CommissionStatus Status { get; set; }
        public DateTime? PayoutDate { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Navigation property
        public virtual Deal? Deal { get; set; }
    }
}
