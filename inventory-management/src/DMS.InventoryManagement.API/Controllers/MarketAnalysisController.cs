using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/inventory")]
    [Authorize]
    public class MarketAnalysisController : ControllerBase
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVehiclePricingRepository _pricingRepository;
        private readonly IExternalMarketDataService _marketDataService;
        private readonly IMarketPricingService _marketPricingService;
        private readonly ILogger<MarketAnalysisController> _logger;

        public MarketAnalysisController(
            IVehicleRepository vehicleRepository,
            IVehiclePricingRepository pricingRepository,
            IExternalMarketDataService marketDataService,
            IMarketPricingService marketPricingService,
            ILogger<MarketAnalysisController> logger)
        {
            _vehicleRepository = vehicleRepository ?? throw new ArgumentNullException(nameof(vehicleRepository));
            _pricingRepository = pricingRepository ?? throw new ArgumentNullException(nameof(pricingRepository));
            _marketDataService = marketDataService ?? throw new ArgumentNullException(nameof(marketDataService));
            _marketPricingService = marketPricingService ?? throw new ArgumentNullException(nameof(marketPricingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Gets comprehensive market analysis for a specific vehicle
        /// </summary>
        [HttpGet("market-analysis/{vehicleId}")]
        public async Task<ActionResult<MarketAnalysisDto>> GetMarketAnalysis(Guid vehicleId)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    return NotFound($"Vehicle with ID {vehicleId} not found");
                }

                var pricing = await _pricingRepository.GetByVehicleIdAsync(vehicleId);
                if (pricing == null)
                {
                    return NotFound($"Pricing information for vehicle {vehicleId} not found");
                }

                // Get competitive listings from market data service
                var marketData = await _marketDataService.GetCompetitiveListingsAsync(
                    vehicle.Year,
                    vehicle.Make,
                    vehicle.Model,
                    vehicle.Trim,
                    vehicle.Mileage,
                    50); // 50 mile radius

                // Get market trends
                var trendData = await _marketDataService.GetMarketTrendsAsync(
                    vehicle.Year,
                    vehicle.Make,
                    vehicle.Model,
                    6); // Last 6 months

                // Get price recommendations
                var marketValue = await _marketPricingService.GetMarketValueAsync(
                    vehicle.Year,
                    vehicle.Make,
                    vehicle.Model,
                    vehicle.Trim,
                    vehicle.Mileage);

                var priceRecommendation = await _marketPricingService.GetPriceRecommendationAsync(vehicleId);

                // Build the comprehensive market analysis response
                var response = new MarketAnalysisDto
                {
                    MarketComparison = new MarketComparisonDto
                    {
                        VehiclePrice = pricing.InternetPrice,
                        MarketAverage = marketData.AveragePrice,
                        PriceDifference = pricing.InternetPrice - marketData.AveragePrice,
                        PercentilePlacement = CalculatePercentile(pricing.InternetPrice, marketData)
                    },
                    CompetitiveListings = marketData.Listings.Select(l => new CompetitiveListingDto
                    {
                        Name = l.Source,
                        Price = l.Price,
                        Distance = l.Distance,
                        IsYours = false
                    }).Take(5).ToList(),
                    PriceRecommendation = new PriceRecommendationDto
                    {
                        MinPrice = priceRecommendation.MinRecommendedPrice,
                        MaxPrice = priceRecommendation.MaxRecommendedPrice,
                        SimilarVehicleCount = marketData.TotalListings,
                        PredictedDaysToSell = (int)marketData.AverageDaysOnMarket,
                        PricingStrategy = DeterminePricingStrategy(pricing.InternetPrice, marketData.AveragePrice)
                    },
                    PriceTrend = trendData.PriceHistory.Select(p => new PricePointDto
                    {
                        Date = p.Date.ToString("MMM"),
                        Price = p.AveragePrice
                    }).ToList(),
                    PriceChangePercent = trendData.PriceChangePercentage
                };

                // Add the current vehicle price to competitive listings as "Your Price"
                response.CompetitiveListings.Add(new CompetitiveListingDto
                {
                    Name = "Your Price",
                    Price = pricing.InternetPrice,
                    Distance = 0,
                    IsYours = true
                });

                // Sort the competitive listings by price
                response.CompetitiveListings = response.CompetitiveListings
                    .OrderBy(l => l.Price)
                    .ToList();

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving market analysis for vehicle {VehicleId}", vehicleId);
                return StatusCode(500, "An error occurred while retrieving market analysis");
            }
        }

        /// <summary>
        /// Gets market trends for vehicles matching the specified criteria
        /// </summary>
        [HttpGet("market-trends")]
        public async Task<ActionResult<MarketTrendDto>> GetMarketTrends(
            [Required] int year, 
            [Required] string make, 
            [Required] string model, 
            int months = 6)
        {
            try
            {
                var trendData = await _marketDataService.GetMarketTrendsAsync(year, make, model, months);

                var response = new MarketTrendDto
                {
                    PriceHistory = trendData.PriceHistory.Select(p => new PricePointDto
                    {
                        Date = p.Date.ToString("MMM"),
                        Price = p.AveragePrice
                    }).ToList(),
                    InventoryHistory = trendData.InventoryHistory.Select(i => new InventoryPointDto
                    {
                        Date = i.Date.ToString("MMM"),
                        Count = i.Count
                    }).ToList(),
                    PriceChangePercent = trendData.PriceChangePercentage,
                    InventoryChangePercent = trendData.InventoryChangePercentage
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving market trends for {Year} {Make} {Model}", year, make, model);
                return StatusCode(500, "An error occurred while retrieving market trends");
            }
        }

        /// <summary>
        /// Gets competitive listings for a specific vehicle
        /// </summary>
        [HttpGet("competitive-listings/{vehicleId}")]
        public async Task<ActionResult<IEnumerable<CompetitiveListingDto>>> GetCompetitiveListings(
            Guid vehicleId, 
            [Range(1, 500)] int radius = 50)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    return NotFound($"Vehicle with ID {vehicleId} not found");
                }

                var pricing = await _pricingRepository.GetByVehicleIdAsync(vehicleId);

                var marketData = await _marketDataService.GetCompetitiveListingsAsync(
                    vehicle.Year,
                    vehicle.Make,
                    vehicle.Model,
                    vehicle.Trim,
                    vehicle.Mileage,
                    radius);

                var response = marketData.Listings.Select(l => new CompetitiveListingDto
                {
                    Name = l.Source,
                    Price = l.Price,
                    Distance = l.Distance,
                    IsYours = false
                }).ToList();

                // Add the current vehicle price
                if (pricing != null)
                {
                    response.Add(new CompetitiveListingDto
                    {
                        Name = "Your Price",
                        Price = pricing.InternetPrice,
                        Distance = 0,
                        IsYours = true
                    });
                }

                return Ok(response.OrderBy(l => l.Price));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving competitive listings for vehicle {VehicleId}", vehicleId);
                return StatusCode(500, "An error occurred while retrieving competitive listings");
            }
        }

        /// <summary>
        /// Gets price recommendations for a specific vehicle
        /// </summary>
        [HttpGet("price-recommendations/{vehicleId}")]
        public async Task<ActionResult<PriceRecommendationDto>> GetPriceRecommendations(Guid vehicleId)
        {
            try
            {
                var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (vehicle == null)
                {
                    return NotFound($"Vehicle with ID {vehicleId} not found");
                }

                var pricing = await _pricingRepository.GetByVehicleIdAsync(vehicleId);
                if (pricing == null)
                {
                    return NotFound($"Pricing information for vehicle {vehicleId} not found");
                }

                // Get price recommendations from the market pricing service
                var priceRecommendation = await _marketPricingService.GetPriceRecommendationAsync(vehicleId);

                // Get market data for additional context
                var marketData = await _marketDataService.GetCompetitiveListingsAsync(
                    vehicle.Year,
                    vehicle.Make,
                    vehicle.Model,
                    vehicle.Trim,
                    vehicle.Mileage,
                    50);

                var response = new PriceRecommendationDto
                {
                    MinPrice = priceRecommendation.MinRecommendedPrice,
                    MaxPrice = priceRecommendation.MaxRecommendedPrice,
                    SimilarVehicleCount = marketData.TotalListings,
                    PredictedDaysToSell = priceRecommendation.PredictedDaysToSell,
                    PricingStrategy = DeterminePricingStrategy(pricing.InternetPrice, marketData.AveragePrice)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving price recommendations for vehicle {VehicleId}", vehicleId);
                return StatusCode(500, "An error occurred while retrieving price recommendations");
            }
        }

        #region Helper Methods

        private int CalculatePercentile(decimal vehiclePrice, MarketData marketData)
        {
            if (!marketData.Listings.Any())
            {
                return 50; // Default to middle if no listings
            }

            // Extract prices from listings
            var prices = marketData.Listings.Select(l => (decimal)l.Price).OrderBy(p => p).ToList();
            
            // Count how many are below the current price
            var belowCount = prices.Count(p => p < vehiclePrice);
            
            // Calculate percentile (0-100)
            return (int)Math.Round((double)belowCount / prices.Count * 100);
        }

        private string DeterminePricingStrategy(decimal vehiclePrice, decimal marketAverage)
        {
            var diff = vehiclePrice - marketAverage;
            var percentDiff = (diff / marketAverage) * 100;

            if (percentDiff < -5)
                return "Aggressively Priced";
            else if (percentDiff >= -5 && percentDiff <= 2)
                return "Competitively Priced";
            else if (percentDiff > 2 && percentDiff <= 8)
                return "Premium Priced";
            else
                return "Above Market";
        }

        #endregion
    }

    #region DTOs

    public class MarketAnalysisDto
    {
        public MarketComparisonDto MarketComparison { get; set; }
        public List<CompetitiveListingDto> CompetitiveListings { get; set; } = new();
        public PriceRecommendationDto PriceRecommendation { get; set; }
        public List<PricePointDto> PriceTrend { get; set; } = new();
        public double PriceChangePercent { get; set; }
    }

    public class MarketComparisonDto
    {
        public decimal VehiclePrice { get; set; }
        public decimal MarketAverage { get; set; }
        public decimal PriceDifference { get; set; }
        public int PercentilePlacement { get; set; }
    }

    public class CompetitiveListingDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Distance { get; set; }
        public bool IsYours { get; set; }
    }

    public class PriceRecommendationDto
    {
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public int SimilarVehicleCount { get; set; }
        public int PredictedDaysToSell { get; set; }
        public string PricingStrategy { get; set; }
    }

    public class MarketTrendDto
    {
        public List<PricePointDto> PriceHistory { get; set; } = new();
        public List<InventoryPointDto> InventoryHistory { get; set; } = new();
        public double PriceChangePercent { get; set; }
        public double InventoryChangePercent { get; set; }
    }

    public class PricePointDto
    {
        public string Date { get; set; }
        public decimal Price { get; set; }
    }

    public class InventoryPointDto
    {
        public string Date { get; set; }
        public int Count { get; set; }
    }

    #endregion
}
