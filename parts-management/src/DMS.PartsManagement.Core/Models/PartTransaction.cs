using System;

namespace DMS.PartsManagement.Core.Models
{
    public enum TransactionType
    {
        Receipt,
        Issue,
        Return,
        Adjustment,
        Transfer
    }

    public class PartTransaction
    {
        public Guid Id { get; set; }
        public TransactionType TransactionType { get; set; }
        public Guid PartId { get; set; }
        public int Quantity { get; set; }
        public Guid? SourceLocationId { get; set; }
        public Guid? DestinationLocationId { get; set; }
        public string? ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
        public Guid? UserId { get; set; }
        public string Notes { get; set; } = string.Empty;
        public decimal UnitCost { get; set; }
        public decimal ExtendedCost { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal ExtendedPrice { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public virtual Part? Part { get; set; }
        public virtual Location? SourceLocation { get; set; }
        public virtual Location? DestinationLocation { get; set; }
    }
}
