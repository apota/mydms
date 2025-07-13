using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.FinancialManagement.Core.Models
{
    /// <summary>
    /// Represents the status of an invoice
    /// </summary>
    public enum InvoiceStatus
    {
        Draft,
        Sent,
        Paid,
        Overdue,
        Canceled
    }

    /// <summary>
    /// Represents an invoice in the financial management system
    /// </summary>
    public class Invoice : IAuditableEntity
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }        public Guid CustomerId { get; set; }
        public Guid? SalesOrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public InvoiceStatus Status { get; set; }
        public string PaymentTerms { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public Guid? TaxCodeId { get; set; }
        
        // Navigation properties
        public TaxCode? TaxCode { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        
        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
