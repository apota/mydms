using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using DMS.SalesManagement.Core.DTOs;
using DMS.SalesManagement.Core.Services;

namespace DMS.SalesManagement.API.Controllers
{
    [ApiController]
    [Route("api/sales/integration")]
    public class IntegrationController : ControllerBase
    {
        private readonly IIntegrationService _integrationService;

        public IntegrationController(IIntegrationService integrationService)
        {
            _integrationService = integrationService;
        }

        /// <summary>
        /// Gets available accessories for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>A list of available accessories</returns>
        [HttpGet("vehicles/{vehicleId}/accessories")]
        public async Task<ActionResult<IEnumerable<AccessoryDto>>> GetVehicleAccessories(string vehicleId)
        {
            var accessories = await _integrationService.GetVehicleAccessoriesAsync(vehicleId);
            return Ok(accessories);
        }

        /// <summary>
        /// Gets compatible parts for a vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <returns>A list of compatible parts</returns>
        [HttpGet("vehicles/{vehicleId}/parts")]
        public async Task<ActionResult<IEnumerable<PartDto>>> GetCompatibleParts(string vehicleId)
        {
            var parts = await _integrationService.GetCompatiblePartsAsync(vehicleId);
            return Ok(parts);
        }

        /// <summary>
        /// Reserve parts for a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <param name="request">The parts reservation request</param>
        /// <returns>A reservation confirmation</returns>
        [HttpPost("deals/{dealId}/reserve-parts")]
        public async Task<ActionResult<PartsReservationDto>> ReservePartsForDeal(string dealId, [FromBody] ReservePartsRequestDto request)
        {
            var reservation = await _integrationService.ReservePartsForDealAsync(dealId, request);
            return Ok(reservation);
        }

        /// <summary>
        /// Gets customer service history
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>A list of service history items</returns>
        [HttpGet("customers/{customerId}/service-history")]
        public async Task<ActionResult<IEnumerable<ServiceHistoryDto>>> GetCustomerServiceHistory(string customerId)
        {
            var serviceHistory = await _integrationService.GetCustomerServiceHistoryAsync(customerId);
            return Ok(serviceHistory);
        }

        /// <summary>
        /// Gets financial quote for a deal
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <returns>Financial quotes for the deal</returns>
        [HttpGet("deals/{dealId}/financial-quotes")]
        public async Task<ActionResult<IEnumerable<FinancialQuoteDto>>> GetFinancialQuotesForDeal(string dealId)
        {
            var quotes = await _integrationService.GetFinancialQuotesForDealAsync(dealId);
            return Ok(quotes);
        }

        /// <summary>
        /// Submit deal to financial institution
        /// </summary>
        /// <param name="dealId">The deal ID</param>
        /// <param name="request">The financing request details</param>
        /// <returns>The financial application result</returns>
        [HttpPost("deals/{dealId}/submit-financing")]
        public async Task<ActionResult<FinancingApplicationResultDto>> SubmitDealForFinancing(string dealId, [FromBody] FinancingRequestDto request)
        {
            var result = await _integrationService.SubmitDealForFinancingAsync(dealId, request);
            return Ok(result);
        }
    }
}
