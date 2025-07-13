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
    /// Repository implementation for Campaign entities
    /// </summary>
    public class CampaignRepository : EfRepository<Campaign>, ICampaignRepository
    {
        private readonly CrmDbContext _dbContext;

        public CampaignRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Campaign>> GetByTypeAsync(CampaignType type, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Campaigns
                .Where(c => c.Type == type)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Campaign>> GetByStatusAsync(CampaignStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Campaigns
                .Where(c => c.Status == status)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Campaign>> GetByScheduledDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Campaigns
                .Where(c => c.StartDate >= startDate && c.StartDate <= endDate)
                .OrderBy(c => c.StartDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Campaign>> GetBySegmentAsync(Guid segmentId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Campaigns
                .Where(c => c.CampaignSegments.Any(cs => cs.SegmentId == segmentId && cs.IsActive))
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Campaign> UpdateMetricsAsync(Guid id, CampaignMetrics metrics, CancellationToken cancellationToken = default)
        {
            var campaign = await _dbContext.Campaigns.FindAsync(id);
            if (campaign == null)
                throw new InvalidOperationException($"Campaign with ID {id} not found");

            campaign.Metrics = metrics;
            campaign.UpdatedAt = DateTime.UtcNow;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return campaign;
        }
    }
}
