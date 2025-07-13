using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.ServiceManagement.Core.Entities;
using DMS.ServiceManagement.Core.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMS.ServiceManagement.API.Controllers
{
    [ApiController]
    [Route("api/service/jobs")]
    public class ServiceJobsController : ControllerBase
    {
        private readonly IServiceJobService _serviceJobService;
        private readonly ILogger<ServiceJobsController> _logger;

        public ServiceJobsController(IServiceJobService serviceJobService, ILogger<ServiceJobsController> logger)
        {
            _serviceJobService = serviceJobService;
            _logger = logger;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ServiceJob>>> GetAllServiceJobs()
        {
            try
            {
                var serviceJobs = await _serviceJobService.GetAllServiceJobsAsync();
                return Ok(serviceJobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all service jobs");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving service jobs");
            }
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceJob>> GetServiceJobById(Guid id)
        {
            try
            {
                var serviceJob = await _serviceJobService.GetServiceJobByIdAsync(id);
                if (serviceJob == null)
                {
                    return NotFound($"Service Job with ID {id} not found");
                }
                return Ok(serviceJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving service job with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving service job");
            }
        }

        [HttpGet("repair-order/{repairOrderId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ServiceJob>>> GetServiceJobsByRepairOrderId(Guid repairOrderId)
        {
            try
            {
                var serviceJobs = await _serviceJobService.GetServiceJobsByRepairOrderIdAsync(repairOrderId);
                return Ok(serviceJobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving service jobs for repair order ID: {repairOrderId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving service jobs");
            }
        }

        [HttpGet("technician/{technicianId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<ServiceJob>>> GetServiceJobsByTechnicianId(Guid technicianId)
        {
            try
            {
                var serviceJobs = await _serviceJobService.GetServiceJobsByTechnicianIdAsync(technicianId);
                return Ok(serviceJobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving service jobs for technician ID: {technicianId}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving service jobs");
            }
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceJob>> CreateServiceJob([FromBody] ServiceJob serviceJob)
        {
            try
            {
                if (serviceJob == null)
                {
                    return BadRequest("Service job data is null");
                }

                var createdServiceJob = await _serviceJobService.CreateServiceJobAsync(serviceJob);
                return CreatedAtAction(nameof(GetServiceJobById), new { id = createdServiceJob.Id }, createdServiceJob);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid service job data");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service job");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating service job");
            }
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceJob>> UpdateServiceJob(Guid id, [FromBody] ServiceJob serviceJob)
        {
            try
            {
                if (serviceJob == null)
                {
                    return BadRequest("Service job data is null");
                }

                if (id != serviceJob.Id)
                {
                    return BadRequest("ID mismatch");
                }

                var existingServiceJob = await _serviceJobService.GetServiceJobByIdAsync(id);
                if (existingServiceJob == null)
                {
                    return NotFound($"Service Job with ID {id} not found");
                }

                var updatedServiceJob = await _serviceJobService.UpdateServiceJobAsync(serviceJob);
                return Ok(updatedServiceJob);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, $"Invalid service job data for ID: {id}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating service job with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating service job");
            }
        }

        [HttpPost("{id:guid}/assign")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceJob>> AssignTechnician(Guid id, [FromBody] TechnicianAssignmentRequest request)
        {
            try
            {
                if (request == null || request.TechnicianId == Guid.Empty)
                {
                    return BadRequest("Technician ID is required");
                }

                var updatedJob = await _serviceJobService.AssignTechnicianAsync(id, request.TechnicianId);
                if (updatedJob == null)
                {
                    return NotFound($"Service Job with ID {id} not found");
                }

                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error assigning technician to service job with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error assigning technician");
            }
        }

        [HttpPost("{id:guid}/start")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceJob>> StartJob(Guid id)
        {
            try
            {
                var updatedJob = await _serviceJobService.StartJobAsync(id);
                if (updatedJob == null)
                {
                    return NotFound($"Service Job with ID {id} not found");
                }

                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error starting service job with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error starting service job");
            }
        }

        [HttpPost("{id:guid}/complete")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceJob>> CompleteJob(Guid id)
        {
            try
            {
                var updatedJob = await _serviceJobService.CompleteJobAsync(id);
                if (updatedJob == null)
                {
                    return NotFound($"Service Job with ID {id} not found");
                }

                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error completing service job with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error completing service job");
            }
        }

        [HttpPost("{id:guid}/parts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ServiceJob>> AddPartsToJob(Guid id, [FromBody] List<ServicePart> parts)
        {
            try
            {
                if (parts == null || parts.Count == 0)
                {
                    return BadRequest("Parts data is required");
                }

                var updatedJob = await _serviceJobService.AddPartsAsync(id, parts);
                if (updatedJob == null)
                {
                    return NotFound($"Service Job with ID {id} not found");
                }

                return Ok(updatedJob);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error adding parts to service job with ID: {id}");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error adding parts to service job");
            }
        }
    }

    public class TechnicianAssignmentRequest
    {
        public Guid TechnicianId { get; set; }
    }
}
