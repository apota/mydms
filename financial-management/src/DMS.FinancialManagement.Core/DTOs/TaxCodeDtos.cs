using System;
using System.ComponentModel.DataAnnotations;

namespace DMS.FinancialManagement.Core.DTOs
{
    /// <summary>
    /// DTO for tax code responses
    /// </summary>
    public record TaxCodeDto
    {
        public Guid Id { get; init; }
        public string Code { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Rate { get; init; }
        public DateTime EffectiveDate { get; init; }
        public DateTime? ExpirationDate { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
    
    /// <summary>
    /// DTO for creating a tax code
    /// </summary>
    public record TaxCodeCreateDto
    {
        [Required]
        [StringLength(20, MinimumLength = 1)]
        public string Code { get; init; } = string.Empty;
        
        [Required]
        [StringLength(200)]
        public string Description { get; init; } = string.Empty;
        
        [Required]
        [Range(0, 100)]
        public decimal Rate { get; init; }
        
        [Required]
        public DateTime EffectiveDate { get; init; }
        
        public DateTime? ExpirationDate { get; init; }
        
        public bool IsActive { get; init; } = true;
    }
    
    /// <summary>
    /// DTO for updating a tax code
    /// </summary>
    public record TaxCodeUpdateDto
    {
        [Required]
        [StringLength(200)]
        public string Description { get; init; } = string.Empty;
        
        [Required]
        [Range(0, 100)]
        public decimal Rate { get; init; }
        
        [Required]
        public DateTime EffectiveDate { get; init; }
        
        public DateTime? ExpirationDate { get; init; }
    }
    
    /// <summary>
    /// DTO for deactivating a tax code
    /// </summary>
    public record TaxCodeDeactivateDto
    {
        [Required]
        public DateTime ExpirationDate { get; init; }
    }
}
