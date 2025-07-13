using System;
using System.Collections.Generic;

namespace DMS.FinancialManagement.Core.DTOs
{
    /// <summary>
    /// DTO for journal entry balance report request
    /// </summary>
    public record JournalEntryBalanceReportRequestDto
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public Guid? ChartOfAccountId { get; init; }
        public bool IncludeUnposted { get; init; }
        public Guid? FinancialPeriodId { get; init; }
        public Guid? DepartmentId { get; init; }
    }
    
    /// <summary>
    /// DTO for journal entry balance report response
    /// </summary>
    public record JournalEntryBalanceReportResponseDto
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public DateTime GeneratedDate { get; init; }
        public string GeneratedBy { get; init; } = string.Empty;
        public List<JournalEntryBalanceReportLineDto> Lines { get; init; } = new();
        public decimal TotalDebits { get; init; }
        public decimal TotalCredits { get; init; }
        public decimal NetBalance { get; init; }
    }
    
    /// <summary>
    /// DTO for a line in the journal entry balance report
    /// </summary>
    public record JournalEntryBalanceReportLineDto
    {
        public Guid AccountId { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public string AccountTypeName { get; init; } = string.Empty;
        public decimal TotalDebits { get; init; }
        public decimal TotalCredits { get; init; }
        public decimal Balance { get; init; }
        public int EntryCount { get; init; }
    }
}
