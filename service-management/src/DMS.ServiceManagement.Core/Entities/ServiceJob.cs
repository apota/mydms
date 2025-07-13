using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.ServiceManagement.Core.Entities
{
    public class ServiceJob
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid RepairOrderId { get; set; }
        
        [Required]
        public JobType JobType { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public JobStatus Status { get; set; }
        
        public int Priority { get; set; }
        
        [Column(TypeName = "decimal(5, 2)")]
        public decimal EstimatedHours { get; set; }
        
        [Column(TypeName = "decimal(5, 2)")]
        public decimal ActualHours { get; set; }
        
        public string LaborOperationCode { get; set; }
        
        public string LaborOperationDescription { get; set; }
        
        public Guid? TechnicianId { get; set; }
        
        public DateTime? StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public bool CustomerAuthorized { get; set; }
        
        public DateTime? AuthorizationTime { get; set; }
        
        public Guid? AuthorizedById { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal LaborCharge { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PartsCharge { get; set; }
        
        public string WarrantyType { get; set; }
        
        public WarrantyPayType WarrantyPayType { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("RepairOrderId")]
        public virtual RepairOrder RepairOrder { get; set; }
        
        public virtual ICollection<ServicePart> Parts { get; set; } = new List<ServicePart>();
        
        public virtual ICollection<InspectionResult> InspectionResults { get; set; } = new List<InspectionResult>();
    }
    
    public enum JobType
    {
        Maintenance,
        Repair,
        Recall,
        Warranty,
        Internal
    }
    
    public enum JobStatus
    {
        NotStarted,
        InProgress,
        Completed,
        WaitingParts,
        OnHold
    }
    
    public enum WarrantyPayType
    {
        CustomerPay,
        Warranty,
        Internal,
        Goodwill
    }
    
    public class InspectionResult
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid ServiceJobId { get; set; }
        
        public Guid InspectionPointId { get; set; }
        
        public InspectionResultType Result { get; set; }
        
        public string Notes { get; set; }
        
        public List<string> Images { get; set; } = new List<string>();
        
        [ForeignKey("ServiceJobId")]
        public virtual ServiceJob ServiceJob { get; set; }
    }
    
    public enum InspectionResultType
    {
        Pass,
        Fail,
        Warning,
        NotApplicable
    }
}
