using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.CRM.Core.Repositories
{
    /// <summary>
    /// Repository interface for LoyaltyReward entities
    /// </summary>
    public interface ILoyaltyRewardRepository : IRepository<LoyaltyReward>
    {
        /// <summary>
        /// Gets active loyalty rewards
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of active loyalty rewards</returns>
        Task<IEnumerable<LoyaltyReward>> GetActiveRewardsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets rewards by category
        /// </summary>
        /// <param name="category">The reward category</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of rewards in the specified category</returns>
        Task<IEnumerable<LoyaltyReward>> GetByCategoryAsync(LoyaltyRewardCategory category, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets rewards eligible for a specific tier
        /// </summary>
        /// <param name="tier">The loyalty tier</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of rewards eligible for the tier</returns>
        Task<IEnumerable<LoyaltyReward>> GetEligibleForTierAsync(LoyaltyTier tier, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets rewards that a customer can afford with their points
        /// </summary>
        /// <param name="customerPoints">The customer's available points</param>
        /// <param name="tier">The customer's loyalty tier</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of affordable rewards for the customer</returns>
        Task<IEnumerable<LoyaltyReward>> GetAffordableRewardsAsync(int customerPoints, LoyaltyTier tier, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets rewards by status
        /// </summary>
        /// <param name="status">The reward status</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of rewards with the specified status</returns>
        Task<IEnumerable<LoyaltyReward>> GetByStatusAsync(LoyaltyRewardStatus status, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets rewards by points cost range
        /// </summary>
        /// <param name="minPoints">Minimum points cost</param>
        /// <param name="maxPoints">Maximum points cost</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of rewards within the points cost range</returns>
        Task<IEnumerable<LoyaltyReward>> GetByPointsCostRangeAsync(int minPoints, int maxPoints, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets featured rewards (sorted by display order)
        /// </summary>
        /// <param name="limit">Maximum number of rewards to return</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of featured rewards</returns>
        Task<IEnumerable<LoyaltyReward>> GetFeaturedRewardsAsync(int limit, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates reward quantity after redemption
        /// </summary>
        /// <param name="rewardId">The reward ID</param>
        /// <param name="quantityRedeemed">The quantity that was redeemed</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the operation</returns>
        Task UpdateQuantityRedeemedAsync(Guid rewardId, int quantityRedeemed, CancellationToken cancellationToken = default);
    }
}
