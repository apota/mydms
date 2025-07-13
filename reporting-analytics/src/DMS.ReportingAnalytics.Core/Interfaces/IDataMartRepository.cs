using DMS.ReportingAnalytics.Core.Models;

namespace DMS.ReportingAnalytics.Core.Interfaces;

/// <summary>
/// Interface for data mart repository operations.
/// </summary>
public interface IDataMartRepository
{
    /// <summary>
    /// Gets all data mart definitions.
    /// </summary>
    /// <returns>A collection of data mart definitions.</returns>
    Task<IEnumerable<DataMartDefinition>> GetAllDataMartsAsync();
    
    /// <summary>
    /// Gets a data mart definition by ID.
    /// </summary>
    /// <param name="martId">The data mart ID.</param>
    /// <returns>The data mart definition if found, otherwise null.</returns>
    Task<DataMartDefinition?> GetDataMartByIdAsync(Guid martId);
    
    /// <summary>
    /// Gets a data mart definition by name.
    /// </summary>
    /// <param name="martName">The data mart name.</param>
    /// <returns>The data mart definition if found, otherwise null.</returns>
    Task<DataMartDefinition?> GetDataMartByNameAsync(string martName);
    
    /// <summary>
    /// Creates a new data mart definition.
    /// </summary>
    /// <param name="dataMart">The data mart definition to create.</param>
    /// <returns>The created data mart definition with ID assigned.</returns>
    Task<DataMartDefinition> CreateDataMartAsync(DataMartDefinition dataMart);
    
    /// <summary>
    /// Updates an existing data mart definition.
    /// </summary>
    /// <param name="dataMart">The data mart definition to update.</param>
    /// <returns>True if update was successful, otherwise false.</returns>
    Task<bool> UpdateDataMartAsync(DataMartDefinition dataMart);
    
    /// <summary>
    /// Deletes a data mart definition.
    /// </summary>
    /// <param name="martId">The ID of the data mart to delete.</param>
    /// <returns>True if deletion was successful, otherwise false.</returns>
    Task<bool> DeleteDataMartAsync(Guid martId);
    
    /// <summary>
    /// Updates the refresh status of a data mart.
    /// </summary>
    /// <param name="martId">The data mart ID.</param>
    /// <param name="status">The new status.</param>
    /// <param name="lastRefreshDate">The date of the last refresh.</param>
    /// <returns>True if update was successful, otherwise false.</returns>
    Task<bool> UpdateDataMartStatusAsync(Guid martId, DataMartStatus status, DateTime? lastRefreshDate = null);
    
    /// <summary>
    /// Gets data marts due for refresh.
    /// </summary>
    /// <param name="referenceTime">The reference time to check against.</param>
    /// <returns>A collection of data marts that need to be refreshed.</returns>
    Task<IEnumerable<DataMartDefinition>> GetDataMartsDueForRefreshAsync(DateTime referenceTime);
}
