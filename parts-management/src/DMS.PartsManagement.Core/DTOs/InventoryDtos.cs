using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.DTOs
{
    // DTOs for retrieving inventory data
    public record PartInventoryDto
    {
        public Guid Id { get; init; }
        public Guid PartId { get; init; }
        public string PartNumber { get; init; } = string.Empty;
        public string PartDescription { get; init; } = string.Empty;
        public Guid LocationId { get; init; }
        public string LocationName { get; init; } = string.Empty;
        public int QuantityOnHand { get; init; }
        public int QuantityAvailable { get; init; }
        public int QuantityAllocated { get; init; }
        public int QuantityOnOrder { get; init; }
        public int MinimumLevel { get; init; }
        public int MaximumLevel { get; init; }
        public int ReorderPoint { get; init; }
        public int ReorderQuantity { get; init; }
        public string BinLocation { get; init; } = string.Empty;
        public DateTime? LastCountDate { get; init; }
        public DateTime? LastReceiptDate { get; init; }
        public DateTime? LastIssuedDate { get; init; }
        public string MovementClass { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
    
    public record PartInventorySummaryDto
    {
        public Guid Id { get; init; }
        public Guid LocationId { get; init; }
        public string LocationName { get; init; } = string.Empty;
        public int QuantityOnHand { get; init; }
        public int QuantityAvailable { get; init; }
        public string BinLocation { get; init; } = string.Empty;
    }
    
    public record LocationInventorySummaryDto
    {
        public Guid LocationId { get; init; }
        public string LocationName { get; init; } = string.Empty;
        public int TotalParts { get; init; }
        public decimal TotalValue { get; init; }
        public int LowStockCount { get; init; }
        public int OutOfStockCount { get; init; }
    }
    
    // DTOs for creating and updating inventory
    public record CreatePartInventoryDto
    {
        [Required]
        public Guid PartId { get; init; }
        
        [Required]
        public Guid LocationId { get; init; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int QuantityOnHand { get; init; }
        
        [Range(0, int.MaxValue)]
        public int MinimumLevel { get; init; }
        
        [Range(0, int.MaxValue)]
        public int MaximumLevel { get; init; }
        
        [Range(0, int.MaxValue)]
        public int ReorderPoint { get; init; }
        
        [Range(0, int.MaxValue)]
        public int ReorderQuantity { get; init; }
        
        [StringLength(50)]
        public string BinLocation { get; init; } = string.Empty;
    }
    
    public record UpdatePartInventoryDto
    {
        [Range(0, int.MaxValue)]
        public int? QuantityOnHand { get; init; }
        
        [Range(0, int.MaxValue)]
        public int? MinimumLevel { get; init; }
        
        [Range(0, int.MaxValue)]
        public int? MaximumLevel { get; init; }
        
        [Range(0, int.MaxValue)]
        public int? ReorderPoint { get; init; }
        
        [Range(0, int.MaxValue)]
        public int? ReorderQuantity { get; init; }
        
        [StringLength(50)]
        public string? BinLocation { get; init; }
    }
    
    public record InventoryCountDto
    {
        [Required]
        public Guid PartId { get; init; }
        
        [Required]
        public Guid LocationId { get; init; }
        
        [Required]
        [Range(0, int.MaxValue)]
        public int CountedQuantity { get; init; }
        
        [StringLength(500)]
        public string? Notes { get; init; }
    }
    
    public record InventoryAdjustmentDto
    {
        [Required]
        public Guid PartId { get; init; }
        
        [Required]
        public Guid LocationId { get; init; }
        
        [Required]
        public int QuantityChange { get; init; }
        
        [Required]
        [StringLength(500)]
        public string Reason { get; init; } = string.Empty;
    }
}
