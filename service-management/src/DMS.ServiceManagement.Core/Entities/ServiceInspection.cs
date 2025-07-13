using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.ServiceManagement.Core.Entities
{
    public class ServiceInspection
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid RepairOrderId { get; set; }
        
        public Guid? TechnicianId { get; set; }
        
        [Required]
        public InspectionType Type { get; set; }
        
        [Required]
        public InspectionStatus Status { get; set; }
        
        public DateTime? StartTime { get; set; }
        
        public DateTime? EndTime { get; set; }
        
        public List<RecommendedService> RecommendedServices { get; set; } = new List<RecommendedService>();
        
        public List<string> InspectionImages { get; set; } = new List<string>();
        
        public string Notes { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("RepairOrderId")]
        public virtual RepairOrder RepairOrder { get; set; }
    }
    
    public enum InspectionType
    {
        MultiPoint,
        PreDelivery,
        Safety
    }
    
    public enum InspectionStatus
    {
        NotStarted,
        InProgress,
        Completed
    }
    
    public class RecommendedService
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid ServiceInspectionId { get; set; }
        
        [Required]
        public string Description { get; set; }
        
        [Required]
        public ServiceUrgency Urgency { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal EstimatedPrice { get; set; }
        
        [ForeignKey("ServiceInspectionId")]
        public virtual ServiceInspection ServiceInspection { get; set; }
    }
    
    public enum ServiceUrgency
    {
        Critical,
        Soon,
        Future
    }
}
