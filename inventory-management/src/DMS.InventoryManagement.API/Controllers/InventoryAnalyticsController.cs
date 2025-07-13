using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.InventoryManagement.API.Models;
using DMS.InventoryManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMS.InventoryManagement.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/inventory/analytics")]
    public class InventoryAnalyticsController : ControllerBase
    {
        private readonly IInventoryAnalyticsService _analyticsService;
        private readonly ILogger<InventoryAnalyticsController> _logger;

        public InventoryAnalyticsController(
            IInventoryAnalyticsService analyticsService,
            ILogger<InventoryAnalyticsController> logger)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("aging")]
        public async Task<ActionResult<AgingAnalyticsDto>> GetAgingAnalytics([FromQuery] AgingAnalyticsRequestDto request)
        {
            try
            {
                var result = await _analyticsService.GetAgingAnalyticsAsync(
                    request.LocationIds, 
                    request.VehicleTypes, 
                    request.DateRange);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving aging analytics");
                return StatusCode(500, "Error retrieving aging analytics data");
            }
        }

        [HttpGet("valuation")]
        public async Task<ActionResult<InventoryValuationDto>> GetInventoryValuation([FromQuery] ValuationRequestDto request)
        {
            try
            {
                var result = await _analyticsService.GetInventoryValuationAsync(
                    request.GroupBy, 
                    request.LocationIds, 
                    request.IncludeSold);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory valuation");
                return StatusCode(500, "Error retrieving inventory valuation data");
            }
        }

        [HttpGet("turnover")]
        public async Task<ActionResult<TurnoverMetricsDto>> GetTurnoverMetrics([FromQuery] TurnoverRequestDto request)
        {
            try
            {
                var result = await _analyticsService.GetTurnoverMetricsAsync(
                    request.TimePeriod,
                    request.VehicleTypes,
                    request.LocationIds);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving turnover metrics");
                return StatusCode(500, "Error retrieving turnover metrics data");
            }
        }

        [HttpGet("inventory-mix")]
        public async Task<ActionResult<InventoryMixAnalysisDto>> GetInventoryMixAnalysis([FromQuery] InventoryMixRequestDto request)
        {
            try
            {
                var result = await _analyticsService.GetInventoryMixAnalysisAsync(
                    request.GroupingFactor,
                    request.ComparisonPeriod);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving inventory mix analysis");
                return StatusCode(500, "Error retrieving inventory mix analysis data");
            }
        }

        [HttpGet("price-competitiveness")]
        public async Task<ActionResult<PriceCompetitivenessDto>> GetPriceCompetitiveness([FromQuery] PriceCompetitivenessRequestDto request)
        {
            try
            {
                var result = await _analyticsService.GetPriceCompetitivenessAsync(
                    request.VehicleIds,
                    request.MarketRadius);
                
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving price competitiveness");
                return StatusCode(500, "Error retrieving price competitiveness data");
            }
        }
    }
}
