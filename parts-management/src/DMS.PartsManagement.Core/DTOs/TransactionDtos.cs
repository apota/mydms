using System;
using System.ComponentModel.DataAnnotations;

namespace DMS.PartsManagement.Core.DTOs
{
    public class PartTransactionDto
    {
        public Guid Id { get; set; }
        public string TransactionType { get; set; }
        public Guid PartId { get; set; }
        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public int Quantity { get; set; }
        public Guid? SourceLocationId { get; set; }
        public string SourceLocationName { get; set; }
        public Guid? DestinationLocationId { get; set; }
        public string DestinationLocationName { get; set; }
        public string ReferenceType { get; set; }
        public Guid? ReferenceId { get; set; }
        public string ReferenceNumber { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Notes { get; set; }
        public decimal UnitCost { get; set; }
        public decimal ExtendedCost { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? ExtendedPrice { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PartTransactionSummaryDto
    {
        public Guid Id { get; set; }
        public string TransactionType { get; set; }
        public int Quantity { get; set; }
        public string LocationName { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime TransactionDate { get; set; }
    }

    public class IssuePartsDto
    {
        [Required]
        public Guid PartId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Required]
        public Guid LocationId { get; set; }
        
        [Required]
        public string ReferenceType { get; set; }
        
        public Guid? ReferenceId { get; set; }
        
        public string ReferenceNumber { get; set; }
        
        public decimal? UnitPrice { get; set; }
        
        public string Notes { get; set; }
    }

    public class ReturnPartsDto
    {
        [Required]
        public Guid PartId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Required]
        public Guid LocationId { get; set; }
        
        [Required]
        public string ReferenceType { get; set; }
        
        public Guid? ReferenceId { get; set; }
        
        public string ReferenceNumber { get; set; }
        
        public string Notes { get; set; }
    }

    public class AdjustInventoryDto
    {
        [Required]
        public Guid PartId { get; set; }
        
        [Required]
        [Range(-1000000, 1000000)]
        public int QuantityAdjustment { get; set; }
        
        [Required]
        public Guid LocationId { get; set; }
        
        [Required]
        public string AdjustmentReason { get; set; }
        
        public string Notes { get; set; }
    }

    public class TransferPartsDto
    {
        [Required]
        public Guid PartId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; }
        
        [Required]
        public Guid SourceLocationId { get; set; }
        
        [Required]
        public Guid DestinationLocationId { get; set; }
        
        public string Notes { get; set; }
    }
}
