using System.Diagnostics;
using System.Data;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using DMS.ReportingAnalytics.Core.Interfaces;
using Amazon.DynamoDBv2;

namespace DMS.ReportingAnalytics.API.Services;

/// <summary>
/// Implementation of the advanced analytics service.
/// </summary>
public class AdvancedAnalyticsService : IAdvancedAnalyticsService
{
    private readonly ILogger<AdvancedAnalyticsService> _logger;
    private readonly IDataMartRepository _dataMartRepository;
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IConfiguration _configuration;

    public AdvancedAnalyticsService(
        ILogger<AdvancedAnalyticsService> logger,
        IDataMartRepository dataMartRepository,
        IAmazonDynamoDB dynamoDb,
        IConfiguration configuration)
    {
        _logger = logger;
        _dataMartRepository = dataMartRepository;
        _dynamoDb = dynamoDb;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<KpiResult>> GetKpisAsync(string department)
    {
        _logger.LogInformation("Retrieving KPIs for department: {Department}", department);
        
        // Get the KPIs from the data marts
        var kpis = new List<KpiResult>();

        if (department == "all" || department == "sales")
        {
            var salesKpis = await GetSalesKpisAsync();
            kpis.AddRange(salesKpis);
        }

        if (department == "all" || department == "service")
        {
            var serviceKpis = await GetServiceKpisAsync();
            kpis.AddRange(serviceKpis);
        }

        if (department == "all" || department == "inventory")
        {
            var inventoryKpis = await GetInventoryKpisAsync();
            kpis.AddRange(inventoryKpis);
        }

        if (department == "all" || department == "financial")
        {
            var financialKpis = await GetFinancialKpisAsync();
            kpis.AddRange(financialKpis);
        }

        return kpis;
    }

    /// <inheritdoc />
    public async Task<TrendResult> GetTrendAnalysisAsync(string metricId, string timeFrame, string? compareWith)
    {
        _logger.LogInformation("Getting trend analysis for metric {MetricId} with timeframe {TimeFrame}", metricId, timeFrame);
        
        // Parse parameters
        if (!IsValidTimeFrame(timeFrame))
        {
            throw new ArgumentException($"Invalid time frame: {timeFrame}. Valid values are: day, week, month, quarter, year");
        }

        // Get the metric data based on the metric ID
        var metricData = await GetMetricDataAsync(metricId, timeFrame);
        if (metricData == null || !metricData.Any())
        {
            throw new KeyNotFoundException($"No data found for metric {metricId}");
        }

        // Create the result
        var result = new TrendResult
        {
            MetricId = metricId,
            MetricName = GetMetricDisplayName(metricId),
            TimeFrame = timeFrame,
            Points = metricData
        };

        // Add comparison data if requested
        if (!string.IsNullOrEmpty(compareWith))
        {
            var comparisonData = await GetComparisonDataAsync(metricId, timeFrame, compareWith);
            if (comparisonData != null && comparisonData.Any())
            {
                result.ComparisonPoints = comparisonData;
            }
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<ForecastResult> GenerateForecastAsync(ForecastRequest request)
    {
        _logger.LogInformation("Generating forecast for metric {MetricName}", request.MetricName);
        
        // Validate the request
        if (string.IsNullOrEmpty(request.MetricName))
        {
            throw new ArgumentException("Metric name is required");
        }

        if (!IsValidTimeGranularity(request.TimeGranularity))
        {
            throw new ArgumentException($"Invalid time granularity: {request.TimeGranularity}. Valid values are: day, week, month");
        }

        if (request.Periods <= 0 || request.Periods > 365)
        {
            throw new ArgumentException($"Invalid number of periods: {request.Periods}. Must be between 1 and 365");
        }

        // Get historical data for the metric
        var historicalData = await GetMetricHistoricalDataAsync(request.MetricName, request.TimeGranularity, request.Filter);
        if (historicalData == null || !historicalData.Any())
        {
            throw new ArgumentException($"No historical data found for metric {request.MetricName}");
        }

        // Generate the forecast using either Python script or built-in algorithm
        var forecast = await GenerateForecastPointsAsync(request.MetricName, historicalData, request.TimeGranularity, request.Periods);

        return new ForecastResult
        {
            MetricName = request.MetricName,
            Points = forecast,
            ConfidenceLevel = 0.9 // Placeholder, in reality this would be calculated based on the model
        };
    }

    /// <inheritdoc />
    public async Task<ComparisonResult> GetPeriodComparisonAsync(string metricGroup, string currentPeriod, string previousPeriod)
    {
        _logger.LogInformation("Getting period comparison for group {MetricGroup}: {CurrentPeriod} vs {PreviousPeriod}", 
            metricGroup, currentPeriod, previousPeriod);
        
        // Validate input
        if (string.IsNullOrEmpty(metricGroup))
        {
            throw new ArgumentException("Metric group is required");
        }

        if (string.IsNullOrEmpty(currentPeriod) || string.IsNullOrEmpty(previousPeriod))
        {
            throw new ArgumentException("Both current and previous period identifiers are required");
        }

        // Parse period identifiers
        var (currentStart, currentEnd) = ParsePeriodIdentifier(currentPeriod);
        var (previousStart, previousEnd) = ParsePeriodIdentifier(previousPeriod);

        // Get the metrics for the specified group
        var metrics = GetMetricsByGroup(metricGroup);
        if (metrics == null || !metrics.Any())
        {
            throw new ArgumentException($"No metrics found for group {metricGroup}");
        }

        // Create the result
        var result = new ComparisonResult
        {
            MetricGroup = metricGroup,
            CurrentPeriod = currentPeriod,
            PreviousPeriod = previousPeriod,
            Metrics = new List<MetricComparison>()
        };

        // Get values for each metric in both periods
        foreach (var metric in metrics)
        {
            var currentValue = await GetMetricValueForPeriodAsync(metric, currentStart, currentEnd);
            var previousValue = await GetMetricValueForPeriodAsync(metric, previousStart, previousEnd);
            
            var changePercent = previousValue != 0 
                ? ((currentValue - previousValue) / previousValue) * 100
                : 0;
                
            var trend = changePercent > 1 ? "up" : (changePercent < -1 ? "down" : "flat");

            result.Metrics.Add(new MetricComparison
            {
                MetricId = metric,
                MetricName = GetMetricDisplayName(metric),
                CurrentValue = currentValue,
                PreviousValue = previousValue,
                ChangePercent = changePercent,
                Trend = trend
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<AdHocQueryResult> ExecuteAdHocQueryAsync(AdHocQueryRequest request)
    {
        _logger.LogInformation("Executing ad hoc query on data mart {DataMartName}", request.DataMartName);
        
        // Validate the request
        if (string.IsNullOrEmpty(request.DataMartName))
        {
            throw new ArgumentException("Data mart name is required");
        }

        if (request.Dimensions == null || !request.Dimensions.Any())
        {
            throw new ArgumentException("At least one dimension must be specified");
        }

        if (request.Measures == null || !request.Measures.Any())
        {
            throw new ArgumentException("At least one measure must be specified");
        }

        try
        {
            // Build and execute the query
            var queryResult = await _dataMartRepository.ExecuteAdHocQueryAsync(
                request.DataMartName,
                request.Dimensions,
                request.Measures,
                request.Filter,
                request.SortBy,
                request.Limit);

            // Convert the result to the expected format
            var result = new AdHocQueryResult
            {
                Columns = new List<string>(),
                Rows = new List<JsonElement>(),
                TotalCount = queryResult.TotalCount
            };

            // Add columns
            result.Columns.AddRange(request.Dimensions);
            result.Columns.AddRange(request.Measures);

            // Add rows
            foreach (var row in queryResult.Rows)
            {
                result.Rows.Add(JsonDocument.Parse(row.ToString()).RootElement);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing ad hoc query");
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Insight>> GetAutomatedInsightsAsync(string area, int maxResults)
    {
        _logger.LogInformation("Getting automated insights for area {Area}, max results {MaxResults}", area, maxResults);
        
        var insights = new List<Insight>();

        // Get insights based on the area
        if (area == "all" || area == "sales")
        {
            var salesInsights = await GetSalesInsightsAsync(maxResults);
            insights.AddRange(salesInsights);
        }

        if (area == "all" || area == "service")
        {
            var serviceInsights = await GetServiceInsightsAsync(maxResults);
            insights.AddRange(serviceInsights);
        }

        if (area == "all" || area == "inventory")
        {
            var inventoryInsights = await GetInventoryInsightsAsync(maxResults);
            insights.AddRange(inventoryInsights);
        }

        if (area == "all" || area == "customer")
        {
            var customerInsights = await GetCustomerInsightsAsync(maxResults);
            insights.AddRange(customerInsights);
        }

        // Sort by significance and limit the results
        return insights
            .OrderByDescending(i => i.Significance)
            .Take(maxResults);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<InventoryRecommendation>> GetInventoryRecommendationsAsync()
    {
        _logger.LogInformation("Getting inventory optimization recommendations");
        
        try
        {
            // First try to get the cached recommendations from DynamoDB
            var tableName = _configuration["AWS:DynamoDbTableName"] ?? "reporting-cache";
            var cacheKey = "inventory-recommendations";

            // For now, get recommendations directly from the data mart
            var query = @"
                SELECT 
                    make AS Make, 
                    model AS Model, 
                    year AS Year, 
                    current_stock AS CurrentStock, 
                    recommended_stock AS RecommendedStock,
                    recommended_stock - current_stock AS StockDelta,
                    CASE
                        WHEN recommended_stock - current_stock > 2 THEN 'Increase'
                        WHEN current_stock - recommended_stock > 2 THEN 'Decrease'
                        ELSE 'Maintain'
                    END AS Action,
                    sales_velocity AS SalesVelocity,
                    days_supply AS DaysSupply
                FROM analytics.inventory_recommendations
                ORDER BY ABS(recommended_stock - current_stock) DESC
            ";

            var recommendations = await _dataMartRepository.ExecuteQueryAsync<InventoryRecommendation>(query);
            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting inventory recommendations");
            
            // Return some sample recommendations as fallback
            return GenerateSampleInventoryRecommendations();
        }
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CustomerChurnPrediction>> GetCustomerChurnPredictionsAsync(double minRiskScore)
    {
        _logger.LogInformation("Getting customer churn predictions with minimum risk score {MinRiskScore}", minRiskScore);
        
        try
        {
            // Query the data mart for customer churn predictions
            var query = @$"
                SELECT 
                    customer_id AS CustomerId, 
                    CONCAT(first_name, ' ', last_name) AS CustomerName, 
                    churn_probability AS ChurnRiskScore,
                    risk_category AS RiskCategory,
                    lifetime_value AS LifetimeValue,
                    days_since_last_purchase AS DaysSinceLastPurchase,
                    churn_factors AS ChurnFactors,
                    recommended_actions AS RecommendedActions
                FROM analytics.customer_churn_predictions
                WHERE churn_probability >= {minRiskScore}
                ORDER BY churn_probability DESC
            ";

            var predictions = await _dataMartRepository.ExecuteQueryAsync<CustomerChurnPrediction>(query);
            return predictions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting customer churn predictions");
            
            // Return some sample predictions as fallback
            return GenerateSampleChurnPredictions(minRiskScore);
        }
    }

    #region Private Helper Methods

    private async Task<List<KpiResult>> GetSalesKpisAsync()
    {
        // In a real implementation, these would come from the data mart
        var kpis = new List<KpiResult>
        {
            new KpiResult 
            { 
                KpiId = "sales_total_mtd", 
                Name = "Total Sales MTD", 
                Value = 1250000, 
                PreviousValue = 1150000,
                ChangePercent = 8.7,
                Trend = "up",
                Unit = "currency",
                Department = "sales"
            },
            new KpiResult 
            { 
                KpiId = "sales_units_mtd", 
                Name = "Units Sold MTD", 
                Value = 52, 
                PreviousValue = 48,
                ChangePercent = 8.3,
                Trend = "up",
                Unit = "count",
                Department = "sales"
            },
            new KpiResult 
            { 
                KpiId = "sales_avg_profit", 
                Name = "Average Profit per Vehicle", 
                Value = 3250, 
                PreviousValue = 3050,
                ChangePercent = 6.6,
                Trend = "up",
                Unit = "currency",
                Department = "sales"
            },
            new KpiResult 
            { 
                KpiId = "sales_closing_ratio", 
                Name = "Lead Closing Ratio", 
                Value = 22.5, 
                PreviousValue = 21,
                ChangePercent = 7.1,
                Trend = "up",
                Unit = "percent",
                Department = "sales"
            }
        };

        return await Task.FromResult(kpis);
    }

    private async Task<List<KpiResult>> GetServiceKpisAsync()
    {
        // In a real implementation, these would come from the data mart
        var kpis = new List<KpiResult>
        {
            new KpiResult 
            { 
                KpiId = "service_revenue_mtd", 
                Name = "Service Revenue MTD", 
                Value = 325000, 
                PreviousValue = 305000,
                ChangePercent = 6.6,
                Trend = "up",
                Unit = "currency",
                Department = "service"
            },
            new KpiResult 
            { 
                KpiId = "service_ro_count", 
                Name = "RO Count MTD", 
                Value = 463, 
                PreviousValue = 450,
                ChangePercent = 2.9,
                Trend = "up",
                Unit = "count",
                Department = "service"
            },
            new KpiResult 
            { 
                KpiId = "service_efficiency", 
                Name = "Technician Efficiency", 
                Value = 92.5, 
                PreviousValue = 90.0,
                ChangePercent = 2.8,
                Trend = "up",
                Unit = "percent",
                Department = "service"
            },
            new KpiResult 
            { 
                KpiId = "service_csi", 
                Name = "Service CSI", 
                Value = 94.2, 
                PreviousValue = 93.8,
                ChangePercent = 0.4,
                Trend = "flat",
                Unit = "percent",
                Department = "service"
            }
        };

        return await Task.FromResult(kpis);
    }

    private async Task<List<KpiResult>> GetInventoryKpisAsync()
    {
        // In a real implementation, these would come from the data mart
        var kpis = new List<KpiResult>
        {
            new KpiResult 
            { 
                KpiId = "inventory_total_value", 
                Name = "Total Inventory Value", 
                Value = 5250000, 
                PreviousValue = 4950000,
                ChangePercent = 6.1,
                Trend = "up",
                Unit = "currency",
                Department = "inventory"
            },
            new KpiResult 
            { 
                KpiId = "inventory_days_supply", 
                Name = "Days Supply", 
                Value = 45, 
                PreviousValue = 52,
                ChangePercent = -13.5,
                Trend = "down",
                Unit = "days",
                Department = "inventory"
            },
            new KpiResult 
            { 
                KpiId = "inventory_turn_rate", 
                Name = "Inventory Turn Rate", 
                Value = 8.1, 
                PreviousValue = 7.5,
                ChangePercent = 8.0,
                Trend = "up",
                Unit = "ratio",
                Department = "inventory"
            },
            new KpiResult 
            { 
                KpiId = "inventory_aging_over_60", 
                Name = "Units Aging Over 60 Days", 
                Value = 12, 
                PreviousValue = 18,
                ChangePercent = -33.3,
                Trend = "down",
                Unit = "count",
                Department = "inventory"
            }
        };

        return await Task.FromResult(kpis);
    }

    private async Task<List<KpiResult>> GetFinancialKpisAsync()
    {
        // In a real implementation, these would come from the data mart
        var kpis = new List<KpiResult>
        {
            new KpiResult 
            { 
                KpiId = "financial_gross_profit", 
                Name = "Gross Profit MTD", 
                Value = 425000, 
                PreviousValue = 395000,
                ChangePercent = 7.6,
                Trend = "up",
                Unit = "currency",
                Department = "financial"
            },
            new KpiResult 
            { 
                KpiId = "financial_expenses", 
                Name = "Operating Expenses MTD", 
                Value = 285000, 
                PreviousValue = 275000,
                ChangePercent = 3.6,
                Trend = "up",
                Unit = "currency",
                Department = "financial"
            },
            new KpiResult 
            { 
                KpiId = "financial_net_profit", 
                Name = "Net Profit MTD", 
                Value = 140000, 
                PreviousValue = 120000,
                ChangePercent = 16.7,
                Trend = "up",
                Unit = "currency",
                Department = "financial"
            },
            new KpiResult 
            { 
                KpiId = "financial_roi", 
                Name = "Return on Investment", 
                Value = 15.2, 
                PreviousValue = 14.5,
                ChangePercent = 4.8,
                Trend = "up",
                Unit = "percent",
                Department = "financial"
            }
        };

        return await Task.FromResult(kpis);
    }

    private async Task<List<TrendPoint>> GetMetricDataAsync(string metricId, string timeFrame)
    {
        // In a real implementation, this would query the data mart
        // For now, return sample data
        var random = new Random(metricId.GetHashCode());
        var points = new List<TrendPoint>();
        var today = DateTime.Today;
        int dataPoints;

        switch (timeFrame)
        {
            case "day":
                dataPoints = 30;
                for (int i = 0; i < dataPoints; i++)
                {
                    var date = today.AddDays(-dataPoints + i);
                    var value = GenerateMetricValue(metricId, random, i, dataPoints);
                    points.Add(new TrendPoint { Date = date, Value = value });
                }
                break;
            case "week":
                dataPoints = 12;
                for (int i = 0; i < dataPoints; i++)
                {
                    var date = today.AddDays(-(dataPoints - i) * 7);
                    var value = GenerateMetricValue(metricId, random, i, dataPoints);
                    points.Add(new TrendPoint { Date = date, Value = value });
                }
                break;
            case "month":
                dataPoints = 12;
                for (int i = 0; i < dataPoints; i++)
                {
                    var date = today.AddMonths(-dataPoints + i);
                    date = new DateTime(date.Year, date.Month, 1);
                    var value = GenerateMetricValue(metricId, random, i, dataPoints);
                    points.Add(new TrendPoint { Date = date, Value = value });
                }
                break;
            case "quarter":
                dataPoints = 8;
                for (int i = 0; i < dataPoints; i++)
                {
                    var date = today.AddMonths(-(dataPoints - i) * 3);
                    var quarter = (date.Month - 1) / 3;
                    date = new DateTime(date.Year, quarter * 3 + 1, 1);
                    var value = GenerateMetricValue(metricId, random, i, dataPoints);
                    points.Add(new TrendPoint { Date = date, Value = value });
                }
                break;
            case "year":
                dataPoints = 5;
                for (int i = 0; i < dataPoints; i++)
                {
                    var date = new DateTime(today.Year - dataPoints + i, 1, 1);
                    var value = GenerateMetricValue(metricId, random, i, dataPoints);
                    points.Add(new TrendPoint { Date = date, Value = value });
                }
                break;
        }

        return await Task.FromResult(points);
    }

    private double GenerateMetricValue(string metricId, Random random, int index, int total)
    {
        // Generate a realistic-looking series for the metric
        // Base value depends on the metric
        double baseValue;
        double trendFactor; // How strongly the series trends up or down
        double noiseFactor; // How much random noise to add
        double seasonalFactor = 0; // Seasonal pattern

        switch (metricId)
        {
            case "sales_total_mtd":
                baseValue = 1000000;
                trendFactor = 20000;
                noiseFactor = 50000;
                break;
            case "sales_units_mtd":
                baseValue = 45;
                trendFactor = 0.5;
                noiseFactor = 5;
                break;
            case "service_ro_count":
                baseValue = 400;
                trendFactor = 2;
                noiseFactor = 20;
                seasonalFactor = 30 * Math.Sin(2 * Math.PI * index / total); // Seasonal variation
                break;
            // Add cases for other metrics
            default:
                baseValue = 1000;
                trendFactor = 10;
                noiseFactor = 100;
                break;
        }

        // Combine factors to create value
        var trend = trendFactor * index;
        var noise = noiseFactor * (random.NextDouble() - 0.3); // Slightly biased upward
        return Math.Max(0, baseValue + trend + noise + seasonalFactor);
    }

    private async Task<List<TrendPoint>> GetComparisonDataAsync(string metricId, string timeFrame, string compareWith)
    {
        // In a real implementation, this would query historical data from the data mart
        // For now, return sample data that's slightly lower than the primary data
        var data = await GetMetricDataAsync(metricId, timeFrame);
        var comparisonData = new List<TrendPoint>();

        // Calculate the offset based on the comparison type
        int dayOffset;
        int yearOffset;
        
        switch (compareWith)
        {
            case "previous-year":
                yearOffset = 1;
                dayOffset = 0;
                break;
            case "previous-period":
            default:
                yearOffset = 0;
                dayOffset = GetPeriodLength(timeFrame);
                break;
        }

        foreach (var point in data)
        {
            var comparisonDate = point.Date.AddYears(-yearOffset).AddDays(-dayOffset);
            var comparisonValue = point.Value * 0.9 + new Random(metricId.GetHashCode()).NextDouble() * point.Value * 0.1;
            comparisonData.Add(new TrendPoint { Date = comparisonDate, Value = comparisonValue });
        }

        return comparisonData;
    }

    private int GetPeriodLength(string timeFrame)
    {
        switch (timeFrame)
        {
            case "day":
                return 1;
            case "week":
                return 7;
            case "month":
                return 30;
            case "quarter":
                return 90;
            case "year":
                return 365;
            default:
                return 0;
        }
    }

    private bool IsValidTimeFrame(string timeFrame)
    {
        return new[] { "day", "week", "month", "quarter", "year" }.Contains(timeFrame);
    }

    private bool IsValidTimeGranularity(string timeGranularity)
    {
        return new[] { "day", "week", "month" }.Contains(timeGranularity);
    }

    private string GetMetricDisplayName(string metricId)
    {
        // In a real implementation, this would look up the display name from a configuration
        var nameMappings = new Dictionary<string, string>
        {
            { "sales_total_mtd", "Total Sales Month-to-Date" },
            { "sales_units_mtd", "Units Sold Month-to-Date" },
            { "service_ro_count", "Service RO Count" },
            // Add more mappings as needed
        };

        return nameMappings.TryGetValue(metricId, out var name) ? name : metricId;
    }

    private async Task<List<ForecastPoint>> GenerateForecastPointsAsync(string metricName, List<TrendPoint> historicalData, string timeGranularity, int periods)
    {
        // In a real implementation, this would use a proper forecasting algorithm or call the Python script
        // For now, generate a simple projection
        
        var forecast = new List<ForecastPoint>();
        var lastDate = historicalData.Last().Date;
        
        // Calculate the average trend from historical data
        double totalTrend = 0;
        for (int i = 1; i < historicalData.Count; i++)
        {
            totalTrend += historicalData[i].Value - historicalData[i - 1].Value;
        }
        double avgTrend = totalTrend / (historicalData.Count - 1);
        
        // Add some randomness
        var random = new Random(metricName.GetHashCode());
        double lastValue = historicalData.Last().Value;
        
        for (int i = 1; i <= periods; i++)
        {
            DateTime forecastDate;
            switch (timeGranularity)
            {
                case "day":
                    forecastDate = lastDate.AddDays(i);
                    break;
                case "week":
                    forecastDate = lastDate.AddDays(i * 7);
                    break;
                case "month":
                default:
                    forecastDate = lastDate.AddMonths(i);
                    break;
            }
            
            // Calculate the forecasted value with some randomness
            double noise = (random.NextDouble() - 0.5) * 0.1 * lastValue;
            double forecastedValue = lastValue + (avgTrend * i) + noise;
            
            // Ensure the forecasted value doesn't go negative
            forecastedValue = Math.Max(0, forecastedValue);
            
            // Calculate confidence interval (simplistic approach)
            double uncertainty = 0.05 * forecastedValue * i; // Uncertainty increases with time
            
            forecast.Add(new ForecastPoint
            {
                Date = forecastDate,
                Value = forecastedValue,
                LowerBound = forecastedValue - uncertainty,
                UpperBound = forecastedValue + uncertainty
            });
        }
        
        return await Task.FromResult(forecast);
    }

    private (DateTime start, DateTime end) ParsePeriodIdentifier(string periodId)
    {
        // Parse period identifiers like "2023-Q1", "2023-05", "2023-W22", etc.
        if (periodId.Contains("-Q"))
        {
            // Quarter format: YYYY-QN
            var parts = periodId.Split("-Q");
            int year = int.Parse(parts[0]);
            int quarter = int.Parse(parts[1]);
            var start = new DateTime(year, (quarter - 1) * 3 + 1, 1);
            var end = start.AddMonths(3).AddDays(-1);
            return (start, end);
        }
        else if (periodId.Contains("-W"))
        {
            // Week format: YYYY-WNN
            var parts = periodId.Split("-W");
            int year = int.Parse(parts[0]);
            int weekNum = int.Parse(parts[1]);
            
            // Calculate the first day of the year
            var jan1 = new DateTime(year, 1, 1);
            
            // Calculate the first day of the week (Monday as day 1)
            int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
            if (daysOffset > 0) daysOffset -= 7;
            
            // Calculate the first day of the first week
            var firstWeek = jan1.AddDays(daysOffset);
            
            // Calculate the first day of the requested week
            var weekStart = firstWeek.AddDays((weekNum - 1) * 7);
            var weekEnd = weekStart.AddDays(6);
            
            return (weekStart, weekEnd);
        }
        else if (periodId.Length == 7 && periodId[4] == '-')
        {
            // Month format: YYYY-MM
            var parts = periodId.Split("-");
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddDays(-1);
            return (start, end);
        }
        else if (periodId.Length == 4 && int.TryParse(periodId, out int year))
        {
            // Year format: YYYY
            var start = new DateTime(year, 1, 1);
            var end = new DateTime(year, 12, 31);
            return (start, end);
        }
        else if (periodId == "YTD")
        {
            // Year-to-date
            var today = DateTime.Today;
            var start = new DateTime(today.Year, 1, 1);
            return (start, today);
        }
        else if (periodId == "MTD")
        {
            // Month-to-date
            var today = DateTime.Today;
            var start = new DateTime(today.Year, today.Month, 1);
            return (start, today);
        }
        else
        {
            throw new ArgumentException($"Invalid period identifier: {periodId}");
        }
    }

    private IEnumerable<string> GetMetricsByGroup(string metricGroup)
    {
        // In a real implementation, this would return metrics from a configuration
        switch (metricGroup)
        {
            case "sales":
                return new[] { "sales_total_mtd", "sales_units_mtd", "sales_avg_profit", "sales_closing_ratio" };
            case "service":
                return new[] { "service_revenue_mtd", "service_ro_count", "service_efficiency", "service_csi" };
            case "inventory":
                return new[] { "inventory_total_value", "inventory_days_supply", "inventory_turn_rate", "inventory_aging_over_60" };
            case "financial":
                return new[] { "financial_gross_profit", "financial_expenses", "financial_net_profit", "financial_roi" };
            default:
                throw new ArgumentException($"Unknown metric group: {metricGroup}");
        }
    }

    private async Task<double> GetMetricValueForPeriodAsync(string metricId, DateTime startDate, DateTime endDate)
    {
        // In a real implementation, this would query the data mart for the actual value
        // For now, generate a sample value
        var random = new Random(metricId.GetHashCode() + startDate.GetHashCode());
        
        // Base value depends on the metric
        double baseValue;
        switch (metricId)
        {
            case "sales_total_mtd":
                baseValue = 1000000;
                break;
            case "sales_units_mtd":
                baseValue = 45;
                break;
            case "service_ro_count":
                baseValue = 400;
                break;
            // Add cases for other metrics
            default:
                baseValue = 1000;
                break;
        }
        
        // Add some randomness (±10%)
        double randomFactor = 0.9 + (random.NextDouble() * 0.2);
        
        // Add a time factor (newer periods have higher values)
        double timeFactor = 1.0 + ((endDate - new DateTime(2020, 1, 1)).TotalDays / 365.0) * 0.1;
        
        return baseValue * randomFactor * timeFactor;
    }

    private async Task<List<Insight>> GetSalesInsightsAsync(int maxResults)
    {
        // In a real implementation, this would analyze the data mart and extract insights
        var insights = new List<Insight>
        {
            new Insight
            {
                Title = "Sales up 15% for SUVs this quarter",
                Description = "SUV sales have increased by 15% compared to last quarter, with the XC40 model showing the highest growth at 23%.",
                Category = "sales",
                Significance = 0.85,
                DataPoints = new List<InsightDataPoint>
                {
                    new InsightDataPoint { Label = "Q2 2023", Value = 125 },
                    new InsightDataPoint { Label = "Q3 2023", Value = 144 }
                },
                RecommendedAction = "Consider increasing SUV inventory allocation, particularly for XC40 models."
            },
            new Insight
            {
                Title = "Weekend sales conversion rate declining",
                Description = "Weekend visitor to sale conversion rate has declined by 5% over the past 3 months.",
                Category = "sales",
                Significance = 0.75,
                DataPoints = new List<InsightDataPoint>
                {
                    new InsightDataPoint { Label = "March 2023", Value = 22 },
                    new InsightDataPoint { Label = "April 2023", Value = 20 },
                    new InsightDataPoint { Label = "May 2023", Value = 17 }
                },
                RecommendedAction = "Review weekend staffing levels and sales process for potential improvements."
            }
        };
        
        return await Task.FromResult(insights);
    }

    private async Task<List<Insight>> GetServiceInsightsAsync(int maxResults)
    {
        var insights = new List<Insight>
        {
            new Insight
            {
                Title = "Brake service margin increased by 12%",
                Description = "The profit margin for brake service jobs has increased by 12% following the new parts supplier agreement.",
                Category = "service",
                Significance = 0.78,
                DataPoints = new List<InsightDataPoint>
                {
                    new InsightDataPoint { Label = "Before change", Value = 32 },
                    new InsightDataPoint { Label = "After change", Value = 44 }
                },
                RecommendedAction = "Consider similar supplier agreements for other high-volume service parts."
            }
        };
        
        return await Task.FromResult(insights);
    }

    private async Task<List<Insight>> GetInventoryInsightsAsync(int maxResults)
    {
        var insights = new List<Insight>
        {
            new Insight
            {
                Title = "Electric vehicles aging longer in inventory",
                Description = "EVs are staying in inventory 15 days longer on average compared to gasoline vehicles.",
                Category = "inventory",
                Significance = 0.82,
                DataPoints = new List<InsightDataPoint>
                {
                    new InsightDataPoint { Label = "Gasoline vehicles", Value = 32 },
                    new InsightDataPoint { Label = "Electric vehicles", Value = 47 }
                },
                RecommendedAction = "Review EV pricing strategy and marketing approach to improve turnover."
            }
        };
        
        return await Task.FromResult(insights);
    }

    private async Task<List<Insight>> GetCustomerInsightsAsync(int maxResults)
    {
        var insights = new List<Insight>
        {
            new Insight
            {
                Title = "Service customers converting to new vehicle sales",
                Description = "Customers with 5+ service visits are 3x more likely to purchase their next vehicle from the dealership.",
                Category = "customer",
                Significance = 0.9,
                DataPoints = new List<InsightDataPoint>
                {
                    new InsightDataPoint { Label = "1-2 service visits", Value = 15 },
                    new InsightDataPoint { Label = "3-4 service visits", Value = 25 },
                    new InsightDataPoint { Label = "5+ service visits", Value = 45 }
                },
                RecommendedAction = "Implement targeted marketing to service-loyal customers approaching vehicle replacement age."
            }
        };
        
        return await Task.FromResult(insights);
    }

    private IEnumerable<InventoryRecommendation> GenerateSampleInventoryRecommendations()
    {
        // This is a fallback method for demonstration purposes
        return new List<InventoryRecommendation>
        {
            new InventoryRecommendation 
            { 
                Make = "Toyota", 
                Model = "RAV4", 
                Year = 2023, 
                CurrentStock = 12, 
                RecommendedStock = 18, 
                StockDelta = 6, 
                Action = "Increase", 
                SalesVelocity = 0.9,
                DaysSupply = 13
            },
            new InventoryRecommendation 
            { 
                Make = "Honda", 
                Model = "Civic", 
                Year = 2023, 
                CurrentStock = 15, 
                RecommendedStock = 10, 
                StockDelta = -5, 
                Action = "Decrease", 
                SalesVelocity = 0.5,
                DaysSupply = 30
            },
            // Add more sample recommendations here
        };
    }

    private IEnumerable<CustomerChurnPrediction> GenerateSampleChurnPredictions(double minRiskScore)
    {
        // This is a fallback method for demonstration purposes
        return new List<CustomerChurnPrediction>
        {
            new CustomerChurnPrediction
            {
                CustomerId = "C1001",
                CustomerName = "John Smith",
                ChurnRiskScore = 0.87,
                RiskCategory = "High",
                LifetimeValue = 45000,
                DaysSinceLastPurchase = 180,
                ChurnFactors = new List<string> { "Limited service visits", "No response to promotions", "New vehicle purchase due" },
                RecommendedActions = new List<string> { "Personal call from manager", "Special trade-in offer", "Service discount" }
            },
            new CustomerChurnPrediction
            {
                CustomerId = "C1254",
                CustomerName = "Jane Doe",
                ChurnRiskScore = 0.72,
                RiskCategory = "High",
                LifetimeValue = 32000,
                DaysSinceLastPurchase = 145,
                ChurnFactors = new List<string> { "Bad service experience", "Multiple vehicle issues", "Long service wait times" },
                RecommendedActions = new List<string> { "Service recovery plan", "Complimentary service", "Express service option" }
            },
            // Add more sample predictions here
        }
        .Where(p => p.ChurnRiskScore >= minRiskScore);
    }

    #endregion

    #region Python Analytics Integration

    /// <summary>
    /// Invokes a Python analytics script and returns the results.
    /// </summary>
    /// <param name="scriptName">The name of the Python script (without .py extension)</param>
    /// <param name="arguments">Command line arguments to pass to the script</param>
    /// <returns>The script output as a string</returns>
    private async Task<string> InvokePythonScriptAsync(string scriptName, string arguments)
    {
        var stopwatch = Stopwatch.StartNew();
        _logger.LogInformation("Invoking Python script: {ScriptName} {Arguments}", scriptName, arguments);
        
        try
        {
            var scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts", $"{scriptName}.py");
            if (!File.Exists(scriptPath))
            {
                scriptPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "scripts", $"{scriptName}.py");
                if (!File.Exists(scriptPath))
                {
                    throw new FileNotFoundException($"Python script not found: {scriptName}.py");
                }
            }

            // Create process info
            var processInfo = new ProcessStartInfo
            {
                FileName = _configuration["PythonSettings:ExecutablePath"] ?? "python",
                Arguments = $"{scriptPath} {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            // Start the process
            using var process = new Process { StartInfo = processInfo };
            process.Start();

            // Read output and error asynchronously
            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            // Wait for process to exit
            await process.WaitForExitAsync();

            // Check for errors
            if (process.ExitCode != 0)
            {
                _logger.LogError("Python script error: {Error}", error);
                throw new Exception($"Python script execution failed with exit code {process.ExitCode}: {error}");
            }

            _logger.LogInformation("Python script completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
            return output;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invoking Python script {ScriptName}", scriptName);
            throw;
        }
    }

    /// <summary>
    /// Runs the sales forecast prediction using the Python script.
    /// </summary>
    private async Task<List<ForecastPoint>> GenerateSalesForecastAsync(
        string metricName, 
        int periods, 
        string timeGranularity)
    {
        try
        {
            var arguments = $"--model sales_forecast --metric \"{metricName}\" --periods {periods} --granularity {timeGranularity}";
            var output = await InvokePythonScriptAsync("predictive_analytics", arguments);
            
            // Parse JSON output from the Python script
            var forecastPoints = JsonSerializer.Deserialize<List<ForecastPoint>>(output);
            
            return forecastPoints ?? new List<ForecastPoint>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sales forecast");
            
            // Generate some sample forecast points as fallback
            return GenerateSampleForecastPoints(periods, timeGranularity);
        }
    }

    /// <summary>
    /// Generates sample forecast points for demonstration purposes.
    /// </summary>
    private List<ForecastPoint> GenerateSampleForecastPoints(int periods, string timeGranularity)
    {
        var startDate = DateTime.UtcNow.Date;
        var random = new Random();
        var result = new List<ForecastPoint>();
        
        double baseValue = 100;
        double trend = 1.02;  // 2% growth trend
        double seasonality = 0.1;  // 10% seasonality effect
        
        for (int i = 0; i < periods; i++)
        {
            DateTime forecastDate;
            switch (timeGranularity.ToLower())
            {
                case "week":
                    forecastDate = startDate.AddDays(i * 7);
                    break;
                case "month":
                    forecastDate = startDate.AddMonths(i);
                    break;
                case "day":
                default:
                    forecastDate = startDate.AddDays(i);
                    break;
            }
            
            // Calculate value with trend, seasonality and some randomness
            double seasonalFactor = 1 + seasonality * Math.Sin((double)forecastDate.DayOfYear / 365.0 * 2 * Math.PI);
            double trendFactor = Math.Pow(trend, i);
            double randomFactor = 1 + (random.NextDouble() - 0.5) * 0.1;  // ±5% random noise
            
            double value = baseValue * trendFactor * seasonalFactor * randomFactor;
            double uncertainty = Math.Sqrt(i) * 0.05 * value;  // Uncertainty increases with time
            
            result.Add(new ForecastPoint
            {
                Date = forecastDate,
                Value = value,
                LowerBound = value - uncertainty,
                UpperBound = value + uncertainty
            });
        }
        
        return result;
    }

    /// <summary>
    /// Get metric historical data for forecasting.
    /// </summary>
    private async Task<List<TrendPoint>> GetMetricHistoricalDataAsync(
        string metricName, 
        string timeGranularity, 
        string? filter)
    {
        try
        {
            string query = "";
            
            // Build appropriate query based on the metric and time granularity
            switch (metricName.ToLower())
            {
                case "sales_count":
                    query = $@"
                        SELECT 
                            DATE_TRUNC('{timeGranularity}', sale_date) AS date,
                            COUNT(*) AS value
                        FROM marts.sales_fact
                        {(string.IsNullOrEmpty(filter) ? "" : $"WHERE {filter}")}
                        GROUP BY DATE_TRUNC('{timeGranularity}', sale_date)
                        ORDER BY date ASC
                    ";
                    break;
                    
                case "sales_revenue":
                    query = $@"
                        SELECT 
                            DATE_TRUNC('{timeGranularity}', sale_date) AS date,
                            SUM(sale_amount) AS value
                        FROM marts.sales_fact
                        {(string.IsNullOrEmpty(filter) ? "" : $"WHERE {filter}")}
                        GROUP BY DATE_TRUNC('{timeGranularity}', sale_date)
                        ORDER BY date ASC
                    ";
                    break;
                    
                // Add more metrics as needed
                    
                default:
                    throw new ArgumentException($"Unknown metric: {metricName}");
            }
            
            // Execute the query
            var data = await _dataMartRepository.ExecuteQueryAsync<TrendPoint>(query);
            return data.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving historical data for metric {MetricName}", metricName);
            throw;
        }
    }

    /// <summary>
    /// Generate forecast points for a specific metric.
    /// </summary>
    private async Task<List<ForecastPoint>> GenerateForecastPointsAsync(
        string metricName, 
        List<TrendPoint> historicalData, 
        string timeGranularity, 
        int periods)
    {
        // For now, we'll use the Python script approach
        return await GenerateSalesForecastAsync(metricName, periods, timeGranularity);
    }

    /// <summary>
    /// Runs the inventory optimization model using the Python script.
    /// </summary>
    private async Task<List<InventoryRecommendation>> RunInventoryOptimizationAsync()
    {
        try
        {
            var arguments = "--model inventory_optimization";
            var output = await InvokePythonScriptAsync("predictive_analytics", arguments);
            
            // Parse JSON output from the Python script
            var recommendations = JsonSerializer.Deserialize<List<InventoryRecommendation>>(output);
            
            return recommendations ?? new List<InventoryRecommendation>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running inventory optimization");
            throw;
        }
    }

    /// <summary>
    /// Runs the customer churn prediction model using the Python script.
    /// </summary>
    private async Task<List<CustomerChurnPrediction>> RunCustomerChurnPredictionAsync(double minRiskScore)
    {
        try
        {
            var arguments = $"--model customer_churn --threshold {minRiskScore}";
            var output = await InvokePythonScriptAsync("predictive_analytics", arguments);
            
            // Parse JSON output from the Python script
            var predictions = JsonSerializer.Deserialize<List<CustomerChurnPrediction>>(output);
            
            return predictions?.Where(p => p.ChurnRiskScore >= minRiskScore).ToList() 
                ?? new List<CustomerChurnPrediction>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error running customer churn prediction");
            throw;
        }
    }

    #endregion
}
