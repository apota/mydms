namespace DMS.ReportingAnalytics.Core.Models;

/// <summary>
/// Represents a widget on a dashboard.
/// </summary>
public class DashboardWidget
{
    /// <summary>
    /// Unique identifier for the widget.
    /// </summary>
    public Guid WidgetId { get; set; }
    
    /// <summary>
    /// Reference to the dashboard this widget belongs to.
    /// </summary>
    public Guid DashboardId { get; set; }
    
    /// <summary>
    /// Type of widget (Chart, Table, KPI, etc.).
    /// </summary>
    public WidgetType WidgetType { get; set; }
    
    /// <summary>
    /// Data source for the widget (can be a report ID or direct query).
    /// </summary>
    public string DataSource { get; set; } = string.Empty;
    
    /// <summary>
    /// Title of the widget.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON string describing the position of the widget.
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON string describing the size of the widget.
    /// </summary>
    public string Size { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON string containing configuration details for the widget.
    /// </summary>
    public string Configuration { get; set; } = string.Empty;
    
    /// <summary>
    /// Interval in seconds for refreshing the widget data.
    /// </summary>
    public int RefreshInterval { get; set; }
    
    /// <summary>
    /// Reference to the dashboard this widget belongs to.
    /// </summary>
    public DashboardDefinition? Dashboard { get; set; }
}

/// <summary>
/// Types of dashboard widgets.
/// </summary>
public enum WidgetType
{
    Chart,
    Table,
    KPI,
    Gauge,
    List,
    Text,
    Custom
}
