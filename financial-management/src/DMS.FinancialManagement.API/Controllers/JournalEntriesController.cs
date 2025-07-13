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
    public class JournalEntriesController : ControllerBase
    {
        private readonly IJournalEntryService _journalEntryService;
        private readonly ILogger<JournalEntriesController> _logger;

        public JournalEntriesController(IJournalEntryService journalEntryService, ILogger<JournalEntriesController> logger)
        {
            _journalEntryService = journalEntryService;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets all journal entries
        /// </summary>
        /// <param name="skip">The number of entries to skip (for pagination)</param>
        /// <param name="take">The number of entries to take (for pagination)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of journal entry summaries</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JournalEntrySummaryDto>>> GetAllEntries(
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all journal entries (skip: {Skip}, take: {Take})", skip, take);
            var entries = await _journalEntryService.GetAllEntriesAsync(skip, take, cancellationToken);
            return Ok(entries);
        }
        
        /// <summary>
        /// Gets a journal entry by ID
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The journal entry with the specified ID</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<JournalEntryDto>> GetEntryById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting journal entry by ID: {Id}", id);
            var entry = await _journalEntryService.GetEntryByIdAsync(id, cancellationToken);
            
            if (entry == null)
            {
                return NotFound();
            }
            
            return Ok(entry);
        }
        
        /// <summary>
        /// Gets journal entries by date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="skip">The number of entries to skip</param>
        /// <param name="take">The number of entries to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of journal entry summaries in the specified date range</returns>
        [HttpGet("date-range")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<JournalEntrySummaryDto>>> GetEntriesByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate, 
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            if (startDate > endDate)
            {
                return BadRequest("Start date must be before or equal to end date");
            }
            
            _logger.LogInformation("Getting journal entries by date range: {StartDate} to {EndDate}", startDate, endDate);
            var entries = await _journalEntryService.GetEntriesByDateRangeAsync(startDate, endDate, skip, take, cancellationToken);
            return Ok(entries);
        }
        
        /// <summary>
        /// Gets journal entries by financial period
        /// </summary>
        /// <param name="financialPeriodId">The financial period ID</param>
        /// <param name="skip">The number of entries to skip</param>
        /// <param name="take">The number of entries to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of journal entry summaries in the specified financial period</returns>
        [HttpGet("financial-period/{financialPeriodId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<JournalEntrySummaryDto>>> GetEntriesByFinancialPeriod(
            Guid financialPeriodId, 
            [FromQuery] int skip = 0, 
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting journal entries by financial period: {FinancialPeriodId}", financialPeriodId);
            var entries = await _journalEntryService.GetEntriesByFinancialPeriodAsync(financialPeriodId, skip, take, cancellationToken);
            return Ok(entries);
        }
        
        /// <summary>
        /// Creates a new journal entry
        /// </summary>
        /// <param name="createDto">The journal entry data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created journal entry</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<JournalEntryDto>> CreateEntry(
            JournalEntryCreateDto createDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new journal entry");
            try
            {
                var createdEntry = await _journalEntryService.CreateEntryAsync(createDto, cancellationToken);
                return CreatedAtAction(nameof(GetEntryById), new { id = createdEntry.Id }, createdEntry);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating journal entry");
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Updates an existing journal entry
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="updateDto">The updated journal entry data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated journal entry</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<JournalEntryDto>> UpdateEntry(
            Guid id,
            JournalEntryUpdateDto updateDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating journal entry: {Id}", id);
            try
            {
                var updatedEntry = await _journalEntryService.UpdateEntryAsync(id, updateDto, cancellationToken);
                
                if (updatedEntry == null)
                {
                    return NotFound();
                }
                
                return Ok(updatedEntry);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating journal entry {Id}", id);
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Deletes a journal entry
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteEntry(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting journal entry: {Id}", id);
            try
            {
                var result = await _journalEntryService.DeleteEntryAsync(id, cancellationToken);
                
                if (!result)
                {
                    return NotFound();
                }
                
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when deleting journal entry {Id}", id);
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Posts a journal entry
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="postDto">The posting data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The posted journal entry</returns>
        [HttpPost("{id}/post")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<JournalEntryDto>> PostEntry(
            Guid id,
            JournalEntryPostDto postDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Posting journal entry: {Id}", id);
            try
            {
                var postedEntry = await _journalEntryService.PostEntryAsync(id, postDto, cancellationToken);
                
                if (postedEntry == null)
                {
                    return NotFound();
                }
                
                return Ok(postedEntry);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when posting journal entry {Id}", id);
                return BadRequest(ex.Message);
            }
        }
        
        /// <summary>
        /// Reverses a journal entry
        /// </summary>
        /// <param name="id">The journal entry ID</param>
        /// <param name="reverseDto">The reversal data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The reversal journal entry</returns>
        [HttpPost("{id}/reverse")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<JournalEntryDto>> ReverseEntry(
            Guid id,
            JournalEntryReverseDto reverseDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Reversing journal entry: {Id}", id);
            try
            {
                var reversalEntry = await _journalEntryService.ReverseEntryAsync(id, reverseDto, cancellationToken);
                
                if (reversalEntry == null)
                {
                    return NotFound();
                }
                
                return Ok(reversalEntry);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when reversing journal entry {Id}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}
