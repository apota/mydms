using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Interface for market pricing tools integration
    /// Handles integration with pricing tools like vAuto, Kelley Blue Book, etc.
    /// </summary>
    public interface IMarketPricingService
    {
        /// <summary>
        /// Gets market value for a vehicle based on its specifications
        /// </summary>
        /// <param name="vin">Vehicle Identification Number</param>
        /// <returns>Market value information</returns>
        Task<MarketValuation> GetMarketValueByVinAsync(string vin);
        
        /// <summary>
        /// Gets market value for a vehicle based on its specifications
        /// </summary>
        /// <param name="make">Vehicle make</param>
        /// <param name="model">Vehicle model</param>
        /// <param name="year">Vehicle year</param>
        /// <param name="trim">Vehicle trim (optional)</param>
        /// <param name="mileage">Vehicle mileage (optional)</param>
        /// <param name="condition">Vehicle condition (optional)</param>
        /// <returns>Market value information</returns>
        Task<MarketValuation> GetMarketValueBySpecsAsync(string make, string model, int year, string trim = null, int? mileage = null, string condition = null);
        
        /// <summary>
        /// Gets competitive vehicle listings in the market
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle to compare</param>
        /// <param name="radius">Search radius in miles</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>List of competitive listings</returns>
        Task<List<CompetitorListing>> GetCompetitivePricingAsync(Guid vehicleId, int radius = 50, int maxResults = 10);
        
        /// <summary>
        /// Gets price recommendations for a vehicle
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>Price recommendations</returns>
        Task<PriceRecommendation> GetPriceRecommendationsAsync(Guid vehicleId);
        
        /// <summary>
        /// Gets historical price trends for similar vehicles
        /// </summary>
        /// <param name="make">Vehicle make</param>
        /// <param name="model">Vehicle model</param>
        /// <param name="year">Vehicle year</param>
        /// <param name="trim">Vehicle trim (optional)</param>
        /// <param name="months">Number of months of history to retrieve</param>
        /// <returns>Historical price data points</returns>
        Task<List<PriceTrendPoint>> GetHistoricalPriceTrendsAsync(string make, string model, int year, string trim = null, int months = 6);
        
        /// <summary>
        /// Get market days-on-lot statistics for similar vehicles
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <returns>Days-on-lot statistics</returns>
        Task<DaysOnLotStatistics> GetMarketDaysOnLotStatisticsAsync(Guid vehicleId);
    }
    
    /// <summary>
    /// Represents market valuation for a vehicle
    /// </summary>
    public class MarketValuation
    {
        /// <summary>
        /// Gets or sets the retail/clean value
        /// </summary>
        public decimal RetailValue { get; set; }
        
        /// <summary>
        /// Gets or sets the wholesale/trade-in value
        /// </summary>
        public decimal WholesaleValue { get; set; }
        
        /// <summary>
        /// Gets or sets the private party sale value
        /// </summary>
        public decimal PrivatePartyValue { get; set; }
        
        /// <summary>
        /// Gets or sets the confidence level (0-100)
        /// </summary>
        public int ConfidenceLevel { get; set; }
        
        /// <summary>
        /// Gets or sets the data source (KBB, NADA, Black Book, etc.)
        /// </summary>
        public string DataSource { get; set; }
        
        /// <summary>
        /// Gets or sets the valuation date
        /// </summary>
        public DateTime ValuationDate { get; set; }
        
        /// <summary>
        /// Gets or sets the regional adjustments
        /// </summary>
        public decimal RegionalAdjustment { get; set; }
        
        /// <summary>
        /// Gets or sets the adjusted values based on options and condition
        /// </summary>
        public Dictionary<string, decimal> Adjustments { get; set; }
    }
    
    /// <summary>
    /// Represents a competitor's vehicle listing
    /// </summary>
    public class CompetitorListing
    {
        /// <summary>
        /// Gets or sets the dealer name
        /// </summary>
        public string DealerName { get; set; }
        
        /// <summary>
        /// Gets or sets the vehicle year
        /// </summary>
        public int Year { get; set; }
        
        /// <summary>
        /// Gets or sets the vehicle make
        /// </summary>
        public string Make { get; set; }
        
        /// <summary>
        /// Gets or sets the vehicle model
        /// </summary>
        public string Model { get; set; }
        
        /// <summary>
        /// Gets or sets the vehicle trim
        /// </summary>
        public string Trim { get; set; }
        
        /// <summary>
        /// Gets or sets the vehicle mileage
        /// </summary>
        public int Mileage { get; set; }
        
        /// <summary>
        /// Gets or sets the listing price
        /// </summary>
        public decimal Price { get; set; }
        
        /// <summary>
        /// Gets or sets the days on market
        /// </summary>
        public int DaysOnMarket { get; set; }
        
        /// <summary>
        /// Gets or sets the distance from dealer
        /// </summary>
        public double Distance { get; set; }
        
        /// <summary>
        /// Gets or sets the listing URL
        /// </summary>
        public string ListingUrl { get; set; }
        
        /// <summary>
        /// Gets or sets any special features or options
        /// </summary>
        public List<string> SpecialFeatures { get; set; }
        
        /// <summary>
        /// Gets or sets the listing photo URL
        /// </summary>
        public string PhotoUrl { get; set; }
    }
    
    /// <summary>
    /// Represents price recommendations for a vehicle
    /// </summary>
    public class PriceRecommendation
    {
        /// <summary>
        /// Gets or sets the recommended list price
        /// </summary>
        public decimal RecommendedPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the minimum recommended price
        /// </summary>
        public decimal MinimumPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum recommended price
        /// </summary>
        public decimal MaximumPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the estimated time to sell at recommended price (in days)
        /// </summary>
        public int EstimatedTimeToSellDays { get; set; }
        
        /// <summary>
        /// Gets or sets the estimated gross profit at recommended price
        /// </summary>
        public decimal EstimatedGrossProfit { get; set; }
        
        /// <summary>
        /// Gets or sets price tiers with estimated time to sell
        /// </summary>
        public Dictionary<decimal, int> PriceTiers { get; set; }
        
        /// <summary>
        /// Gets or sets the price percentile (0-100) relative to market
        /// </summary>
        public int MarketPercentile { get; set; }
        
        /// <summary>
        /// Gets or sets the price recommendation rationale
        /// </summary>
        public string Rationale { get; set; }
    }
    
    /// <summary>
    /// Represents a price trend data point
    /// </summary>
    public class PriceTrendPoint
    {
        /// <summary>
        /// Gets or sets the date of the trend point
        /// </summary>
        public DateTime Date { get; set; }
        
        /// <summary>
        /// Gets or sets the average price at that date
        /// </summary>
        public decimal AveragePrice { get; set; }
        
        /// <summary>
        /// Gets or sets the minimum price at that date
        /// </summary>
        public decimal MinPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum price at that date
        /// </summary>
        public decimal MaxPrice { get; set; }
        
        /// <summary>
        /// Gets or sets the volume of sales/listings
        /// </summary>
        public int Volume { get; set; }
    }
    
    /// <summary>
    /// Represents statistics about days on lot for similar vehicles
    /// </summary>
    public class DaysOnLotStatistics
    {
        /// <summary>
        /// Gets or sets the average days on lot
        /// </summary>
        public int AverageDaysOnLot { get; set; }
        
        /// <summary>
        /// Gets or sets the median days on lot
        /// </summary>
        public int MedianDaysOnLot { get; set; }
        
        /// <summary>
        /// Gets or sets the minimum days on lot
        /// </summary>
        public int MinimumDaysOnLot { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum days on lot
        /// </summary>
        public int MaximumDaysOnLot { get; set; }
        
        /// <summary>
        /// Gets or sets the distribution of days on lot
        /// </summary>
        public Dictionary<string, int> Distribution { get; set; }
    }
}
