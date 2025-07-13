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
    /// Repository implementation for LoyaltyRedemption entities
    /// </summary>
    public class LoyaltyRedemptionRepository : EfRepository<LoyaltyRedemption>, ILoyaltyRedemptionRepository
    {
        private readonly CrmDbContext _dbContext;

        public LoyaltyRedemptionRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyRedemption>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRedemptions
                .Include(lr => lr.Reward)
                .Include(lr => lr.Customer)
                .Where(lr => lr.CustomerId == customerId)
                .OrderByDescending(lr => lr.RedemptionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyRedemption>> GetByCustomerIdAndStatusAsync(Guid customerId, RedeemedRewardStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRedemptions
                .Include(lr => lr.Reward)
                .Include(lr => lr.Customer)
                .Where(lr => lr.CustomerId == customerId && lr.Status == status)
                .OrderByDescending(lr => lr.RedemptionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<LoyaltyRedemption?> GetByRedemptionCodeAsync(string redemptionCode, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRedemptions
                .Include(lr => lr.Reward)
                .Include(lr => lr.Customer)
                .FirstOrDefaultAsync(lr => lr.RedemptionCode == redemptionCode, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyRedemption>> GetByRewardIdAsync(Guid rewardId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRedemptions
                .Include(lr => lr.Customer)
                .Where(lr => lr.RewardId == rewardId)
                .OrderByDescending(lr => lr.RedemptionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyRedemption>> GetExpiringRedemptionsAsync(int days, CancellationToken cancellationToken = default)
        {
            var checkDate = DateTime.UtcNow.AddDays(days);
            
            return await _dbContext.LoyaltyRedemptions
                .Include(lr => lr.Reward)
                .Include(lr => lr.Customer)
                .Where(lr => lr.Status == RedeemedRewardStatus.Active && 
                    lr.ExpirationDate <= checkDate && 
                    lr.ExpirationDate > DateTime.UtcNow)
                .OrderBy(lr => lr.ExpirationDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyRedemption>> GetByDateRangeAsync(DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRedemptions
                .Include(lr => lr.Reward)
                .Include(lr => lr.Customer)
                .Where(lr => lr.RedemptionDate >= fromDate && lr.RedemptionDate <= toDate)
                .OrderByDescending(lr => lr.RedemptionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyRedemption>> GetPendingApprovalAsync(CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRedemptions
                .Include(lr => lr.Reward)
                .Include(lr => lr.Customer)
                .Where(lr => lr.Status == RedeemedRewardStatus.Pending)
                .OrderBy(lr => lr.RedemptionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateRedemptionStatusAsync(Guid redemptionId, RedeemedRewardStatus status, string? usedBy = null, string? notes = null, CancellationToken cancellationToken = default)
        {
            var redemption = await _dbContext.LoyaltyRedemptions
                .FirstOrDefaultAsync(lr => lr.Id == redemptionId, cancellationToken);

            if (redemption != null)
            {
                redemption.Status = status;
                redemption.UpdatedAt = DateTime.UtcNow;

                if (status == RedeemedRewardStatus.Used)
                {
                    redemption.UsedDate = DateTime.UtcNow;
                    redemption.UsedBy = usedBy;
                }

                if (!string.IsNullOrEmpty(notes))
                {
                    redemption.Notes = notes;
                }

                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }

        /// <inheritdoc />
        public async Task<int> GetRedemptionCountByRewardAsync(Guid rewardId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyRedemptions
                .CountAsync(lr => lr.RewardId == rewardId, cancellationToken);
        }
    }
}
