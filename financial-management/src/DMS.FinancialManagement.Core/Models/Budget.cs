using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.FinancialManagement.Core.Models
{
    /// <summary>
    /// Represents a budget in the financial management system
    /// </summary>
    public class Budget : IAuditableEntity
    {
        public Guid Id { get; set; }
        public int FiscalYear { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
        public string? ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        
        // Navigation properties
        public ICollection<BudgetLine> BudgetLines { get; set; } = new List<BudgetLine>();
        
        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
