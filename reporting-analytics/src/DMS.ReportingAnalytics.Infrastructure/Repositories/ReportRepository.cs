using Microsoft.EntityFrameworkCore;
using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;
using DMS.ReportingAnalytics.Infrastructure.Data;

namespace DMS.ReportingAnalytics.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for report operations.
/// </summary>
public class ReportRepository : IReportRepository
{
    private readonly ReportingDbContext _dbContext;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="ReportRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    public ReportRepository(ReportingDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<ReportDefinition>> GetAllReportsAsync()
    {
        return await _dbContext.Reports.ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<ReportDefinition?> GetReportByIdAsync(Guid reportId)
    {
        return await _dbContext.Reports
            .Include(r => r.ScheduledReports)
            .Include(r => r.ExecutionHistory)
            .FirstOrDefaultAsync(r => r.ReportId == reportId);
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<ReportDefinition>> GetReportsByCategoryAsync(string category)
    {
        return await _dbContext.Reports
            .Where(r => r.Category == category)
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<ReportDefinition> CreateReportAsync(ReportDefinition report)
    {
        report.CreatedDate = DateTime.UtcNow;
        report.ModifiedDate = DateTime.UtcNow;
        
        _dbContext.Reports.Add(report);
        await _dbContext.SaveChangesAsync();
        
        return report;
    }
    
    /// <inheritdoc/>
    public async Task<bool> UpdateReportAsync(ReportDefinition report)
    {
        var existingReport = await _dbContext.Reports.FindAsync(report.ReportId);
        
        if (existingReport == null)
        {
            return false;
        }
        
        existingReport.ReportName = report.ReportName;
        existingReport.Description = report.Description;
        existingReport.Category = report.Category;
        existingReport.Status = report.Status;
        existingReport.Parameters = report.Parameters;
        existingReport.SourceQuery = report.SourceQuery;
        existingReport.Permissions = report.Permissions;
        existingReport.ModifiedDate = DateTime.UtcNow;
        
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> DeleteReportAsync(Guid reportId)
    {
        var report = await _dbContext.Reports.FindAsync(reportId);
        
        if (report == null)
        {
            return false;
        }
        
        _dbContext.Reports.Remove(report);
        await _dbContext.SaveChangesAsync();
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<string>> GetAllCategoriesAsync()
    {
        return await _dbContext.Reports
            .Select(r => r.Category)
            .Distinct()
            .ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<ReportExecutionHistory> RecordExecutionAsync(ReportExecutionHistory execution)
    {
        execution.ExecutionDate = DateTime.UtcNow;
        
        _dbContext.ReportExecutionHistory.Add(execution);
        await _dbContext.SaveChangesAsync();
        
        return execution;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<ReportExecutionHistory>> GetExecutionHistoryAsync(Guid reportId)
    {
        return await _dbContext.ReportExecutionHistory
            .Where(e => e.ReportId == reportId)
            .OrderByDescending(e => e.ExecutionDate)
            .ToListAsync();
    }
}
