using System;
using DMS.Shared.Core.Models;

namespace DMS.FinancialManagement.Core.Models
{
    /// <summary>
    /// Represents the payment method used for a payment
    /// </summary>
    public enum PaymentMethod
    {
        Cash,
        Check,
        CreditCard,
        DebitCard,
        BankTransfer,
        ElectronicFundsTransfer,
        Other
    }
    
    /// <summary>
    /// Represents the status of a payment
    /// </summary>
    public enum PaymentStatus
    {
        Pending,
        Cleared,
        Failed,
        Voided
    }
    
    /// <summary>
    /// Represents the type of entity a payment is associated with
    /// </summary>
    public enum EntityType
    {
        Customer,
        Vendor
    }
    
    /// <summary>
    /// Represents a payment in the financial management system
    /// </summary>
    public class Payment : IAuditableEntity
    {
        public Guid Id { get; set; }
        public string PaymentNumber { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string Reference { get; set; } = string.Empty;
        public Guid EntityId { get; set; }
        public EntityType EntityType { get; set; }
        public PaymentStatus Status { get; set; }
        public Guid? InvoiceId { get; set; }
        public Invoice? Invoice { get; set; }
        
        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public string? ProcessedBy { get; set; }
        public DateTime? ProcessedDate { get; set; }
    }
}
