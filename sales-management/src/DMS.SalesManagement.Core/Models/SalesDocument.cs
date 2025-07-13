using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.SalesManagement.Core.Models
{
    /// <summary>
    /// Represents the type of sales document
    /// </summary>
    public enum DocumentType
    {
        PurchaseOrder,
        FinanceApplication,
        Title,
        Insurance,
        Contract,
        Disclosure,
        Other
    }

    /// <summary>
    /// Represents the status of a document
    /// </summary>
    public enum DocumentStatus
    {
        Draft,
        Signed,
        Complete,
        Invalid
    }

    /// <summary>
    /// Represents the status of a signature
    /// </summary>
    public enum SignatureStatus
    {
        Pending,
        Signed,
        Rejected
    }

    /// <summary>
    /// Represents a signature required on a document
    /// </summary>
    public class RequiredSignature
    {
        public Guid Id { get; set; }
        public string? Role { get; set; }
        public string? Name { get; set; }
        public SignatureStatus Status { get; set; }
        public DateTime? SignedDate { get; set; }
    }

    /// <summary>
    /// Represents a document associated with a sales transaction
    /// </summary>
    public class SalesDocument : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid DealId { get; set; }
        public DocumentType Type { get; set; }
        public string? Name { get; set; }
        public string? Filename { get; set; }
        public string? Location { get; set; }
        public DocumentStatus Status { get; set; }
        public List<RequiredSignature> RequiredSignatures { get; set; } = new List<RequiredSignature>();
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Navigation property
        public virtual Deal? Deal { get; set; }
    }
}
