using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.SalesManagement.Core.Models;
using DMS.SalesManagement.Core.Repositories;
using DMS.Shared.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DMS.SalesManagement.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository implementation for Deal entities
    /// </summary>
    public class DealRepository : EfRepository<Deal>, IDealRepository
    {
        private readonly SalesDbContext _dbContext;
        
        public DealRepository(SalesDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Deal>> GetBySalesRepIdAsync(string salesRepId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Deals
                .Where(d => d.SalesRepId == salesRepId)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Deal>> GetByStatusAsync(DealStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Deals
                .Where(d => d.Status == status)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Deal>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Deals
                .Where(d => d.CustomerId == customerId)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Deal>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Deals
                .Where(d => d.VehicleId == vehicleId)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Deal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Deals
                .Where(d => d.CreatedAt.Date >= startDate.Date && d.CreatedAt.Date <= endDate.Date)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Deal?> GetWithAllRelatedDataAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Deals
                .Include(d => d.AddOns)
                .Include(d => d.Commissions)
                .Include(d => d.Documents)
                .ThenInclude(d => d.RequiredSignatures)
                .Include(d => d.Fees)
                .Include(d => d.StatusHistory)
                .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Deal> AddStatusHistoryAsync(Guid dealId, DealStatusHistory statusHistory, CancellationToken cancellationToken = default)
        {
            var deal = await _dbContext.Deals
                .Include(d => d.StatusHistory)
                .FirstOrDefaultAsync(d => d.Id == dealId, cancellationToken);
                
            if (deal == null)
            {
                throw new KeyNotFoundException($"Deal with ID {dealId} not found.");
            }
            
            statusHistory.Id = Guid.NewGuid();
            deal.StatusHistory.Add(statusHistory);
            deal.Status = statusHistory.Status;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return deal;
        }
    }
}
