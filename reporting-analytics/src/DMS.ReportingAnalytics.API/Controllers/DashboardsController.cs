using Microsoft.AspNetCore.Mvc;
using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;
using System.Text.Json;

namespace DMS.ReportingAnalytics.API.Controllers;

/// <summary>
/// Controller for managing dashboards.
/// </summary>
[ApiController]
[Route("api/dashboards")]
public class DashboardsController : ControllerBase
{
    private readonly IDashboardRepository _dashboardRepository;
    private readonly ILogger<DashboardsController> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardsController"/> class.
    /// </summary>
    /// <param name="dashboardRepository">The dashboard repository.</param>
    /// <param name="logger">The logger.</param>
    public DashboardsController(
        IDashboardRepository dashboardRepository,
        ILogger<DashboardsController> logger)
    {
        _dashboardRepository = dashboardRepository;
        _logger = logger;
    }
    
    /// <summary>
    /// Gets all dashboards.
    /// </summary>
    /// <returns>A collection of dashboard definitions.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DashboardDefinition>>> GetAllDashboards()
    {
        try
        {
            var dashboards = await _dashboardRepository.GetAllDashboardsAsync();
            return Ok(dashboards);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboards");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving dashboards");
        }
    }
    
    /// <summary>
    /// Gets a dashboard by ID.
    /// </summary>
    /// <param name="id">The dashboard ID.</param>
    /// <returns>The dashboard definition.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardDefinition>> GetDashboardById(Guid id)
    {
        try
        {
            var dashboard = await _dashboardRepository.GetDashboardByIdAsync(id);
            
            if (dashboard == null)
            {
                return NotFound($"Dashboard with ID {id} not found");
            }
            
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard {DashboardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving dashboard");
        }
    }
    
    /// <summary>
    /// Creates a new dashboard.
    /// </summary>
    /// <param name="dashboard">The dashboard definition to create.</param>
    /// <returns>The created dashboard definition.</returns>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DashboardDefinition>> CreateDashboard(DashboardDefinition dashboard)
    {
        try
        {
            if (dashboard == null)
            {
                return BadRequest("Dashboard cannot be null");
            }
            
            var createdDashboard = await _dashboardRepository.CreateDashboardAsync(dashboard);
            
            return CreatedAtAction(nameof(GetDashboardById), new { id = createdDashboard.DashboardId }, createdDashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating dashboard");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error creating dashboard");
        }
    }
    
    /// <summary>
    /// Updates an existing dashboard.
    /// </summary>
    /// <param name="id">The dashboard ID.</param>
    /// <param name="dashboard">The updated dashboard definition.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDashboard(Guid id, DashboardDefinition dashboard)
    {
        try
        {
            if (dashboard == null || id != dashboard.DashboardId)
            {
                return BadRequest("Invalid dashboard data");
            }
            
            var success = await _dashboardRepository.UpdateDashboardAsync(dashboard);
            
            if (!success)
            {
                return NotFound($"Dashboard with ID {id} not found");
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating dashboard {DashboardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating dashboard");
        }
    }
    
    /// <summary>
    /// Deletes a dashboard.
    /// </summary>
    /// <param name="id">The dashboard ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDashboard(Guid id)
    {
        try
        {
            var success = await _dashboardRepository.DeleteDashboardAsync(id);
            
            if (!success)
            {
                return NotFound($"Dashboard with ID {id} not found");
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting dashboard {DashboardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error deleting dashboard");
        }
    }
    
    /// <summary>
    /// Gets all available widget types.
    /// </summary>
    /// <returns>A collection of widget types.</returns>
    [HttpGet("widgets")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<IEnumerable<string>> GetWidgetTypes()
    {
        try
        {
            var widgetTypes = Enum.GetNames(typeof(WidgetType));
            return Ok(widgetTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving widget types");
            return StatusCode(StatusCodes.Status500InternalServerError, "Error retrieving widget types");
        }
    }
    
    /// <summary>
    /// Adds a widget to a dashboard.
    /// </summary>
    /// <param name="id">The dashboard ID.</param>
    /// <param name="widget">The widget to add.</param>
    /// <returns>The created widget.</returns>
    [HttpPost("{id}/widgets")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DashboardWidget>> AddWidgetToDashboard(Guid id, DashboardWidget widget)
    {
        try
        {
            if (widget == null)
            {
                return BadRequest("Widget cannot be null");
            }
            
            var dashboard = await _dashboardRepository.GetDashboardByIdAsync(id);
            
            if (dashboard == null)
            {
                return NotFound($"Dashboard with ID {id} not found");
            }
            
            widget.DashboardId = id;
            var createdWidget = await _dashboardRepository.AddWidgetAsync(widget);
            
            return CreatedAtAction(nameof(GetDashboardById), new { id = id }, createdWidget);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding widget to dashboard {DashboardId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error adding widget to dashboard");
        }
    }
    
    /// <summary>
    /// Updates a widget on a dashboard.
    /// </summary>
    /// <param name="id">The dashboard ID.</param>
    /// <param name="widgetId">The widget ID.</param>
    /// <param name="widget">The updated widget.</param>
    /// <returns>No content if successful.</returns>
    [HttpPut("{id}/widgets/{widgetId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDashboardWidget(Guid id, Guid widgetId, DashboardWidget widget)
    {
        try
        {
            if (widget == null || widgetId != widget.WidgetId || id != widget.DashboardId)
            {
                return BadRequest("Invalid widget data");
            }
            
            var success = await _dashboardRepository.UpdateWidgetAsync(widget);
            
            if (!success)
            {
                return NotFound($"Widget with ID {widgetId} not found");
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating widget {WidgetId} on dashboard {DashboardId}", widgetId, id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error updating widget");
        }
    }
    
    /// <summary>
    /// Removes a widget from a dashboard.
    /// </summary>
    /// <param name="id">The dashboard ID.</param>
    /// <param name="widgetId">The widget ID.</param>
    /// <returns>No content if successful.</returns>
    [HttpDelete("{id}/widgets/{widgetId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveWidgetFromDashboard(Guid id, Guid widgetId)
    {
        try
        {
            var widgets = await _dashboardRepository.GetDashboardWidgetsAsync(id);
            
            if (!widgets.Any(w => w.WidgetId == widgetId))
            {
                return NotFound($"Widget with ID {widgetId} not found on dashboard {id}");
            }
            
            var success = await _dashboardRepository.RemoveWidgetAsync(widgetId);
            
            if (!success)
            {
                return NotFound($"Widget with ID {widgetId} not found");
            }
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing widget {WidgetId} from dashboard {DashboardId}", widgetId, id);
            return StatusCode(StatusCodes.Status500InternalServerError, "Error removing widget");
        }
    }
}
