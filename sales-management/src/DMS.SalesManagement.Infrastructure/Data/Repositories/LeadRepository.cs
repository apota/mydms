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
    /// Repository implementation for Lead entities
    /// </summary>
    public class LeadRepository : EfRepository<Lead>, ILeadRepository
    {
        private readonly SalesDbContext _dbContext;
        
        public LeadRepository(SalesDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Lead>> GetBySalesRepIdAsync(string salesRepId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Leads
                .Where(l => l.AssignedSalesRepId == salesRepId)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Lead>> GetByStatusAsync(LeadStatus status, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Leads
                .Where(l => l.Status == status)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Lead>> GetByFollowupDateAsync(DateTime date, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Leads
                .Where(l => l.FollowupDate.HasValue && l.FollowupDate.Value.Date <= date.Date)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<Lead> AddActivityAsync(Guid leadId, LeadActivity activity, CancellationToken cancellationToken = default)
        {
            var lead = await _dbContext.Leads
                .Include(l => l.Activities)
                .FirstOrDefaultAsync(l => l.Id == leadId, cancellationToken);
                
            if (lead == null)
            {
                throw new KeyNotFoundException($"Lead with ID {leadId} not found.");
            }
            
            activity.Id = Guid.NewGuid();
            lead.Activities.Add(activity);
            lead.LastActivityDate = activity.Date;
            
            await _dbContext.SaveChangesAsync(cancellationToken);
            return lead;
        }
    }
}
