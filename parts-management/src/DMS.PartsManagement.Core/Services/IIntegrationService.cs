using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;

namespace DMS.PartsManagement.Core.Services
{
    /// <summary>
    /// Interface for integration with other DMS modules
    /// </summary>
    public interface IIntegrationService
    {
        #region CRM Integration

        /// <summary>
        /// Get customer information from CRM module
        /// </summary>
        /// <param name="customerId">Customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Customer information</returns>
        Task<CustomerDto> GetCustomerInfoFromCrmAsync(Guid customerId, CancellationToken cancellationToken = default);

        #endregion

        #region Financial Integration

        /// <summary>
        /// Get pricing information for a part from Financial Management module
        /// </summary>
        /// <param name="partId">Part ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Pricing information</returns>
        Task<PartPricingDto> GetPartPricingFromFinancialAsync(Guid partId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Create invoice for parts in Financial Management module
        /// </summary>
        /// <param name="createInvoiceDto">Invoice creation data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created invoice</returns>
        Task<InvoiceDto> CreateInvoiceInFinancialAsync(CreateInvoiceDto createInvoiceDto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Get invoice details from Financial Management module
        /// </summary>
        /// <param name="invoiceId">Invoice ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Invoice information</returns>
        Task<InvoiceDto> GetInvoiceFromFinancialAsync(Guid invoiceId, CancellationToken cancellationToken = default);

        #endregion

        #region Service Integration

        /// <summary>
        /// Get parts assigned to a service order from Service Management module
        /// </summary>
        /// <param name="serviceOrderId">Service order ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of assigned parts</returns>
        Task<IEnumerable<ServiceOrderPartDto>> GetPartsForServiceOrderAsync(Guid serviceOrderId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Assign parts to a service order in Service Management module
        /// </summary>
        /// <param name="serviceOrderId">Service order ID</param>
        /// <param name="assignPartsDto">Parts assignment data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of assigned parts</returns>
        Task<IEnumerable<ServiceOrderPartDto>> AssignPartsToServiceOrderAsync(Guid serviceOrderId, AssignServiceOrderPartsDto assignPartsDto, CancellationToken cancellationToken = default);

        #endregion

        #region Sales Integration

        /// <summary>
        /// Get accessories for a vehicle from Sales Management module
        /// </summary>
        /// <param name="vehicleId">Vehicle ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of accessories</returns>
        Task<IEnumerable<VehicleAccessoryDto>> GetAccessoriesForVehicleAsync(Guid vehicleId, CancellationToken cancellationToken = default);

        #endregion
    }
}
