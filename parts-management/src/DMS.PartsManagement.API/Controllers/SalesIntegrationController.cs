using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Services;

namespace DMS.PartsManagement.API.Controllers
{
    [ApiController]
    [Route("api/integration/sales")]
    public class SalesIntegrationController : ControllerBase
    {
        private readonly IIntegrationService _integrationService;

        public SalesIntegrationController(IIntegrationService integrationService)
        {
            _integrationService = integrationService;
        }

        /// <summary>
        /// Gets accessories for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>List of accessories</returns>
        [HttpGet("vehicles/{vehicleId}/accessories")]
        public async Task<ActionResult<IEnumerable<AccessoryDto>>> GetVehicleAccessories(string vehicleId)
        {
            var accessories = await _integrationService.GetVehicleAccessoriesAsync(vehicleId);
            return Ok(accessories);
        }

        /// <summary>
        /// Gets parts that are compatible with a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>List of compatible parts</returns>
        [HttpGet("vehicles/{vehicleId}/compatible-parts")]
        public async Task<ActionResult<IEnumerable<PartDto>>> GetCompatibleParts(string vehicleId)
        {
            var parts = await _integrationService.GetCompatiblePartsAsync(vehicleId);
            return Ok(parts);
        }

        /// <summary>
        /// Reserves parts for a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <param name="request">The parts reservation request</param>
        /// <returns>The reservation result</returns>
        [HttpPost("deals/{dealId}/reserve-parts")]
        public async Task<ActionResult<PartsReservationDto>> ReservePartsForDeal(string dealId, [FromBody] ReservePartsRequestDto request)
        {
            var reservation = await _integrationService.ReservePartsForDealAsync(dealId, request);
            return Ok(reservation);
        }

        /// <summary>
        /// Gets order status information for parts on a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <returns>The order status</returns>
        [HttpGet("deals/{dealId}/parts-orders")]
        public async Task<ActionResult<IEnumerable<DealPartsOrderStatusDto>>> GetDealPartsOrdersStatus(string dealId)
        {
            var orderStatuses = await _integrationService.GetDealPartsOrdersStatusAsync(dealId);
            return Ok(orderStatuses);
        }

        /// <summary>
        /// Gets installed parts and accessories for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>The installed parts</returns>
        [HttpGet("vehicles/{vehicleId}/installed-parts")]
        public async Task<ActionResult<IEnumerable<InstalledPartDto>>> GetInstalledVehicleParts(string vehicleId)
        {
            var installedParts = await _integrationService.GetInstalledVehiclePartsAsync(vehicleId);
            return Ok(installedParts);
        }

        /// <summary>
        /// Gets accessory installation time estimate
        /// </summary>
        /// <param name="request">The accessories to calculate installation time for</param>
        /// <returns>The installation time estimate</returns>
        [HttpPost("accessories/installation-estimate")]
        public async Task<ActionResult<InstallationEstimateDto>> GetAccessoryInstallationEstimate([FromBody] AccessoryInstallationRequestDto request)
        {
            var estimate = await _integrationService.CalculateAccessoryInstallationEstimateAsync(request);
            return Ok(estimate);
        }
    }
}
