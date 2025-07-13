using DMS.PartsManagement.Core.Services;
using DMS.PartsManagement.Core.DTOs;
using DMS.PartsManagement.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace DMS.PartsManagement.API.Controllers
{
    [ApiController]
    [Route("api/cores")]
    public class CoreTrackingController : ControllerBase
    {
        private readonly ICoreTrackingService _coreTrackingService;
        private readonly ILogger<CoreTrackingController> _logger;

        public CoreTrackingController(ICoreTrackingService coreTrackingService, ILogger<CoreTrackingController> logger)
        {
            _coreTrackingService = coreTrackingService ?? throw new ArgumentNullException(nameof(coreTrackingService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all core tracking records with pagination
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of core tracking records</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CoreTrackingDto>>> GetAllCoreTracking(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all core tracking records with skip: {Skip}, take: {Take}", skip, take);
            var cores = await _coreTrackingService.GetAllCoreTrackingAsync(skip, take, cancellationToken);
            return Ok(cores);
        }

        /// <summary>
        /// Get core tracking record by ID
        /// </summary>
        /// <param name="id">Core tracking ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Core tracking details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CoreTrackingDto>> GetCoreTrackingById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting core tracking by ID: {Id}", id);
            var core = await _coreTrackingService.GetCoreTrackingByIdAsync(id, cancellationToken);

            if (core == null)
            {
                _logger.LogWarning("Core tracking record with ID {Id} not found", id);
                return NotFound();
            }

            return Ok(core);
        }

        /// <summary>
        /// Get core tracking records by part ID
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of core tracking records for the specified part</returns>
        [HttpGet("part/{partId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CoreTrackingDto>>> GetCoreTrackingByPartId(
            Guid partId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting core tracking for part ID: {PartId}", partId);
            var cores = await _coreTrackingService.GetCoreTrackingByPartIdAsync(partId, skip, take, cancellationToken);
            return Ok(cores);
        }

        /// <summary>
        /// Get core tracking records by status
        /// </summary>
        /// <param name="status">Core tracking status</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of core tracking records with the specified status</returns>
        [HttpGet("status/{status}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<CoreTrackingDto>>> GetCoreTrackingByStatus(
            CoreTrackingStatus status,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting core tracking with status: {Status}", status);
            var cores = await _coreTrackingService.GetCoreTrackingByStatusAsync(status, skip, take, cancellationToken);
            return Ok(cores);
        }

        /// <summary>
        /// Create a new core tracking record
        /// </summary>
        /// <param name="createCoreTrackingDto">Core tracking creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created core tracking record</returns>
        [HttpPost("track")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, PartsSales, Admin")]
        public async Task<ActionResult<CoreTrackingDto>> CreateCoreTracking(
            CreateCoreTrackingDto createCoreTrackingDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating core tracking for part ID: {PartId}", createCoreTrackingDto.PartId);
                var core = await _coreTrackingService.CreateCoreTrackingAsync(createCoreTrackingDto, cancellationToken);
                return CreatedAtAction(nameof(GetCoreTrackingById), new { id = core.Id }, core);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating core tracking record");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Process core return
        /// </summary>
        /// <param name="id">Core tracking ID</param>
        /// <param name="processReturnDto">Core return processing data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated core tracking record</returns>
        [HttpPut("{id}/return")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, PartsSales, Admin")]
        public async Task<ActionResult<CoreTrackingDto>> ProcessCoreReturn(
            Guid id,
            ProcessCoreReturnDto processReturnDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Processing return for core tracking ID: {Id}", id);
                var core = await _coreTrackingService.ProcessCoreReturnAsync(id, processReturnDto, cancellationToken);
                
                if (core == null)
                {
                    _logger.LogWarning("Core tracking record with ID {Id} not found", id);
                    return NotFound();
                }
                
                return Ok(core);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing core return for ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Apply credit for returned core
        /// </summary>
        /// <param name="id">Core tracking ID</param>
        /// <param name="applyCreditDto">Credit application data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated core tracking record</returns>
        [HttpPut("{id}/credit")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<ActionResult<CoreTrackingDto>> ApplyCredit(
            Guid id,
            ApplyCreditDto applyCreditDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Applying credit for core tracking ID: {Id}", id);
                var core = await _coreTrackingService.ApplyCreditAsync(id, applyCreditDto, cancellationToken);
                
                if (core == null)
                {
                    _logger.LogWarning("Core tracking record with ID {Id} not found", id);
                    return NotFound();
                }
                
                return Ok(core);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying credit for core tracking ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get total outstanding core value
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total outstanding core value</returns>
        [HttpGet("outstanding-value")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<decimal>> GetTotalOutstandingCoreValue(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting total outstanding core value");
            var value = await _coreTrackingService.GetTotalOutstandingCoreValueAsync(cancellationToken);
            return Ok(value);
        }
    }
}
