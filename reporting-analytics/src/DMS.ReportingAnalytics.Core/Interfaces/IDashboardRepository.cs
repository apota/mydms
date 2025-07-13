using DMS.ReportingAnalytics.Core.Models;

namespace DMS.ReportingAnalytics.Core.Interfaces;

/// <summary>
/// Interface for dashboard repository operations.
/// </summary>
public interface IDashboardRepository
{
    /// <summary>
    /// Gets all dashboard definitions.
    /// </summary>
    /// <returns>A collection of dashboard definitions.</returns>
    Task<IEnumerable<DashboardDefinition>> GetAllDashboardsAsync();
    
    /// <summary>
    /// Gets a dashboard definition by ID.
    /// </summary>
    /// <param name="dashboardId">The dashboard ID.</param>
    /// <returns>The dashboard definition if found, otherwise null.</returns>
    Task<DashboardDefinition?> GetDashboardByIdAsync(Guid dashboardId);
    
    /// <summary>
    /// Gets dashboards by owner.
    /// </summary>
    /// <param name="owner">The owner's ID.</param>
    /// <returns>A collection of dashboard definitions owned by the specified user.</returns>
    Task<IEnumerable<DashboardDefinition>> GetDashboardsByOwnerAsync(string owner);
    
    /// <summary>
    /// Gets the default dashboard for a user.
    /// </summary>
    /// <param name="owner">The owner's ID.</param>
    /// <returns>The default dashboard definition if found, otherwise null.</returns>
    Task<DashboardDefinition?> GetDefaultDashboardAsync(string owner);
    
    /// <summary>
    /// Creates a new dashboard definition.
    /// </summary>
    /// <param name="dashboard">The dashboard definition to create.</param>
    /// <returns>The created dashboard definition with ID assigned.</returns>
    Task<DashboardDefinition> CreateDashboardAsync(DashboardDefinition dashboard);
    
    /// <summary>
    /// Updates an existing dashboard definition.
    /// </summary>
    /// <param name="dashboard">The dashboard definition to update.</param>
    /// <returns>True if update was successful, otherwise false.</returns>
    Task<bool> UpdateDashboardAsync(DashboardDefinition dashboard);
    
    /// <summary>
    /// Deletes a dashboard definition.
    /// </summary>
    /// <param name="dashboardId">The ID of the dashboard to delete.</param>
    /// <returns>True if deletion was successful, otherwise false.</returns>
    Task<bool> DeleteDashboardAsync(Guid dashboardId);
    
    /// <summary>
    /// Gets all widgets for a dashboard.
    /// </summary>
    /// <param name="dashboardId">The dashboard ID.</param>
    /// <returns>A collection of dashboard widgets for the specified dashboard.</returns>
    Task<IEnumerable<DashboardWidget>> GetDashboardWidgetsAsync(Guid dashboardId);
    
    /// <summary>
    /// Adds a widget to a dashboard.
    /// </summary>
    /// <param name="widget">The widget to add.</param>
    /// <returns>The created widget with ID assigned.</returns>
    Task<DashboardWidget> AddWidgetAsync(DashboardWidget widget);
    
    /// <summary>
    /// Updates a dashboard widget.
    /// </summary>
    /// <param name="widget">The widget to update.</param>
    /// <returns>True if update was successful, otherwise false.</returns>
    Task<bool> UpdateWidgetAsync(DashboardWidget widget);
    
    /// <summary>
    /// Removes a widget from a dashboard.
    /// </summary>
    /// <param name="widgetId">The ID of the widget to remove.</param>
    /// <returns>True if removal was successful, otherwise false.</returns>
    Task<bool> RemoveWidgetAsync(Guid widgetId);
}
