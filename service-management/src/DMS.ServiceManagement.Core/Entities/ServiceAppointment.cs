using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DMS.ServiceManagement.Core.Entities
{
    public class ServiceAppointment
    {
        [Key]
        public Guid Id { get; set; }
        
        public Guid CustomerId { get; set; }
        
        public Guid VehicleId { get; set; }
        
        [Required]
        public AppointmentType AppointmentType { get; set; }
        
        [Required]
        public AppointmentStatus Status { get; set; }
        
        [Required]
        public DateTime ScheduledStartTime { get; set; }
        
        [Required]
        public DateTime ScheduledEndTime { get; set; }
        
        public DateTime? ActualStartTime { get; set; }
        
        public DateTime? ActualEndTime { get; set; }
        
        public Guid? AdvisorId { get; set; }
        
        public Guid? BayId { get; set; }
        
        public TransportationType TransportationType { get; set; }
        
        public string CustomerConcerns { get; set; }
        
        public string AppointmentNotes { get; set; }
        
        public ConfirmationStatus ConfirmationStatus { get; set; }
        
        public DateTime? ConfirmationTime { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
        
        [Required]
        public DateTime UpdatedAt { get; set; }
        
        // Navigation properties
        [ForeignKey("BayId")]
        public virtual ServiceBay Bay { get; set; }
        
        public virtual RepairOrder RepairOrder { get; set; }
        
        // Foreign key relationships would be set up in DbContext
    }
    
    public enum AppointmentType
    {
        Maintenance,
        Repair,
        Recall,
        Diagnostic,
        Inspection,
        Other
    }
    
    public enum AppointmentStatus
    {
        Scheduled,
        Confirmed,
        InProgress,
        Completed,
        Canceled
    }
    
    public enum TransportationType
    {
        Self,
        Pickup,
        Loaner,
        Shuttle
    }
    
    public enum ConfirmationStatus
    {
        Pending,
        Confirmed,
        Declined
    }
}
