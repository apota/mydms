using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.SalesManagement.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.SalesManagement.Core.Repositories
{
    /// <summary>
    /// Repository interface for Lead entities
    /// </summary>
    public interface ILeadRepository : IRepository<Lead>
    {
        /// <summary>
        /// Gets leads assigned to a specific sales representative
        /// </summary>
        /// <param name="salesRepId">The ID of the sales representative</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of leads assigned to the specified sales representative</returns>
        Task<IEnumerable<Lead>> GetBySalesRepIdAsync(string salesRepId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets leads with a specific status
        /// </summary>
        /// <param name="status">The lead status to filter by</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of leads with the specified status</returns>
        Task<IEnumerable<Lead>> GetByStatusAsync(LeadStatus status, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets leads that require follow-up by a specific date
        /// </summary>
        /// <param name="date">The follow-up date</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of leads requiring follow-up by the specified date</returns>
        Task<IEnumerable<Lead>> GetByFollowupDateAsync(DateTime date, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a new activity to a lead
        /// </summary>
        /// <param name="leadId">The ID of the lead</param>
        /// <param name="activity">The activity to add</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated lead</returns>
        Task<Lead> AddActivityAsync(Guid leadId, LeadActivity activity, CancellationToken cancellationToken = default);
    }
}
