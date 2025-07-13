using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.Models
{
    public enum OrderStatus
    {
        Draft,
        Submitted,
        Partial,
        Complete,
        Cancelled
    }

    public enum OrderType
    {
        Stock,
        Special,
        Emergency
    }

    public class PartOrder
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedReceiveDate { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Draft;
        public OrderType OrderType { get; set; }
        public Guid? RequestorId { get; set; }
        public string ShippingMethod { get; set; } = string.Empty;
        public string? TrackingNumber { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<PartOrderLine> OrderLines { get; set; } = new List<PartOrderLine>();
    }
}
