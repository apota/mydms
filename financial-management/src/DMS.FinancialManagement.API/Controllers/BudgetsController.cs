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
    public class BudgetsController : ControllerBase
    {
        private readonly IBudgetService _budgetService;
        private readonly ILogger<BudgetsController> _logger;

        public BudgetsController(IBudgetService budgetService, ILogger<BudgetsController> logger)
        {
            _budgetService = budgetService;
            _logger = logger;
        }
        
        /// <summary>
        /// Gets all budgets
        /// </summary>
        /// <param name="year">Optional filter by fiscal year</param>
        /// <param name="skip">The number of budgets to skip (for pagination)</param>
        /// <param name="take">The number of budgets to take (for pagination)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of budgets</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BudgetDto>>> GetAllBudgets(
            [FromQuery] int? year = null,
            [FromQuery] int skip = 0,
            [FromQuery] int take = 50,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting all budgets (year: {Year}, skip: {Skip}, take: {Take})", year, skip, take);
            var budgets = await _budgetService.GetAllBudgetsAsync(year, skip, take, cancellationToken);
            return Ok(budgets);
        }

        /// <summary>
        /// Gets a budget by ID
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The budget with the specified ID</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BudgetDetailDto>> GetBudgetById(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting budget by ID: {Id}", id);
            var budget = await _budgetService.GetBudgetByIdAsync(id, cancellationToken);
            
            if (budget == null)
            {
                return NotFound();
            }
            
            return Ok(budget);
        }

        /// <summary>
        /// Gets budgets by fiscal year
        /// </summary>
        /// <param name="year">The fiscal year</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of budgets for the specified fiscal year</returns>
        [HttpGet("year/{year}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<BudgetDto>>> GetBudgetsByYear(
            int year,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting budgets for fiscal year: {Year}", year);
            var budgets = await _budgetService.GetBudgetsByYearAsync(year, cancellationToken);
            return Ok(budgets);
        }

        /// <summary>
        /// Creates a new budget
        /// </summary>
        /// <param name="createDto">The budget data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The created budget</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "FinanceAdmin")]
        public async Task<ActionResult<BudgetDetailDto>> CreateBudget(
            BudgetCreateDto createDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Creating new budget for year: {Year}, department: {Department}", 
                createDto.FiscalYear, createDto.Department);
            
            try
            {
                var budget = await _budgetService.CreateBudgetAsync(createDto, cancellationToken);
                return CreatedAtAction(nameof(GetBudgetById), new { id = budget.Id }, budget);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when creating budget");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Updates an existing budget
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="updateDto">The updated budget data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated budget</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "FinanceAdmin")]
        public async Task<ActionResult<BudgetDetailDto>> UpdateBudget(
            Guid id,
            BudgetUpdateDto updateDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Updating budget: {Id}", id);
            
            try
            {
                var budget = await _budgetService.UpdateBudgetAsync(id, updateDto, cancellationToken);
                
                if (budget == null)
                {
                    return NotFound();
                }
                
                return Ok(budget);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when updating budget {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Approves a budget
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="approveDto">The approval data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The approved budget</returns>
        [HttpPatch("{id}/approve")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "FinanceManager")]
        public async Task<ActionResult<BudgetDetailDto>> ApproveBudget(
            Guid id,
            BudgetApproveDto approveDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Approving budget: {Id}", id);
            
            try
            {
                var budget = await _budgetService.ApproveBudgetAsync(id, approveDto, cancellationToken);
                
                if (budget == null)
                {
                    return NotFound();
                }
                
                return Ok(budget);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when approving budget {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Rejects a budget
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="rejectDto">The rejection data</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The rejected budget</returns>
        [HttpPatch("{id}/reject")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "FinanceManager")]
        public async Task<ActionResult<BudgetDetailDto>> RejectBudget(
            Guid id,
            BudgetRejectDto rejectDto,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Rejecting budget: {Id}", id);
            
            try
            {
                var budget = await _budgetService.RejectBudgetAsync(id, rejectDto, cancellationToken);
                
                if (budget == null)
                {
                    return NotFound();
                }
                
                return Ok(budget);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when rejecting budget {Id}", id);
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Gets budget comparison with actuals
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Budget comparison report with actuals</returns>
        [HttpGet("{id}/comparison")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BudgetComparisonReportDto>> GetBudgetComparison(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Getting budget comparison for budget: {Id}", id);
            
            var report = await _budgetService.GetBudgetComparisonAsync(id, cancellationToken);
            
            if (report == null)
            {
                return NotFound();
            }
            
            return Ok(report);
        }

        /// <summary>
        /// Deletes a draft budget
        /// </summary>
        /// <param name="id">The budget ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>No content if successful</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Policy = "FinanceAdmin")]
        public async Task<ActionResult> DeleteBudget(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Deleting budget: {Id}", id);
            
            try
            {
                var success = await _budgetService.DeleteBudgetAsync(id, cancellationToken);
                
                if (!success)
                {
                    return NotFound();
                }
                
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid operation when deleting budget {Id}", id);
                return BadRequest(ex.Message);
            }
        }
    }
}
