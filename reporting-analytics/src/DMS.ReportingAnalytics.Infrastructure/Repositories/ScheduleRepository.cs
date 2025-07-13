using Microsoft.EntityFrameworkCore;
using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;
using DMS.ReportingAnalytics.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DMS.ReportingAnalytics.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for scheduled reports.
/// </summary>
public class ScheduleRepository : IScheduleRepository
{
    private readonly ReportingDbContext _dbContext;
    private readonly ILogger<ScheduleRepository> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ScheduleRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    public ScheduleRepository(ReportingDbContext dbContext, ILogger<ScheduleRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<ScheduledReport>> GetAllSchedulesAsync()
    {
        return await _dbContext.ScheduledReports.ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<ScheduledReport?> GetScheduleByIdAsync(Guid scheduleId)
    {
        return await _dbContext.ScheduledReports
            .Include(s => s.Report)
            .Include(s => s.ExecutionHistory)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<ScheduledReport>> GetSchedulesByReportIdAsync(Guid reportId)
    {
        return await _dbContext.ScheduledReports
            .Where(s => s.ReportId == reportId)
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<ScheduledReport>> GetSchedulesDueToRunAsync(DateTime runTime)
    {
        return await _dbContext.ScheduledReports
            .Where(s => s.Status == ScheduleStatus.Active && s.NextRunDate <= runTime)
            .Include(s => s.Report)
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<ScheduledReport> CreateScheduleAsync(ScheduledReport schedule)
    {
        _dbContext.ScheduledReports.Add(schedule);
        await _dbContext.SaveChangesAsync();
        
        return schedule;
    }
    
    /// <inheritdoc/>
    public async Task<bool> UpdateScheduleAsync(ScheduledReport schedule)
    {
        var existingSchedule = await _dbContext.ScheduledReports.FindAsync(schedule.ScheduleId);
        
        if (existingSchedule == null)
        {
            return false;
        }
        
        existingSchedule.Schedule = schedule.Schedule;
        existingSchedule.Format = schedule.Format;
        existingSchedule.Recipients = schedule.Recipients;
        existingSchedule.Subject = schedule.Subject;
        existingSchedule.Message = schedule.Message;
        existingSchedule.Status = schedule.Status;
        
        if (schedule.LastRunDate.HasValue)
        {
            existingSchedule.LastRunDate = schedule.LastRunDate;
        }
        
        if (schedule.NextRunDate.HasValue)
        {
            existingSchedule.NextRunDate = schedule.NextRunDate;
        }
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> DeleteScheduleAsync(Guid scheduleId)
    {
        var schedule = await _dbContext.ScheduledReports.FindAsync(scheduleId);
        
        if (schedule == null)
        {
            return false;
        }
        
        _dbContext.ScheduledReports.Remove(schedule);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> UpdateScheduleRunDatesAsync(Guid scheduleId, DateTime lastRunDate, DateTime nextRunDate)
    {
        var schedule = await _dbContext.ScheduledReports.FindAsync(scheduleId);
        
        if (schedule == null)
        {
            return false;
        }
        
        schedule.LastRunDate = lastRunDate;
        schedule.NextRunDate = nextRunDate;
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
}
