using Microsoft.EntityFrameworkCore;
using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;
using DMS.ReportingAnalytics.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DMS.ReportingAnalytics.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for data mart operations.
/// </summary>
public class DataMartRepository : IDataMartRepository
{
    private readonly ReportingDbContext _dbContext;
    private readonly ILogger<DataMartRepository> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMartRepository"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    /// <param name="logger">The logger.</param>
    public DataMartRepository(ReportingDbContext dbContext, ILogger<DataMartRepository> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<DataMartDefinition>> GetAllDataMartsAsync()
    {
        return await _dbContext.DataMarts.ToListAsync();
    }
    
    /// <inheritdoc/>
    public async Task<DataMartDefinition?> GetDataMartByIdAsync(Guid martId)
    {
        return await _dbContext.DataMarts
            .FirstOrDefaultAsync(m => m.MartId == martId);
    }
    
    /// <inheritdoc/>
    public async Task<DataMartDefinition?> GetDataMartByNameAsync(string martName)
    {
        return await _dbContext.DataMarts
            .FirstOrDefaultAsync(m => m.MartName == martName);
    }
    
    /// <inheritdoc/>
    public async Task<DataMartDefinition> CreateDataMartAsync(DataMartDefinition dataMart)
    {
        _dbContext.DataMarts.Add(dataMart);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Created new data mart {DataMartId} - {DataMartName}", dataMart.MartId, dataMart.MartName);
        
        return dataMart;
    }
    
    /// <inheritdoc/>
    public async Task<bool> UpdateDataMartAsync(DataMartDefinition dataMart)
    {
        var existingMart = await _dbContext.DataMarts.FindAsync(dataMart.MartId);
        
        if (existingMart == null)
        {
            return false;
        }
        
        existingMart.MartName = dataMart.MartName;
        existingMart.Description = dataMart.Description;
        existingMart.RefreshSchedule = dataMart.RefreshSchedule;
        existingMart.Status = dataMart.Status;
        existingMart.Dependencies = dataMart.Dependencies;
        existingMart.Configuration = dataMart.Configuration;
        
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Updated data mart {DataMartId} - {DataMartName}", dataMart.MartId, dataMart.MartName);
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> DeleteDataMartAsync(Guid martId)
    {
        var dataMart = await _dbContext.DataMarts.FindAsync(martId);
        
        if (dataMart == null)
        {
            return false;
        }
        
        _dbContext.DataMarts.Remove(dataMart);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation("Deleted data mart {DataMartId}", martId);
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<bool> UpdateDataMartStatusAsync(Guid martId, DataMartStatus status, DateTime? lastRefreshDate = null)
    {
        var dataMart = await _dbContext.DataMarts.FindAsync(martId);
        
        if (dataMart == null)
        {
            return false;
        }
        
        dataMart.Status = status;
        
        if (lastRefreshDate.HasValue)
        {
            dataMart.LastRefreshDate = lastRefreshDate;
        }
        
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation(
            "Updated data mart {DataMartId} status to {Status}{LastRefresh}", 
            martId, 
            status, 
            lastRefreshDate.HasValue ? $" with last refresh date {lastRefreshDate}" : "");
        
        return true;
    }
    
    /// <inheritdoc/>
    public async Task<IEnumerable<DataMartDefinition>> GetDataMartsDueForRefreshAsync(DateTime referenceTime)
    {
        // Get all active data marts
        var dataMarts = await _dbContext.DataMarts
            .Where(m => m.Status == DataMartStatus.Active)
            .ToListAsync();
        
        // Filter based on cron expressions
        var result = new List<DataMartDefinition>();
        
        foreach (var mart in dataMarts)
        {
            if (IsDataMartDueForRefresh(mart, referenceTime))
            {
                result.Add(mart);
            }
        }
        
        return result;
    }
    
    private bool IsDataMartDueForRefresh(DataMartDefinition dataMart, DateTime referenceTime)
    {
        // For simplicity, we're just assuming data marts with no last refresh date or
        // those that haven't been refreshed in the last 24 hours are due for refresh
        // In a real implementation, you would evaluate the cron expression against the reference time
        
        if (!dataMart.LastRefreshDate.HasValue)
        {
            return true;
        }
        
        return (referenceTime - dataMart.LastRefreshDate.Value).TotalHours >= 24;
    }
}
