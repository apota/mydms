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
    /// Repository implementation for CustomerLoyalty entities
    /// </summary>
    public class CustomerLoyaltyRepository : EfRepository<CustomerLoyalty>, ICustomerLoyaltyRepository
    {
        private readonly CrmDbContext _dbContext;

        public CustomerLoyaltyRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<CustomerLoyalty?> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerLoyalties
                .Include(cl => cl.Customer)
                .Include(cl => cl.Transactions.OrderByDescending(t => t.TransactionDate).Take(10))
                .Include(cl => cl.Redemptions.Where(r => r.Status == RedeemedRewardStatus.Active))
                .FirstOrDefaultAsync(cl => cl.CustomerId == customerId, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerLoyalty>> GetByTierAsync(LoyaltyTier tier, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerLoyalties
                .Include(cl => cl.Customer)
                .Where(cl => cl.CurrentTier == tier && cl.IsActive)
                .OrderByDescending(cl => cl.CurrentPoints)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerLoyalty>> GetWithExpiringPointsAsync(int days, CancellationToken cancellationToken = default)
        {
            var checkDate = DateTime.UtcNow.AddDays(days);
            
            return await _dbContext.CustomerLoyalties
                .Include(cl => cl.Customer)
                .Include(cl => cl.Transactions)
                .Where(cl => cl.IsActive && 
                    cl.Transactions.Any(t => t.TransactionType == LoyaltyTransactionType.Earned && 
                        t.ExpirationDate <= checkDate && t.ExpirationDate > DateTime.UtcNow))
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerLoyalty>> GetByTierWithActivitySinceAsync(LoyaltyTier tier, DateTime sinceDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerLoyalties
                .Include(cl => cl.Customer)
                .Where(cl => cl.CurrentTier == tier && 
                    cl.IsActive && 
                    cl.LastActivityDate >= sinceDate)
                .OrderByDescending(cl => cl.LastActivityDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerLoyalty>> GetTopCustomersByLifetimePointsAsync(int topCount, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerLoyalties
                .Include(cl => cl.Customer)
                .Where(cl => cl.IsActive)
                .OrderByDescending(cl => cl.LifetimePointsEarned)
                .Take(topCount)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task UpdateLastActivityDateAsync(Guid customerId, DateTime activityDate, CancellationToken cancellationToken = default)
        {
            var customerLoyalty = await _dbContext.CustomerLoyalties
                .FirstOrDefaultAsync(cl => cl.CustomerId == customerId, cancellationToken);

            if (customerLoyalty != null)
            {
                customerLoyalty.LastActivityDate = activityDate;
                customerLoyalty.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
