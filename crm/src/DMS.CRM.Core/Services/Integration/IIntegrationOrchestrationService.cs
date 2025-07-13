using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.Services.Integration
{
    /// <summary>
    /// Orchestrates integration between CRM and other modules
    /// </summary>
    public interface IIntegrationOrchestrationService
    {
        /// <summary>
        /// Gets comprehensive customer data from all modules
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Comprehensive customer data</returns>
        Task<Customer360DTO> GetCustomer360DataAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Synchronizes customer data across all integrated modules
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Synchronization result</returns>
        Task<IntegrationSyncResultDTO> SynchronizeCustomerAcrossModulesAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Updates customer data across all modules when changes occur
        /// </summary>
        /// <param name="customer">Updated customer data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful</returns>
        Task<bool> PropagateCustomerChangesAsync(Customer customer, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates and enriches customer data using information from other modules
        /// </summary>
        /// <param name="customerDto">Basic customer data</param>
        /// <param name="sourceSystem">Source system identifier</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created and enriched customer</returns>
        Task<CustomerDTO> CreateEnrichedCustomerAsync(CustomerCreateDTO customerDto, string sourceSystem, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets all interactions with a customer across all modules
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="startDate">Start date for interactions</param>
        /// <param name="endDate">End date for interactions</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of customer interactions across all modules</returns>
        Task<IEnumerable<CrossModuleInteractionDTO>> GetCustomerCrossModuleInteractionsAsync(
            Guid customerId, 
            DateTime startDate, 
            DateTime endDate, 
            CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets recommended actions for a customer based on data from all modules
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of recommended actions</returns>
        Task<IEnumerable<RecommendedActionDTO>> GetRecommendedActionsAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Calculates customer lifetime value based on data from all modules
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer lifetime value data</returns>
        Task<CustomerLifetimeValueDTO> CalculateCustomerLifetimeValueAsync(Guid customerId, CancellationToken cancellationToken = default);
    }
}
