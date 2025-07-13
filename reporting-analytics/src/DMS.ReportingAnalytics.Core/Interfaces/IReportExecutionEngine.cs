namespace DMS.ReportingAnalytics.Core.Interfaces;

/// <summary>
/// Interface for report execution engine.
/// </summary>
public interface IReportExecutionEngine
{
    /// <summary>
    /// Executes a report with the specified parameters.
    /// </summary>
    /// <param name="reportId">The ID of the report to execute.</param>
    /// <param name="parameters">JSON string containing execution parameters.</param>
    /// <param name="userId">The ID of the user executing the report.</param>
    /// <returns>A unique execution ID that can be used to retrieve results.</returns>
    Task<Guid> ExecuteReportAsync(Guid reportId, string parameters, string userId);
    
    /// <summary>
    /// Checks the status of a report execution.
    /// </summary>
    /// <param name="executionId">The execution ID.</param>
    /// <returns>The current status of the execution.</returns>
    Task<ExecutionStatus> GetExecutionStatusAsync(Guid executionId);
    
    /// <summary>
    /// Gets the results of a report execution.
    /// </summary>
    /// <param name="executionId">The execution ID.</param>
    /// <returns>The report execution results as a JSON string.</returns>
    Task<string> GetExecutionResultsAsync(Guid executionId);
    
    /// <summary>
    /// Cancels a running report execution.
    /// </summary>
    /// <param name="executionId">The execution ID to cancel.</param>
    /// <returns>True if cancellation was successful, otherwise false.</returns>
    Task<bool> CancelExecutionAsync(Guid executionId);
    
    /// <summary>
    /// Exports report results to the specified format.
    /// </summary>
    /// <param name="executionId">The execution ID with results to export.</param>
    /// <param name="format">The export format (PDF, Excel, CSV, etc.).</param>
    /// <returns>The URL or path to the exported file.</returns>
    Task<string> ExportReportAsync(Guid executionId, string format);
}

/// <summary>
/// Status for report execution.
/// </summary>
public enum ExecutionStatus
{
    Success,
    Failed,
    Canceled,
    Running,
    Queued
}
