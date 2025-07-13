using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.CRM.Core.Repositories
{
    /// <summary>
    /// Repository interface for CustomerInteraction entities
    /// </summary>
    public interface ICustomerInteractionRepository : IRepository<CustomerInteraction>
    {
        Task<CustomerInteraction> GetByIdAsync(Guid id);
        Task<IEnumerable<CustomerInteraction>> GetAllAsync(int skip = 0, int take = 50);
        Task<CustomerInteraction> AddAsync(CustomerInteraction interaction);
        Task<CustomerInteraction> UpdateAsync(CustomerInteraction interaction);
        Task<bool> DeleteAsync(Guid id);

        /// <summary>
        /// Gets interactions for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <returns>A collection of interactions for the specified customer</returns>        Task<IEnumerable<CustomerInteraction>> GetByCustomerIdAsync(Guid customerId, int skip = 0, int take = 50);
        Task<IEnumerable<CustomerInteraction>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets interactions by multiple filter criteria
        /// </summary>
        /// <param name="startDate">Optional start date filter</param>
        /// <param name="endDate">Optional end date filter</param>
        /// <param name="type">Optional interaction type filter</param>
        /// <param name="channel">Optional communication channel filter</param>
        /// <param name="status">Optional status filter</param>
        /// <param name="requiresFollowUp">Optional follow-up requirement filter</param>
        /// <param name="searchTerm">Optional search term for subject/content</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <returns>A filtered collection of interactions</returns>
        Task<IEnumerable<CustomerInteraction>> GetByFilterAsync(
            DateTime? startDate, 
            DateTime? endDate, 
            InteractionType? type, 
            CommunicationChannel? channel, 
            InteractionStatus? status,
            bool? requiresFollowUp,
            string searchTerm,
            int skip = 0, 
            int take = 50);
        
        /// <summary>
        /// Gets interactions that require follow-up within a date range
        /// </summary>
        /// <param name="startDate">The start date for follow-up</param>
        /// <param name="endDate">The end date for follow-up</param>
        /// <returns>A collection of interactions requiring follow-up</returns>
        Task<IEnumerable<CustomerInteraction>> GetFollowUpsForDateRangeAsync(DateTime startDate, DateTime endDate);
        
        /// <summary>
        /// Gets interactions related to a specific campaign
        /// </summary>
        /// <param name="campaignId">The campaign ID</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <returns>A collection of interactions for the specified campaign</returns>
        Task<IEnumerable<CustomerInteraction>> GetByCampaignIdAsync(Guid campaignId, int skip = 0, int take = 50);
        
        /// <summary>
        /// Gets interactions by type
        /// </summary>
        /// <param name="type">The interaction type</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of interactions of the specified type</returns>
        Task<IEnumerable<CustomerInteraction>> GetByTypeAsync(InteractionType type, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets interactions in a date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of interactions within the specified date range</returns>
        Task<IEnumerable<CustomerInteraction>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets interactions by user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of interactions for the specified user</returns>
        Task<IEnumerable<CustomerInteraction>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets interactions related to an entity
        /// </summary>
        /// <param name="relatedToType">The entity type</param>
        /// <param name="relatedToId">The entity ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of interactions related to the specified entity</returns>
        Task<IEnumerable<CustomerInteraction>> GetByRelatedEntityAsync(string relatedToType, Guid relatedToId, CancellationToken cancellationToken = default);
    }
}
