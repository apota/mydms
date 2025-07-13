using DMS.InventoryManagement.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.Core.Services
{
    /// <summary>
    /// Service interface for advanced vehicle searching capabilities
    /// </summary>
    public interface IVehicleSearchService
    {
        /// <summary>
        /// Searches for vehicles using advanced filtering criteria
        /// </summary>
        /// <param name="criteria">The search criteria</param>
        /// <returns>A list of vehicles matching the criteria</returns>
        Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchCriteria criteria);
        
        /// <summary>
        /// Finds similar vehicles based on a reference vehicle
        /// </summary>
        /// <param name="vehicleId">The reference vehicle ID</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>A list of similar vehicles</returns>
        Task<IEnumerable<Vehicle>> FindSimilarVehiclesAsync(Guid vehicleId, int maxResults = 5);
        
        /// <summary>
        /// Gets recommended vehicles based on customer preferences
        /// </summary>
        /// <param name="customerPreferences">Customer preference criteria</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>A list of recommended vehicles</returns>
        Task<IEnumerable<Vehicle>> GetRecommendedVehiclesAsync(CustomerPreferences customerPreferences, int maxResults = 10);
    }

    /// <summary>
    /// Criteria for searching vehicles
    /// </summary>
    public class VehicleSearchCriteria
    {
        /// <summary>
        /// Gets or sets the search query
        /// </summary>
        public string? Query { get; set; }
        
        /// <summary>
        /// Gets or sets the make filter
        /// </summary>
        public string? Make { get; set; }
        
        /// <summary>
        /// Gets or sets the model filter
        /// </summary>
        public string? Model { get; set; }
        
        /// <summary>
        /// Gets or sets the year range
        /// </summary>
        public Range<int>? YearRange { get; set; }
        
        /// <summary>
        /// Gets or sets the price range
        /// </summary>
        public Range<decimal>? PriceRange { get; set; }
        
        /// <summary>
        /// Gets or sets the mileage range
        /// </summary>
        public Range<int>? MileageRange { get; set; }
        
        /// <summary>
        /// Gets or sets the vehicle type filter
        /// </summary>
        public VehicleType? VehicleType { get; set; }
        
        /// <summary>
        /// Gets or sets the vehicle status filter
        /// </summary>
        public VehicleStatus? VehicleStatus { get; set; }
        
        /// <summary>
        /// Gets or sets the features to filter by
        /// </summary>
        public List<string>? Features { get; set; }
        
        /// <summary>
        /// Gets or sets the number of items to skip for pagination
        /// </summary>
        public int Skip { get; set; } = 0;
        
        /// <summary>
        /// Gets or sets the number of items to take for pagination
        /// </summary>
        public int Take { get; set; } = 20;
        
        /// <summary>
        /// Gets or sets the sort field
        /// </summary>
        public string? SortBy { get; set; }
        
        /// <summary>
        /// Gets or sets the sort direction
        /// </summary>
        public bool SortDescending { get; set; } = false;
    }

    /// <summary>
    /// Range class for numeric filters
    /// </summary>
    /// <typeparam name="T">The type of the range</typeparam>
    public class Range<T> where T : struct, IComparable<T>
    {
        /// <summary>
        /// Gets or sets the minimum value
        /// </summary>
        public T? Min { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum value
        /// </summary>
        public T? Max { get; set; }
    }

    /// <summary>
    /// Customer preferences for vehicle recommendations
    /// </summary>
    public class CustomerPreferences
    {
        /// <summary>
        /// Gets or sets the preferred makes
        /// </summary>
        public List<string>? PreferredMakes { get; set; }
        
        /// <summary>
        /// Gets or sets the preferred models
        /// </summary>
        public List<string>? PreferredModels { get; set; }
        
        /// <summary>
        /// Gets or sets the preferred price range
        /// </summary>
        public Range<decimal>? PriceRange { get; set; }
        
        /// <summary>
        /// Gets or sets the preferred year range
        /// </summary>
        public Range<int>? YearRange { get; set; }
        
        /// <summary>
        /// Gets or sets the preferred features
        /// </summary>
        public List<string>? DesiredFeatures { get; set; }
        
        /// <summary>
        /// Gets or sets the preferred body style
        /// </summary>
        public string? BodyStyle { get; set; }
    }
}
