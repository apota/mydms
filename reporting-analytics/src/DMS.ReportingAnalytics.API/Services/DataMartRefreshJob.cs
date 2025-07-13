using DMS.ReportingAnalytics.Core.Interfaces;
using DMS.ReportingAnalytics.Core.Models;
using Quartz;

namespace DMS.ReportingAnalytics.API.Services;

/// <summary>
/// Quartz job for refreshing data marts.
/// </summary>
public class DataMartRefreshJob : IJob
{
    private readonly IDataMartRepository _dataMartRepository;
    private readonly ILogger<DataMartRefreshJob> _logger;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DataMartRefreshJob"/> class.
    /// </summary>
    /// <param name="dataMartRepository">The data mart repository.</param>
    /// <param name="logger">The logger.</param>
    public DataMartRefreshJob(
        IDataMartRepository dataMartRepository,
        ILogger<DataMartRefreshJob> logger)
    {
        _dataMartRepository = dataMartRepository;
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
            _logger.LogInformation("Starting data mart refresh job at {CurrentTime}", DateTime.UtcNow);
            
            var now = DateTime.UtcNow;
            var dataMarts = await _dataMartRepository.GetDataMartsDueForRefreshAsync(now);
            
            foreach (var dataMart in dataMarts)
            {
                try
                {
                    _logger.LogInformation("Refreshing data mart {DataMartId} - {DataMartName}", 
                        dataMart.MartId, dataMart.MartName);
                    
                    // Update status to Building
                    await _dataMartRepository.UpdateDataMartStatusAsync(dataMart.MartId, DataMartStatus.Building);
                    
                    // Refresh the data mart
                    await RefreshDataMart(dataMart);
                    
                    // Update status to Active and set last refresh date
                    await _dataMartRepository.UpdateDataMartStatusAsync(dataMart.MartId, DataMartStatus.Active, DateTime.UtcNow);
                    
                    _logger.LogInformation("Successfully refreshed data mart {DataMartId}", dataMart.MartId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing data mart {DataMartId}", dataMart.MartId);
                    
                    // Update status to Failed
                    await _dataMartRepository.UpdateDataMartStatusAsync(dataMart.MartId, DataMartStatus.Failed);
                }
            }
            
            _logger.LogInformation("Completed data mart refresh job");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in data mart refresh job");
            throw;
        }
    }
    
    private Task RefreshDataMart(DataMartDefinition dataMart)
    {
        // This would be implemented with actual data mart refresh logic
        // For example:
        // 1. Extract data from source systems
        // 2. Transform the data
        // 3. Load into the data warehouse
        
        // For now, just add a delay to simulate processing
        return Task.Delay(TimeSpan.FromSeconds(5));
    }
}
