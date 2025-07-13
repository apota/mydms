using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.FinancialManagement.Core.DTOs;
using DMS.FinancialManagement.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DMS.FinancialManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TaxCodesController : ControllerBase
    {
        private readonly ITaxCodeService _taxCodeService;
        private readonly ILogger<TaxCodesController> _logger;

        public TaxCodesController(ITaxCodeService taxCodeService, ILogger<TaxCodesController> logger)
        {
            _taxCodeService = taxCodeService;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets all tax codes
        /// </summary>
        /// <param name="skip">The number of tax codes to skip (for pagination)</param>
        /// <param name="take">The number of tax codes to take (for pagination)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of tax codes</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TaxCodeDto>>> GetAllTaxCodes(
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all tax codes (skip: {Skip}, take: {Take})", skip, take);
            var taxCodes = await _taxCodeService.GetAllTaxCodesAsync(skip, take, cancellationToken);
            return Ok(taxCodes);
        }
        
        /// <summary>
        /// Gets a tax code by ID
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The tax code with the specified ID</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaxCodeDto>> GetTaxCodeById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting tax code by ID: {Id}", id);
            var taxCode = await _taxCodeService.GetTaxCodeByIdAsync(id, cancellationToken);
            
            if (taxCode == null)
            {
                return NotFound();
            }
            
            return Ok(taxCode);
        }
        
        /// <summary>
        /// Gets a tax code by code
        /// </summary>
        /// <param name="code">The tax code</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The tax code with the specified code</returns>
        [HttpGet("code/{code}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TaxCodeDto>> GetTaxCodeByCode(
            string code,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting tax code by code: {Code}", code);
            var taxCode = await _taxCodeService.GetTaxCodeByCodeAsync(code, cancellationToken);
            
            if (taxCode == null)
            {
                return NotFound();
            }
            
            return Ok(taxCode);
        }
        
        /// <summary>
        /// Gets active tax codes as of a specific date
        /// </summary>
        /// <param name="date">The date to check (defaults to today)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of active tax codes as of the specified date</returns>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TaxCodeDto>>> GetActiveTaxCodes(
            [FromQuery] DateTime? date = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting active tax codes as of date: {Date}", date ?? DateTime.Today);
            var taxCodes = await _taxCodeService.GetActiveTaxCodesAsync(date, cancellationToken);
            return Ok(taxCodes);
        }
        
        /// <summary>
        /// Creates a new tax code
        /// </summary>
        /// <param name="createDto">The tax code data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created tax code</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "FinanceAdmin")]
        public async Task<ActionResult<TaxCodeDto>> CreateTaxCode(
            TaxCodeCreateDto createDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new tax code with code: {Code}", createDto.Code);
            try
            {
                var taxCode = await _taxCodeService.CreateTaxCodeAsync(createDto, cancellationToken);
                return CreatedAtAction(nameof(GetTaxCodeById), new { id = taxCode.Id }, taxCode);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating tax code");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Updates an existing tax code
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="updateDto">The updated tax code data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated tax code</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "FinanceAdmin")]
        public async Task<ActionResult<TaxCodeDto>> UpdateTaxCode(
            Guid id,
            TaxCodeUpdateDto updateDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating tax code: {Id}", id);
            try
            {
                var taxCode = await _taxCodeService.UpdateTaxCodeAsync(id, updateDto, cancellationToken);
                
                if (taxCode == null)
                {
                    return NotFound();
                }
                
                return Ok(taxCode);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating tax code {Id}", id);
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Activates a tax code
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The activated tax code</returns>
        [HttpPatch("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "FinanceAdmin")]
        public async Task<ActionResult<TaxCodeDto>> ActivateTaxCode(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Activating tax code: {Id}", id);
            var taxCode = await _taxCodeService.ActivateTaxCodeAsync(id, cancellationToken);
            
            if (taxCode == null)
            {
                return NotFound();
            }
            
            return Ok(taxCode);
        }
        
        /// <summary>
        /// Deactivates a tax code
        /// </summary>
        /// <param name="id">The tax code ID</param>
        /// <param name="deactivateDto">The deactivation data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The deactivated tax code</returns>
        [HttpPatch("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [Authorize(Policy = "FinanceAdmin")]
        public async Task<ActionResult<TaxCodeDto>> DeactivateTaxCode(
            Guid id,
            TaxCodeDeactivateDto deactivateDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deactivating tax code: {Id}", id);
            var taxCode = await _taxCodeService.DeactivateTaxCodeAsync(id, deactivateDto, cancellationToken);
            
            if (taxCode == null)
            {
                return NotFound();
            }
            
            return Ok(taxCode);
        }
    }
}
