using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace DMS.CRM.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CustomerSurveysController : ControllerBase
    {
        private readonly ICustomerSurveyService _customerSurveyService;
        private readonly ILogger<CustomerSurveysController> _logger;

        public CustomerSurveysController(
            ICustomerSurveyService customerSurveyService,
            ILogger<CustomerSurveysController> logger)
        {
            _customerSurveyService = customerSurveyService ?? throw new ArgumentNullException(nameof(customerSurveyService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all customer surveys", Description = "Retrieves a paginated list of all customer surveys")]
        [SwaggerResponse(200, "List of customer surveys retrieved successfully", typeof(IEnumerable<CustomerSurveyDto>))]
        public async Task<ActionResult<IEnumerable<CustomerSurveyDto>>> GetAllCustomerSurveys(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var surveys = await _customerSurveyService.GetAllSurveysAsync(skip, take);
                return Ok(surveys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer surveys");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer surveys");
            }
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get customer survey by ID", Description = "Retrieves a specific customer survey by its ID")]
        [SwaggerResponse(200, "Customer survey retrieved successfully", typeof(CustomerSurveyDto))]
        [SwaggerResponse(404, "Customer survey not found")]
        public async Task<ActionResult<CustomerSurveyDto>> GetCustomerSurveyById(Guid id)
        {
            try
            {
                var survey = await _customerSurveyService.GetSurveyByIdAsync(id);
                if (survey == null)
                {
                    return NotFound($"Customer survey with ID {id} not found");
                }
                return Ok(survey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer survey with ID: {SurveyId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer survey");
            }
        }

        [HttpGet("customer/{customerId:guid}")]
        [SwaggerOperation(Summary = "Get surveys by customer ID", Description = "Retrieves all surveys for a specific customer")]
        [SwaggerResponse(200, "Customer surveys retrieved successfully", typeof(IEnumerable<CustomerSurveyDto>))]
        public async Task<ActionResult<IEnumerable<CustomerSurveyDto>>> GetSurveysByCustomerId(
            Guid customerId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var surveys = await _customerSurveyService.GetSurveysByCustomerIdAsync(customerId, skip, take);
                return Ok(surveys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving surveys for customer with ID: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer surveys");
            }
        }

        [HttpGet("campaign/{campaignId:guid}")]
        [SwaggerOperation(Summary = "Get surveys by campaign ID", Description = "Retrieves all surveys associated with a specific campaign")]
        [SwaggerResponse(200, "Campaign surveys retrieved successfully", typeof(IEnumerable<CustomerSurveyDto>))]
        public async Task<ActionResult<IEnumerable<CustomerSurveyDto>>> GetSurveysByCampaignId(
            Guid campaignId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var surveys = await _customerSurveyService.GetSurveysByCampaignIdAsync(campaignId, skip, take);
                return Ok(surveys);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving surveys for campaign with ID: {CampaignId}", campaignId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaign surveys");
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create customer survey", Description = "Creates a new customer survey")]
        [SwaggerResponse(201, "Customer survey created successfully", typeof(CustomerSurveyDto))]
        [SwaggerResponse(400, "Invalid customer survey data")]
        public async Task<ActionResult<CustomerSurveyDto>> CreateCustomerSurvey([FromBody] CustomerSurveyCreateDto surveyDto)
        {
            try
            {
                if (surveyDto == null)
                {
                    return BadRequest("Customer survey data is null");
                }

                var createdSurvey = await _customerSurveyService.CreateSurveyAsync(surveyDto);
                return CreatedAtAction(nameof(GetCustomerSurveyById), new { id = createdSurvey.Id }, createdSurvey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer survey");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating customer survey");
            }
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update customer survey", Description = "Updates an existing customer survey")]
        [SwaggerResponse(200, "Customer survey updated successfully", typeof(CustomerSurveyDto))]
        [SwaggerResponse(404, "Customer survey not found")]
        [SwaggerResponse(400, "Invalid customer survey data")]
        public async Task<ActionResult<CustomerSurveyDto>> UpdateCustomerSurvey(Guid id, [FromBody] CustomerSurveyUpdateDto surveyDto)
        {
            try
            {
                if (surveyDto == null)
                {
                    return BadRequest("Customer survey data is null");
                }

                var updatedSurvey = await _customerSurveyService.UpdateSurveyAsync(id, surveyDto);
                if (updatedSurvey == null)
                {
                    return NotFound($"Customer survey with ID {id} not found");
                }

                return Ok(updatedSurvey);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating customer survey with ID: {SurveyId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating customer survey");
            }
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Delete customer survey", Description = "Deletes an existing customer survey")]
        [SwaggerResponse(204, "Customer survey deleted successfully")]
        [SwaggerResponse(404, "Customer survey not found")]
        public async Task<ActionResult> DeleteCustomerSurvey(Guid id)
        {
            try
            {
                var result = await _customerSurveyService.DeleteSurveyAsync(id);
                if (!result)
                {
                    return NotFound($"Customer survey with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer survey with ID: {SurveyId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting customer survey");
            }
        }

        [HttpGet("analytics")]
        [SwaggerOperation(Summary = "Get survey analytics", Description = "Retrieves aggregated analytics from customer surveys")]
        [SwaggerResponse(200, "Survey analytics retrieved successfully")]
        public async Task<ActionResult<Dictionary<string, object>>> GetSurveyAnalytics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-3);
                var end = endDate ?? DateTime.UtcNow;

                var analytics = await _customerSurveyService.GetSurveyAnalyticsAsync(start, end);
                return Ok(analytics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving survey analytics");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving survey analytics");
            }
        }

        [HttpGet("satisfaction-trend")]
        [SwaggerOperation(Summary = "Get customer satisfaction trend", Description = "Retrieves customer satisfaction trend over a specified time period")]
        [SwaggerResponse(200, "Customer satisfaction trend retrieved successfully")]
        public async Task<ActionResult<Dictionary<DateTime, double>>> GetSatisfactionTrend(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string timeInterval = "month") // Options: day, week, month, quarter, year
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddMonths(-12);
                var end = endDate ?? DateTime.UtcNow;

                var trend = await _customerSurveyService.GetSatisfactionTrendAsync(start, end, timeInterval);
                return Ok(trend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer satisfaction trend");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer satisfaction trend");
            }
        }
    }
}
