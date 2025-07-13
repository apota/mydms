using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.Models
{
    public class Location
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public Address Address { get; set; } = new Address();
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<PartInventory> Inventories { get; set; } = new List<PartInventory>();
    }
}
