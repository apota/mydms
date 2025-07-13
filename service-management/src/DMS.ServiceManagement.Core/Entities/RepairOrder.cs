using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.ServiceManagement.Core.Entities
{
    public class RepairOrder
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(20)]
        public string Number { get; set; }
        
        public Guid? AppointmentId { get; set; }
        
        [Required]
        public Guid CustomerId { get; set; }
        
        [Required]
        public Guid VehicleId { get; set; }
        
        public Guid? AdvisorId { get; set; }
        
        [Required]
        public RepairOrderStatus Status { get; set; }
        
        public int Mileage { get; set; }
        
        [Required]
        public DateTime OpenDate { get; set; }
        
        public DateTime? PromiseDate { get; set; }
        
        public DateTime? CompletionDate { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal LaborRate { get; set; }
        
        [Column(TypeName = "decimal(5, 2)")]
        public decimal TaxRate { get; set; }
        
        public string CustomerNotes { get; set; }
        
        public string InternalNotes { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalEstimatedAmount { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TotalActualAmount { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal LaborTotal { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal PartsTotal { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal DiscountTotal { get; set; }
        
        [Column(TypeName = "decimal(10, 2)")]
        public decimal TaxTotal { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("AppointmentId")]
        public virtual ServiceAppointment Appointment { get; set; }
        
        public virtual ICollection<ServiceJob> ServiceJobs { get; set; } = new List<ServiceJob>();
        
        public virtual ICollection<ServiceInspection> Inspections { get; set; } = new List<ServiceInspection>();
    }
    
    public enum RepairOrderStatus
    {
        Open,
        InProgress,
        OnHold,
        Completed,
        Invoiced,
        Closed
    }
}
