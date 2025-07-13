using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Models;
using DMS.InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly IVehicleSearchService _vehicleSearchService;
        private readonly ILogger<SearchController> _logger;

        public SearchController(
            IVehicleSearchService vehicleSearchService,
            ILogger<SearchController> logger)
        {
            _vehicleSearchService = vehicleSearchService;
            _logger = logger;
        }

        /// <summary>
        /// Searches for vehicles using advanced filtering criteria
        /// </summary>
        /// <param name="criteria">The search criteria</param>
        /// <returns>A list of vehicles matching the criteria</returns>
        [HttpPost("vehicles")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> SearchVehicles([FromBody] VehicleSearchCriteriaDto criteria)
        {
            _logger.LogInformation("Searching vehicles with criteria: {@Criteria}", criteria);
            
            try
            {
                var searchCriteria = MapToCoreModel(criteria);
                var vehicles = await _vehicleSearchService.SearchAsync(searchCriteria);
                
                return Ok(vehicles.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching for vehicles");
                return StatusCode(500, "An error occurred while searching for vehicles");
            }
        }

        /// <summary>
        /// Finds similar vehicles based on a reference vehicle
        /// </summary>
        /// <param name="id">The reference vehicle ID</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>A list of similar vehicles</returns>
        [HttpGet("vehicles/{id}/similar")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDto>), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> FindSimilarVehicles(Guid id, [FromQuery] int maxResults = 5)
        {
            _logger.LogInformation("Finding similar vehicles to vehicle with ID: {id}", id);
            
            try
            {
                var vehicles = await _vehicleSearchService.FindSimilarVehiclesAsync(id, maxResults);
                return Ok(vehicles.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding similar vehicles for vehicle ID {id}", id);
                return StatusCode(500, "An error occurred while finding similar vehicles");
            }
        }

        /// <summary>
        /// Gets recommended vehicles based on customer preferences
        /// </summary>
        /// <param name="preferences">Customer preference criteria</param>
        /// <param name="maxResults">Maximum number of results to return</param>
        /// <returns>A list of recommended vehicles</returns>
        [HttpPost("vehicles/recommended")]
        [Authorize(Policy = "InventoryRead")]
        [ProducesResponseType(typeof(IEnumerable<VehicleDto>), 200)]
        public async Task<ActionResult<IEnumerable<VehicleDto>>> GetRecommendedVehicles(
            [FromBody] CustomerPreferencesDto preferences, 
            [FromQuery] int maxResults = 10)
        {
            _logger.LogInformation("Getting recommended vehicles based on customer preferences");
            
            try
            {
                var corePreferences = MapToCoreModel(preferences);
                var vehicles = await _vehicleSearchService.GetRecommendedVehiclesAsync(corePreferences, maxResults);
                
                return Ok(vehicles.Select(MapToDto));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting recommended vehicles");
                return StatusCode(500, "An error occurred while getting recommended vehicles");
            }
        }

        private VehicleSearchCriteria MapToCoreModel(VehicleSearchCriteriaDto dto)
        {
            return new VehicleSearchCriteria
            {
                Query = dto.Query,
                Make = dto.Make,
                Model = dto.Model,
                YearRange = dto.YearFrom.HasValue || dto.YearTo.HasValue 
                    ? new Range<int> { Min = dto.YearFrom, Max = dto.YearTo } 
                    : null,
                PriceRange = dto.PriceFrom.HasValue || dto.PriceTo.HasValue 
                    ? new Range<decimal> { Min = dto.PriceFrom, Max = dto.PriceTo } 
                    : null,
                MileageRange = dto.MileageFrom.HasValue || dto.MileageTo.HasValue 
                    ? new Range<int> { Min = dto.MileageFrom, Max = dto.MileageTo } 
                    : null,
                VehicleType = dto.VehicleType,
                VehicleStatus = dto.VehicleStatus,
                Features = dto.Features?.ToList(),
                Skip = dto.Skip,
                Take = dto.Take,
                SortBy = dto.SortBy,
                SortDescending = dto.SortDescending
            };
        }

        private CustomerPreferences MapToCoreModel(CustomerPreferencesDto dto)
        {
            return new CustomerPreferences
            {
                PreferredMakes = dto.PreferredMakes?.ToList(),
                PreferredModels = dto.PreferredModels?.ToList(),
                PriceRange = dto.PriceFrom.HasValue || dto.PriceTo.HasValue 
                    ? new Range<decimal> { Min = dto.PriceFrom, Max = dto.PriceTo } 
                    : null,
                YearRange = dto.YearFrom.HasValue || dto.YearTo.HasValue 
                    ? new Range<int> { Min = dto.YearFrom, Max = dto.YearTo } 
                    : null,
                DesiredFeatures = dto.DesiredFeatures?.ToList(),
                BodyStyle = dto.BodyStyle
            };
        }

        private VehicleDto MapToDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                Id = vehicle.Id,
                VIN = vehicle.VIN,
                StockNumber = vehicle.StockNumber,
                Make = vehicle.Make,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Trim = vehicle.Trim,
                ExteriorColor = vehicle.ExteriorColor,
                InteriorColor = vehicle.InteriorColor,
                Mileage = vehicle.Mileage,
                VehicleType = vehicle.VehicleType,
                Status = vehicle.Status,
                ListPrice = vehicle.ListPrice,
                MSRP = vehicle.MSRP,
                AcquisitionDate = vehicle.AcquisitionDate,
                AcquisitionSource = vehicle.AcquisitionSource,
                LotLocation = vehicle.LotLocation,
                Features = vehicle.Features?.Select(f => new VehicleFeatureDto { Name = f.Name, Value = f.Value }).ToList(),
                Images = vehicle.Images?.Select(i => new VehicleImageDto { Id = i.Id, Url = i.Url, IsPrimary = i.IsPrimary }).ToList(),
                LocationId = vehicle.LocationId,
                DaysInInventory = vehicle.AgingInfo?.DaysInInventory ?? 0,
                AgingAlertLevel = vehicle.AgingInfo?.AgingAlertLevel.ToString()
            };
        }
    }

    /// <summary>
    /// DTO for vehicle search criteria
    /// </summary>
    public class VehicleSearchCriteriaDto
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
        /// Gets or sets the minimum year
        /// </summary>
        public int? YearFrom { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum year
        /// </summary>
        public int? YearTo { get; set; }
        
        /// <summary>
        /// Gets or sets the minimum price
        /// </summary>
        public decimal? PriceFrom { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum price
        /// </summary>
        public decimal? PriceTo { get; set; }
        
        /// <summary>
        /// Gets or sets the minimum mileage
        /// </summary>
        public int? MileageFrom { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum mileage
        /// </summary>
        public int? MileageTo { get; set; }
        
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
        public IEnumerable<string>? Features { get; set; }
        
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
    /// DTO for customer preferences
    /// </summary>
    public class CustomerPreferencesDto
    {
        /// <summary>
        /// Gets or sets the preferred makes
        /// </summary>
        public IEnumerable<string>? PreferredMakes { get; set; }
        
        /// <summary>
        /// Gets or sets the preferred models
        /// </summary>
        public IEnumerable<string>? PreferredModels { get; set; }
        
        /// <summary>
        /// Gets or sets the minimum price preference
        /// </summary>
        public decimal? PriceFrom { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum price preference
        /// </summary>
        public decimal? PriceTo { get; set; }
        
        /// <summary>
        /// Gets or sets the minimum year preference
        /// </summary>
        public int? YearFrom { get; set; }
        
        /// <summary>
        /// Gets or sets the maximum year preference
        /// </summary>
        public int? YearTo { get; set; }
        
        /// <summary>
        /// Gets or sets the desired features
        /// </summary>
        public IEnumerable<string>? DesiredFeatures { get; set; }
        
        /// <summary>
        /// Gets or sets the preferred body style
        /// </summary>
        public string? BodyStyle { get; set; }
    }
}
