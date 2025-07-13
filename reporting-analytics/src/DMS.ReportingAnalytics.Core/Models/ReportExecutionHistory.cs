namespace DMS.ReportingAnalytics.Core.Models;

/// <summary>
/// Represents a record of a report execution.
/// </summary>
public class ReportExecutionHistory
{
    /// <summary>
    /// Unique identifier for the execution.
    /// </summary>
    public Guid ExecutionId { get; set; }
    
    /// <summary>
    /// Reference to the report that was executed.
    /// </summary>
    public Guid ReportId { get; set; }
    
    /// <summary>
    /// Optional reference to the schedule that triggered this execution.
    /// </summary>
    public Guid? ScheduleId { get; set; }
    
    /// <summary>
    /// Identifier of the user who triggered the execution.
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time when the execution occurred.
    /// </summary>
    public DateTime ExecutionDate { get; set; }
    
    /// <summary>
    /// Duration of the execution in milliseconds.
    /// </summary>
    public long Duration { get; set; }
    
    /// <summary>
    /// Status of the execution.
    /// </summary>
    public ExecutionStatus Status { get; set; }
    
    /// <summary>
    /// JSON string of parameters used for this execution.
    /// </summary>
    public string Parameters { get; set; } = string.Empty;
    
    /// <summary>
    /// Location where the output was stored.
    /// </summary>
    public string? OutputLocation { get; set; }
    
    /// <summary>
    /// Error message if the execution failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Reference to the report definition.
    /// </summary>
    public ReportDefinition? Report { get; set; }
    
    /// <summary>
    /// Reference to the schedule that triggered this execution, if any.
    /// </summary>
    public ScheduledReport? Schedule { get; set; }
}

/// <summary>
/// Status values for a report execution.
/// </summary>
public enum ExecutionStatus
{
    Success,
    Failed,
    Canceled,
    Running
}
