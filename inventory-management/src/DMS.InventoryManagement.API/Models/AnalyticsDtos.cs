using System;
using System.Collections.Generic;

namespace DMS.InventoryManagement.API.Models
{
    public class AgingAnalyticsRequestDto
    {
        public List<Guid>? LocationIds { get; set; }
        public List<string>? VehicleTypes { get; set; } // New, Used, Certified
        public DateRangeDto? DateRange { get; set; }
    }

    public class DateRangeDto
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class AgingAnalyticsDto
    {
        public Dictionary<string, AgingBracketDto> AgingBrackets { get; set; } = new();
        public List<AgingHeatMapItemDto> HeatMapData { get; set; } = new();
        public List<AgingTrendDto> TrendData { get; set; } = new();
        public List<AgingAlertDto> CriticalAlerts { get; set; } = new();
    }

    public class AgingBracketDto
    {
        public int Total { get; set; }
        public decimal TotalValue { get; set; }
        public List<VehicleSummaryDto> Vehicles { get; set; } = new();
    }

    public class AgingHeatMapItemDto
    {
        public string Category { get; set; } = string.Empty;
        public string Subcategory { get; set; } = string.Empty;
        public int Count { get; set; }
        public int AverageDaysInInventory { get; set; }
        public string ColorCode { get; set; } = string.Empty; // For visual representation
    }

    public class AgingTrendDto
    {
        public DateTime Date { get; set; }
        public double AverageDaysInInventory { get; set; }
        public int TotalVehicles { get; set; }
        public Dictionary<string, int> ByAgingCategory { get; set; } = new();
    }

    public class AgingAlertDto
    {
        public Guid VehicleId { get; set; }
        public string StockNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int DaysInInventory { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal SuggestedPrice { get; set; }
        public string RecommendedAction { get; set; } = string.Empty;
    }

    public class ValuationRequestDto
    {
        public string GroupBy { get; set; } = "make"; // make, model, year, condition, etc.
        public List<Guid>? LocationIds { get; set; }
        public bool IncludeSold { get; set; } = false;
    }

    public class InventoryValuationDto
    {
        public decimal TotalCost { get; set; }
        public decimal TotalRetailValue { get; set; }
        public decimal PotentialGrossProfit { get; set; }
        public Dictionary<string, ValuationGroupDto> ValuationByGroup { get; set; } = new();
        public List<ValuationTrendDto> TrendData { get; set; } = new();
    }

    public class ValuationGroupDto
    {
        public string Name { get; set; } = string.Empty;
        public int VehicleCount { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalRetailValue { get; set; }
        public decimal AverageCost { get; set; }
        public decimal AverageRetailValue { get; set; }
    }

    public class ValuationTrendDto
    {
        public DateTime Date { get; set; }
        public decimal TotalValue { get; set; }
        public int VehicleCount { get; set; }
    }

    public class TurnoverRequestDto
    {
        public string TimePeriod { get; set; } = "month"; // day, week, month, quarter, year
        public List<string>? VehicleTypes { get; set; }
        public List<Guid>? LocationIds { get; set; }
    }

    public class TurnoverMetricsDto
    {
        public double AverageDaysToSell { get; set; }
        public double TurnoverRate { get; set; } // Vehicles sold / Average inventory
        public List<TurnoverBySegmentDto> BySegment { get; set; } = new();
        public List<TurnoverTrendDto> TrendData { get; set; } = new();
    }

    public class TurnoverBySegmentDto
    {
        public string Segment { get; set; } = string.Empty; // Make, Model, Year, etc.
        public double AverageDaysToSell { get; set; }
        public double TurnoverRate { get; set; }
        public int VehiclesSold { get; set; }
        public int CurrentInventory { get; set; }
    }

    public class TurnoverTrendDto
    {
        public DateTime Date { get; set; }
        public double TurnoverRate { get; set; }
        public double AverageDaysToSell { get; set; }
    }

    public class InventoryMixRequestDto
    {
        public string GroupingFactor { get; set; } = "make"; // make, model, bodyStyle, etc.
        public string ComparisonPeriod { get; set; } = "previous-month"; // previous-month, previous-quarter, previous-year
    }

    public class InventoryMixAnalysisDto
    {
        public List<InventoryMixItemDto> CurrentMix { get; set; } = new();
        public List<InventoryMixItemDto> ComparisonMix { get; set; } = new();
        public List<SalesTrendDto> SalesTrendByGroup { get; set; } = new();
        public List<MixRecommendationDto> Recommendations { get; set; } = new();
    }

    public class InventoryMixItemDto
    {
        public string Group { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
        public double PercentageChange { get; set; }
        public double AverageTurnover { get; set; }
    }

    public class SalesTrendDto
    {
        public string Group { get; set; } = string.Empty;
        public List<MonthlyDataPointDto> MonthlySales { get; set; } = new();
    }

    public class MonthlyDataPointDto
    {
        public DateTime Month { get; set; }
        public int Count { get; set; }
    }

    public class MixRecommendationDto
    {
        public string Group { get; set; } = string.Empty;
        public string RecommendationType { get; set; } = string.Empty; // Increase, Decrease, Maintain
        public string Reason { get; set; } = string.Empty;
        public double IdealPercentage { get; set; }
    }

    public class PriceCompetitivenessRequestDto
    {
        public List<Guid>? VehicleIds { get; set; }
        public int MarketRadius { get; set; } = 50; // miles
    }

    public class PriceCompetitivenessDto
    {
        public List<VehicleCompetitivenessDto> VehicleAnalysis { get; set; } = new();
        public MarketSummaryDto MarketSummary { get; set; } = new();
    }

    public class VehicleCompetitivenessDto
    {
        public Guid VehicleId { get; set; }
        public string StockNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal CurrentPrice { get; set; }
        public decimal AverageMarketPrice { get; set; }
        public decimal MinMarketPrice { get; set; }
        public decimal MaxMarketPrice { get; set; }
        public int PercentilePosition { get; set; } // 0-100, where this vehicle sits in the market
        public string PricePosition { get; set; } = string.Empty; // Underpriced, Competitive, Overpriced
        public int DaysOnMarket { get; set; }
        public int CompetitorCount { get; set; }
    }

    public class MarketSummaryDto
    {
        public int TotalCompetitorVehicles { get; set; }
        public double AverageDaysOnMarket { get; set; }
        public Dictionary<string, int> InventoryByCompetitor { get; set; } = new();
        public Dictionary<string, decimal> PriceRangeByTrim { get; set; } = new();
    }

    public class VehicleSummaryDto
    {
        public Guid Id { get; set; }
        public string StockNumber { get; set; } = string.Empty;
        public string Vin { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string Make { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Trim { get; set; } = string.Empty;
        public int DaysInInventory { get; set; }
        public decimal ListPrice { get; set; }
        public decimal Cost { get; set; }
    }
}
