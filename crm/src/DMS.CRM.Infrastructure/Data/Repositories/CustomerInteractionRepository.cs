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
    /// Repository implementation for CustomerInteraction entities
    /// </summary>
    public class CustomerInteractionRepository : EfRepository<CustomerInteraction>, ICustomerInteractionRepository
    {
        private readonly CrmDbContext _dbContext;

        public CustomerInteractionRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<CustomerInteraction> GetByIdAsync(Guid id)
        {
            var interaction = await _dbContext.CustomerInteractions
                .Include(i => i.Customer)
                .FirstOrDefaultAsync(i => i.Id == id);
            return interaction ?? throw new InvalidOperationException($"CustomerInteraction with ID {id} not found");
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetAllAsync(int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerInteractions
                .Include(i => i.Customer)
                .OrderByDescending(i => i.InteractionDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<CustomerInteraction> AddAsync(CustomerInteraction interaction)
        {
            _dbContext.CustomerInteractions.Add(interaction);
            await _dbContext.SaveChangesAsync();
            return interaction;
        }

        /// <inheritdoc />
        public async Task<CustomerInteraction> UpdateAsync(CustomerInteraction interaction)
        {
            _dbContext.CustomerInteractions.Update(interaction);
            await _dbContext.SaveChangesAsync();
            return interaction;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
            {
                await DeleteAsync(entity, CancellationToken.None);
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetByCustomerIdAsync(Guid customerId, int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerInteractions
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.InteractionDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerInteractions
                .Where(i => i.CustomerId == customerId)
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetByFilterAsync(
            DateTime? startDate, 
            DateTime? endDate, 
            InteractionType? type, 
            CommunicationChannel? channel, 
            InteractionStatus? status,
            bool? requiresFollowUp,
            string searchTerm,
            int skip = 0, 
            int take = 50)
        {
            var query = _dbContext.CustomerInteractions.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(i => i.InteractionDate >= startDate.Value);
                
            if (endDate.HasValue)
                query = query.Where(i => i.InteractionDate <= endDate.Value);
                
            if (type.HasValue)
                query = query.Where(i => i.Type == type.Value);
                
            if (status.HasValue)
                query = query.Where(i => i.Status == status.Value);
                
            if (requiresFollowUp.HasValue)
                query = query.Where(i => i.RequiresFollowUp == requiresFollowUp.Value);
                
            // Note: Channel filtering would need to be implemented based on ChannelId mapping
            // This could be done through a lookup table or enum mapping
                
            if (!string.IsNullOrEmpty(searchTerm))
                query = query.Where(i => 
                    (i.Subject != null && i.Subject.Contains(searchTerm)) || 
                    (i.Content != null && i.Content.Contains(searchTerm)));

            return await query
                .OrderByDescending(i => i.InteractionDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetFollowUpsForDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbContext.CustomerInteractions
                .Where(i => i.RequiresFollowUp && 
                           i.FollowUpDate.HasValue &&
                           i.FollowUpDate.Value >= startDate &&
                           i.FollowUpDate.Value <= endDate)
                .OrderBy(i => i.FollowUpDate)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetByCampaignIdAsync(Guid campaignId, int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerInteractions
                .Where(i => i.RelatedToType == "Campaign" && i.RelatedToId == campaignId)
                .OrderByDescending(i => i.InteractionDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetByTypeAsync(InteractionType type, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerInteractions
                .Where(i => i.Type == type)
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerInteractions
                .Where(i => i.InteractionDate >= startDate && i.InteractionDate <= endDate)
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerInteractions
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync(cancellationToken);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerInteraction>> GetByRelatedEntityAsync(string relatedToType, Guid relatedToId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.CustomerInteractions
                .Where(i => i.RelatedToType == relatedToType && i.RelatedToId == relatedToId)
                .OrderByDescending(i => i.InteractionDate)
                .ToListAsync(cancellationToken);
        }
    }
}
