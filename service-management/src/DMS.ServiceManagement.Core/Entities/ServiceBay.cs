using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DMS.ServiceManagement.Core.Entities
{
    public class ServiceBay
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string Name { get; set; }
        
        [Required]
        public BayType Type { get; set; }
        
        [Required]
        public BayStatus Status { get; set; }
        
        public Guid? CurrentJobId { get; set; }
        
        public List<string> Equipment { get; set; } = new List<string>();
        
        public Guid LocationId { get; set; }
        
        // Navigation properties
        public virtual ICollection<ServiceAppointment> Appointments { get; set; } = new List<ServiceAppointment>();
    }
    
    public enum BayType
    {
        Express,
        General,
        Specialized,
        Alignment,
        Diagnostic
    }
    
    public enum BayStatus
    {
        Available,
        Occupied,
        OutOfService
    }
}
