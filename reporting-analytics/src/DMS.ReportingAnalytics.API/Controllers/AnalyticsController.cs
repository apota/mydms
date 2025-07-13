using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.API.Services;

namespace DMS.ReportingAnalytics.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly ILogger<AnalyticsController> _logger;
    private readonly IAdvancedAnalyticsService _analyticsService;
    private readonly IDataMartRepository _dataMartRepository;

    public AnalyticsController(
        ILogger<AnalyticsController> logger,
        IAdvancedAnalyticsService analyticsService,
        IDataMartRepository dataMartRepository)
    {
        _logger = logger;
        _analyticsService = analyticsService;
        _dataMartRepository = dataMartRepository;
    }

    /// <summary>
    /// Get key performance indicators
    /// </summary>
    [HttpGet("kpis")]
    public async Task<ActionResult<IEnumerable<KpiResult>>> GetKpis([FromQuery] string department = "all")
    {
        try
        {
            var kpis = await _analyticsService.GetKpisAsync(department);
            return Ok(kpis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving KPIs for {Department}", department);
            return StatusCode(500, "An error occurred while retrieving KPIs");
        }
    }

    /// <summary>
    /// Get trend analysis for a metric
    /// </summary>
    [HttpGet("trends/{metricId}")]
    public async Task<ActionResult<TrendResult>> GetTrend(
        string metricId, 
        [FromQuery] string timeFrame = "month", 
        [FromQuery] string? compareWith = null)
    {
        try
        {
            var trend = await _analyticsService.GetTrendAnalysisAsync(metricId, timeFrame, compareWith);
            return Ok(trend);
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Metric '{metricId}' not found");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trend analysis for metric {MetricId}", metricId);
            return StatusCode(500, "An error occurred while retrieving trend analysis");
        }
    }

    /// <summary>
    /// Generate forecast based on historical data
    /// </summary>
    [HttpPost("forecast")]
    public async Task<ActionResult<ForecastResult>> GenerateForecast(ForecastRequest request)
    {
        try
        {
            var forecast = await _analyticsService.GenerateForecastAsync(request);
            return Ok(forecast);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating forecast for {MetricName}", request.MetricName);
            return StatusCode(500, "An error occurred while generating forecast");
        }
    }

    /// <summary>
    /// Get period-over-period comparisons
    /// </summary>
    [HttpGet("comparisons")]
    public async Task<ActionResult<ComparisonResult>> GetComparisons(
        [FromQuery] string metricGroup, 
        [FromQuery] string currentPeriod, 
        [FromQuery] string previousPeriod)
    {
        try
        {
            var comparison = await _analyticsService.GetPeriodComparisonAsync(metricGroup, currentPeriod, previousPeriod);
            return Ok(comparison);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving period comparison for {MetricGroup}", metricGroup);
            return StatusCode(500, "An error occurred while retrieving period comparison");
        }
    }

    /// <summary>
    /// Execute ad-hoc analytics query
    /// </summary>
    [HttpPost("ad-hoc")]
    public async Task<ActionResult<AdHocQueryResult>> ExecuteAdHocQuery(AdHocQueryRequest request)
    {
        try
        {
            var result = await _analyticsService.ExecuteAdHocQueryAsync(request);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ad-hoc query");
            return StatusCode(500, "An error occurred while executing ad-hoc query");
        }
    }

    /// <summary>
    /// Get automated insights from data
    /// </summary>
    [HttpGet("insights")]
    public async Task<ActionResult<IEnumerable<Insight>>> GetInsights(
        [FromQuery] string area = "all", 
        [FromQuery] int maxResults = 10)
    {
        try
        {
            var insights = await _analyticsService.GetAutomatedInsightsAsync(area, maxResults);
            return Ok(insights);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving automated insights for {Area}", area);
            return StatusCode(500, "An error occurred while retrieving insights");
        }
    }

    /// <summary>
    /// Get inventory optimization recommendations
    /// </summary>
    [HttpGet("recommendations/inventory")]
    public async Task<ActionResult<IEnumerable<InventoryRecommendation>>> GetInventoryRecommendations()
    {
        try
        {
            var recommendations = await _analyticsService.GetInventoryRecommendationsAsync();
            return Ok(recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving inventory recommendations");
            return StatusCode(500, "An error occurred while retrieving inventory recommendations");
        }
    }

    /// <summary>
    /// Get customer churn predictions
    /// </summary>
    [HttpGet("predictions/customer-churn")]
    public async Task<ActionResult<IEnumerable<CustomerChurnPrediction>>> GetCustomerChurnPredictions(
        [FromQuery] double minRiskScore = 0.5)
    {
        try
        {
            var predictions = await _analyticsService.GetCustomerChurnPredictionsAsync(minRiskScore);
            return Ok(predictions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving customer churn predictions");
            return StatusCode(500, "An error occurred while retrieving customer churn predictions");
        }
    }
}

// Request and response models

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
    public List<JsonElement> Rows { get; set; } = new();
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
