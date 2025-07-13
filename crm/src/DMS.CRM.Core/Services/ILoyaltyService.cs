using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;

namespace DMS.CRM.Core.Services
{
    /// <summary>
    /// Service for managing customer loyalty program
    /// </summary>
    public interface ILoyaltyService
    {
        /// <summary>
        /// Gets customer's loyalty status
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer loyalty status</returns>
        Task<CustomerLoyaltyStatusDTO> GetCustomerLoyaltyStatusAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds loyalty points to a customer's account
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="points">Number of points to add</param>
        /// <param name="source">Source of points (e.g. "Purchase", "Service", "Referral")</param>
        /// <param name="referenceId">Reference ID (e.g. invoice number, service appointment)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated points balance</returns>
        Task<int> AddLoyaltyPointsAsync(
            Guid customerId, 
            int points, 
            string source, 
            string referenceId, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Redeems loyalty points for a reward
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="rewardId">The reward ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Redemption result</returns>
        Task<LoyaltyRedemptionResultDTO> RedeemPointsAsync(Guid customerId, Guid rewardId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets loyalty point history for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of loyalty point transactions</returns>
        Task<IEnumerable<LoyaltyTransactionDTO>> GetLoyaltyPointHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets available loyalty rewards
        /// </summary>
        /// <param name="tierLevel">Loyalty tier level filter (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available rewards</returns>
        Task<IEnumerable<LoyaltyRewardDTO>> GetAvailableRewardsAsync(string tierLevel = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets rewards redeemed by a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of redeemed rewards</returns>
        Task<IEnumerable<RedeemedRewardDTO>> GetRedeemedRewardsAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates customer's loyalty tier
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="newTier">New loyalty tier</param>
        /// <param name="reason">Reason for tier change</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated loyalty status</returns>
        Task<CustomerLoyaltyStatusDTO> UpdateLoyaltyTierAsync(
            Guid customerId, 
            string newTier, 
            string reason, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calculates points that would be awarded for a transaction
        /// </summary>
        /// <param name="transactionType">Type of transaction</param>
        /// <param name="transactionAmount">Transaction amount</param>
        /// <param name="customerTier">Customer's loyalty tier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Points that would be awarded</returns>
        Task<int> CalculateEarnablePointsAsync(
            string transactionType, 
            decimal transactionAmount, 
            string customerTier, 
            CancellationToken cancellationToken = default);
    }
}
