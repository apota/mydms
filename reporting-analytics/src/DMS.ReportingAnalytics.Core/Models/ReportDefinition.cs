namespace DMS.ReportingAnalytics.Core.Models;

/// <summary>
/// Represents a report definition in the reporting system.
/// </summary>
public class ReportDefinition
{
    /// <summary>
    /// Unique identifier for the report.
    /// </summary>
    public Guid ReportId { get; set; }
    
    /// <summary>
    /// Name of the report.
    /// </summary>
    public string ReportName { get; set; } = string.Empty;
    
    /// <summary>
    /// Description of the report's purpose and content.
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Category of the report for organization purposes.
    /// </summary>
    public string Category { get; set; } = string.Empty;
    
    /// <summary>
    /// User who owns the report.
    /// </summary>
    public string Owner { get; set; } = string.Empty;
    
    /// <summary>
    /// Date when the report was created.
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Date when the report was last modified.
    /// </summary>
    public DateTime ModifiedDate { get; set; }
    
    /// <summary>
    /// Indicates if this is a system report (true) or user-created report (false).
    /// </summary>
    public bool IsSystem { get; set; }
    
    /// <summary>
    /// Status of the report (Active, Draft, Archived).
    /// </summary>
    public ReportStatus Status { get; set; }
    
    /// <summary>
    /// JSON string containing the report parameters.
    /// </summary>
    public string Parameters { get; set; } = string.Empty;
    
    /// <summary>
    /// Source query for the report.
    /// </summary>
    public string SourceQuery { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON string containing permissions for the report.
    /// </summary>
    public string Permissions { get; set; } = string.Empty;
    
    /// <summary>
    /// Collection of scheduled executions for this report.
    /// </summary>
    public ICollection<ScheduledReport>? ScheduledReports { get; set; }
    
    /// <summary>
    /// Collection of execution records for this report.
    /// </summary>
    public ICollection<ReportExecutionHistory>? ExecutionHistory { get; set; }
}

/// <summary>
/// Status values for a report.
/// </summary>
public enum ReportStatus
{
    Draft,
    Active,
    Archived
}
