using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.ServiceManagement.Core.Entities
{
    public class ServicePart
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid ServiceJobId { get; set; }
        
        [Required]
        public Guid PartId { get; set; }
        
        public int Quantity { get; set; }
        
        [Required]
        public PartStatus Status { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitCost { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal UnitPrice { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal ExtendedPrice { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DiscountAmount { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TaxAmount { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalAmount { get; set; }
        
        public DateTime? RequestTime { get; set; }
        
        public DateTime? ReceivedTime { get; set; }
        
        public DateTime? InstalledTime { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("ServiceJobId")]
        public virtual ServiceJob ServiceJob { get; set; }
    }
    
    public enum PartStatus
    {
        Requested,
        OnOrder,
        InStock,
        Installed,
        Returned
    }
}
