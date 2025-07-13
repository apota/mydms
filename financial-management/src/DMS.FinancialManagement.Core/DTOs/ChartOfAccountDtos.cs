using System;
using System.Collections.Generic;
using DMS.FinancialManagement.Core.Models;

namespace DMS.FinancialManagement.Core.DTOs
{
    /// <summary>
    /// DTO for chart of account responses
    /// </summary>
    public record ChartOfAccountDto
    {
        public Guid Id { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public AccountType AccountType { get; init; }
        public Guid? ParentAccountId { get; init; }
        public string ParentAccountName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<ChartOfAccountDto> ChildAccounts { get; init; } = new();
    }
    
    /// <summary>
    /// DTO for creating a chart of account
    /// </summary>
    public record ChartOfAccountCreateDto
    {
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public AccountType AccountType { get; init; }
        public Guid? ParentAccountId { get; init; }
        public string Description { get; init; } = string.Empty;
    }
    
    /// <summary>
    /// DTO for updating a chart of account
    /// </summary>
    public record ChartOfAccountUpdateDto
    {
        public string AccountName { get; init; } = string.Empty;
        public AccountType AccountType { get; init; }
        public Guid? ParentAccountId { get; init; }
        public string Description { get; init; } = string.Empty;
        public bool IsActive { get; init; }
    }
    
    /// <summary>
    /// DTO for returning a simplified chart of account
    /// </summary>
    public record ChartOfAccountSummaryDto
    {
        public Guid Id { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public AccountType AccountType { get; init; }
        public bool IsActive { get; init; }
    }
}
