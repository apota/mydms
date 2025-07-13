using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.CRM.Core.Repositories
{
    /// <summary>
    /// Repository interface for Customer entities
    /// </summary>
    public interface ICustomerRepository : IRepository<Customer>
    {
        /// <summary>
        /// Gets a customer by email address
        /// </summary>
        /// <param name="email">The email address</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The customer with the specified email, or null if not found</returns>
        Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customers by status
        /// </summary>
        /// <param name="status">The customer status</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of customers with the specified status</returns>
        Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Searches customers by name or email
        /// </summary>
        /// <param name="searchTerm">The search term</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of customers matching the search term</returns>
        Task<IEnumerable<Customer>> SearchAsync(string searchTerm, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customers by loyalty tier
        /// </summary>
        /// <param name="tier">The loyalty tier</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of customers with the specified loyalty tier</returns>
        Task<IEnumerable<Customer>> GetByLoyaltyTierAsync(LoyaltyTier tier, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customers with a purchase of a specific vehicle model
        /// </summary>
        /// <param name="modelId">The vehicle model ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of customers who purchased the specified vehicle model</returns>
        Task<IEnumerable<Customer>> GetByVehicleModelAsync(Guid modelId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a complete customer profile with all related data
        /// </summary>
        /// <param name="id">The customer ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The customer with all related data, or null if not found</returns>
        Task<Customer?> GetCompleteProfileAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
