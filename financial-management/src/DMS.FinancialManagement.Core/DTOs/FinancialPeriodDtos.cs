using System;
using DMS.FinancialManagement.Core.Models;

namespace DMS.FinancialManagement.Core.DTOs
{
    /// <summary>
    /// DTO for financial period responses
    /// </summary>
    public record FinancialPeriodDto
    {
        public Guid Id { get; init; }
        public int FiscalYear { get; init; }
        public int PeriodNumber { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public bool IsClosed { get; init; }
        public DateTime? ClosedDate { get; init; }
        public string? ClosedBy { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
    
    /// <summary>
    /// DTO for creating a financial period
    /// </summary>
    public record FinancialPeriodCreateDto
    {
        public int FiscalYear { get; init; }
        public int PeriodNumber { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
    
    /// <summary>
    /// DTO for updating a financial period
    /// </summary>
    public record FinancialPeriodUpdateDto
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
    
    /// <summary>
    /// DTO for closing a financial period
    /// </summary>
    public record FinancialPeriodCloseDto
    {
        public DateTime ClosedDate { get; init; }
    }
}
