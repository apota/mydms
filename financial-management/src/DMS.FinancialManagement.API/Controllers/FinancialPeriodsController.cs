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
    public class FinancialPeriodsController : ControllerBase
    {
        private readonly IFinancialPeriodService _financialPeriodService;
        private readonly ILogger<FinancialPeriodsController> _logger;

        public FinancialPeriodsController(IFinancialPeriodService financialPeriodService, ILogger<FinancialPeriodsController> logger)
        {
            _financialPeriodService = financialPeriodService;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets all financial periods
        /// </summary>
        /// <param name="skip">The number of periods to skip (for pagination)</param>
        /// <param name="take">The number of periods to take (for pagination)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of financial periods</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FinancialPeriodDto>>> GetAllPeriods(
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all financial periods (skip: {Skip}, take: {Take})", skip, take);
            var periods = await _financialPeriodService.GetAllPeriodsAsync(skip, take, cancellationToken);
            return Ok(periods);
        }
        
        /// <summary>
        /// Gets a financial period by ID
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The financial period with the specified ID</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FinancialPeriodDto>> GetPeriodById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting financial period by ID: {Id}", id);
            var period = await _financialPeriodService.GetPeriodByIdAsync(id, cancellationToken);
            
            if (period == null)
            {
                return NotFound();
            }
            
            return Ok(period);
        }
        
        /// <summary>
        /// Gets financial periods by fiscal year
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of financial periods for the specified fiscal year</returns>
        [HttpGet("fiscal-year/{fiscalYear}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<FinancialPeriodDto>>> GetPeriodsByFiscalYear(
            int fiscalYear,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting financial periods for fiscal year: {FiscalYear}", fiscalYear);
            var periods = await _financialPeriodService.GetPeriodsByFiscalYearAsync(fiscalYear, cancellationToken);
            return Ok(periods);
        }
        
        /// <summary>
        /// Gets the current financial period
        /// </summary>
        /// <param name="date">The date to check (optional)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The current financial period</returns>
        [HttpGet("current")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FinancialPeriodDto>> GetCurrentPeriod(
            [FromQuery] DateTime? date = null,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting current financial period for date: {Date}", date ?? DateTime.Today);
            var period = await _financialPeriodService.GetCurrentPeriodAsync(date, cancellationToken);
            
            if (period == null)
            {
                return NotFound("No financial period found for the specified date");
            }
            
            return Ok(period);
        }
        
        /// <summary>
        /// Gets a financial period by fiscal year and period number
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="periodNumber">The period number</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The financial period for the specified year and period number</returns>
        [HttpGet("fiscal-year/{fiscalYear}/period/{periodNumber}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<FinancialPeriodDto>> GetPeriodByYearAndPeriod(
            int fiscalYear,
            int periodNumber,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting financial period for year {FiscalYear} and period {PeriodNumber}", fiscalYear, periodNumber);
            var period = await _financialPeriodService.GetPeriodByYearAndNumberAsync(fiscalYear, periodNumber, cancellationToken);
            
            if (period == null)
            {
                return NotFound();
            }
            
            return Ok(period);
        }
        
        /// <summary>
        /// Creates a new financial period
        /// </summary>
        /// <param name="createDto">The financial period data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created financial period</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FinancialPeriodDto>> CreatePeriod(
            FinancialPeriodCreateDto createDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new financial period for year {FiscalYear}, period {PeriodNumber}", createDto.FiscalYear, createDto.PeriodNumber);
            try
            {
                var createdPeriod = await _financialPeriodService.CreatePeriodAsync(createDto, cancellationToken);
                return CreatedAtAction(nameof(GetPeriodById), new { id = createdPeriod.Id }, createdPeriod);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating financial period");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Updates an existing financial period
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="updateDto">The updated financial period data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated financial period</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FinancialPeriodDto>> UpdatePeriod(
            Guid id,
            FinancialPeriodUpdateDto updateDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating financial period: {Id}", id);
            try
            {
                var updatedPeriod = await _financialPeriodService.UpdatePeriodAsync(id, updateDto, cancellationToken);
                
                if (updatedPeriod == null)
                {
                    return NotFound();
                }
                
                return Ok(updatedPeriod);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating financial period {Id}", id);
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Closes a financial period
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="closeDto">The closing data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The closed financial period</returns>
        [HttpPost("{id}/close")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FinancialPeriodDto>> ClosePeriod(
            Guid id,
            FinancialPeriodCloseDto closeDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Closing financial period: {Id}", id);
            try
            {
                var closedPeriod = await _financialPeriodService.ClosePeriodAsync(id, closeDto, cancellationToken);
                
                if (closedPeriod == null)
                {
                    return NotFound();
                }
                
                return Ok(closedPeriod);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when closing financial period {Id}", id);
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Reopens a financial period
        /// </summary>
        /// <param name="id">The financial period ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The reopened financial period</returns>
        [HttpPost("{id}/reopen")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<FinancialPeriodDto>> ReopenPeriod(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Reopening financial period: {Id}", id);
            try
            {
                var reopenedPeriod = await _financialPeriodService.ReopenPeriodAsync(id, cancellationToken);
                
                if (reopenedPeriod == null)
                {
                    return NotFound();
                }
                
                return Ok(reopenedPeriod);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when reopening financial period {Id}", id);
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Generates financial periods for a fiscal year
        /// </summary>
        /// <param name="fiscalYear">The fiscal year</param>
        /// <param name="startMonth">The starting month (1-12)</param>
        /// <param name="periodCount">The number of periods to generate</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The generated financial periods</returns>
        [HttpPost("generate-for-year/{fiscalYear}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<FinancialPeriodDto>>> GeneratePeriodsForYear(
            int fiscalYear,
            [FromQuery] int startMonth = 1,
            [FromQuery] int periodCount = 12,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Generating financial periods for fiscal year {FiscalYear} (startMonth: {StartMonth}, periodCount: {PeriodCount})", 
                fiscalYear, startMonth, periodCount);
                
            try
            {
                var periods = await _financialPeriodService.GeneratePeriodsForYearAsync(fiscalYear, startMonth, periodCount, cancellationToken);
                return Ok(periods);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when generating financial periods for year {FiscalYear}", fiscalYear);
                return BadRequest(ex.Message);
            }
        }
    }
}
