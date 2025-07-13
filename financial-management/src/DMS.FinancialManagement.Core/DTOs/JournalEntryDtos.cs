using System;
using System.Collections.Generic;
using DMS.FinancialManagement.Core.Models;

namespace DMS.FinancialManagement.Core.DTOs
{
    /// <summary>
    /// DTO for journal entry responses
    /// </summary>
    public record JournalEntryDto
    {
        public Guid Id { get; init; }
        public string EntryNumber { get; init; } = string.Empty;
        public DateTime EntryDate { get; init; }
        public DateTime PostingDate { get; init; }
        public string Description { get; init; } = string.Empty;
        public string Reference { get; init; } = string.Empty;
        public bool IsPosted { get; init; }
        public bool IsRecurring { get; init; }
        public Guid? FinancialPeriodId { get; init; }
        public string FinancialPeriodName { get; init; } = string.Empty;
        public List<JournalLineItemDto> LineItems { get; init; } = new();
        public decimal TotalDebits { get; init; }
        public decimal TotalCredits { get; init; }
        public bool IsBalanced => TotalDebits == TotalCredits;
        public DateTime CreatedAt { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
    
    /// <summary>
    /// DTO for journal line item responses
    /// </summary>
    public record JournalLineItemDto
    {
        public Guid Id { get; init; }
        public Guid AccountId { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal DebitAmount { get; init; }
        public decimal CreditAmount { get; init; }
        public Guid? DepartmentId { get; init; }
        public string? DepartmentName { get; init; }
        public Guid? CostCenterId { get; init; }
        public string? CostCenterName { get; init; }
    }
    
    /// <summary>
    /// DTO for creating a journal entry
    /// </summary>
    public record JournalEntryCreateDto
    {
        public DateTime EntryDate { get; init; }
        public string Description { get; init; } = string.Empty;
        public string Reference { get; init; } = string.Empty;
        public bool IsRecurring { get; init; }
        public Guid? FinancialPeriodId { get; init; }
        public List<JournalLineItemCreateDto> LineItems { get; init; } = new();
    }
    
    /// <summary>
    /// DTO for creating a journal line item
    /// </summary>
    public record JournalLineItemCreateDto
    {
        public Guid AccountId { get; init; }
        public string Description { get; init; } = string.Empty;
        public decimal DebitAmount { get; init; }
        public decimal CreditAmount { get; init; }
        public Guid? DepartmentId { get; init; }
        public Guid? CostCenterId { get; init; }
    }
    
    /// <summary>
    /// DTO for updating a journal entry
    /// </summary>
    public record JournalEntryUpdateDto
    {
        public DateTime EntryDate { get; init; }
        public string Description { get; init; } = string.Empty;
        public string Reference { get; init; } = string.Empty;
        public bool IsRecurring { get; init; }
        public Guid? FinancialPeriodId { get; init; }
        public List<JournalLineItemUpdateDto> LineItems { get; init; } = new();
    }
    
    /// <summary>
    /// DTO for updating a journal line item
    /// </summary>
    public record JournalLineItemUpdateDto
    {
        public Guid? Id { get; init; }
        public Guid AccountId { get; init; }
        public string Description { get; init; } = string.Empty;
        public decimal DebitAmount { get; init; }
        public decimal CreditAmount { get; init; }
        public Guid? DepartmentId { get; init; }
        public Guid? CostCenterId { get; init; }
    }
    
    /// <summary>
    /// DTO for posting a journal entry
    /// </summary>
    public record JournalEntryPostDto
    {
        public DateTime PostingDate { get; init; }
    }
    
    /// <summary>
    /// DTO for reversing a journal entry
    /// </summary>
    public record JournalEntryReverseDto
    {
        public DateTime ReversalDate { get; init; }
        public string Description { get; init; } = string.Empty;
    }
    
    /// <summary>
    /// DTO for returning a simplified journal entry
    /// </summary>
    public record JournalEntrySummaryDto
    {
        public Guid Id { get; init; }
        public string EntryNumber { get; init; } = string.Empty;
        public DateTime EntryDate { get; init; }
        public string Description { get; init; } = string.Empty;
        public string Reference { get; init; } = string.Empty;
        public bool IsPosted { get; init; }
        public decimal TotalAmount { get; init; }
    }
}
