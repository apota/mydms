using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.CRM.Core.Repositories
{
    /// <summary>
    /// Repository interface for LoyaltyRedemption entities
    /// </summary>
    public interface ILoyaltyRedemptionRepository : IRepository<LoyaltyRedemption>
    {
        /// <summary>
        /// Gets redemptions for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of redemptions for the customer</returns>
        Task<IEnumerable<LoyaltyRedemption>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets redemptions for a customer by status
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="status">The redemption status</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of redemptions with the specified status</returns>
        Task<IEnumerable<LoyaltyRedemption>> GetByCustomerIdAndStatusAsync(Guid customerId, RedeemedRewardStatus status, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets redemption by redemption code
        /// </summary>
        /// <param name="redemptionCode">The redemption code</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The redemption with the specified code, or null if not found</returns>
        Task<LoyaltyRedemption?> GetByRedemptionCodeAsync(string redemptionCode, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets redemptions for a specific reward
        /// </summary>
        /// <param name="rewardId">The reward ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of redemptions for the reward</returns>
        Task<IEnumerable<LoyaltyRedemption>> GetByRewardIdAsync(Guid rewardId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets redemptions expiring within specified days
        /// </summary>
        /// <param name="days">Number of days ahead to check for expiring redemptions</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of redemptions expiring within the specified days</returns>
        Task<IEnumerable<LoyaltyRedemption>> GetExpiringRedemptionsAsync(int days, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets redemptions by date range
        /// </summary>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of redemptions within the date range</returns>
        Task<IEnumerable<LoyaltyRedemption>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets redemptions requiring approval
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of redemptions requiring approval</returns>
        Task<IEnumerable<LoyaltyRedemption>> GetPendingApprovalAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates redemption status
        /// </summary>
        /// <param name="redemptionId">The redemption ID</param>
        /// <param name="status">The new status</param>
        /// <param name="usedBy">The user who used the redemption (optional)</param>
        /// <param name="notes">Additional notes (optional)</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the operation</returns>
        Task UpdateRedemptionStatusAsync(Guid redemptionId, RedeemedRewardStatus status, string? usedBy = null, string? notes = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets redemption statistics for a reward
        /// </summary>
        /// <param name="rewardId">The reward ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The total count of redemptions for the reward</returns>
        Task<int> GetRedemptionCountByRewardAsync(Guid rewardId, CancellationToken cancellationToken = default);
    }
}
