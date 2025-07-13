using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.CRM.Core.Repositories
{
    /// <summary>
    /// Repository interface for LoyaltyTransaction entities
    /// </summary>
    public interface ILoyaltyTransactionRepository : IRepository<LoyaltyTransaction>
    {
        /// <summary>
        /// Gets loyalty transactions for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of loyalty transactions for the customer</returns>
        Task<IEnumerable<LoyaltyTransaction>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets loyalty transactions for a customer with pagination
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="skip">Number of records to skip</param>
        /// <param name="take">Number of records to take</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of loyalty transactions for the customer</returns>
        Task<IEnumerable<LoyaltyTransaction>> GetByCustomerIdAsync(Guid customerId, int skip, int take, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets loyalty transactions by type for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="transactionType">The transaction type</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of loyalty transactions of the specified type</returns>
        Task<IEnumerable<LoyaltyTransaction>> GetByCustomerIdAndTypeAsync(Guid customerId, LoyaltyTransactionType transactionType, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets loyalty transactions by date range
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of loyalty transactions within the date range</returns>
        Task<IEnumerable<LoyaltyTransaction>> GetByDateRangeAsync(Guid customerId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets transactions with expiring points
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="expirationDate">The expiration date to check against</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of transactions with points expiring by the specified date</returns>
        Task<IEnumerable<LoyaltyTransaction>> GetExpiringPointsAsync(Guid customerId, DateTime expirationDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets total points earned by a customer in a date range
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Total points earned in the date range</returns>
        Task<int> GetTotalPointsEarnedAsync(Guid customerId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets total points redeemed by a customer in a date range
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="fromDate">Start date</param>
        /// <param name="toDate">End date</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>Total points redeemed in the date range</returns>
        Task<int> GetTotalPointsRedeemedAsync(Guid customerId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
    }
}
