using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.SalesManagement.Core.Models;
using DMS.Shared.Core.Data;

namespace DMS.SalesManagement.Core.Repositories
{
    /// <summary>
    /// Repository interface for Deal entities
    /// </summary>
    public interface IDealRepository : IRepository<Deal>
    {
        /// <summary>
        /// Gets deals assigned to a specific sales representative
        /// </summary>
        /// <param name="salesRepId">The ID of the sales representative</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of deals assigned to the specified sales representative</returns>
        Task<IEnumerable<Deal>> GetBySalesRepIdAsync(string salesRepId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets deals with a specific status
        /// </summary>
        /// <param name="status">The deal status to filter by</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of deals with the specified status</returns>
        Task<IEnumerable<Deal>> GetByStatusAsync(DealStatus status, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets deals for a specific customer
        /// </summary>
        /// <param name="customerId">The ID of the customer</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of deals for the specified customer</returns>
        Task<IEnumerable<Deal>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets deals for a specific vehicle
        /// </summary>
        /// <param name="vehicleId">The ID of the vehicle</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of deals for the specified vehicle</returns>
        Task<IEnumerable<Deal>> GetByVehicleIdAsync(Guid vehicleId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets deals created within a date range
        /// </summary>
        /// <param name="startDate">The start date</param>
        /// <param name="endDate">The end date</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>A collection of deals created within the specified date range</returns>
        Task<IEnumerable<Deal>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets a deal with all related entities (add-ons, commissions, documents)
        /// </summary>
        /// <param name="id">The ID of the deal</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The deal with all related entities if found, otherwise null</returns>
        Task<Deal?> GetWithAllRelatedDataAsync(Guid id, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Adds a status update to the deal history
        /// </summary>
        /// <param name="dealId">The ID of the deal</param>
        /// <param name="statusHistory">The status history to add</param>
        /// <param name="cancellationToken">The cancellation token</param>
        /// <returns>The updated deal</returns>
        Task<Deal> AddStatusHistoryAsync(Guid dealId, DealStatusHistory statusHistory, CancellationToken cancellationToken = default);
    }
}
