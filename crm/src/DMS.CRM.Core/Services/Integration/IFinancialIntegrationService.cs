using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;

namespace DMS.CRM.Core.Services.Integration
{
    /// <summary>
    /// Integration service for connecting CRM with Financial Management module
    /// </summary>
    public interface IFinancialIntegrationService
    {
        /// <summary>
        /// Gets customer payment history
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of payment history items</returns>
        Task<IEnumerable<CustomerPaymentHistoryDTO>> GetCustomerPaymentHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customer's active financing details
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of active financing agreements</returns>
        Task<IEnumerable<CustomerFinancingDTO>> GetCustomerActiveFinancingAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customer's credit status
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer credit status</returns>
        Task<CustomerCreditStatusDTO> GetCustomerCreditStatusAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Requests customer pre-approval for financing
        /// </summary>
        /// <param name="preapprovalDto">Pre-approval details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Pre-approval result</returns>
        Task<FinancingPreapprovalResultDTO> RequestFinancingPreapprovalAsync(FinancingPreapprovalDTO preapprovalDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customized financing offers for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of financing offers</returns>
        Task<IEnumerable<FinancingOfferDTO>> GetCustomerFinancingOffersAsync(Guid customerId, Guid vehicleId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customer invoice history
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of customer invoices</returns>
        Task<IEnumerable<CustomerInvoiceDTO>> GetCustomerInvoiceHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Checks if customer has outstanding balance
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Outstanding balance details</returns>
        Task<CustomerOutstandingBalanceDTO> GetCustomerOutstandingBalanceAsync(Guid customerId, CancellationToken cancellationToken = default);
    }
}
