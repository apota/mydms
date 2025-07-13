namespace DMS.ReportingAnalytics.Core.Interfaces;

/// <summary>
/// Interface for the advanced analytics service that provides 
/// business intelligence capabilities.
/// </summary>
public interface IAdvancedAnalyticsService
{
    /// <summary>
    /// Gets key performance indicators for a department or the entire dealership.
    /// </summary>
    /// <param name="department">The department to get KPIs for, or "all" for all departments.</param>
    /// <returns>A collection of KPI results.</returns>
    Task<IEnumerable<KpiResult>> GetKpisAsync(string department);
    
    /// <summary>
    /// Gets trend analysis for a specific metric.
    /// </summary>
    /// <param name="metricId">The ID of the metric to analyze.</param>
    /// <param name="timeFrame">The time frame to analyze (day, week, month, quarter, year).</param>
    /// <param name="compareWith">Optional comparison period (previous-period, previous-year).</param>
    /// <returns>Trend analysis results.</returns>
    Task<TrendResult> GetTrendAnalysisAsync(string metricId, string timeFrame, string? compareWith);
    
    /// <summary>
    /// Generates a forecast based on historical data.
    /// </summary>
    /// <param name="request">The forecast request parameters.</param>
    /// <returns>Forecast results.</returns>
    Task<ForecastResult> GenerateForecastAsync(ForecastRequest request);
    
    /// <summary>
    /// Gets period-over-period comparison for a group of metrics.
    /// </summary>
    /// <param name="metricGroup">The group of metrics to compare.</param>
    /// <param name="currentPeriod">The current period identifier.</param>
    /// <param name="previousPeriod">The previous period identifier.</param>
    /// <returns>Comparison results.</returns>
    Task<ComparisonResult> GetPeriodComparisonAsync(string metricGroup, string currentPeriod, string previousPeriod);
    
    /// <summary>
    /// Executes an ad-hoc analytics query.
    /// </summary>
    /// <param name="request">The query request parameters.</param>
    /// <returns>Query results.</returns>
    Task<AdHocQueryResult> ExecuteAdHocQueryAsync(AdHocQueryRequest request);
    
    /// <summary>
    /// Gets automated insights from data.
    /// </summary>
    /// <param name="area">The area to find insights for, or "all" for all areas.</param>
    /// <param name="maxResults">The maximum number of insights to return.</param>
    /// <returns>A collection of insights.</returns>
    Task<IEnumerable<Insight>> GetAutomatedInsightsAsync(string area, int maxResults);
    
    /// <summary>
    /// Gets inventory optimization recommendations.
    /// </summary>
    /// <returns>A collection of inventory recommendations.</returns>
    Task<IEnumerable<InventoryRecommendation>> GetInventoryRecommendationsAsync();
    
    /// <summary>
    /// Gets customer churn predictions.
    /// </summary>
    /// <param name="minRiskScore">The minimum risk score threshold (0-1) for including customers.</param>
    /// <returns>A collection of customer churn predictions.</returns>
    Task<IEnumerable<CustomerChurnPrediction>> GetCustomerChurnPredictionsAsync(double minRiskScore);
}

// Request and result models (matching those in the controller)
public class ForecastRequest
{
    public string MetricName { get; set; } = string.Empty;
    public string TimeGranularity { get; set; } = "day"; // day, week, month
    public int Periods { get; set; } = 30;
    public string? Filter { get; set; }
}

public class ForecastResult
{
    public string MetricName { get; set; } = string.Empty;
    public List<ForecastPoint> Points { get; set; } = new();
    public double ConfidenceLevel { get; set; }
}

public class ForecastPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public double? LowerBound { get; set; }
    public double? UpperBound { get; set; }
}

public class AdHocQueryRequest
{
    public string DataMartName { get; set; } = string.Empty;
    public List<string> Dimensions { get; set; } = new();
    public List<string> Measures { get; set; } = new();
    public string? Filter { get; set; }
    public List<string>? SortBy { get; set; }
    public int? Limit { get; set; }
}

public class AdHocQueryResult
{
    public List<string> Columns { get; set; } = new();
    public List<System.Text.Json.JsonElement> Rows { get; set; } = new();
    public int TotalCount { get; set; }
}

public class KpiResult
{
    public string KpiId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Value { get; set; }
    public double? PreviousValue { get; set; }
    public double? ChangePercent { get; set; }
    public string Trend { get; set; } = string.Empty; // up, down, flat
    public string Unit { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
}

public class TrendResult
{
    public string MetricId { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public string TimeFrame { get; set; } = string.Empty;
    public List<TrendPoint> Points { get; set; } = new();
    public List<TrendPoint>? ComparisonPoints { get; set; }
}

public class TrendPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
}

public class ComparisonResult
{
    public string MetricGroup { get; set; } = string.Empty;
    public string CurrentPeriod { get; set; } = string.Empty;
    public string PreviousPeriod { get; set; } = string.Empty;
    public List<MetricComparison> Metrics { get; set; } = new();
}

public class MetricComparison
{
    public string MetricId { get; set; } = string.Empty;
    public string MetricName { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double PreviousValue { get; set; }
    public double ChangePercent { get; set; }
    public string Trend { get; set; } = string.Empty; // up, down, flat
}

public class Insight
{
    public string InsightId { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime DiscoveredDate { get; set; } = DateTime.UtcNow;
    public double Significance { get; set; } // 0-1 scale
    public List<InsightDataPoint>? DataPoints { get; set; }
    public string? RecommendedAction { get; set; }
}

public class InsightDataPoint
{
    public string Label { get; set; } = string.Empty;
    public double Value { get; set; }
}

public class InventoryRecommendation
{
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public int CurrentStock { get; set; }
    public int RecommendedStock { get; set; }
    public int StockDelta { get; set; }
    public string Action { get; set; } = string.Empty; // Increase, Decrease, Maintain
    public double SalesVelocity { get; set; }
    public int DaysSupply { get; set; }
}

public class CustomerChurnPrediction
{
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public double ChurnRiskScore { get; set; }
    public string RiskCategory { get; set; } = string.Empty; // High, Medium, Low
    public double LifetimeValue { get; set; }
    public int DaysSinceLastPurchase { get; set; }
    public List<string>? ChurnFactors { get; set; }
    public List<string>? RecommendedActions { get; set; }
}
