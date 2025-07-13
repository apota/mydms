using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.CRM.Core.Repositories
{
    /// <summary>
    /// Repository interface for Campaign entities
    /// </summary>
    public interface ICampaignRepository : IRepository<Campaign>
    {
        /// <summary>
        /// Gets campaigns by type
        /// </summary>
        /// <param name="type">The campaign type</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of campaigns of the specified type</returns>
        Task<IEnumerable<Campaign>> GetByTypeAsync(CampaignType type, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets campaigns by status
        /// </summary>
        /// <param name="status">The campaign status</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of campaigns with the specified status</returns>
        Task<IEnumerable<Campaign>> GetByStatusAsync(CampaignStatus status, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets campaigns scheduled in a date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of campaigns scheduled within the specified date range</returns>
        Task<IEnumerable<Campaign>> GetByScheduledDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets campaigns by segment
        /// </summary>
        /// <param name="segmentId">The segment ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of campaigns targeting the specified segment</returns>
        Task<IEnumerable<Campaign>> GetBySegmentAsync(Guid segmentId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates campaign metrics
        /// </summary>
        /// <param name="id">The campaign ID</param>
        /// <param name="metrics">The updated metrics</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated campaign</returns>
        Task<Campaign> UpdateMetricsAsync(Guid id, CampaignMetrics metrics, CancellationToken cancellationToken = default);
    }
}
