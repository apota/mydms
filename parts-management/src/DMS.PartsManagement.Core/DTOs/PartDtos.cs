using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DMS.PartsManagement.Core.Models;

namespace DMS.PartsManagement.Core.DTOs
{
    // DTOs for retrieving part data
    public record PartDto
    {
        public Guid Id { get; init; }
        public string PartNumber { get; init; } = string.Empty;
        public Guid ManufacturerId { get; init; }
        public string ManufacturerName { get; init; } = string.Empty;
        public string ManufacturerPartNumber { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Guid CategoryId { get; init; }
        public string CategoryName { get; init; } = string.Empty;
        public decimal Weight { get; init; }
        public DimensionsDto? Dimensions { get; init; }
        public List<string> ReplacementFor { get; init; } = new List<string>();
        public List<CrossReferenceDto> CrossReferences { get; init; } = new List<CrossReferenceDto>();
        public FitmentDataDto? FitmentData { get; init; }
        public bool IsSerialized { get; init; }
        public bool IsSpecialOrder { get; init; }
        public bool HasCore { get; init; }
        public decimal CoreValue { get; init; }
        public string Notes { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string? SupercededBy { get; init; }
        public List<string> Images { get; init; } = new List<string>();
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
    
    public record PartSummaryDto
    {
        public Guid Id { get; init; }
        public string PartNumber { get; init; } = string.Empty;
        public string ManufacturerName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string CategoryName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public decimal RetailPrice { get; init; }
        public int QuantityOnHand { get; init; }
        public int QuantityAvailable { get; init; }
    }
    
    public record PartDetailDto : PartDto
    {
        public PartPricingDto? Pricing { get; init; }
        public List<PartInventorySummaryDto> Inventories { get; init; } = new List<PartInventorySummaryDto>();
    }
    
    public record DimensionsDto
    {
        public decimal Length { get; init; }
        public decimal Width { get; init; }
        public decimal Height { get; init; }
        public string UnitOfMeasure { get; init; } = "mm";
    }
    
    public record CrossReferenceDto
    {
        public Guid ManufacturerId { get; init; }
        public string ManufacturerName { get; init; } = string.Empty;
        public string PartNumber { get; init; } = string.Empty;
    }
    
    public record FitmentDataDto
    {
        public List<int> Years { get; init; } = new List<int>();
        public List<string> Makes { get; init; } = new List<string>();
        public List<string> Models { get; init; } = new List<string>();
        public List<string> Trims { get; init; } = new List<string>();
        public List<string> Engines { get; init; } = new List<string>();
    }
    
    // DTOs for creating and updating parts
    public record CreatePartDto
    {
        [Required]
        [StringLength(50)]
        public string PartNumber { get; init; } = string.Empty;
        
        [Required]
        public Guid ManufacturerId { get; init; }
        
        [Required]
        [StringLength(50)]
        public string ManufacturerPartNumber { get; init; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Description { get; init; } = string.Empty;
        
        [Required]
        public Guid CategoryId { get; init; }
        
        public decimal Weight { get; init; }
        
        public DimensionsDto? Dimensions { get; init; }
        
        public List<string> ReplacementFor { get; init; } = new List<string>();
        
        public List<CrossReferenceCreateDto> CrossReferences { get; init; } = new List<CrossReferenceCreateDto>();
        
        public FitmentDataDto? FitmentData { get; init; }
        
        public bool IsSerialized { get; init; }
        
        public bool IsSpecialOrder { get; init; }
        
        public bool HasCore { get; init; }
        
        public decimal CoreValue { get; init; }
        
        [StringLength(1000)]
        public string Notes { get; init; } = string.Empty;
        
        public List<string> Images { get; init; } = new List<string>();
    }
    
    public record CrossReferenceCreateDto
    {
        [Required]
        public Guid ManufacturerId { get; init; }
        
        [Required]
        [StringLength(50)]
        public string PartNumber { get; init; } = string.Empty;
    }
    
    public record UpdatePartDto
    {
        [StringLength(50)]
        public string? ManufacturerPartNumber { get; init; }
        
        [StringLength(200)]
        public string? Description { get; init; }
        
        public Guid? CategoryId { get; init; }
        
        public decimal? Weight { get; init; }
        
        public DimensionsDto? Dimensions { get; init; }
        
        public List<string>? ReplacementFor { get; init; }
        
        public List<CrossReferenceCreateDto>? CrossReferences { get; init; }
        
        public FitmentDataDto? FitmentData { get; init; }
        
        public bool? IsSerialized { get; init; }
        
        public bool? IsSpecialOrder { get; init; }
        
        public bool? HasCore { get; init; }
        
        public decimal? CoreValue { get; init; }
        
        [StringLength(1000)]
        public string? Notes { get; init; }
        
        public string? Status { get; init; }
        
        public string? SupercededBy { get; init; }
        
        public List<string>? Images { get; init; }
    }
}
