namespace DMS.ReportingAnalytics.Core.Models;

/// <summary>
/// Represents a scheduled report execution.
/// </summary>
public class ScheduledReport
{
    /// <summary>
    /// Unique identifier for the schedule.
    /// </summary>
    public Guid ScheduleId { get; set; }
    
    /// <summary>
    /// Reference to the report to be executed.
    /// </summary>
    public Guid ReportId { get; set; }
    
    /// <summary>
    /// Cron expression defining the execution schedule.
    /// </summary>
    public string Schedule { get; set; } = string.Empty;
    
    /// <summary>
    /// Output format for the report (PDF, Excel, CSV, etc.).
    /// </summary>
    public string Format { get; set; } = string.Empty;
    
    /// <summary>
    /// JSON array of recipients.
    /// </summary>
    public string Recipients { get; set; } = string.Empty;
    
    /// <summary>
    /// Email subject line for report distribution.
    /// </summary>
    public string Subject { get; set; } = string.Empty;
    
    /// <summary>
    /// Email body message for report distribution.
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// Date of the last execution.
    /// </summary>
    public DateTime? LastRunDate { get; set; }
    
    /// <summary>
    /// Date of the next scheduled execution.
    /// </summary>
    public DateTime? NextRunDate { get; set; }
    
    /// <summary>
    /// Current status of the schedule.
    /// </summary>
    public ScheduleStatus Status { get; set; }
    
    /// <summary>
    /// Reference to the report definition.
    /// </summary>
    public ReportDefinition? Report { get; set; }
    
    /// <summary>
    /// Collection of execution records for this scheduled report.
    /// </summary>
    public ICollection<ReportExecutionHistory>? ExecutionHistory { get; set; }
}

/// <summary>
/// Status values for a scheduled report.
/// </summary>
public enum ScheduleStatus
{
    Active,
    Paused,
    Completed,
    Error
}
