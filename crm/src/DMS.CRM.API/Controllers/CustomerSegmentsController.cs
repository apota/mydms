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
    public class CustomerSegmentsController : ControllerBase
    {
        private readonly ICustomerSegmentService _segmentService;
        private readonly ILogger<CustomerSegmentsController> _logger;

        public CustomerSegmentsController(
            ICustomerSegmentService segmentService,
            ILogger<CustomerSegmentsController> logger)
        {
            _segmentService = segmentService ?? throw new ArgumentNullException(nameof(segmentService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet]
        [SwaggerOperation(Summary = "Get all customer segments", Description = "Retrieves a paginated list of all customer segments")]
        [SwaggerResponse(200, "List of segments retrieved successfully", typeof(IEnumerable<CustomerSegmentDto>))]
        public async Task<ActionResult<IEnumerable<CustomerSegmentDto>>> GetAllSegments(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var segments = await _segmentService.GetAllSegmentsAsync(skip, take);
                return Ok(segments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customer segments");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer segments");
            }
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get segment by ID", Description = "Retrieves a specific customer segment by its ID")]
        [SwaggerResponse(200, "Segment retrieved successfully", typeof(CustomerSegmentDto))]
        [SwaggerResponse(404, "Segment not found")]
        public async Task<ActionResult<CustomerSegmentDto>> GetSegmentById(Guid id)
        {
            try
            {
                var segment = await _segmentService.GetSegmentByIdAsync(id);
                if (segment == null)
                {
                    return NotFound($"Segment with ID {id} not found");
                }
                return Ok(segment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving segment with ID: {SegmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving customer segment");
            }
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Create segment", Description = "Creates a new customer segment")]
        [SwaggerResponse(201, "Segment created successfully", typeof(CustomerSegmentDto))]
        [SwaggerResponse(400, "Invalid segment data")]
        public async Task<ActionResult<CustomerSegmentDto>> CreateSegment([FromBody] CustomerSegmentCreateDto segmentDto)
        {
            try
            {
                if (segmentDto == null)
                {
                    return BadRequest("Segment data is null");
                }

                var createdSegment = await _segmentService.CreateSegmentAsync(segmentDto);
                return CreatedAtAction(nameof(GetSegmentById), new { id = createdSegment.Id }, createdSegment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating customer segment");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating customer segment");
            }
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(Summary = "Update segment", Description = "Updates an existing customer segment")]
        [SwaggerResponse(200, "Segment updated successfully", typeof(CustomerSegmentDto))]
        [SwaggerResponse(404, "Segment not found")]
        [SwaggerResponse(400, "Invalid segment data")]
        public async Task<ActionResult<CustomerSegmentDto>> UpdateSegment(Guid id, [FromBody] CustomerSegmentUpdateDto segmentDto)
        {
            try
            {
                if (segmentDto == null)
                {
                    return BadRequest("Segment data is null");
                }

                var updatedSegment = await _segmentService.UpdateSegmentAsync(id, segmentDto);
                if (updatedSegment == null)
                {
                    return NotFound($"Segment with ID {id} not found");
                }

                return Ok(updatedSegment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating segment with ID: {SegmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating customer segment");
            }
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(Summary = "Delete segment", Description = "Deletes an existing customer segment")]
        [SwaggerResponse(204, "Segment deleted successfully")]
        [SwaggerResponse(404, "Segment not found")]
        public async Task<ActionResult> DeleteSegment(Guid id)
        {
            try
            {
                var result = await _segmentService.DeleteSegmentAsync(id);
                if (!result)
                {
                    return NotFound($"Segment with ID {id} not found");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting segment with ID: {SegmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting customer segment");
            }
        }

        [HttpGet("{id:guid}/size")]
        [SwaggerOperation(Summary = "Calculate segment size", Description = "Calculates the number of customers in a specific segment")]
        [SwaggerResponse(200, "Segment size calculated successfully")]
        [SwaggerResponse(404, "Segment not found")]
        public async Task<ActionResult<int>> CalculateSegmentSize(Guid id)
        {
            try
            {
                var size = await _segmentService.CalculateSegmentSizeAsync(id);
                return Ok(new { SegmentId = id, CustomerCount = size });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating size for segment with ID: {SegmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error calculating segment size");
            }
        }

        [HttpGet("{id:guid}/customers")]
        [SwaggerOperation(Summary = "Get customers in segment", Description = "Retrieves customers that are part of a specific segment")]
        [SwaggerResponse(200, "Segment customers retrieved successfully", typeof(IEnumerable<CustomerDto>))]
        [SwaggerResponse(404, "Segment not found")]
        public async Task<ActionResult<IEnumerable<CustomerDto>>> GetCustomersInSegment(
            Guid id,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50)
        {
            try
            {
                var customers = await _segmentService.GetCustomersInSegmentAsync(id, skip, take);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving customers for segment with ID: {SegmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving segment customers");
            }
        }

        [HttpPost("{id:guid}/customers/{customerId:guid}")]
        [SwaggerOperation(Summary = "Add customer to segment", Description = "Adds a customer to a static segment")]
        [SwaggerResponse(200, "Customer added to segment successfully")]
        [SwaggerResponse(400, "Cannot add customer to dynamic segment")]
        [SwaggerResponse(404, "Segment or customer not found")]
        public async Task<ActionResult> AddCustomerToSegment(Guid id, Guid customerId)
        {
            try
            {
                var result = await _segmentService.AddCustomerToSegmentAsync(id, customerId);
                if (!result)
                {
                    return BadRequest("Failed to add customer to segment. Check if the segment is dynamic or if the customer/segment exists.");
                }

                return Ok(new { Message = "Customer added to segment successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding customer {CustomerId} to segment {SegmentId}", customerId, id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding customer to segment");
            }
        }

        [HttpDelete("{id:guid}/customers/{customerId:guid}")]
        [SwaggerOperation(Summary = "Remove customer from segment", Description = "Removes a customer from a static segment")]
        [SwaggerResponse(200, "Customer removed from segment successfully")]
        [SwaggerResponse(400, "Cannot remove customer from dynamic segment")]
        [SwaggerResponse(404, "Segment or customer not found")]
        public async Task<ActionResult> RemoveCustomerFromSegment(Guid id, Guid customerId)
        {
            try
            {
                var result = await _segmentService.RemoveCustomerFromSegmentAsync(id, customerId);
                if (!result)
                {
                    return BadRequest("Failed to remove customer from segment. Check if the segment is dynamic or if the customer/segment exists.");
                }

                return Ok(new { Message = "Customer removed from segment successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing customer {CustomerId} from segment {SegmentId}", customerId, id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error removing customer from segment");
            }
        }

        [HttpPost("search")]
        [SwaggerOperation(Summary = "Search segments by criteria", Description = "Searches for segments matching the provided criteria")]
        [SwaggerResponse(200, "Segments retrieved successfully", typeof(IEnumerable<CustomerSegmentDto>))]
        public async Task<ActionResult<IEnumerable<CustomerSegmentDto>>> SearchSegmentsByCriteria([FromBody] Dictionary<string, object> criteria)
        {
            try
            {
                if (criteria == null || !criteria.Any())
                {
                    return BadRequest("Search criteria cannot be empty");
                }

                var segments = await _segmentService.GetSegmentsByCriteria(criteria);
                return Ok(segments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching segments by criteria");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error searching segments");
            }
        }

        [HttpGet("{id:guid}/campaigns")]
        [SwaggerOperation(Summary = "Get campaigns using segment", Description = "Retrieves campaigns that target a specific customer segment")]
        [SwaggerResponse(200, "Campaigns retrieved successfully", typeof(IEnumerable<CampaignDto>))]
        [SwaggerResponse(404, "Segment not found")]
        public async Task<ActionResult<IEnumerable<CampaignDto>>> GetCampaignsBySegment(Guid id)
        {
            try
            {
                var campaigns = await _segmentService.GetCampaignsBySegmentAsync(id);
                return Ok(campaigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving campaigns for segment with ID: {SegmentId}", id);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving campaigns for segment");
            }
        }
    }
}
