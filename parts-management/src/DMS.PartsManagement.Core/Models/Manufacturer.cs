using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.Models
{
    public class Manufacturer
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
    }
}
