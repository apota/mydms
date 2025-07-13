using System;
using DMS.Shared.Core.Models;

namespace DMS.FinancialManagement.Core.Models
{
    /// <summary>
    /// Represents a line item in a journal entry
    /// </summary>
    public class JournalLineItem : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid JournalEntryId { get; set; }
        public JournalEntry JournalEntry { get; set; } = null!;
        public Guid AccountId { get; set; }
        public ChartOfAccount Account { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public decimal DebitAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? CostCenterId { get; set; }
        
        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
