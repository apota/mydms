using System;
using System.Collections.Generic;
using DMS.SalesManagement.Core.Models;

namespace DMS.SalesManagement.Core.DTOs
{
    /// <summary>
    /// DTO for Deal entity
    /// </summary>
    public class DealDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public string? SalesRepId { get; set; }
        public DealStatus Status { get; set; }
        public DealType DealType { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid? TradeInVehicleId { get; set; }
        public decimal TradeInValue { get; set; }
        public decimal DownPayment { get; set; }
        public int? FinancingTermMonths { get; set; }
        public decimal? FinancingRate { get; set; }
        public decimal? MonthlyPayment { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public List<FeeDto> Fees { get; set; } = new List<FeeDto>();
        public decimal TotalPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<DealStatusHistoryDto> StatusHistory { get; set; } = new List<DealStatusHistoryDto>();
        public List<DealAddOnDto> AddOns { get; set; } = new List<DealAddOnDto>();
    }

    /// <summary>
    /// DTO for Fee value object
    /// </summary>
    public class FeeDto
    {
        public Guid Id { get; set; }
        public string? Type { get; set; }
        public decimal Amount { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// DTO for DealStatusHistory entity
    /// </summary>
    public class DealStatusHistoryDto
    {
        public Guid Id { get; set; }
        public DealStatus Status { get; set; }
        public DateTime Date { get; set; }
        public string? UserId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for creating a new deal
    /// </summary>
    public class CreateDealDto
    {
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public string? SalesRepId { get; set; }
        public DealType DealType { get; set; }
        public decimal PurchasePrice { get; set; }
        public Guid? TradeInVehicleId { get; set; }
        public decimal TradeInValue { get; set; }
        public decimal DownPayment { get; set; }
        public int? FinancingTermMonths { get; set; }
        public decimal? FinancingRate { get; set; }
        public decimal TaxRate { get; set; }
        public List<FeeDto>? Fees { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing deal
    /// </summary>
    public class UpdateDealDto
    {
        public string? SalesRepId { get; set; }
        public DealType? DealType { get; set; }
        public decimal? PurchasePrice { get; set; }
        public Guid? TradeInVehicleId { get; set; }
        public decimal? TradeInValue { get; set; }
        public decimal? DownPayment { get; set; }
        public int? FinancingTermMonths { get; set; }
        public decimal? FinancingRate { get; set; }
        public decimal? TaxRate { get; set; }
        public List<FeeDto>? Fees { get; set; }
    }

    /// <summary>
    /// DTO for calculating deal financial details
    /// </summary>
    public class CalculateDealDto
    {
        public decimal PurchasePrice { get; set; }
        public decimal TradeInValue { get; set; }
        public decimal DownPayment { get; set; }
        public decimal TaxRate { get; set; }
        public int? FinancingTermMonths { get; set; }
        public decimal? FinancingRate { get; set; }
        public List<FeeDto> Fees { get; set; } = new List<FeeDto>();
    }

    /// <summary>
    /// DTO for deal calculation results
    /// </summary>
    public class DealCalculationResultDto
    {
        public decimal TaxAmount { get; set; }
        public decimal TotalFees { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal? MonthlyPayment { get; set; }
        public decimal AmountToFinance { get; set; }
    }

    /// <summary>
    /// DTO for deal status update
    /// </summary>
    public class DealStatusUpdateDto
    {
        public DealStatus Status { get; set; }
        public string? Notes { get; set; }
    }
}
