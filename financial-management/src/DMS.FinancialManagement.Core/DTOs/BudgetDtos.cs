using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DMS.FinancialManagement.Core.DTOs
{
    /// <summary>
    /// DTO for detailed budget responses, including all budget lines
    /// </summary>
    public record BudgetDetailDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int FiscalYear { get; init; }
        public bool IsApproved { get; init; }
        public bool IsRejected { get; init; }
        public string Status { get; init; } = string.Empty;
        public DateTime? ApprovalDate { get; init; }
        public string? ApprovedBy { get; init; }
        public DateTime? RejectionDate { get; init; }
        public string? RejectedBy { get; init; }
        public string? RejectionReason { get; init; }
        public string? Department { get; init; }
        public List<BudgetLineDetailDto> BudgetLines { get; init; } = new();
        public decimal TotalPlannedAmount { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string? UpdatedBy { get; init; }
    }
    
    /// <summary>
    /// DTO for detailed budget line responses
    /// </summary>
    public record BudgetLineDetailDto
    {
        public Guid Id { get; init; }
        public Guid BudgetId { get; init; }
        public Guid AccountId { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public Guid FinancialPeriodId { get; init; }
        public string FinancialPeriodName { get; init; } = string.Empty;
        public decimal PlannedAmount { get; init; }
        public string? Notes { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public string? UpdatedBy { get; init; }
    }
    /// <summary>
    /// DTO for budget responses
    /// </summary>
    public record BudgetDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public int FiscalYear { get; init; }
        public bool IsApproved { get; init; }
        public DateTime? ApprovalDate { get; init; }
        public string? ApprovedBy { get; init; }
        public List<BudgetLineDto> BudgetLines { get; init; } = new();
        public decimal TotalPlannedAmount { get; init; }
        public DateTime CreatedAt { get; init; }
        public string? CreatedBy { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }
    
    /// <summary>
    /// DTO for budget line responses
    /// </summary>
    public record BudgetLineDto
    {
        public Guid Id { get; init; }
        public Guid BudgetId { get; init; }
        public Guid AccountId { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public Guid FinancialPeriodId { get; init; }
        public string FinancialPeriodName { get; init; } = string.Empty;
        public decimal PlannedAmount { get; init; }
        public string? Notes { get; init; }
    }
    
    /// <summary>
    /// DTO for budget summary responses
    /// </summary>
    public record BudgetSummaryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public int FiscalYear { get; init; }
        public bool IsApproved { get; init; }
        public DateTime? ApprovalDate { get; init; }
        public decimal TotalPlannedAmount { get; init; }
        public int BudgetLineCount { get; init; }
    }
    
    /// <summary>
    /// DTO for creating a budget
    /// </summary>
    public record BudgetCreateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; init; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; init; } = string.Empty;
        
        [Required]
        [Range(2000, 2100)]
        public int FiscalYear { get; init; }
        
        [Required]
        public List<BudgetLineCreateDto> BudgetLines { get; init; } = new();
    }
    
    /// <summary>
    /// DTO for creating a budget line
    /// </summary>
    public record BudgetLineCreateDto
    {
        [Required]
        public Guid AccountId { get; init; }
        
        [Required]
        public Guid FinancialPeriodId { get; init; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal PlannedAmount { get; init; }
        
        [StringLength(500)]
        public string? Notes { get; init; }
    }
    
    /// <summary>
    /// DTO for updating a budget
    /// </summary>
    public record BudgetUpdateDto
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string Name { get; init; } = string.Empty;
        
        [StringLength(500)]
        public string Description { get; init; } = string.Empty;
        
        [Required]
        public List<BudgetLineUpdateDto> BudgetLines { get; init; } = new();
    }
    
    /// <summary>
    /// DTO for updating a budget line
    /// </summary>
    public record BudgetLineUpdateDto
    {
        public Guid? Id { get; init; }
        
        [Required]
        public Guid AccountId { get; init; }
        
        [Required]
        public Guid FinancialPeriodId { get; init; }
        
        [Required]
        [Range(0, double.MaxValue)]
        public decimal PlannedAmount { get; init; }
        
        [StringLength(500)]
        public string? Notes { get; init; }
    }
      /// <summary>
    /// DTO for approving a budget
    /// </summary>
    public record BudgetApproveDto
    {
        [Required]
        public DateTime ApprovalDate { get; init; }
        
        [StringLength(500)]
        public string? Comments { get; init; }
    }
    
    /// <summary>
    /// DTO for rejecting a budget
    /// </summary>
    public record BudgetRejectDto
    {
        [Required]
        public DateTime RejectionDate { get; init; }
        
        [Required]
        [StringLength(500, MinimumLength = 10)]
        public string RejectionReason { get; init; } = string.Empty;
    }
      /// <summary>
    /// DTO for budget comparison
    /// </summary>
    public record BudgetComparisonDto
    {
        public Guid BudgetId { get; init; }
        public string BudgetName { get; init; } = string.Empty;
        public int FiscalYear { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public List<BudgetComparisonLineDto> ComparisonLines { get; init; } = new();
        public decimal TotalBudgeted { get; init; }
        public decimal TotalActual { get; init; }
        public decimal Variance { get; init; }
        public decimal VariancePercentage { get; init; }
    }
    
    /// <summary>
    /// DTO for budget comparison report
    /// </summary>
    public record BudgetComparisonReportDto
    {
        public Guid BudgetId { get; init; }
        public string BudgetName { get; init; } = string.Empty;
        public int FiscalYear { get; init; }
        public DateTime GeneratedAt { get; init; }
        public string GeneratedBy { get; init; } = string.Empty;
        public BudgetComparisonDto Comparison { get; init; } = new();
        public List<BudgetComparisonByPeriodDto> PeriodComparisons { get; init; } = new();
        public decimal YearToDateBudgeted { get; init; }
        public decimal YearToDateActual { get; init; }
        public decimal YearToDateVariance { get; init; }
        public decimal YearToDateVariancePercentage { get; init; }
    }
    
    /// <summary>
    /// DTO for budget comparison by period
    /// </summary>
    public record BudgetComparisonByPeriodDto
    {
        public Guid FinancialPeriodId { get; init; }
        public string FinancialPeriodName { get; init; } = string.Empty;
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
        public decimal TotalBudgeted { get; init; }
        public decimal TotalActual { get; init; }
        public decimal Variance { get; init; }
        public decimal VariancePercentage { get; init; }
        public List<BudgetComparisonLineDto> ComparisonLines { get; init; } = new();
    }
    
    /// <summary>
    /// DTO for budget comparison line
    /// </summary>
    public record BudgetComparisonLineDto
    {
        public Guid AccountId { get; init; }
        public string AccountCode { get; init; } = string.Empty;
        public string AccountName { get; init; } = string.Empty;
        public decimal BudgetedAmount { get; init; }
        public decimal ActualAmount { get; init; }
        public decimal Variance { get; init; }
        public decimal VariancePercentage { get; init; }
    }
}
