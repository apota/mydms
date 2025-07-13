using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.Models
{
    public enum MovementClass
    {
        Fast,
        Medium,
        Slow,
        Dead
    }

    public class PartInventory
    {
        public Guid Id { get; set; }
        public Guid PartId { get; set; }
        public Guid LocationId { get; set; }
        public int QuantityOnHand { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantityAllocated { get; set; }
        public int QuantityOnOrder { get; set; }
        public int MinimumLevel { get; set; }
        public int MaximumLevel { get; set; }
        public int ReorderPoint { get; set; }
        public int ReorderQuantity { get; set; }
        public string BinLocation { get; set; } = string.Empty;
        public DateTime? LastCountDate { get; set; }
        public DateTime? LastReceiptDate { get; set; }
        public DateTime? LastIssuedDate { get; set; }
        public MovementClass MovementClass { get; set; } = MovementClass.Medium;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Part? Part { get; set; }
        public virtual Location? Location { get; set; }
    }
}
