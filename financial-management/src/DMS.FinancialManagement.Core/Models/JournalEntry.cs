using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.FinancialManagement.Core.Models
{
    /// <summary>
    /// Represents a journal entry in the general ledger
    /// </summary>
    public class JournalEntry : IAuditableEntity
    {
        public Guid Id { get; set; }
        public string EntryNumber { get; set; } = string.Empty;
        public DateTime EntryDate { get; set; }
        public DateTime PostingDate { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Reference { get; set; } = string.Empty;
        public bool IsPosted { get; set; }
        public bool IsRecurring { get; set; }
        
        // Navigation properties
        public ICollection<JournalLineItem> LineItems { get; set; } = new List<JournalLineItem>();
        public Guid? FinancialPeriodId { get; set; }
        public FinancialPeriod? FinancialPeriod { get; set; }
        
        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
