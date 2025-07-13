using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.CRM.Core.Repositories;
using DMS.Shared.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DMS.CRM.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository implementation for LoyaltyReward entities
    /// </summary>
    public class LoyaltyRewardRepository : EfRepository<LoyaltyReward>, ILoyaltyRewardRepository
    {
        private readonly CrmDbContext _dbContext;

        public LoyaltyRewardRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyReward>> GetActiveRewardsAsync(CancellationToken cancellationToken = default)
        {
            var currentDate = DateTime.UtcNow;
            return await _dbContext.LoyaltyRewards
                .Where(lr => lr.Status == LoyaltyRewardStatus.Active &&
                    (lr.StartDate == null || lr.StartDate <= currentDate) &&
                    (lr.EndDate == null || lr.EndDate >= currentDate) &&
                    (lr.QuantityAvailable == null || lr.QuantityRedeemed < lr.QuantityAvailable))
                .OrderBy(lr => lr.DisplayOrder)
                .ThenBy(lr => lr.PointsCost)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyReward>> GetByCategoryAsync(LoyaltyRewardCategory category, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRewards
                .Where(lr => lr.Category == category && lr.Status == LoyaltyRewardStatus.Active)
                .OrderBy(lr => lr.DisplayOrder)
                .ThenBy(lr => lr.PointsCost)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyReward>> GetEligibleForTierAsync(LoyaltyTier tier, CancellationToken cancellationToken = default)
        {
            var tierName = tier.ToString();
            return await _dbContext.LoyaltyRewards
                .Where(lr => lr.Status == LoyaltyRewardStatus.Active &&
                    (lr.EligibleTiers.Contains("\"" + tierName + "\"") || lr.EligibleTiers.Contains("All")))
                .OrderBy(lr => lr.DisplayOrder)
                .ThenBy(lr => lr.PointsCost)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyReward>> GetAffordableRewardsAsync(int customerPoints, LoyaltyTier tier, CancellationToken cancellationToken = default)
        {
            var tierName = tier.ToString();
            var currentDate = DateTime.UtcNow;
            
            return await _dbContext.LoyaltyRewards
                .Where(lr => lr.Status == LoyaltyRewardStatus.Active &&
                    lr.PointsCost <= customerPoints &&
                    (lr.EligibleTiers.Contains("\"" + tierName + "\"") || lr.EligibleTiers.Contains("All")) &&
                    (lr.StartDate == null || lr.StartDate <= currentDate) &&
                    (lr.EndDate == null || lr.EndDate >= currentDate) &&
                    (lr.QuantityAvailable == null || lr.QuantityRedeemed < lr.QuantityAvailable))
                .OrderBy(lr => lr.DisplayOrder)
                .ThenBy(lr => lr.PointsCost)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyReward>> GetByStatusAsync(LoyaltyRewardStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRewards
                .Where(lr => lr.Status == status)
                .OrderBy(lr => lr.DisplayOrder)
                .ThenBy(lr => lr.Name)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyReward>> GetByPointsCostRangeAsync(int minPoints, int maxPoints, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRewards
                .Where(lr => lr.Status == LoyaltyRewardStatus.Active &&
                    lr.PointsCost >= minPoints && lr.PointsCost <= maxPoints)
                .OrderBy(lr => lr.PointsCost)
                .ThenBy(lr => lr.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyReward>> GetFeaturedRewardsAsync(int limit, CancellationToken cancellationToken = default)
        {
            var currentDate = DateTime.UtcNow;
            return await _dbContext.LoyaltyRewards
                .Where(lr => lr.Status == LoyaltyRewardStatus.Active &&
                    (lr.StartDate == null || lr.StartDate <= currentDate) &&
                    (lr.EndDate == null || lr.EndDate >= currentDate) &&
                    (lr.QuantityAvailable == null || lr.QuantityRedeemed < lr.QuantityAvailable))
                .OrderBy(lr => lr.DisplayOrder)
                .ThenBy(lr => lr.PointsCost)
                .Take(limit)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateQuantityRedeemedAsync(Guid rewardId, int quantityRedeemed, CancellationToken cancellationToken = default)
        {
            var reward = await _dbContext.LoyaltyRewards
                .FirstOrDefaultAsync(lr => lr.Id == rewardId, cancellationToken);

            if (reward != null)
            {
                reward.QuantityRedeemed += quantityRedeemed;
                reward.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
