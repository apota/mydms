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
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        private readonly ILogger<CampaignsController> _logger;

        public CampaignsController(
            ICampaignService campaignService,
            ILogger<CampaignsController> logger)
        {
            _campaignService = campaignService ?? throw new ArgumentNullException(nameof(campaignService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all campaigns", Description = "Retrieves a paginated list of all campaigns")]
        [SwaggerResponse(200, "List of campaigns retrieved successfully", typeof(IEnumerable<CampaignDto>))]
        public async Task<ActionResult<IEnumerable<CampaignDto>>> GetAllCampaigns(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var campaigns = await _campaignService.GetAllCampaignsAsync(skip, take);
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaigns");
            }
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get campaign by ID", Description = "Retrieves a specific campaign by its ID")]
        [SwaggerResponse(200, "Campaign retrieved successfully", typeof(CampaignDto))]
        [SwaggerResponse(404, "Campaign not found")]
        public async Task<ActionResult<CampaignDto>> GetCampaignById(Guid id)
        {
            try
            {
                var campaign = await _campaignService.GetCampaignByIdAsync(id);
                if (campaign == null)
                {
                    return NotFound($"Campaign with ID {id} not found");
                }
                return Ok(campaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaign with ID: {CampaignId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaign");
            }
        }

        [HttpGet("active")]
        [SwaggerOperation(Summary = "Get active campaigns", Description = "Retrieves a list of active campaigns for the specified date")]
        [SwaggerResponse(200, "List of active campaigns retrieved successfully", typeof(IEnumerable<CampaignDto>))]
        public async Task<ActionResult<IEnumerable<CampaignDto>>> GetActiveCampaigns(
            [FromQuery] DateTime? date,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var targetDate = date ?? DateTime.UtcNow;
                var campaigns = await _campaignService.GetActiveCampaignsAsync(targetDate, skip, take);
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving active campaigns");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving active campaigns");
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create campaign", Description = "Creates a new marketing campaign")]
        [SwaggerResponse(201, "Campaign created successfully", typeof(CampaignDto))]
        [SwaggerResponse(400, "Invalid campaign data")]
        public async Task<ActionResult<CampaignDto>> CreateCampaign([FromBody] CampaignCreateDto campaignDto)
        {
            try
            {
                if (campaignDto == null)
                {
                    return BadRequest("Campaign data is null");
                }

                var createdCampaign = await _campaignService.CreateCampaignAsync(campaignDto);
                return CreatedAtAction(nameof(GetCampaignById), new { id = createdCampaign.Id }, createdCampaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating campaign");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating campaign");
            }
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update campaign", Description = "Updates an existing campaign")]
        [SwaggerResponse(200, "Campaign updated successfully", typeof(CampaignDto))]
        [SwaggerResponse(404, "Campaign not found")]
        [SwaggerResponse(400, "Invalid campaign data")]
        public async Task<ActionResult<CampaignDto>> UpdateCampaign(Guid id, [FromBody] CampaignUpdateDto campaignDto)
        {
            try
            {
                if (campaignDto == null)
                {
                    return BadRequest("Campaign data is null");
                }

                var updatedCampaign = await _campaignService.UpdateCampaignAsync(id, campaignDto);
                if (updatedCampaign == null)
                {
                    return NotFound($"Campaign with ID {id} not found");
                }

                return Ok(updatedCampaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating campaign with ID: {CampaignId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating campaign");
            }
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Delete campaign", Description = "Deletes an existing campaign")]
        [SwaggerResponse(204, "Campaign deleted successfully")]
        [SwaggerResponse(404, "Campaign not found")]
        public async Task<ActionResult> DeleteCampaign(Guid id)
        {
            try
            {
                var result = await _campaignService.DeleteCampaignAsync(id);
                if (!result)
                {
                    return NotFound($"Campaign with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting campaign with ID: {CampaignId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting campaign");
            }
        }

        [HttpPost("{id:guid}/activate")]
        [SwaggerOperation(Summary = "Activate campaign", Description = "Activates an existing campaign")]
        [SwaggerResponse(200, "Campaign activated successfully")]
        [SwaggerResponse(404, "Campaign not found")]
        public async Task<ActionResult> ActivateCampaign(Guid id)
        {
            try
            {
                var result = await _campaignService.ActivateCampaignAsync(id);
                if (!result)
                {
                    return NotFound($"Campaign with ID {id} not found");
                }

                return Ok(new { Message = "Campaign activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating campaign with ID: {CampaignId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error activating campaign");
            }
        }

        [HttpPost("{id:guid}/deactivate")]
        [SwaggerOperation(Summary = "Deactivate campaign", Description = "Deactivates an existing campaign")]
        [SwaggerResponse(200, "Campaign deactivated successfully")]
        [SwaggerResponse(404, "Campaign not found")]
        public async Task<ActionResult> DeactivateCampaign(Guid id)
        {
            try
            {
                var result = await _campaignService.DeactivateCampaignAsync(id);
                if (!result)
                {
                    return NotFound($"Campaign with ID {id} not found");
                }

                return Ok(new { Message = "Campaign deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating campaign with ID: {CampaignId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deactivating campaign");
            }
        }

        [HttpGet("{id:guid}/metrics")]
        [SwaggerOperation(Summary = "Get campaign metrics", Description = "Retrieves performance metrics for a specific campaign")]
        [SwaggerResponse(200, "Campaign metrics retrieved successfully")]
        [SwaggerResponse(404, "Campaign not found")]
        public async Task<ActionResult<Dictionary<string, object>>> GetCampaignMetrics(Guid id)
        {
            try
            {
                var metrics = await _campaignService.GetCampaignMetricsAsync(id);
                if (metrics == null)
                {
                    return NotFound($"Campaign with ID {id} not found");
                }

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving metrics for campaign with ID: {CampaignId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaign metrics");
            }
        }

        [HttpGet("{id:guid}/customers")]
        [SwaggerOperation(Summary = "Get campaign target customers", Description = "Retrieves the list of customers targeted by a specific campaign")]
        [SwaggerResponse(200, "Campaign target customers retrieved successfully", typeof(IEnumerable<CustomerDto>))]
        [SwaggerResponse(404, "Campaign not found")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCampaignTargetCustomers(
            Guid id,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var customers = await _campaignService.GetCampaignTargetCustomersAsync(id, skip, take);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving target customers for campaign with ID: {CampaignId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaign target customers");
            }
        }

        [HttpGet("{id:guid}/interactions")]
        [SwaggerOperation(Summary = "Get campaign interactions", Description = "Retrieves the list of customer interactions associated with a specific campaign")]
        [SwaggerResponse(200, "Campaign interactions retrieved successfully", typeof(IEnumerable<CustomerInteractionDto>))]
        [SwaggerResponse(404, "Campaign not found")]
        public async Task<ActionResult<IEnumerable<CustomerInteractionDto>>> GetCampaignInteractions(
            Guid id,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var interactions = await _campaignService.GetCampaignInteractionsAsync(id, skip, take);
                return Ok(interactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving interactions for campaign with ID: {CampaignId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaign interactions");
            }
        }

        [HttpGet("type/{type}")]
        [SwaggerOperation(Summary = "Get campaigns by type", Description = "Retrieves a list of campaigns of a specific type")]
        [SwaggerResponse(200, "List of campaigns retrieved successfully", typeof(IEnumerable<CampaignDto>))]
        public async Task<ActionResult<IEnumerable<CampaignDto>>> GetCampaignsByType(CampaignType type)
        {
            try
            {
                var campaigns = await _campaignService.GetCampaignsByTypeAsync(type);
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns of type: {CampaignType}", type);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaigns");
            }
        }

        [HttpGet("status/{status}")]
        [SwaggerOperation(Summary = "Get campaigns by status", Description = "Retrieves a list of campaigns with a specific status")]
        [SwaggerResponse(200, "List of campaigns retrieved successfully", typeof(IEnumerable<CampaignDto>))]
        public async Task<ActionResult<IEnumerable<CampaignDto>>> GetCampaignsByStatus(CampaignStatus status)
        {
            try
            {
                var campaigns = await _campaignService.GetCampaignsByStatusAsync(status);
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns with status: {CampaignStatus}", status);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaigns");
            }
        }

        [HttpGet("date-range")]
        [SwaggerOperation(Summary = "Get campaigns by date range", Description = "Retrieves a list of campaigns scheduled within a specific date range")]
        [SwaggerResponse(200, "List of campaigns retrieved successfully", typeof(IEnumerable<CampaignDto>))]
        public async Task<ActionResult<IEnumerable<CampaignDto>>> GetCampaignsByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            try
            {
                if (startDate > endDate)
                {
                    return BadRequest("Start date must be before or equal to end date");
                }

                var campaigns = await _campaignService.GetCampaignsByDateRangeAsync(startDate, endDate);
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns in date range: {StartDate} to {EndDate}", startDate, endDate);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaigns");
            }
        }

        [HttpGet("segment/{segmentId:guid}")]
        [SwaggerOperation(Summary = "Get campaigns by segment", Description = "Retrieves a list of campaigns targeting a specific customer segment")]
        [SwaggerResponse(200, "List of campaigns retrieved successfully", typeof(IEnumerable<CampaignDto>))]
        public async Task<ActionResult<IEnumerable<CampaignDto>>> GetCampaignsBySegment(Guid segmentId)
        {
            try
            {
                var campaigns = await _campaignService.GetCampaignsBySegmentAsync(segmentId);
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns targeting segment ID: {SegmentId}", segmentId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaigns");
            }
        }

        [HttpPut("{id:guid}/metrics")]
        [SwaggerOperation(Summary = "Update campaign metrics", Description = "Updates the performance metrics for a specific campaign")]
        [SwaggerResponse(200, "Campaign metrics updated successfully", typeof(CampaignDto))]
        [SwaggerResponse(404, "Campaign not found")]
        public async Task<ActionResult<CampaignDto>> UpdateCampaignMetrics(Guid id, [FromBody] CampaignMetricsUpdateDto metricsDto)
        {
            try
            {
                if (metricsDto == null)
                {
                    return BadRequest("Metrics data is null");
                }

                var updatedCampaign = await _campaignService.UpdateCampaignMetricsAsync(id, metricsDto);
                if (updatedCampaign == null)
                {
                    return NotFound($"Campaign with ID {id} not found");
                }

                return Ok(updatedCampaign);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating metrics for campaign with ID: {CampaignId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating campaign metrics");
            }
        }
    }
}
