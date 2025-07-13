using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.Core.Services
{
    public interface IExternalMarketDataService
    {
        /// <summary>
        /// Gets competitive listings from various marketplaces for similar vehicles.
        /// </summary>
        /// <param name="year">Vehicle year</param>
        /// <param name="make">Vehicle manufacturer</param>
        /// <param name="model">Vehicle model</param>
        /// <param name="trim">Vehicle trim (optional)</param>
        /// <param name="mileage">Vehicle mileage (optional)</param>
        /// <param name="radius">Search radius in miles</param>
        /// <returns>Market data including competitive listings and statistics</returns>
        Task<MarketData> GetCompetitiveListingsAsync(
            int year, 
            string make, 
            string model, 
            string? trim, 
            int? mileage, 
            int radius);
        
        /// <summary>
        /// Gets market trends for a specific type of vehicle over time
        /// </summary>
        /// <param name="year">Vehicle year</param>
        /// <param name="make">Vehicle manufacturer</param>
        /// <param name="model">Vehicle model</param>
        /// <param name="months">Number of months of historical data to retrieve</param>
        /// <returns>Market trend data including price history</returns>
        Task<MarketTrendData> GetMarketTrendsAsync(
            int year, 
            string make, 
            string model, 
            int months = 6);
        
        /// <summary>
        /// Gets price depreciation forecast for a specific vehicle
        /// </summary>
        /// <param name="year">Vehicle year</param>
        /// <param name="make">Vehicle manufacturer</param>
        /// <param name="model">Vehicle model</param>
        /// <param name="trim">Vehicle trim</param>
        /// <param name="mileage">Current mileage</param>
        /// <param name="months">Number of months to forecast</param>
        /// <returns>Forecasted price data points</returns>
        Task<List<PricePoint>> GetDepreciationForecastAsync(
            int year, 
            string make, 
            string model, 
            string trim, 
            int mileage, 
            int months = 12);
    }

    public class MarketData
    {
        public List<dynamic> Listings { get; set; } = new List<dynamic>();
        public decimal AveragePrice { get; set; }
        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
        public double AverageMileage { get; set; }
        public double AverageDaysOnMarket { get; set; }
        public int TotalListings { get; set; }
    }

    public class MarketTrendData
    {
        public List<PricePoint> PriceHistory { get; set; } = new List<PricePoint>();
        public List<InventoryPoint> InventoryHistory { get; set; } = new List<InventoryPoint>();
        public double PriceChangePercentage { get; set; }
        public double InventoryChangePercentage { get; set; }
    }

    public class PricePoint
    {
        public System.DateTime Date { get; set; }
        public decimal AveragePrice { get; set; }
        public int SampleSize { get; set; }
    }

    public class InventoryPoint
    {
        public System.DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
