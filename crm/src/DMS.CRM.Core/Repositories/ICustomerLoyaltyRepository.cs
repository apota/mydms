using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.CRM.Core.Repositories
{
    /// <summary>
    /// Repository interface for CustomerLoyalty entities
    /// </summary>
    public interface ICustomerLoyaltyRepository : IRepository<CustomerLoyalty>
    {
        /// <summary>
        /// Gets customer loyalty by customer ID
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The customer loyalty data, or null if not found</returns>
        Task<CustomerLoyalty?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customers by loyalty tier
        /// </summary>
        /// <param name="tier">The loyalty tier</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of customer loyalty records with the specified tier</returns>
        Task<IEnumerable<CustomerLoyalty>> GetByTierAsync(LoyaltyTier tier, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customers with points expiring within specified days
        /// </summary>
        /// <param name="days">Number of days ahead to check for expiring points</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of customer loyalty records with expiring points</returns>
        Task<IEnumerable<CustomerLoyalty>> GetWithExpiringPointsAsync(int days, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customers by tier with activity since specified date
        /// </summary>
        /// <param name="tier">The loyalty tier</param>
        /// <param name="sinceDate">The date to check activity since</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of customer loyalty records with recent activity</returns>
        Task<IEnumerable<CustomerLoyalty>> GetByTierWithActivitySinceAsync(LoyaltyTier tier, DateTime sinceDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets top customers by lifetime points earned
        /// </summary>
        /// <param name="topCount">Number of top customers to return</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of top customer loyalty records</returns>
        Task<IEnumerable<CustomerLoyalty>> GetTopCustomersByLifetimePointsAsync(int topCount, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates last activity date for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="activityDate">The activity date</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the operation</returns>
        Task UpdateLastActivityDateAsync(Guid customerId, DateTime activityDate, CancellationToken cancellationToken = default);
    }
}
