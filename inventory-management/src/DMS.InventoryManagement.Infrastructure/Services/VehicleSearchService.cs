using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Repositories;
using DMS.InventoryManagement.Core.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.Infrastructure.Services
{
    /// <summary>
    /// Implementation of IVehicleSearchService for advanced vehicle searching capabilities
    /// </summary>
    public class VehicleSearchService : IVehicleSearchService
    {
        private readonly IVehicleRepository _vehicleRepository;
        private readonly ILogger<VehicleSearchService> _logger;

        /// <summary>
        /// Initializes a new instance of the VehicleSearchService class
        /// </summary>
        /// <param name="vehicleRepository">The vehicle repository</param>
        /// <param name="logger">The logger</param>
        public VehicleSearchService(
            IVehicleRepository vehicleRepository,
            ILogger<VehicleSearchService> logger)
        {
            _vehicleRepository = vehicleRepository;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Vehicle>> SearchAsync(VehicleSearchCriteria criteria)
        {
            _logger.LogInformation("Searching vehicles with criteria: {criteria}", criteria);

            try
            {
                // Get all vehicles and apply filters in memory
                // In a real implementation, this would be translated to database queries for efficiency
                var vehicles = await _vehicleRepository.GetAllAsync();
                
                var filteredVehicles = vehicles.AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(criteria.Query))
                {
                    filteredVehicles = filteredVehicles.Where(v => 
                        v.Make.Contains(criteria.Query, StringComparison.OrdinalIgnoreCase) ||
                        v.Model.Contains(criteria.Query, StringComparison.OrdinalIgnoreCase) ||
                        v.VIN.Contains(criteria.Query, StringComparison.OrdinalIgnoreCase) ||
                        v.StockNumber.Contains(criteria.Query, StringComparison.OrdinalIgnoreCase)
                    );
                }

                if (!string.IsNullOrEmpty(criteria.Make))
                {
                    filteredVehicles = filteredVehicles.Where(v => v.Make.Equals(criteria.Make, StringComparison.OrdinalIgnoreCase));
                }

                if (!string.IsNullOrEmpty(criteria.Model))
                {
                    filteredVehicles = filteredVehicles.Where(v => v.Model.Equals(criteria.Model, StringComparison.OrdinalIgnoreCase));
                }

                if (criteria.YearRange != null)
                {
                    if (criteria.YearRange.Min.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.Year >= criteria.YearRange.Min.Value);
                    }

                    if (criteria.YearRange.Max.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.Year <= criteria.YearRange.Max.Value);
                    }
                }

                if (criteria.PriceRange != null)
                {
                    if (criteria.PriceRange.Min.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.ListPrice >= criteria.PriceRange.Min.Value);
                    }

                    if (criteria.PriceRange.Max.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.ListPrice <= criteria.PriceRange.Max.Value);
                    }
                }

                if (criteria.MileageRange != null)
                {
                    if (criteria.MileageRange.Min.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.Mileage >= criteria.MileageRange.Min.Value);
                    }

                    if (criteria.MileageRange.Max.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.Mileage <= criteria.MileageRange.Max.Value);
                    }
                }

                if (criteria.VehicleType.HasValue)
                {
                    filteredVehicles = filteredVehicles.Where(v => v.VehicleType == criteria.VehicleType.Value);
                }

                if (criteria.VehicleStatus.HasValue)
                {
                    filteredVehicles = filteredVehicles.Where(v => v.Status == criteria.VehicleStatus.Value);
                }

                if (criteria.Features != null && criteria.Features.Any())
                {
                    filteredVehicles = filteredVehicles.Where(v => 
                        criteria.Features.All(f => 
                            v.Features.Any(vf => vf.Name.Equals(f, StringComparison.OrdinalIgnoreCase))
                        )
                    );
                }

                // Apply sorting
                if (!string.IsNullOrEmpty(criteria.SortBy))
                {
                    switch (criteria.SortBy.ToLower())
                    {
                        case "price":
                            filteredVehicles = criteria.SortDescending
                                ? filteredVehicles.OrderByDescending(v => v.ListPrice)
                                : filteredVehicles.OrderBy(v => v.ListPrice);
                            break;
                        case "year":
                            filteredVehicles = criteria.SortDescending
                                ? filteredVehicles.OrderByDescending(v => v.Year)
                                : filteredVehicles.OrderBy(v => v.Year);
                            break;
                        case "mileage":
                            filteredVehicles = criteria.SortDescending
                                ? filteredVehicles.OrderByDescending(v => v.Mileage)
                                : filteredVehicles.OrderBy(v => v.Mileage);
                            break;
                        case "make":
                            filteredVehicles = criteria.SortDescending
                                ? filteredVehicles.OrderByDescending(v => v.Make)
                                : filteredVehicles.OrderBy(v => v.Make);
                            break;
                        case "model":
                            filteredVehicles = criteria.SortDescending
                                ? filteredVehicles.OrderByDescending(v => v.Model)
                                : filteredVehicles.OrderBy(v => v.Model);
                            break;
                        default:
                            filteredVehicles = filteredVehicles.OrderBy(v => v.StockNumber);
                            break;
                    }
                }
                else
                {
                    // Default sorting by stock number
                    filteredVehicles = filteredVehicles.OrderBy(v => v.StockNumber);
                }

                // Apply pagination
                var result = filteredVehicles
                    .Skip(criteria.Skip)
                    .Take(criteria.Take)
                    .ToList();

                _logger.LogInformation("Found {count} vehicles matching criteria", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for vehicles with criteria: {criteria}", criteria);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Vehicle>> FindSimilarVehiclesAsync(Guid vehicleId, int maxResults = 5)
        {
            _logger.LogInformation("Finding similar vehicles to vehicle with ID: {vehicleId}", vehicleId);

            try
            {
                var sourceVehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
                if (sourceVehicle == null)
                {
                    _logger.LogWarning("Vehicle with ID {vehicleId} not found", vehicleId);
                    return Enumerable.Empty<Vehicle>();
                }

                var vehicles = await _vehicleRepository.GetAllAsync();
                
                // Find similar vehicles based on make, model, and year
                var similarVehicles = vehicles
                    .Where(v => v.Id != vehicleId) // Exclude the source vehicle
                    .Where(v => !v.IsDeleted)
                    .Where(v => v.Status == VehicleStatus.FrontLine || v.Status == VehicleStatus.InStock)
                    .AsEnumerable() // Bring into memory for complex scoring
                    .Select(v => new
                    {
                        Vehicle = v,
                        SimilarityScore = CalculateSimilarityScore(sourceVehicle, v)
                    })
                    .OrderByDescending(v => v.SimilarityScore)
                    .Take(maxResults)
                    .Select(v => v.Vehicle)
                    .ToList();

                _logger.LogInformation("Found {count} similar vehicles", similarVehicles.Count);
                return similarVehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar vehicles for vehicle ID {vehicleId}", vehicleId);
                throw;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<Vehicle>> GetRecommendedVehiclesAsync(CustomerPreferences preferences, int maxResults = 10)
        {
            _logger.LogInformation("Getting recommended vehicles based on customer preferences");

            try
            {
                var vehicles = await _vehicleRepository.GetAllAsync();
                
                // Filter vehicles based on preferences
                var filteredVehicles = vehicles.AsQueryable();
                
                // Only show vehicles that are available for sale
                filteredVehicles = filteredVehicles.Where(v => !v.IsDeleted && 
                    (v.Status == VehicleStatus.FrontLine || v.Status == VehicleStatus.InStock));
                
                if (preferences.PreferredMakes != null && preferences.PreferredMakes.Any())
                {
                    filteredVehicles = filteredVehicles.Where(v => 
                        preferences.PreferredMakes.Contains(v.Make, StringComparer.OrdinalIgnoreCase));
                }
                
                if (preferences.PreferredModels != null && preferences.PreferredModels.Any())
                {
                    filteredVehicles = filteredVehicles.Where(v => 
                        preferences.PreferredModels.Contains(v.Model, StringComparer.OrdinalIgnoreCase));
                }
                
                if (preferences.PriceRange != null)
                {
                    if (preferences.PriceRange.Min.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.ListPrice >= preferences.PriceRange.Min.Value);
                    }
                    
                    if (preferences.PriceRange.Max.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.ListPrice <= preferences.PriceRange.Max.Value);
                    }
                }
                
                if (preferences.YearRange != null)
                {
                    if (preferences.YearRange.Min.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.Year >= preferences.YearRange.Min.Value);
                    }
                    
                    if (preferences.YearRange.Max.HasValue)
                    {
                        filteredVehicles = filteredVehicles.Where(v => v.Year <= preferences.YearRange.Max.Value);
                    }
                }
                
                // Score vehicles based on how well they match the desired features
                var scoredVehicles = filteredVehicles
                    .AsEnumerable() // Process in memory for complex scoring
                    .Select(v => new
                    {
                        Vehicle = v,
                        Score = CalculatePreferenceMatchScore(v, preferences)
                    })
                    .OrderByDescending(v => v.Score)
                    .Take(maxResults)
                    .Select(v => v.Vehicle)
                    .ToList();
                
                _logger.LogInformation("Found {count} recommended vehicles", scoredVehicles.Count);
                return scoredVehicles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended vehicles");
                throw;
            }
        }

        private int CalculateSimilarityScore(Vehicle source, Vehicle target)
        {
            int score = 0;
            
            // Base matching on key attributes
            if (source.Make.Equals(target.Make, StringComparison.OrdinalIgnoreCase))
                score += 5;
                
            if (source.Model.Equals(target.Model, StringComparison.OrdinalIgnoreCase))
                score += 5;
                
            if (source.Year == target.Year)
                score += 3;
            else if (Math.Abs(source.Year - target.Year) <= 2)
                score += 2;
                
            // Trim level
            if (!string.IsNullOrEmpty(source.Trim) && 
                !string.IsNullOrEmpty(target.Trim) && 
                source.Trim.Equals(target.Trim, StringComparison.OrdinalIgnoreCase))
                score += 3;
                
            // Price similarity (within 20%)
            var priceDiff = Math.Abs(source.ListPrice - target.ListPrice);
            var priceRatio = priceDiff / source.ListPrice;
            if (priceRatio <= 0.1m)
                score += 3;
            else if (priceRatio <= 0.2m)
                score += 2;
                
            // Mileage similarity (within 20%)
            if (source.Mileage > 0 && target.Mileage > 0)
            {
                var mileageDiff = Math.Abs(source.Mileage - target.Mileage);
                var mileageRatio = (float)mileageDiff / source.Mileage;
                if (mileageRatio <= 0.1f)
                    score += 3;
                else if (mileageRatio <= 0.2f)
                    score += 2;
            }
            
            // Matching features
            var sourceFeatures = source.Features.Select(f => f.Name.ToLowerInvariant()).ToHashSet();
            var targetFeatures = target.Features.Select(f => f.Name.ToLowerInvariant()).ToHashSet();
            
            var commonFeatures = sourceFeatures.Intersect(targetFeatures).Count();
            score += Math.Min(commonFeatures, 5); // Cap at 5 points for features
            
            return score;
        }

        private int CalculatePreferenceMatchScore(Vehicle vehicle, CustomerPreferences preferences)
        {
            int score = 0;
            
            // Make/model preferences score higher
            if (preferences.PreferredMakes != null && 
                preferences.PreferredMakes.Any(m => m.Equals(vehicle.Make, StringComparison.OrdinalIgnoreCase)))
                score += 5;
                
            if (preferences.PreferredModels != null && 
                preferences.PreferredModels.Any(m => m.Equals(vehicle.Model, StringComparison.OrdinalIgnoreCase)))
                score += 5;
                
            // Price - score higher for closer match to middle of price range
            if (preferences.PriceRange != null)
            {
                var midPrice = ((preferences.PriceRange.Max ?? decimal.MaxValue) + 
                               (preferences.PriceRange.Min ?? 0m)) / 2m;
                
                var priceDiff = Math.Abs(vehicle.ListPrice - midPrice);
                var priceRange = (preferences.PriceRange.Max ?? decimal.MaxValue) - 
                                (preferences.PriceRange.Min ?? 0m);
                
                if (priceRange > 0 && priceDiff / priceRange <= 0.1m)
                    score += 4;
                else if (priceRange > 0 && priceDiff / priceRange <= 0.2m)
                    score += 3;
                else if (priceRange > 0 && priceDiff / priceRange <= 0.3m)
                    score += 2;
                else
                    score += 1;
            }
            
            // Year preferences
            if (preferences.YearRange != null)
            {
                var midYear = ((preferences.YearRange.Max ?? int.MaxValue) + 
                              (preferences.YearRange.Min ?? 0)) / 2;
                
                var yearDiff = Math.Abs(vehicle.Year - midYear);
                var yearRange = (preferences.YearRange.Max ?? int.MaxValue) - 
                               (preferences.YearRange.Min ?? 0);
                
                if (yearRange > 0 && yearDiff <= yearRange * 0.1)
                    score += 3;
                else if (yearRange > 0 && yearDiff <= yearRange * 0.2)
                    score += 2;
                else if (yearRange > 0 && yearDiff <= yearRange * 0.3)
                    score += 1;
            }
            
            // Feature preferences
            if (preferences.DesiredFeatures != null && preferences.DesiredFeatures.Any())
            {
                var vehicleFeatures = vehicle.Features.Select(f => f.Name.ToLowerInvariant()).ToHashSet();
                
                foreach (var feature in preferences.DesiredFeatures)
                {
                    if (vehicleFeatures.Contains(feature.ToLowerInvariant()))
                        score += 1;
                }
            }
            
            return score;
        }
    }
}
