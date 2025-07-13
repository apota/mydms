using Microsoft.AspNetCore.Mvc;
using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;
using System.Text.Json;

namespace DMS.ReportingAnalytics.API.Controllers;

/// <summary>
/// Controller for managing reports.
/// </summary>
[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportRepository _reportRepository;
    private readonly IReportExecutionEngine _executionEngine;
    private readonly ILogger<ReportsController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportsController"/> class.
    /// </summary>
    /// <param name="reportRepository">The report repository.</param>
    /// <param name="executionEngine">The report execution engine.</param>
    /// <param name="logger">The logger.</param>
    public ReportsController(
        IReportRepository reportRepository,
        IReportExecutionEngine executionEngine,
        ILogger<ReportsController> logger)
    {
        _reportRepository = reportRepository;
        _executionEngine = executionEngine;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets all reports.
    /// </summary>
    /// <returns>A collection of report definitions.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ReportDefinition>>> GetAllReports()
    {
        try
        {
            var reports = await _reportRepository.GetAllReportsAsync();
            return Ok(reports);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving reports");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving reports");
        }
    }
    
    /// <summary>
    /// Gets a report by ID.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>The report definition.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportDefinition>> GetReportById(Guid id)
    {
        try
        {
            var report = await _reportRepository.GetReportByIdAsync(id);
            
            if (report == null)
            {
                return NotFound($"Report with ID {id} not found");
            }
            
            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving report");
        }
    }
    
    /// <summary>
    /// Creates a new report.
    /// </summary>
    /// <param name="report">The report definition to create.</param>
    /// <returns>The created report definition.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ReportDefinition>> CreateReport(ReportDefinition report)
    {
        try
        {
            if (report == null)
            {
                return BadRequest("Report cannot be null");
            }
            
            var createdReport = await _reportRepository.CreateReportAsync(report);
            
            return CreatedAtAction(nameof(GetReportById), new { id = createdReport.ReportId }, createdReport);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating report");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating report");
        }
    }
    
    /// <summary>
    /// Updates an existing report.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <param name="report">The updated report definition.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReport(Guid id, ReportDefinition report)
    {
        try
        {
            if (report == null || id != report.ReportId)
            {
                return BadRequest("Invalid report data");
            }
            
            var success = await _reportRepository.UpdateReportAsync(report);
            
            if (!success)
            {
                return NotFound($"Report with ID {id} not found");
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating report");
        }
    }
    
    /// <summary>
    /// Deletes a report.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReport(Guid id)
    {
        try
        {
            var success = await _reportRepository.DeleteReportAsync(id);
            
            if (!success)
            {
                return NotFound($"Report with ID {id} not found");
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting report");
        }
    }
    
    /// <summary>
    /// Gets all report categories.
    /// </summary>
    /// <returns>A collection of distinct category names.</returns>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetReportCategories()
    {
        try
        {
            var categories = await _reportRepository.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report categories");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving report categories");
        }
    }
    
    /// <summary>
    /// Executes a report.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <param name="parameters">The execution parameters.</param>
    /// <returns>The execution ID.</returns>
    [HttpPost("{id}/execute")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Guid>> ExecuteReport(Guid id, [FromBody] JsonElement parameters)
    {
        try
        {
            var report = await _reportRepository.GetReportByIdAsync(id);
            
            if (report == null)
            {
                return NotFound($"Report with ID {id} not found");
            }
            
            // In a real implementation, you would get the user ID from authentication
            var userId = HttpContext.User.Identity?.Name ?? "anonymous";
            
            var executionId = await _executionEngine.ExecuteReportAsync(id, parameters.ToString(), userId);
            
            return AcceptedAtAction(nameof(GetExecutionStatus), new { id = executionId }, executionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error executing report");
        }
    }
    
    /// <summary>
    /// Gets the status of a report execution.
    /// </summary>
    /// <param name="id">The execution ID.</param>
    /// <returns>The execution status.</returns>
    [HttpGet("executions/{id}/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ExecutionStatus>> GetExecutionStatus(Guid id)
    {
        try
        {
            var status = await _executionEngine.GetExecutionStatusAsync(id);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution status {ExecutionId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving execution status");
        }
    }
    
    /// <summary>
    /// Gets the results of a report execution.
    /// </summary>
    /// <param name="id">The execution ID.</param>
    /// <returns>The execution results.</returns>
    [HttpGet("executions/{id}/results")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<string>> GetExecutionResults(Guid id)
    {
        try
        {
            var results = await _executionEngine.GetExecutionResultsAsync(id);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution results {ExecutionId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving execution results");
        }
    }
    
    /// <summary>
    /// Gets the execution history for a report.
    /// </summary>
    /// <param name="id">The report ID.</param>
    /// <returns>A collection of execution history records.</returns>
    [HttpGet("{id}/executions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<ReportExecutionHistory>>> GetReportExecutionHistory(Guid id)
    {
        try
        {
            var report = await _reportRepository.GetReportByIdAsync(id);
            
            if (report == null)
            {
                return NotFound($"Report with ID {id} not found");
            }
            
            var history = await _reportRepository.GetExecutionHistoryAsync(id);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving execution history for report {ReportId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving execution history");
        }
    }
}
