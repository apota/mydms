using DMS.ReportingAnalytics.Core.Models;

namespace DMS.ReportingAnalytics.Core.Interfaces;

/// <summary>
/// Interface for report repository operations.
/// </summary>
public interface IReportRepository
{
    /// <summary>
    /// Gets all report definitions.
    /// </summary>
    /// <returns>A collection of report definitions.</returns>
    Task<IEnumerable<ReportDefinition>> GetAllReportsAsync();
    
    /// <summary>
    /// Gets a report definition by ID.
    /// </summary>
    /// <param name="reportId">The report ID.</param>
    /// <returns>The report definition if found, otherwise null.</returns>
    Task<ReportDefinition?> GetReportByIdAsync(Guid reportId);
    
    /// <summary>
    /// Gets report definitions by category.
    /// </summary>
    /// <param name="category">The report category.</param>
    /// <returns>A collection of report definitions in the specified category.</returns>
    Task<IEnumerable<ReportDefinition>> GetReportsByCategoryAsync(string category);
    
    /// <summary>
    /// Creates a new report definition.
    /// </summary>
    /// <param name="report">The report definition to create.</param>
    /// <returns>The created report definition with ID assigned.</returns>
    Task<ReportDefinition> CreateReportAsync(ReportDefinition report);
    
    /// <summary>
    /// Updates an existing report definition.
    /// </summary>
    /// <param name="report">The report definition to update.</param>
    /// <returns>True if update was successful, otherwise false.</returns>
    Task<bool> UpdateReportAsync(ReportDefinition report);
    
    /// <summary>
    /// Deletes a report definition.
    /// </summary>
    /// <param name="reportId">The ID of the report to delete.</param>
    /// <returns>True if deletion was successful, otherwise false.</returns>
    Task<bool> DeleteReportAsync(Guid reportId);
    
    /// <summary>
    /// Gets all report categories.
    /// </summary>
    /// <returns>A collection of distinct report categories.</returns>
    Task<IEnumerable<string>> GetAllCategoriesAsync();
    
    /// <summary>
    /// Records a report execution.
    /// </summary>
    /// <param name="execution">The execution history record to create.</param>
    /// <returns>The created execution history record with ID assigned.</returns>
    Task<ReportExecutionHistory> RecordExecutionAsync(ReportExecutionHistory execution);
    
    /// <summary>
    /// Gets execution history for a report.
    /// </summary>
    /// <param name="reportId">The report ID.</param>
    /// <returns>A collection of execution history records for the specified report.</returns>
    Task<IEnumerable<ReportExecutionHistory>> GetExecutionHistoryAsync(Guid reportId);
}
