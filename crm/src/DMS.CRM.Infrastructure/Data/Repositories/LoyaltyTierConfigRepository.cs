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
    /// Repository implementation for LoyaltyTierConfig entities
    /// </summary>
    public class LoyaltyTierConfigRepository : EfRepository<LoyaltyTierConfig>, ILoyaltyTierConfigRepository
    {
        private readonly CrmDbContext _dbContext;

        public LoyaltyTierConfigRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<LoyaltyTierConfig?> GetByTierAsync(LoyaltyTier tier, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTierConfigs
                .FirstOrDefaultAsync(ltc => ltc.Tier == tier, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyTierConfig>> GetActiveConfigurationsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTierConfigs
                .Where(ltc => ltc.IsActive)
                .OrderBy(ltc => ltc.DisplayOrder)
                .ThenBy(ltc => ltc.MinimumPoints)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyTierConfig>> GetOrderedTierConfigurationsAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTierConfigs
                .OrderBy(ltc => ltc.DisplayOrder)
                .ThenBy(ltc => ltc.MinimumPoints)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<LoyaltyTierConfig?> GetTierForPointsAsync(int points, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTierConfigs
                .Where(ltc => ltc.IsActive && ltc.MinimumPoints <= points)
                .OrderByDescending(ltc => ltc.MinimumPoints)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<LoyaltyTierConfig?> GetNextTierAsync(LoyaltyTier currentTier, CancellationToken cancellationToken = default)
        {
            var currentTierConfig = await GetByTierAsync(currentTier, cancellationToken);
            if (currentTierConfig == null)
                return null;

            return await _dbContext.LoyaltyTierConfigs
                .Where(ltc => ltc.IsActive && ltc.MinimumPoints > currentTierConfig.MinimumPoints)
                .OrderBy(ltc => ltc.MinimumPoints)
                .FirstOrDefaultAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateTierStatusAsync(LoyaltyTier tier, bool isActive, CancellationToken cancellationToken = default)
        {
            var tierConfig = await _dbContext.LoyaltyTierConfigs
                .FirstOrDefaultAsync(ltc => ltc.Tier == tier, cancellationToken);

            if (tierConfig != null)
            {
                tierConfig.IsActive = isActive;
                tierConfig.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
