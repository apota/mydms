using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.FinancialManagement.Core.Models
{
    /// <summary>
    /// Represents the type of account in the chart of accounts
    /// </summary>
    public enum AccountType
    {
        Asset,
        Liability,
        Equity,
        Revenue,
        Expense
    }
    
    /// <summary>
    /// Represents an account in the chart of accounts
    /// </summary>
    public class ChartOfAccount : IAuditableEntity
    {
        public Guid Id { get; set; }
        public string AccountCode { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public AccountType AccountType { get; set; }
        public Guid? ParentAccountId { get; set; }
        public ChartOfAccount? ParentAccount { get; set; }
        public ICollection<ChartOfAccount> ChildAccounts { get; set; } = new List<ChartOfAccount>();
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        
        // Navigation properties
        public ICollection<JournalLineItem> JournalLineItems { get; set; } = new List<JournalLineItem>();
        public ICollection<BudgetLine> BudgetLines { get; set; } = new List<BudgetLine>();
        
        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
