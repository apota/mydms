using DMS.PartsManagement.Core.Services;
using DMS.PartsManagement.Core.DTOs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace DMS.PartsManagement.API.Controllers
{
    [ApiController]
    [Route("api/parts")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;
        private readonly ILogger<PartsController> _logger;

        public PartsController(IPartService partService, ILogger<PartsController> logger)
        {
            _partService = partService ?? throw new ArgumentNullException(nameof(partService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Get all parts with pagination
        /// </summary>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parts</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartSummaryDto>>> GetAllParts(
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all parts with skip: {Skip}, take: {Take}", skip, take);
            var parts = await _partService.GetAllPartsAsync(skip, take, cancellationToken);
            return Ok(parts);
        }

        /// <summary>
        /// Get part by ID
        /// </summary>
        /// <param name="id">Part ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Part details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartDetailDto>> GetPartById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting part by ID: {Id}", id);
            var part = await _partService.GetPartByIdAsync(id, cancellationToken);

            if (part == null)
            {
                _logger.LogWarning("Part with ID {Id} not found", id);
                return NotFound();
            }

            return Ok(part);
        }

        /// <summary>
        /// Search parts by term
        /// </summary>
        /// <param name="term">Search term (part number, description, etc.)</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parts matching the search term</returns>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartSummaryDto>>> SearchParts(
            [FromQuery] string term,
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Searching parts with term: {Term}, skip: {Skip}, take: {Take}", term, skip, take);
            var parts = await _partService.SearchPartsAsync(term, skip, take, cancellationToken);
            return Ok(parts);
        }

        /// <summary>
        /// Get part by part number
        /// </summary>
        /// <param name="partNumber">Part number</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Part details</returns>
        [HttpGet("number/{partNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PartDetailDto>> GetPartByPartNumber(
            string partNumber,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting part by part number: {PartNumber}", partNumber);
            var part = await _partService.GetPartByPartNumberAsync(partNumber, cancellationToken);
            
            if (part == null)
            {
                _logger.LogWarning("Part with part number {PartNumber} not found", partNumber);
                return NotFound();
            }
              return Ok(part);
        }

        /// <summary>
        /// Get parts by category
        /// </summary>
        /// <param name="categoryId">Category ID</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parts in the category</returns>
        [HttpGet("category/{categoryId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartSummaryDto>>> GetPartsByCategory(
            Guid categoryId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting parts by category ID: {CategoryId}, skip: {Skip}, take: {Take}", 
                categoryId, skip, take);
            var parts = await _partService.GetPartsByCategoryAsync(categoryId, skip, take, cancellationToken);
            return Ok(parts);
        }

        /// <summary>
        /// Get parts by manufacturer
        /// </summary>
        /// <param name="manufacturerId">Manufacturer ID</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parts from the manufacturer</returns>
        [HttpGet("manufacturer/{manufacturerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PartSummaryDto>>> GetPartsByManufacturer(
            Guid manufacturerId,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting parts by manufacturer ID: {ManufacturerId}, skip: {Skip}, take: {Take}", 
                manufacturerId, skip, take);
            var parts = await _partService.GetPartsByManufacturerAsync(manufacturerId, skip, take, cancellationToken);
            return Ok(parts);
        }

        /// <summary>
        /// Find parts by vehicle fitment
        /// </summary>
        /// <param name="year">Vehicle year</param>
        /// <param name="make">Vehicle make</param>
        /// <param name="model">Vehicle model</param>
        /// <param name="trim">Vehicle trim (optional)</param>
        /// <param name="engine">Vehicle engine (optional)</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parts that fit the vehicle</returns>
        [HttpGet("vehicle/fitment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<PartSummaryDto>>> FindPartsByVehicleFitment(
            [FromQuery, Required] int year,
            [FromQuery, Required] string make,
            [FromQuery, Required] string model,
            [FromQuery] string? trim = null,
            [FromQuery] string? engine = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Finding parts by vehicle fitment: {Year} {Make} {Model}", year, make, model);
                var parts = await _partService.FindPartsByVehicleFitmentAsync(year, make, model, trim, engine, skip, take, cancellationToken);
                return Ok(parts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finding parts by vehicle fitment");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get similar parts that can be cross-sold
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="take">Number of similar parts to return</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of similar parts</returns>
        [HttpGet("{partId}/similar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PartSummaryDto>>> GetSimilarParts(
            Guid partId,
            [FromQuery] int take = 10,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting similar parts for part ID: {PartId}", partId);
            
            if (!await _partService.PartExistsAsync(partId, cancellationToken))
            {
                _logger.LogWarning("Part with ID {Id} not found", partId);
                return NotFound();
            }
            
            var parts = await _partService.GetSimilarPartsAsync(partId, take, cancellationToken);
            return Ok(parts);
        }

        /// <summary>
        /// Get supersession chain for a part
        /// </summary>
        /// <param name="id">Part ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of parts in the supersession chain</returns>
        [HttpGet("{id}/supersessions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PartSummaryDto>>> GetSupersessionChain(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting supersession chain for part with ID: {Id}", id);
            
            if (!await _partService.PartExistsAsync(id, cancellationToken))
            {
                _logger.LogWarning("Part with ID {Id} not found", id);
                return NotFound();
            }
            
            var parts = await _partService.GetSupersessionChainAsync(id, cancellationToken);
            return Ok(parts);
        }

        /// <summary>
        /// Create a new part
        /// </summary>
        /// <param name="createPartDto">Part creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created part</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<ActionResult<PartDetailDto>> CreatePart(
            CreatePartDto createPartDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Creating part with part number: {PartNumber}", createPartDto.PartNumber);
                
                // Check if part with same number already exists
                if (await _partService.PartExistsByPartNumberAsync(createPartDto.PartNumber, cancellationToken))
                {
                    _logger.LogWarning("Part with part number {PartNumber} already exists", createPartDto.PartNumber);
                    return BadRequest($"Part with part number {createPartDto.PartNumber} already exists");
                }
                
                var part = await _partService.CreatePartAsync(createPartDto, cancellationToken);
                return CreatedAtAction(nameof(GetPartById), new { id = part.Id }, part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating part");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Update an existing part
        /// </summary>
        /// <param name="id">Part ID</param>
        /// <param name="updatePartDto">Part update data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated part</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<ActionResult<PartDetailDto>> UpdatePart(
            Guid id,
            UpdatePartDto updatePartDto,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Updating part with ID: {Id}", id);
                
                if (!await _partService.PartExistsAsync(id, cancellationToken))
                {
                    _logger.LogWarning("Part with ID {Id} not found", id);
                    return NotFound();
                }
                
                var part = await _partService.UpdatePartAsync(id, updatePartDto, cancellationToken);
                
                if (part == null)
                {
                    _logger.LogWarning("Part with ID {Id} could not be updated", id);
                    return NotFound();
                }
                
                return Ok(part);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating part with ID: {Id}", id);
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Delete a part
        /// </summary>
        /// <param name="id">Part ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "PartsManager, Admin")]
        public async Task<IActionResult> DeletePart(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                _logger.LogInformation("Deleting part with ID: {Id}", id);
                
                if (!await _partService.PartExistsAsync(id, cancellationToken))
                {
                    _logger.LogWarning("Part with ID {Id} not found", id);
                    return NotFound();
                }
                
                var result = await _partService.DeletePartAsync(id, cancellationToken);
                
                if (!result)
                {
                    _logger.LogWarning("Part with ID {Id} could not be deleted", id);
                    return BadRequest("Part could not be deleted. It may have related records.");
                }
                
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting part with ID: {Id}", id);
                return BadRequest(ex.Message);            }
        }
        
        /// <summary>
        /// Get total count of parts in the system
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Total part count</returns>
        [HttpGet("count")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<int>> GetTotalPartCount(
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting total part count");
            var count = await _partService.GetTotalPartCountAsync(cancellationToken);
            return Ok(count);
        }
    }
}
