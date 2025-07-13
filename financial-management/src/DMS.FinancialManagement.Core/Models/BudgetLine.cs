using System;
using DMS.Shared.Core.Models;

namespace DMS.FinancialManagement.Core.Models
{
    /// <summary>
    /// Represents a line item in a budget
    /// </summary>
    public class BudgetLine : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid BudgetId { get; set; }
        public Budget Budget { get; set; } = null!;
        public Guid AccountId { get; set; }
        public ChartOfAccount Account { get; set; } = null!;
        public Guid FinancialPeriodId { get; set; }
        public FinancialPeriod FinancialPeriod { get; set; } = null!;
        public decimal PlannedAmount { get; set; }
        public string? Notes { get; set; }
        
        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
