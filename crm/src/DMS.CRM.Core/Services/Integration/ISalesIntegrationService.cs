using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;

namespace DMS.CRM.Core.Services.Integration
{
    /// <summary>
    /// Integration service for connecting CRM with Sales Management module
    /// </summary>
    public interface ISalesIntegrationService
    {
        /// <summary>
        /// Gets customer purchase history from Sales module
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of purchase history items</returns>
        Task<IEnumerable<CustomerPurchaseHistoryDTO>> GetCustomerPurchaseHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customer quotes and deals from Sales module
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of quotes and deals</returns>
        Task<IEnumerable<CustomerDealDTO>> GetCustomerDealsAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets sales staff assigned to customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of sales staff</returns>
        Task<IEnumerable<SalesPersonDTO>> GetAssignedSalesStaffAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Synchronizes customer data with Sales module
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if sync is successful</returns>
        Task<bool> SynchronizeCustomerWithSalesAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a new lead in Sales module
        /// </summary>
        /// <param name="leadDto">Lead data transfer object</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created lead ID</returns>
        Task<Guid> CreateLeadInSalesAsync(LeadCreateDTO leadDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customer preferences from sales history
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer preference data</returns>
        Task<CustomerPreferencesDTO> GetCustomerPreferencesFromSalesAsync(Guid customerId, CancellationToken cancellationToken = default);
    }
}
