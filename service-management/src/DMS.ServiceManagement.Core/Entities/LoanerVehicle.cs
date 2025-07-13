using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.ServiceManagement.Core.Entities
{
    public class LoanerVehicle
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid VehicleId { get; set; }
        
        [Required]
        public LoanerStatus Status { get; set; }
        
        public Guid? CurrentCustomerId { get; set; }
        
        public Guid? CurrentRepairOrderId { get; set; }
        
        public DateTime? CheckOutTime { get; set; }
        
        public DateTime? ExpectedReturnTime { get; set; }
        
        public DateTime? ActualReturnTime { get; set; }
        
        public int? CheckOutMileage { get; set; }
        
        public int? CheckInMileage { get; set; }
        
        public string Notes { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("CurrentRepairOrderId")]
        public virtual RepairOrder CurrentRepairOrder { get; set; }
    }
    
    public enum LoanerStatus
    {
        Available,
        Reserved,
        InUse,
        Maintenance
    }
}
