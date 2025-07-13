namespace DMS.ReportingAnalytics.Core.Models;

/// <summary>
/// Represents a dashboard definition in the reporting system.
/// </summary>
public class DashboardDefinition
{
    /// <summary>
    /// Unique identifier for the dashboard.
    /// </summary>
    public Guid DashboardId { get; set; }
    
    /// <summary>
    /// Name of the dashboard.
    /// </summary>
    public string DashboardName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the dashboard's purpose and content.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// User who owns the dashboard.
    /// </summary>
    public string Owner { get; set; } = string.Empty;
    
    /// <summary>
    /// Date when the dashboard was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Date when the dashboard was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
    
    /// <summary>
    /// JSON string describing the layout of widgets.
    /// </summary>
    public string Layout { get; set; } = string.Empty;
    
    /// <summary>
    /// Indicates if this is the default dashboard.
    /// </summary>
    public bool IsDefault { get; set; }
    
    /// <summary>
    /// Status of the dashboard (Active, Draft, Archived).
    /// </summary>
    public DashboardStatus Status { get; set; }
    
    /// <summary>
    /// JSON string containing permissions for the dashboard.
    /// </summary>
    public string Permissions { get; set; } = string.Empty;
    
    /// <summary>
    /// Collection of widgets contained in this dashboard.
    /// </summary>
    public ICollection<DashboardWidget>? Widgets { get; set; }
}

/// <summary>
/// Status values for a dashboard.
/// </summary>
public enum DashboardStatus
{
    Draft,
    Active,
    Archived
}
