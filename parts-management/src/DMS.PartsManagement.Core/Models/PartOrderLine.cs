using System;

namespace DMS.PartsManagement.Core.Models
{
    public enum OrderLineStatus
    {
        Ordered,
        Backordered,
        Received,
        Cancelled
    }

    public enum AllocationType
    {
        Stock,
        Service,
        Customer,
        Wholesale
    }

    public class AllocationInfo
    {
        public AllocationType Type { get; set; }
        public Guid? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
    }

    public class PartOrderLine
    {
        public Guid Id { get; set; }
        public Guid PartOrderId { get; set; }
        public Guid PartId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal ExtendedCost { get; set; }
        public OrderLineStatus Status { get; set; } = OrderLineStatus.Ordered;
        public int ReceivedQuantity { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public AllocationInfo? Allocation { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual PartOrder? PartOrder { get; set; }
        public virtual Part? Part { get; set; }
    }
}
