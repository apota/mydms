using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;
using Quartz;

namespace DMS.ReportingAnalytics.API.Services;

/// <summary>
/// Quartz job for executing scheduled reports.
/// </summary>
public class ScheduledReportJob : IJob
{
    private readonly IScheduleRepository _scheduleRepository;
    private readonly IReportExecutionEngine _executionEngine;
    private readonly IExportService _exportService;
    private readonly ILogger<ScheduledReportJob> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduledReportJob"/> class.
    /// </summary>
    /// <param name="scheduleRepository">The schedule repository.</param>
    /// <param name="executionEngine">The report execution engine.</param>
    /// <param name="exportService">The export service.</param>
    /// <param name="logger">The logger.</param>
    public ScheduledReportJob(
        IScheduleRepository scheduleRepository,
        IReportExecutionEngine executionEngine,
        IExportService exportService,
        ILogger<ScheduledReportJob> logger)
    {
        _scheduleRepository = scheduleRepository;
        _executionEngine = executionEngine;
        _exportService = exportService;
        _logger = logger;
    }
    
    /// <summary>
    /// Executes the job.
    /// </summary>
    /// <param name="context">The job execution context.</param>
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting scheduled report job at {CurrentTime}", DateTime.UtcNow);
            
            var now = DateTime.UtcNow;
            var dueSchedules = await _scheduleRepository.GetSchedulesDueToRunAsync(now);
            
            foreach (var schedule in dueSchedules)
            {
                try
                {
                    _logger.LogInformation("Executing scheduled report {ScheduleId} for report {ReportId}", 
                        schedule.ScheduleId, schedule.ReportId);
                    
                    // Execute the report
                    var executionId = await _executionEngine.ExecuteReportAsync(
                        schedule.ReportId,
                        schedule.Parameters ?? "{}",
                        "system");
                    
                    // Wait for completion (with timeout)
                    var status = await WaitForReportCompletion(executionId);
                    
                    if (status == ExecutionStatus.Success)
                    {
                        // Export the report
                        var outputLocation = await _exportService.ExportScheduledReportAsync(
                            executionId,
                            schedule);
                            
                        _logger.LogInformation("Report exported to {OutputLocation}", outputLocation);
                    }
                    else
                    {
                        _logger.LogWarning("Scheduled report {ScheduleId} execution failed with status {Status}",
                            schedule.ScheduleId, status);
                    }
                    
                    // Calculate next run time based on cron expression
                    var nextRun = CalculateNextRunTime(schedule.Schedule, now);
                    
                    // Update the schedule
                    await _scheduleRepository.UpdateScheduleRunDatesAsync(schedule.ScheduleId, now, nextRun);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing scheduled report {ScheduleId}", schedule.ScheduleId);
                }
            }
            
            _logger.LogInformation("Completed scheduled report job");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in scheduled report job");
            throw;
        }
    }
    
    private async Task<ExecutionStatus> WaitForReportCompletion(Guid executionId)
    {
        // Wait for up to 5 minutes for the report to complete
        var timeout = TimeSpan.FromMinutes(5);
        var start = DateTime.UtcNow;
        
        while (DateTime.UtcNow - start < timeout)
        {
            var status = await _executionEngine.GetExecutionStatusAsync(executionId);
            
            if (status == ExecutionStatus.Success || status == ExecutionStatus.Failed || status == ExecutionStatus.Canceled)
            {
                return status;
            }
            
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
        
        // If we get here, the report timed out
        _logger.LogWarning("Report execution {ExecutionId} timed out", executionId);
        await _executionEngine.CancelExecutionAsync(executionId);
        return ExecutionStatus.Canceled;
    }
    
    private DateTime CalculateNextRunTime(string cronExpression, DateTime fromTime)
    {
        try
        {
            var expression = new CronExpression(cronExpression);
            DateTimeOffset? nextRun = expression.GetNextValidTimeAfter(new DateTimeOffset(fromTime));
            
            return nextRun?.DateTime ?? fromTime.AddDays(1);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating next run time for cron expression {CronExpression}", cronExpression);
            return fromTime.AddDays(1);  // Default to tomorrow if the expression is invalid
        }
    }
}
