using System;
using System.ComponentModel.DataAnnotations;

namespace DMS.PartsManagement.Core.DTOs
{
    public class CoreTrackingDto
    {
        public Guid Id { get; set; }
        public Guid PartId { get; set; }
        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public string CorePartNumber { get; set; }
        public decimal CoreValue { get; set; }
        public string Status { get; set; }
        public DateTime? SoldDate { get; set; }
        public Guid? SoldReferenceId { get; set; }
        public string SoldReferenceNumber { get; set; }
        public DateTime? ReturnedDate { get; set; }
        public Guid? ReturnReferenceId { get; set; }
        public DateTime? CreditedDate { get; set; }
        public decimal? CreditAmount { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateCoreTrackingDto
    {
        [Required]
        public Guid PartId { get; set; }
        
        [Required]
        public string CorePartNumber { get; set; }
        
        [Required]
        [Range(0.01, 10000)]
        public decimal CoreValue { get; set; }
        
        [Required]
        public Guid SoldReferenceId { get; set; }
        
        [Required]
        public string SoldReferenceNumber { get; set; }
        
        public string Notes { get; set; }
    }

    public class ProcessCoreReturnDto
    {
        [Required]
        public DateTime ReturnedDate { get; set; }
        
        public Guid? ReturnReferenceId { get; set; }
        
        public string Notes { get; set; }
        
        public bool IsDamaged { get; set; }
        
        public string DamageDescription { get; set; }
    }

    public class ApplyCreditDto
    {
        [Required]
        public DateTime CreditedDate { get; set; }
        
        [Required]
        [Range(0.01, 10000)]
        public decimal CreditAmount { get; set; }
        
        public string Notes { get; set; }
    }
}
