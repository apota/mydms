namespace DMS.ReportingAnalytics.Core.Models;

/// <summary>
/// Represents a data mart definition for analytics.
/// </summary>
public class DataMartDefinition
{
    /// <summary>
    /// Unique identifier for the data mart.
    /// </summary>
    public Guid MartId { get; set; }
    
    /// <summary>
    /// Name of the data mart.
    /// </summary>
    public string MartName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the data mart's purpose and content.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Cron expression defining the refresh schedule.
    /// </summary>
    public string RefreshSchedule { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time of the last refresh.
    /// </summary>
    public DateTime? LastRefreshDate { get; set; }
    
    /// <summary>
    /// Current status of the data mart.
    /// </summary>
    public DataMartStatus Status { get; set; }
    
    /// <summary>
    /// JSON string describing dependencies on other data sources.
    /// </summary>
    public string Dependencies { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON string containing configuration details for the data mart.
    /// </summary>
    public string Configuration { get; set; } = string.Empty;
}

/// <summary>
/// Status values for a data mart.
/// </summary>
public enum DataMartStatus
{
    Active,
    Building,
    Failed,
    Disabled
}
