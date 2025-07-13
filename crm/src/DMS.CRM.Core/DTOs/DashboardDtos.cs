using System;
using System.Collections.Generic;

namespace DMS.CRM.Core.DTOs
{
    /// <summary>
    /// DTO for dashboard data aggregation
    /// </summary>
    public record DashboardDataDto
    {
        public CustomerMetricsDto CustomerMetrics { get; init; } = new();
        public CampaignMetricsDto CampaignMetrics { get; init; } = new();
        public List<RecentInteractionDto> RecentInteractions { get; init; } = new();
        public SurveyMetricsDto SurveyMetrics { get; init; } = new();
        public SalesReportsDto SalesReports { get; init; } = new();
        public PerformanceMetricsDto PerformanceMetrics { get; init; } = new();
    }

    /// <summary>
    /// Customer-related dashboard metrics
    /// </summary>
    public record CustomerMetricsDto
    {
        public int TotalCustomers { get; init; }
        public int NewCustomersThisMonth { get; init; }
        public decimal RetentionRate { get; init; }
        public bool IsEditable { get; init; } = true;
    }

    /// <summary>
    /// Campaign-related dashboard metrics
    /// </summary>
    public record CampaignMetricsDto
    {
        public int ActiveCampaigns { get; init; }
        public int UpcomingCampaigns { get; init; }
        public int CompletedCampaigns { get; init; }
        public bool IsEditable { get; init; } = true;
    }

    /// <summary>
    /// Recent interaction summary for dashboard
    /// </summary>
    public record RecentInteractionDto
    {
        public Guid Id { get; init; }
        public string Type { get; init; } = string.Empty;
        public string CustomerName { get; init; } = string.Empty;
        public string TimeDisplay { get; init; } = string.Empty;
        public DateTime TimeStamp { get; init; }
    }

    /// <summary>
    /// Survey-related dashboard metrics
    /// </summary>
    public record SurveyMetricsDto
    {
        public decimal SatisfactionScore { get; init; }
        public int ResponsesThisMonth { get; init; }
        public bool IsEditable { get; init; } = true;
    }

    /// <summary>
    /// DTO for updating editable dashboard content
    /// </summary>
    public record DashboardUpdateDto
    {
        public CustomerMetricsUpdateDto? CustomerMetrics { get; init; }
        public DashboardCampaignMetricsUpdateDto? CampaignMetrics { get; init; }
        public SurveyMetricsUpdateDto? SurveyMetrics { get; init; }
    }

    /// <summary>
    /// Editable customer metrics
    /// </summary>
    public record CustomerMetricsUpdateDto
    {
        public decimal? RetentionRate { get; init; }
    }

    /// <summary>
    /// Editable campaign metrics for dashboard
    /// </summary>
    public record DashboardCampaignMetricsUpdateDto
    {
        // Future: Add editable campaign fields
    }

    /// <summary>
    /// Editable survey metrics
    /// </summary>
    public record SurveyMetricsUpdateDto
    {
        public decimal? SatisfactionScore { get; init; }
    }

    /// <summary>
    /// Sales reporting metrics for dashboard
    /// </summary>
    public record SalesReportsDto
    {
        public decimal TotalRevenue { get; init; }
        public decimal MonthlyRevenue { get; init; }
        public int TotalDeals { get; init; }
        public int MonthlyDeals { get; init; }
        public decimal AverageDealValue { get; init; }
        public List<TopSalesPersonDto> TopSalesPersons { get; init; } = new();
        public List<SalesChartDataDto> MonthlySalesChart { get; init; } = new();
    }

    /// <summary>
    /// Performance metrics for dashboard
    /// </summary>
    public record PerformanceMetricsDto
    {
        public decimal ConversionRate { get; init; }
        public decimal LeadToCustomerRate { get; init; }
        public int AverageResponseTime { get; init; } // in hours
        public decimal CustomerLifetimeValue { get; init; }
        public List<MetricTrendDto> Trends { get; init; } = new();
    }

    /// <summary>
    /// Top sales person information
    /// </summary>
    public record TopSalesPersonDto
    {
        public string Name { get; init; } = string.Empty;
        public decimal Revenue { get; init; }
        public int DealsCount { get; init; }
    }

    /// <summary>
    /// Sales chart data point
    /// </summary>
    public record SalesChartDataDto
    {
        public string Month { get; init; } = string.Empty;
        public decimal Revenue { get; init; }
        public int DealsCount { get; init; }
    }

    /// <summary>
    /// Metric trend data
    /// </summary>
    public record MetricTrendDto
    {
        public string MetricName { get; init; } = string.Empty;
        public decimal CurrentValue { get; init; }
        public decimal PreviousValue { get; init; }
        public decimal PercentageChange { get; init; }
        public string TrendDirection { get; init; } = string.Empty; // "up", "down", "stable"
    }
}
