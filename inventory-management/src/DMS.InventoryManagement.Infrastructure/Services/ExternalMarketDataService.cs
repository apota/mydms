using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using DMS.InventoryManagement.Core.Exceptions;
using DMS.InventoryManagement.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    public class ExternalMarketDataService : IExternalMarketDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ExternalMarketDataService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public ExternalMarketDataService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<ExternalMarketDataService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Get configuration settings
            _apiKey = _configuration["MarketData:ApiKey"];
            _baseUrl = _configuration["MarketData:BaseUrl"];

            // Set default request headers
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        }

        public async Task<MarketData> GetCompetitiveListingsAsync(
            int year,
            string make,
            string model,
            string? trim,
            int? mileage,
            int radius)
        {
            try
            {
                // Build query parameters
                var queryParams = new Dictionary<string, string>
                {
                    ["year"] = year.ToString(),
                    ["make"] = make,
                    ["model"] = model,
                    ["radius"] = radius.ToString()
                };

                if (!string.IsNullOrEmpty(trim))
                    queryParams["trim"] = trim;

                if (mileage.HasValue)
                    queryParams["mileage"] = mileage.Value.ToString();

                var requestUrl = BuildRequestUrl("listings", queryParams);
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error getting competitive listings: {ErrorContent}", errorContent);
                    throw new ExternalServiceException(
                        "Failed to get market data",
                        response.StatusCode,
                        errorContent);
                }

                var listingData = await response.Content.ReadFromJsonAsync<ListingsResponse>();

                if (listingData == null || listingData.Listings == null)
                {
                    return new MarketData
                    {
                        Listings = new List<dynamic>(),
                        TotalListings = 0
                    };
                }

                // Convert to MarketData
                var result = new MarketData
                {
                    Listings = listingData.Listings,
                    TotalListings = listingData.Listings.Count,
                    AverageDaysOnMarket = listingData.AverageDaysOnMarket
                };

                // Calculate price statistics if there are listings
                if (listingData.Listings.Any())
                {
                    var prices = listingData.Listings.Select(l => l.Price).ToList();
                    result.AveragePrice = prices.Average();
                    result.MinPrice = prices.Min();
                    result.MaxPrice = prices.Max();

                    var mileages = listingData.Listings
                        .Where(l => l.Mileage != null)
                        .Select(l => (double)l.Mileage)
                        .ToList();

                    if (mileages.Any())
                    {
                        result.AverageMileage = mileages.Average();
                    }
                }

                return result;
            }
            catch (ExternalServiceException)
            {
                throw; // Re-throw already captured external service exceptions
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving competitive listings");
                throw new ExternalServiceException("Error retrieving market data", ex);
            }
        }

        public async Task<MarketTrendData> GetMarketTrendsAsync(
            int year,
            string make,
            string model,
            int months = 6)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    ["year"] = year.ToString(),
                    ["make"] = make,
                    ["model"] = model,
                    ["months"] = months.ToString()
                };

                var requestUrl = BuildRequestUrl("trends", queryParams);
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error getting market trends: {ErrorContent}", errorContent);
                    throw new ExternalServiceException(
                        "Failed to get market trend data",
                        response.StatusCode,
                        errorContent);
                }

                var trendData = await response.Content.ReadFromJsonAsync<TrendResponse>();

                if (trendData == null)
                {
                    return new MarketTrendData
                    {
                        PriceHistory = new List<PricePoint>(),
                        InventoryHistory = new List<InventoryPoint>(),
                    };
                }

                // Convert to MarketTrendData
                var result = new MarketTrendData
                {
                    PriceHistory = trendData.PriceHistory?.Select(p => new PricePoint
                    {
                        Date = p.Date,
                        AveragePrice = p.AveragePrice,
                        SampleSize = p.SampleSize
                    }).ToList() ?? new List<PricePoint>(),

                    InventoryHistory = trendData.InventoryHistory?.Select(i => new InventoryPoint
                    {
                        Date = i.Date,
                        Count = i.Count
                    }).ToList() ?? new List<InventoryPoint>()
                };

                // Calculate change percentages if data available
                if (result.PriceHistory.Count >= 2)
                {
                    var oldest = result.PriceHistory.OrderBy(p => p.Date).First();
                    var newest = result.PriceHistory.OrderByDescending(p => p.Date).First();

                    if (oldest.AveragePrice > 0)
                    {
                        result.PriceChangePercentage = 
                            (double)((newest.AveragePrice - oldest.AveragePrice) / oldest.AveragePrice * 100);
                    }
                }

                if (result.InventoryHistory.Count >= 2)
                {
                    var oldest = result.InventoryHistory.OrderBy(p => p.Date).First();
                    var newest = result.InventoryHistory.OrderByDescending(p => p.Date).First();

                    if (oldest.Count > 0)
                    {
                        result.InventoryChangePercentage = 
                            (double)((newest.Count - oldest.Count) / (double)oldest.Count * 100);
                    }
                }

                return result;
            }
            catch (ExternalServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving market trends");
                throw new ExternalServiceException("Error retrieving market trend data", ex);
            }
        }

        public async Task<List<PricePoint>> GetDepreciationForecastAsync(
            int year,
            string make,
            string model,
            string trim,
            int mileage,
            int months = 12)
        {
            try
            {
                var queryParams = new Dictionary<string, string>
                {
                    ["year"] = year.ToString(),
                    ["make"] = make,
                    ["model"] = model,
                    ["trim"] = trim,
                    ["mileage"] = mileage.ToString(),
                    ["forecastMonths"] = months.ToString()
                };

                var requestUrl = BuildRequestUrl("depreciation", queryParams);
                var response = await _httpClient.GetAsync(requestUrl);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error getting depreciation forecast: {ErrorContent}", errorContent);
                    throw new ExternalServiceException(
                        "Failed to get depreciation forecast data",
                        response.StatusCode,
                        errorContent);
                }

                var forecastData = await response.Content.ReadFromJsonAsync<DepreciationResponse>();

                if (forecastData == null || forecastData.Forecast == null)
                {
                    return new List<PricePoint>();
                }

                return forecastData.Forecast.Select(f => new PricePoint
                {
                    Date = f.Date,
                    AveragePrice = f.PredictedPrice,
                    SampleSize = f.ConfidenceLevel
                }).ToList();
            }
            catch (ExternalServiceException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving depreciation forecast");
                throw new ExternalServiceException("Error retrieving depreciation forecast data", ex);
            }
        }

        #region Private Helper Methods

        private string BuildRequestUrl(string endpoint, Dictionary<string, string> queryParams)
        {
            var builder = new StringBuilder($"{_baseUrl}/{endpoint}?");

            foreach (var param in queryParams)
            {
                builder.Append($"{Uri.EscapeDataString(param.Key)}={Uri.EscapeDataString(param.Value)}&");
            }

            // Remove the trailing &
            if (builder[builder.Length - 1] == '&')
            {
                builder.Length--;
            }

            return builder.ToString();
        }

        #endregion

        #region Response Classes

        private class ListingsResponse
        {
            public List<dynamic> Listings { get; set; } = new List<dynamic>();
            public double AverageDaysOnMarket { get; set; }
        }

        private class TrendResponse
        {
            public List<PricePointResponse> PriceHistory { get; set; } = new List<PricePointResponse>();
            public List<InventoryPointResponse> InventoryHistory { get; set; } = new List<InventoryPointResponse>();
        }

        private class PricePointResponse
        {
            public DateTime Date { get; set; }
            public decimal AveragePrice { get; set; }
            public int SampleSize { get; set; }
        }

        private class InventoryPointResponse
        {
            public DateTime Date { get; set; }
            public int Count { get; set; }
        }

        private class DepreciationResponse
        {
            public List<ForecastPointResponse> Forecast { get; set; } = new List<ForecastPointResponse>();
        }

        private class ForecastPointResponse
        {
            public DateTime Date { get; set; }
            public decimal PredictedPrice { get; set; }
            public int ConfidenceLevel { get; set; } // 1-100
        }

        #endregion
    }
}
