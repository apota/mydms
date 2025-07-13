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
    /// Repository implementation for LoyaltyTransaction entities
    /// </summary>
    public class LoyaltyTransactionRepository : EfRepository<LoyaltyTransaction>, ILoyaltyTransactionRepository
    {
        private readonly CrmDbContext _dbContext;

        public LoyaltyTransactionRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyTransaction>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTransactions
                .Include(lt => lt.Reward)
                .Where(lt => lt.CustomerId == customerId)
                .OrderByDescending(lt => lt.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyTransaction>> GetByCustomerIdAsync(Guid customerId, int skip, int take, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTransactions
                .Include(lt => lt.Reward)
                .Where(lt => lt.CustomerId == customerId)
                .OrderByDescending(lt => lt.TransactionDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyTransaction>> GetByCustomerIdAndTypeAsync(Guid customerId, LoyaltyTransactionType transactionType, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTransactions
                .Include(lt => lt.Reward)
                .Where(lt => lt.CustomerId == customerId && lt.TransactionType == transactionType)
                .OrderByDescending(lt => lt.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyTransaction>> GetByDateRangeAsync(Guid customerId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTransactions
                .Include(lt => lt.Reward)
                .Where(lt => lt.CustomerId == customerId && 
                    lt.TransactionDate >= fromDate && 
                    lt.TransactionDate <= toDate)
                .OrderByDescending(lt => lt.TransactionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<LoyaltyTransaction>> GetExpiringPointsAsync(Guid customerId, DateTime expirationDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTransactions
                .Where(lt => lt.CustomerId == customerId && 
                    lt.TransactionType == LoyaltyTransactionType.Earned && 
                    lt.ExpirationDate <= expirationDate && 
                    lt.ExpirationDate > DateTime.UtcNow)
                .OrderBy(lt => lt.ExpirationDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> GetTotalPointsEarnedAsync(Guid customerId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTransactions
                .Where(lt => lt.CustomerId == customerId && 
                    lt.TransactionType == LoyaltyTransactionType.Earned &&
                    lt.TransactionDate >= fromDate && 
                    lt.TransactionDate <= toDate)
                .SumAsync(lt => lt.Points, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<int> GetTotalPointsRedeemedAsync(Guid customerId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.LoyaltyTransactions
                .Where(lt => lt.CustomerId == customerId && 
                    lt.TransactionType == LoyaltyTransactionType.Redeemed &&
                    lt.TransactionDate >= fromDate && 
                    lt.TransactionDate <= toDate)
                .SumAsync(lt => lt.Points, cancellationToken);
        }
    }
}
