using System;
using System.Collections.Generic;

namespace DMS.PartsManagement.Core.Models
{
    public class PartCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid? ParentCategoryId { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public virtual PartCategory? ParentCategory { get; set; }
        public virtual ICollection<PartCategory> ChildCategories { get; set; } = new List<PartCategory>();
        public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
    }
}
