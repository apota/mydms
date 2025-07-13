using DMS.ReportingAnalytics.Core.Models;

namespace DMS.ReportingAnalytics.Core.Interfaces;

/// <summary>
/// Interface for scheduling report operations.
/// </summary>
public interface IScheduleRepository
{
    /// <summary>
    /// Gets all scheduled reports.
    /// </summary>
    /// <returns>A collection of scheduled reports.</returns>
    Task<IEnumerable<ScheduledReport>> GetAllSchedulesAsync();
    
    /// <summary>
    /// Gets a scheduled report by ID.
    /// </summary>
    /// <param name="scheduleId">The schedule ID.</param>
    /// <returns>The scheduled report if found, otherwise null.</returns>
    Task<ScheduledReport?> GetScheduleByIdAsync(Guid scheduleId);
    
    /// <summary>
    /// Gets scheduled reports for a specific report.
    /// </summary>
    /// <param name="reportId">The report ID.</param>
    /// <returns>A collection of scheduled reports for the specified report.</returns>
    Task<IEnumerable<ScheduledReport>> GetSchedulesByReportIdAsync(Guid reportId);
    
    /// <summary>
    /// Gets scheduled reports that are due to run.
    /// </summary>
    /// <param name="runTime">The reference time to check against.</param>
    /// <returns>A collection of scheduled reports that should be executed.</returns>
    Task<IEnumerable<ScheduledReport>> GetSchedulesDueToRunAsync(DateTime runTime);
    
    /// <summary>
    /// Creates a new scheduled report.
    /// </summary>
    /// <param name="schedule">The schedule to create.</param>
    /// <returns>The created schedule with ID assigned.</returns>
    Task<ScheduledReport> CreateScheduleAsync(ScheduledReport schedule);
    
    /// <summary>
    /// Updates an existing scheduled report.
    /// </summary>
    /// <param name="schedule">The schedule to update.</param>
    /// <returns>True if update was successful, otherwise false.</returns>
    Task<bool> UpdateScheduleAsync(ScheduledReport schedule);
    
    /// <summary>
    /// Deletes a scheduled report.
    /// </summary>
    /// <param name="scheduleId">The ID of the schedule to delete.</param>
    /// <returns>True if deletion was successful, otherwise false.</returns>
    Task<bool> DeleteScheduleAsync(Guid scheduleId);
    
    /// <summary>
    /// Updates the next run date for a schedule.
    /// </summary>
    /// <param name="scheduleId">The schedule ID.</param>
    /// <param name="lastRunDate">The last run date to record.</param>
    /// <param name="nextRunDate">The next run date to schedule.</param>
    /// <returns>True if update was successful, otherwise false.</returns>
    Task<bool> UpdateScheduleRunDatesAsync(Guid scheduleId, DateTime lastRunDate, DateTime nextRunDate);
}
