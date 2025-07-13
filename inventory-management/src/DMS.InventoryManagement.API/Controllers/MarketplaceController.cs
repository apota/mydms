using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DMS.InventoryManagement.Core.Services;

namespace DMS.InventoryManagement.API.Controllers
{
    [ApiController]
    [Route("api/marketplace")]
    [Authorize]
    public class MarketplaceController : ControllerBase
    {
        private readonly IMarketplaceIntegrationService _marketplaceService;
        private readonly ILogger<MarketplaceController> _logger;

        public MarketplaceController(
            IMarketplaceIntegrationService marketplaceService,
            ILogger<MarketplaceController> logger)
        {
            _marketplaceService = marketplaceService ?? throw new ArgumentNullException(nameof(marketplaceService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        /// <summary>
        /// Gets available marketplaces for integration
        /// </summary>
        [HttpGet("available")]
        public async Task<IActionResult> GetAvailableMarketplaces()
        {
            _logger.LogInformation("Getting available marketplaces");
            var marketplaces = await _marketplaceService.GetAvailableMarketplacesAsync();
            return Ok(marketplaces);
        }
        
        /// <summary>
        /// Lists a vehicle on specified marketplaces
        /// </summary>
        [HttpPost("vehicle/{vehicleId}/list")]
        public async Task<IActionResult> ListVehicle(Guid vehicleId, [FromBody] ListVehicleRequest request)
        {
            _logger.LogInformation("Listing vehicle {VehicleId} on marketplaces", vehicleId);
            var result = await _marketplaceService.ListVehicleAsync(vehicleId, request.MarketplaceIds);
            return Ok(result);
        }
        
        /// <summary>
        /// Updates a vehicle listing on specified marketplaces
        /// </summary>
        [HttpPut("vehicle/{vehicleId}/update")]
        public async Task<IActionResult> UpdateVehicleListing(Guid vehicleId, [FromBody] ListVehicleRequest request)
        {
            _logger.LogInformation("Updating vehicle {VehicleId} on marketplaces", vehicleId);
            var result = await _marketplaceService.UpdateVehicleListingAsync(vehicleId, request.MarketplaceIds);
            return Ok(result);
        }
        
        /// <summary>
        /// Removes a vehicle listing from specified marketplaces
        /// </summary>
        [HttpDelete("vehicle/{vehicleId}/remove")]
        public async Task<IActionResult> RemoveVehicleListing(Guid vehicleId, [FromBody] ListVehicleRequest request)
        {
            _logger.LogInformation("Removing vehicle {VehicleId} from marketplaces", vehicleId);
            var result = await _marketplaceService.RemoveVehicleListingAsync(vehicleId, request.MarketplaceIds);
            return Ok(result);
        }
        
        /// <summary>
        /// Gets the listing status for a vehicle across all marketplaces
        /// </summary>
        [HttpGet("vehicle/{vehicleId}/status")]
        public async Task<IActionResult> GetVehicleListingStatus(Guid vehicleId)
        {
            _logger.LogInformation("Getting listing status for vehicle {VehicleId}", vehicleId);
            var result = await _marketplaceService.GetVehicleListingStatusAsync(vehicleId);
            return Ok(result);
        }
        
        /// <summary>
        /// Gets listing statistics for a vehicle across all marketplaces
        /// </summary>
        [HttpGet("vehicle/{vehicleId}/stats")]
        public async Task<IActionResult> GetVehicleListingStats(Guid vehicleId)
        {
            _logger.LogInformation("Getting listing stats for vehicle {VehicleId}", vehicleId);
            var result = await _marketplaceService.GetVehicleListingStatsAsync(vehicleId);
            return Ok(result);
        }
        
        /// <summary>
        /// Verifies marketplace credentials
        /// </summary>
        [HttpPost("verify-credentials")]
        public async Task<IActionResult> VerifyCredentials([FromBody] VerifyCredentialsRequest request)
        {
            _logger.LogInformation("Verifying credentials for marketplace {MarketplaceId}", request.MarketplaceId);
            var result = await _marketplaceService.VerifyMarketplaceCredentialsAsync(request.MarketplaceId, request.Credentials);
            return Ok(new { isValid = result });
        }
        
        /// <summary>
        /// Synchronizes the inventory with all configured marketplaces
        /// </summary>
        [HttpPost("sync")]
        public async Task<IActionResult> SynchronizeInventory()
        {
            _logger.LogInformation("Starting inventory synchronization with marketplaces");
            var result = await _marketplaceService.SynchronizeInventoryAsync();
            return Ok(result);
        }
    }
    
    public class ListVehicleRequest
    {
        public IEnumerable<string> MarketplaceIds { get; set; }
    }
    
    public class VerifyCredentialsRequest
    {
        public string MarketplaceId { get; set; }
        public MarketplaceCredentials Credentials { get; set; }
    }
}
