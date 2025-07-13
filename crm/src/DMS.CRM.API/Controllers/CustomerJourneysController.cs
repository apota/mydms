using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Models;
using DMS.CRM.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DMS.CRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerJourneysController : ControllerBase
    {
        private readonly ICustomerJourneyService _journeyService;
        private readonly ILogger<CustomerJourneysController> _logger;

        public CustomerJourneysController(
            ICustomerJourneyService journeyService,
            ILogger<CustomerJourneysController> logger)
        {
            _journeyService = journeyService ?? throw new ArgumentNullException(nameof(journeyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("customer/{customerId:guid}")]
        [SwaggerOperation(Summary = "Get customer journey", Description = "Retrieves the journey for a specific customer")]
        [SwaggerResponse(200, "Journey retrieved successfully", typeof(CustomerJourneyDto))]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult<CustomerJourneyDto>> GetJourneyByCustomerId(Guid customerId)
        {
            try
            {
                var journey = await _journeyService.GetJourneyByCustomerIdAsync(customerId);
                if (journey == null)
                {
                    return NotFound($"No journey found for customer with ID {customerId}");
                }
                return Ok(journey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving journey for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer journey");
            }
        }

        [HttpPut("customer/{customerId:guid}")]
        [SwaggerOperation(Summary = "Update customer journey stage", Description = "Updates the journey stage for a specific customer")]
        [SwaggerResponse(200, "Journey updated successfully", typeof(CustomerJourneyDto))]
        [SwaggerResponse(404, "Customer not found")]
        [SwaggerResponse(400, "Invalid journey data")]
        public async Task<ActionResult<CustomerJourneyDto>> UpdateJourneyStage(Guid customerId, [FromBody] CustomerJourneyUpdateDto journeyDto)
        {
            try
            {
                if (journeyDto == null)
                {
                    return BadRequest("Journey data is null");
                }

                var updatedJourney = await _journeyService.UpdateJourneyStageAsync(customerId, journeyDto);
                if (updatedJourney == null)
                {
                    return NotFound($"No journey found for customer with ID {customerId}");
                }

                return Ok(updatedJourney);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating journey for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating customer journey");
            }
        }

        [HttpPost("customer/{customerId:guid}/steps")]
        [SwaggerOperation(Summary = "Add journey step", Description = "Adds a new step to a customer's journey")]
        [SwaggerResponse(200, "Step added successfully")]
        [SwaggerResponse(404, "Customer not found")]
        [SwaggerResponse(400, "Invalid step data")]
        public async Task<ActionResult> AddJourneyStep(
            Guid customerId, 
            [FromQuery] JourneyStage stage,
            [FromBody] string notes)
        {
            try
            {
                var result = await _journeyService.AddJourneyStepAsync(customerId, stage, notes);
                if (!result)
                {
                    return NotFound($"No journey found for customer with ID {customerId}");
                }

                return Ok(new { Message = "Journey step added successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding journey step for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding journey step");
            }
        }

        [HttpGet("stage/{stage}")]
        [SwaggerOperation(Summary = "Get customers by stage", Description = "Retrieves all customers currently at a specific journey stage")]
        [SwaggerResponse(200, "Customers retrieved successfully", typeof(IEnumerable<CustomerJourneyDto>))]
        public async Task<ActionResult<IEnumerable<CustomerJourneyDto>>> GetCustomersByStage(
            JourneyStage stage,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var journeys = await _journeyService.GetCustomersByStageAsync(stage, skip, take);
                return Ok(journeys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers for stage: {Stage}", stage);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customers by stage");
            }
        }

        [HttpGet("distribution")]
        [SwaggerOperation(Summary = "Get journey distribution", Description = "Retrieves the distribution of customers across journey stages")]
        [SwaggerResponse(200, "Distribution retrieved successfully")]
        public async Task<ActionResult<Dictionary<JourneyStage, int>>> GetJourneyDistribution()
        {
            try
            {
                var distribution = await _journeyService.GetJourneyDistributionAsync();
                return Ok(distribution);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving journey distribution");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving journey distribution");
            }
        }

        [HttpGet("customer/{customerId:guid}/stats")]
        [SwaggerOperation(Summary = "Get customer journey statistics", Description = "Retrieves detailed statistics about a customer's journey")]
        [SwaggerResponse(200, "Statistics retrieved successfully")]
        [SwaggerResponse(404, "Customer journey not found")]
        public async Task<ActionResult<Dictionary<string, object>>> GetJourneyStats(Guid customerId)
        {
            try
            {
                var stats = await _journeyService.GetJourneyStatsForCustomerAsync(customerId);
                if (stats == null)
                {
                    return NotFound($"No journey found for customer with ID {customerId}");
                }
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving journey stats for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving journey statistics");
            }
        }

        [HttpGet("customer/{customerId:guid}/probability")]
        [SwaggerOperation(Summary = "Calculate purchase probability", Description = "Calculates the probability of a customer making a purchase")]
        [SwaggerResponse(200, "Probability calculated successfully")]
        [SwaggerResponse(404, "Customer journey not found")]
        public async Task<ActionResult<double>> CalculatePurchaseProbability(Guid customerId)
        {
            try
            {
                var probability = await _journeyService.CalculatePurchaseProbabilityAsync(customerId);
                return Ok(new { CustomerId = customerId, PurchaseProbability = probability });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating purchase probability for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error calculating purchase probability");
            }
        }

        [HttpGet("customer/{customerId:guid}/estimated-transition")]
        [SwaggerOperation(Summary = "Estimate next stage transition", Description = "Estimates when a customer will transition to their next journey stage")]
        [SwaggerResponse(200, "Transition estimate calculated successfully")]
        [SwaggerResponse(404, "Customer journey not found")]
        public async Task<ActionResult<DateTime?>> EstimateNextStageTransition(Guid customerId)
        {
            try
            {
                var estimatedDate = await _journeyService.EstimateNextStageTransitionAsync(customerId);
                return Ok(new { CustomerId = customerId, EstimatedTransitionDate = estimatedDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating next stage transition for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error estimating next stage transition");
            }
        }
    }
}
