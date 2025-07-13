using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.CRM.Core.Repositories
{
    /// <summary>
    /// Repository interface for LoyaltyTierConfig entities
    /// </summary>
    public interface ILoyaltyTierConfigRepository : IRepository<LoyaltyTierConfig>
    {
        /// <summary>
        /// Gets loyalty tier configuration by tier level
        /// </summary>
        /// <param name="tier">The loyalty tier</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The tier configuration for the specified tier, or null if not found</returns>
        Task<LoyaltyTierConfig?> GetByTierAsync(LoyaltyTier tier, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all active tier configurations
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of active tier configurations</returns>
        Task<IEnumerable<LoyaltyTierConfig>> GetActiveConfigurationsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets tier configurations ordered by display order
        /// </summary>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of tier configurations ordered by display order</returns>
        Task<IEnumerable<LoyaltyTierConfig>> GetOrderedTierConfigurationsAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets tier configuration for the specified points
        /// </summary>
        /// <param name="points">The number of points</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The tier configuration for the specified points</returns>
        Task<LoyaltyTierConfig?> GetTierForPointsAsync(int points, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets the next tier configuration for upgrade
        /// </summary>
        /// <param name="currentTier">The current tier</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The next tier configuration, or null if already at highest tier</returns>
        Task<LoyaltyTierConfig?> GetNextTierAsync(LoyaltyTier currentTier, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates tier configuration status
        /// </summary>
        /// <param name="tier">The loyalty tier</param>
        /// <param name="isActive">The active status</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A task representing the operation</returns>
        Task UpdateTierStatusAsync(LoyaltyTier tier, bool isActive, CancellationToken cancellationToken = default);
    }
}
