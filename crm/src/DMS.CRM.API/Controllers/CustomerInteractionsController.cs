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
    public class CustomerInteractionsController : ControllerBase
    {
        private readonly ICustomerInteractionService _interactionService;
        private readonly ILogger<CustomerInteractionsController> _logger;

        public CustomerInteractionsController(
            ICustomerInteractionService interactionService,
            ILogger<CustomerInteractionsController> logger)
        {
            _interactionService = interactionService ?? throw new ArgumentNullException(nameof(interactionService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get interactions by filter", Description = "Retrieves interactions based on the provided filters")]
        [SwaggerResponse(200, "Interactions retrieved successfully", typeof(IEnumerable<CustomerInteractionDto>))]
        public async Task<ActionResult<IEnumerable<CustomerInteractionDto>>> GetInteractionsByFilter(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] InteractionType? type,
            [FromQuery] CommunicationChannel? channel,
            [FromQuery] InteractionStatus? status,
            [FromQuery] bool? requiresFollowUp,
            [FromQuery] string? searchTerm,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var interactions = await _interactionService.GetInteractionsByFilterAsync(
                    startDate, endDate, type, channel, status, requiresFollowUp, searchTerm, skip, take);
                return Ok(interactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interactions with filters");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving interactions");
            }
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get interaction by ID", Description = "Retrieves a specific interaction by its ID")]
        [SwaggerResponse(200, "Interaction retrieved successfully", typeof(CustomerInteractionDto))]
        [SwaggerResponse(404, "Interaction not found")]
        public async Task<ActionResult<CustomerInteractionDto>> GetInteractionById(Guid id)
        {
            try
            {
                var interaction = await _interactionService.GetInteractionByIdAsync(id);
                if (interaction == null)
                {
                    return NotFound($"Interaction with ID {id} not found");
                }
                return Ok(interaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interaction with ID: {InteractionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving interaction");
            }
        }

        [HttpGet("customer/{customerId:guid}")]
        [SwaggerOperation(Summary = "Get interactions by customer ID", Description = "Retrieves all interactions for a specific customer")]
        [SwaggerResponse(200, "Interactions retrieved successfully", typeof(IEnumerable<CustomerInteractionDto>))]
        public async Task<ActionResult<IEnumerable<CustomerInteractionDto>>> GetInteractionsByCustomerId(
            Guid customerId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var interactions = await _interactionService.GetInteractionsByCustomerIdAsync(customerId, skip, take);
                return Ok(interactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interactions for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer interactions");
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create interaction", Description = "Creates a new customer interaction")]
        [SwaggerResponse(201, "Interaction created successfully", typeof(CustomerInteractionDto))]
        [SwaggerResponse(400, "Invalid interaction data")]
        public async Task<ActionResult<CustomerInteractionDto>> CreateInteraction([FromBody] CustomerInteractionCreateDto interactionDto)
        {
            try
            {
                if (interactionDto == null)
                {
                    return BadRequest("Interaction data is null");
                }

                var createdInteraction = await _interactionService.CreateInteractionAsync(interactionDto);
                return CreatedAtAction(nameof(GetInteractionById), new { id = createdInteraction.Id }, createdInteraction);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid customer ID when creating interaction");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating interaction");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating interaction");
            }
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update interaction", Description = "Updates an existing interaction")]
        [SwaggerResponse(200, "Interaction updated successfully", typeof(CustomerInteractionDto))]
        [SwaggerResponse(404, "Interaction not found")]
        [SwaggerResponse(400, "Invalid interaction data")]
        public async Task<ActionResult<CustomerInteractionDto>> UpdateInteraction(
            Guid id,
            [FromBody] CustomerInteractionUpdateDto interactionDto)
        {
            try
            {
                if (interactionDto == null)
                {
                    return BadRequest("Interaction data is null");
                }

                var updatedInteraction = await _interactionService.UpdateInteractionAsync(id, interactionDto);
                if (updatedInteraction == null)
                {
                    return NotFound($"Interaction with ID {id} not found");
                }

                return Ok(updatedInteraction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating interaction with ID: {InteractionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating interaction");
            }
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Delete interaction", Description = "Deletes an existing interaction")]
        [SwaggerResponse(204, "Interaction deleted successfully")]
        [SwaggerResponse(404, "Interaction not found")]
        public async Task<ActionResult> DeleteInteraction(Guid id)
        {
            try
            {
                var result = await _interactionService.DeleteInteractionAsync(id);
                if (!result)
                {
                    return NotFound($"Interaction with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting interaction with ID: {InteractionId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting interaction");
            }
        }

        [HttpGet("followups")]
        [SwaggerOperation(Summary = "Get follow-up interactions", Description = "Retrieves interactions that require follow-up within the specified date range")]
        [SwaggerResponse(200, "Follow-up interactions retrieved successfully", typeof(IEnumerable<CustomerInteractionDto>))]
        public async Task<ActionResult<IEnumerable<CustomerInteractionDto>>> GetFollowUps(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.Date;
                var end = endDate ?? start.AddDays(7); // Default to one week from start date

                var followUps = await _interactionService.GetFollowUpsForDateRangeAsync(start, end);
                return Ok(followUps);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving follow-up interactions for date range");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving follow-up interactions");
            }
        }

        [HttpGet("customer/{customerId:guid}/stats")]
        [SwaggerOperation(Summary = "Get customer interaction statistics", Description = "Retrieves interaction statistics for a specific customer")]
        [SwaggerResponse(200, "Customer interaction statistics retrieved successfully", typeof(Dictionary<string, int>))]
        public async Task<ActionResult<Dictionary<string, int>>> GetCustomerInteractionStats(Guid customerId)
        {
            try
            {
                var stats = await _interactionService.GetInteractionStatsByCustomerAsync(customerId);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interaction statistics for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer interaction statistics");
            }
        }

        [HttpGet("stats/channel")]
        [SwaggerOperation(Summary = "Get interaction statistics by channel", Description = "Retrieves interaction statistics grouped by communication channel for a specific date range")]
        [SwaggerResponse(200, "Channel interaction statistics retrieved successfully", typeof(Dictionary<string, int>))]
        public async Task<ActionResult<Dictionary<string, int>>> GetInteractionStatsByChannel(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-1);
                var end = endDate ?? DateTime.UtcNow;

                var stats = await _interactionService.GetInteractionStatsByChannelAsync(start, end);
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interaction statistics by channel");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving channel interaction statistics");
            }
        }
    }
}
