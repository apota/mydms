using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Exceptions;
using DMS.InventoryManagement.Core.Repositories;

namespace DMS.InventoryManagement.Core.Services
{
    public interface IPricingRecommendationService
    {
        /// <summary>
        /// Get pricing recommendations for a vehicle based on market data and internal metrics
        /// </summary>
        Task<PricingRecommendation> GetPricingRecommendationAsync(string vehicleId);
        
        /// <summary>
        /// Get comparable vehicles from the market for competitive analysis
        /// </summary>
        Task<List<ComparableVehicle>> GetComparableVehiclesAsync(string vehicleId);
        
        /// <summary>
        /// Get historical market data for a specific make/model/year
        /// </summary>
        Task<MarketTrend> GetMarketTrendDataAsync(string make, string model, int year, string trim = null);
    }

    public class PricingRecommendationService : IPricingRecommendationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PricingRecommendationService> _logger;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVehiclePricingRepository _pricingRepository;
        private readonly IVehicleCostRepository _costRepository;
        private readonly string _apiBaseUrl;
        private readonly string _apiKey;

        public PricingRecommendationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<PricingRecommendationService> logger,
            IVehicleRepository vehicleRepository,
            IVehiclePricingRepository pricingRepository,
            IVehicleCostRepository costRepository)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _vehicleRepository = vehicleRepository;
            _pricingRepository = pricingRepository;
            _costRepository = costRepository;
            
            _apiBaseUrl = _configuration["ExternalServices:MarketPricing:ApiUrl"];
            _apiKey = _configuration["ExternalServices:MarketPricing:ApiKey"];
            
            if (string.IsNullOrEmpty(_apiBaseUrl) || string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Market pricing service configuration missing.");
            }
        }

        public async Task<PricingRecommendation> GetPricingRecommendationAsync(string vehicleId)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    throw new ArgumentException($"Vehicle not found with ID {vehicleId}", nameof(vehicleId));
                }
                
                var vehiclePricing = await _pricingRepository.GetByVehicleIdAsync(vehicleId);
                var vehicleCost = await _costRepository.GetByVehicleIdAsync(vehicleId);
                
                // Get market data from external API
                var marketData = await GetMarketDataAsync(vehicle);
                
                // Calculate optimal pricing strategy
                var recommendation = CalculateOptimalPricing(vehicle, vehiclePricing, vehicleCost, marketData);
                
                // Log the recommendation
                _logger.LogInformation(
                    "Pricing recommendation for vehicle {VehicleId}: Current ${CurrentPrice}, Recommended ${RecommendedPrice}",
                    vehicleId, 
                    vehiclePricing?.ListPrice ?? 0, 
                    recommendation.RecommendedPrice);
                
                return recommendation;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                _logger.LogError(ex, "Error generating pricing recommendation for vehicle {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error generating pricing recommendation", ex);
            }
        }

        public async Task<List<ComparableVehicle>> GetComparableVehiclesAsync(string vehicleId)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    throw new ArgumentException($"Vehicle not found with ID {vehicleId}", nameof(vehicleId));
                }
                
                // Prepare request to market pricing API
                var requestUrl = $"{_apiBaseUrl}/comparables";
                var requestData = new
                {
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    Year = vehicle.Year,
                    Trim = vehicle.Trim,
                    Mileage = vehicle.Mileage,
                    ZipCode = "10001", // TODO: Get from dealership location
                    Radius = 100,
                    MaxResults = 10
                };
                
                var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                
                // Add API key to header
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
                
                var response = await _httpClient.PostAsync(requestUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new ExternalServiceException($"Market pricing API returned {response.StatusCode}");
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var comparables = JsonSerializer.Deserialize<List<ComparableVehicle>>(responseContent,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                return comparables ?? new List<ComparableVehicle>();
            }
            catch (Exception ex) when (!(ex is ArgumentException || ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error fetching comparable vehicles for {VehicleId}", vehicleId);
                throw new ExternalServiceException("Error fetching comparable vehicles", ex);
            }
        }

        public async Task<MarketTrend> GetMarketTrendDataAsync(string make, string model, int year, string trim = null)
        {
            try
            {
                // Prepare request to market pricing API
                var requestUrl = $"{_apiBaseUrl}/trends?make={make}&model={model}&year={year}";
                if (!string.IsNullOrEmpty(trim))
                {
                    requestUrl += $"&trim={trim}";
                }
                
                // Add API key to header
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
                
                var response = await _httpClient.GetAsync(requestUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new ExternalServiceException($"Market pricing API returned {response.StatusCode}");
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                var trend = JsonSerializer.Deserialize<MarketTrend>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                
                return trend ?? new MarketTrend
                {
                    Make = make,
                    Model = model,
                    Year = year,
                    Trim = trim
                };
            }
            catch (Exception ex) when (!(ex is ExternalServiceException))
            {
                _logger.LogError(ex, "Error fetching market trend data for {Make} {Model} {Year}", make, model, year);
                throw new ExternalServiceException("Error fetching market trend data", ex);
            }
        }

        private async Task<MarketData> GetMarketDataAsync(Vehicle vehicle)
        {
            try
            {
                // Prepare request to market pricing API
                var requestUrl = $"{_apiBaseUrl}/marketdata";
                var requestData = new
                {
                    Make = vehicle.Make,
                    Model = vehicle.Model,
                    Year = vehicle.Year,
                    Trim = vehicle.Trim,
                    Mileage = vehicle.Mileage,
                    ZipCode = "10001", // TODO: Get from dealership location
                    Radius = 100
                };
                
                var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
                
                // Add API key to header
                _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
                
                var response = await _httpClient.PostAsync(requestUrl, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new ExternalServiceException($"Market pricing API returned {response.StatusCode}");
                }
                
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<MarketData>(responseContent, 
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching market data for {Make} {Model} {Year}", 
                    vehicle.Make, vehicle.Model, vehicle.Year);
                
                // Return default market data if API fails
                return new MarketData
                {
                    AveragePrice = 0,
                    AverageDaysToSell = 0,
                    CompetitiveSetCount = 0,
                    DemandIndex = 0
                };
            }
        }

        private PricingRecommendation CalculateOptimalPricing(
            Vehicle vehicle, 
            VehiclePricing pricing, 
            VehicleCost cost, 
            MarketData marketData)
        {
            // Default values if data is missing
            var currentPrice = pricing?.ListPrice ?? 0;
            var totalCost = cost?.TotalCost ?? 0;
            var marketAverage = marketData?.AveragePrice ?? 0;
            
            // Calculate minimum acceptable price (cost + minimum margin)
            var minimumPriceThreshold = totalCost * 1.05m; // 5% margin minimum
            
            // Calculate competitive price
            decimal recommendedPrice = marketAverage;
            
            // If market data is missing or zero, use cost-plus pricing
            if (marketAverage <= 0)
            {
                recommendedPrice = totalCost * 1.20m; // 20% margin default
            }
            
            // Adjust based on vehicle condition and days in inventory
            if (vehicle.Condition == "Excellent")
            {
                recommendedPrice *= 1.05m; // 5% premium for excellent condition
            }
            else if (vehicle.Condition == "Fair")
            {
                recommendedPrice *= 0.95m; // 5% reduction for fair condition
            }
            
            // Calculate days in inventory
            var daysInInventory = 0;
            if (vehicle.AcquisitionDate.HasValue)
            {
                daysInInventory = (int)(DateTime.UtcNow - vehicle.AcquisitionDate.Value).TotalDays;
            }
            
            // Adjust price based on aging
            if (daysInInventory > 60)
            {
                recommendedPrice *= 0.93m; // 7% reduction for aging inventory
            }
            else if (daysInInventory > 45)
            {
                recommendedPrice *= 0.97m; // 3% reduction for older inventory
            }
            
            // Ensure price doesn't go below minimum threshold
            recommendedPrice = Math.Max(recommendedPrice, minimumPriceThreshold);
            
            // Round to nearest $100
            recommendedPrice = Math.Round(recommendedPrice / 100) * 100;
            
            // Prepare recommendation object
            var recommendation = new PricingRecommendation
            {
                VehicleId = vehicle.Id,
                CurrentPrice = currentPrice,
                RecommendedPrice = recommendedPrice,
                MarketAveragePrice = marketAverage,
                TotalCost = totalCost,
                MinimumPrice = minimumPriceThreshold,
                PriceDifference = recommendedPrice - currentPrice,
                PercentageDifference = currentPrice > 0 ? ((recommendedPrice - currentPrice) / currentPrice) * 100 : 0,
                RecommendationReason = GenerateRecommendationReason(vehicle, pricing, marketAverage, daysInInventory, recommendedPrice),
                MarketCompetition = marketData?.CompetitiveSetCount ?? 0,
                MarketDemand = ConvertDemandIndexToText(marketData?.DemandIndex ?? 0),
                AverageDaysToSell = marketData?.AverageDaysToSell ?? 0,
                ConfidenceLevel = CalculateConfidenceLevel(marketData)
            };
            
            return recommendation;
        }
        
        private string GenerateRecommendationReason(
            Vehicle vehicle, 
            VehiclePricing pricing, 
            decimal marketAverage, 
            int daysInInventory,
            decimal recommendedPrice)
        {
            var currentPrice = pricing?.ListPrice ?? 0;
            
            if (Math.Abs(recommendedPrice - currentPrice) < 100)
            {
                return "Your price is aligned with the current market value.";
            }
            
            if (recommendedPrice > currentPrice)
            {
                return $"The vehicle is currently priced {Math.Abs(Math.Round((recommendedPrice - currentPrice) / 100) * 100):C0} below market average. " +
                       $"Consider a price increase to maximize profit while remaining competitive.";
            }
            
            if (daysInInventory > 60)
            {
                return $"This vehicle has been in inventory for {daysInInventory} days. Price reduction recommended to accelerate sale.";
            }
            
            return $"Current price is {Math.Abs(Math.Round((currentPrice - recommendedPrice) / 100) * 100):C0} above market average. " +
                   $"Consider adjusting price to improve competitiveness.";
        }
        
        private string ConvertDemandIndexToText(decimal demandIndex)
        {
            if (demandIndex >= 8)
                return "Very High";
            if (demandIndex >= 6)
                return "High";
            if (demandIndex >= 4)
                return "Medium";
            if (demandIndex >= 2)
                return "Low";
            return "Very Low";
        }
        
        private string CalculateConfidenceLevel(MarketData marketData)
        {
            if (marketData == null || marketData.CompetitiveSetCount < 3)
                return "Low";
            
            if (marketData.CompetitiveSetCount < 10)
                return "Medium";
            
            return "High";
        }
    }

    public class PricingRecommendation
    {
        public string VehicleId { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal RecommendedPrice { get; set; }
        public decimal MarketAveragePrice { get; set; }
        public decimal TotalCost { get; set; }
        public decimal MinimumPrice { get; set; }
        public decimal PriceDifference { get; set; }
        public decimal PercentageDifference { get; set; }
        public string RecommendationReason { get; set; }
        public int MarketCompetition { get; set; }
        public string MarketDemand { get; set; }
        public int AverageDaysToSell { get; set; }
        public string ConfidenceLevel { get; set; }
    }

    public class MarketData
    {
        public decimal AveragePrice { get; set; }
        public int AverageDaysToSell { get; set; }
        public int CompetitiveSetCount { get; set; }
        public decimal DemandIndex { get; set; }
        public decimal PriceHighRange { get; set; }
        public decimal PriceLowRange { get; set; }
    }

    public class ComparableVehicle
    {
        public string Source { get; set; }
        public int Year { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public string Trim { get; set; }
        public string MakeModel => $"{Make} {Model}";
        public int Mileage { get; set; }
        public decimal Price { get; set; }
        public int DaysListed { get; set; }
        public int Distance { get; set; }
        public string ExteriorColor { get; set; }
        public string InteriorColor { get; set; }
        public string Vin { get; set; }
        public string ListingUrl { get; set; }
    }

    public class MarketTrend
    {
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Trim { get; set; }
        public List<MarketTrendPoint> TrendData { get; set; } = new List<MarketTrendPoint>();
        public decimal ThirtyDayPriceChange { get; set; }
        public decimal NinetyDayPriceChange { get; set; }
        public string MarketDirection => 
            NinetyDayPriceChange > 0 ? "Up" : 
            NinetyDayPriceChange < 0 ? "Down" : "Stable";
    }

    public class MarketTrendPoint
    {
        public DateTime Date { get; set; }
        public decimal AveragePrice { get; set; }
        public int InventoryCount { get; set; }
        public int AverageDaysToSell { get; set; }
    }
}
