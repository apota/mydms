using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DMS.PartsManagement.Core.DTOs
{
    public class PartOrderSummaryDto
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime ExpectedReceiveDate { get; set; }
        public string Status { get; set; }
        public string OrderType { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class PartOrderDetailDto : PartOrderSummaryDto
    {
        public Guid RequestorId { get; set; }
        public string RequestorName { get; set; }
        public string ShippingMethod { get; set; }
        public string TrackingNumber { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal TaxAmount { get; set; }
        public string Notes { get; set; }
        public ICollection<PartOrderLineDto> OrderLines { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PartOrderLineDto
    {
        public Guid Id { get; set; }
        public Guid PartId { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public decimal ExtendedCost { get; set; }
        public string Status { get; set; }
        public int ReceivedQuantity { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public AllocationDto Allocation { get; set; }
    }

    public class AllocationDto
    {
        public string Type { get; set; }
        public Guid? ReferenceId { get; set; }
        public string ReferenceType { get; set; }
    }

    public class CreatePartOrderDto
    {
        [Required]
        public Guid SupplierId { get; set; }
        
        [Required]
        public DateTime OrderDate { get; set; }
        
        public DateTime? ExpectedReceiveDate { get; set; }
        
        [Required]
        public string OrderType { get; set; }
        
        public Guid? RequestorId { get; set; }
        
        public string ShippingMethod { get; set; }
        
        public string TrackingNumber { get; set; }
        
        public decimal ShippingCost { get; set; }
        
        public decimal TaxAmount { get; set; }
        
        public string Notes { get; set; }
        
        [Required]
        public List<CreateOrderLineDto> OrderLines { get; set; }
    }

    public class CreateOrderLineDto
    {
        [Required]
        public Guid PartId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        public decimal? UnitCost { get; set; }
        
        public AllocationDto Allocation { get; set; }
    }

    public class UpdatePartOrderDto
    {
        public DateTime? ExpectedReceiveDate { get; set; }
        
        public string ShippingMethod { get; set; }
        
        public string TrackingNumber { get; set; }
        
        public decimal? ShippingCost { get; set; }
        
        public decimal? TaxAmount { get; set; }
        
        public string Notes { get; set; }
        
        public List<UpdateOrderLineDto> OrderLines { get; set; }
    }

    public class UpdateOrderLineDto
    {
        public Guid Id { get; set; }
        
        public int? Quantity { get; set; }
        
        public decimal? UnitCost { get; set; }
        
        public AllocationDto Allocation { get; set; }
    }

    public class PartOrderReceiveDto
    {
        [Required]
        public Guid OrderId { get; set; }
        
        [Required]
        public List<ReceiveOrderLineDto> ReceivedLines { get; set; }
        
        public string Notes { get; set; }
    }

    public class ReceiveOrderLineDto
    {
        [Required]
        public Guid OrderLineId { get; set; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int ReceivedQuantity { get; set; }
        
        public string Notes { get; set; }
    }

    public class PartOrderReceiptDto
    {
        public Guid OrderId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime ReceiveDate { get; set; }
        public string Status { get; set; }
        public List<ReceiptLineDto> ReceivedLines { get; set; }
    }

    public class ReceiptLineDto
    {
        public Guid OrderLineId { get; set; }
        public Guid PartId { get; set; }
        public string PartNumber { get; set; }
        public int ReceivedQuantity { get; set; }
        public int BackorderedQuantity { get; set; }
        public string Status { get; set; }
    }

    public class ReorderRecommendationDto
    {
        public Guid PartId { get; set; }
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public int CurrentStock { get; set; }
        public int ReorderPoint { get; set; }
        public int RecommendedOrderQuantity { get; set; }
        public decimal EstimatedCost { get; set; }
        public Guid PreferredSupplierId { get; set; }
        public string SupplierName { get; set; }
        public int EstimatedLeadTime { get; set; }
    }
}
