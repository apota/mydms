using Microsoft.EntityFrameworkCore;
using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;
using DMS.ReportingAnalytics.Infrastructure.Data;

namespace DMS.ReportingAnalytics.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for dashboard operations.
/// </summary>
public class DashboardRepository : IDashboardRepository
{
    private readonly ReportingDbContext _dbContext;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public DashboardRepository(ReportingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<DashboardDefinition>> GetAllDashboardsAsync()
    {
        return await _dbContext.Dashboards.ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<DashboardDefinition?> GetDashboardByIdAsync(Guid dashboardId)
    {
        return await _dbContext.Dashboards
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.DashboardId == dashboardId);
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<DashboardDefinition>> GetDashboardsByOwnerAsync(string owner)
    {
        return await _dbContext.Dashboards
            .Where(d => d.Owner == owner)
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<DashboardDefinition?> GetDefaultDashboardAsync(string owner)
    {
        return await _dbContext.Dashboards
            .Include(d => d.Widgets)
            .FirstOrDefaultAsync(d => d.Owner == owner && d.IsDefault);
    }
    
    /// <inheritdoc/>
    public async Task<DashboardDefinition> CreateDashboardAsync(DashboardDefinition dashboard)
    {
        dashboard.CreatedDate = DateTime.UtcNow;
        dashboard.ModifiedDate = DateTime.UtcNow;
        
        _dbContext.Dashboards.Add(dashboard);
        await _dbContext.SaveChangesAsync();
        
        return dashboard;
    }
    
    /// <inheritdoc/>
    public async Task<bool> UpdateDashboardAsync(DashboardDefinition dashboard)
    {
        var existingDashboard = await _dbContext.Dashboards.FindAsync(dashboard.DashboardId);
        
        if (existingDashboard == null)
        {
            return false;
        }
        
        existingDashboard.DashboardName = dashboard.DashboardName;
        existingDashboard.Description = dashboard.Description;
        existingDashboard.Layout = dashboard.Layout;
        existingDashboard.IsDefault = dashboard.IsDefault;
        existingDashboard.Status = dashboard.Status;
        existingDashboard.Permissions = dashboard.Permissions;
        existingDashboard.ModifiedDate = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> DeleteDashboardAsync(Guid dashboardId)
    {
        var dashboard = await _dbContext.Dashboards.FindAsync(dashboardId);
        
        if (dashboard == null)
        {
            return false;
        }
        
        _dbContext.Dashboards.Remove(dashboard);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<DashboardWidget>> GetDashboardWidgetsAsync(Guid dashboardId)
    {
        return await _dbContext.DashboardWidgets
            .Where(w => w.DashboardId == dashboardId)
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<DashboardWidget> AddWidgetAsync(DashboardWidget widget)
    {
        _dbContext.DashboardWidgets.Add(widget);
        await _dbContext.SaveChangesAsync();
        
        return widget;
    }
    
    /// <inheritdoc/>
    public async Task<bool> UpdateWidgetAsync(DashboardWidget widget)
    {
        var existingWidget = await _dbContext.DashboardWidgets.FindAsync(widget.WidgetId);
        
        if (existingWidget == null)
        {
            return false;
        }
        
        existingWidget.Title = widget.Title;
        existingWidget.WidgetType = widget.WidgetType;
        existingWidget.DataSource = widget.DataSource;
        existingWidget.Position = widget.Position;
        existingWidget.Size = widget.Size;
        existingWidget.Configuration = widget.Configuration;
        existingWidget.RefreshInterval = widget.RefreshInterval;
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> RemoveWidgetAsync(Guid widgetId)
    {
        var widget = await _dbContext.DashboardWidgets.FindAsync(widgetId);
        
        if (widget == null)
        {
            return false;
        }
        
        _dbContext.DashboardWidgets.Remove(widget);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
}
