using System;
using DMS.Shared.Core.Models;

namespace DMS.SalesManagement.Core.Models
{
    /// <summary>
    /// Represents the type of add-on for a deal
    /// </summary>
    public enum AddOnType
    {
        Warranty,
        Insurance,
        Protection,
        Accessory,
        ServiceContract,
        Other
    }

    /// <summary>
    /// Represents an additional product or service added to a deal
    /// </summary>
    public class DealAddOn : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid DealId { get; set; }
        public AddOnType Type { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public string? Term { get; set; }
        public Guid? ProviderId { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        
        // Navigation property
        public virtual Deal? Deal { get; set; }
    }
}
