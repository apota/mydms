using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.SalesManagement.Core.DTOs;

namespace DMS.SalesManagement.Core.Services
{
    public interface IIntegrationService
    {
        /// <summary>
        /// Get accessories available for a specific vehicle
        /// </summary>
        Task<IEnumerable<AccessoryDto>> GetVehicleAccessoriesAsync(string vehicleId);

        /// <summary>
        /// Get parts compatible with a specific vehicle
        /// </summary>
        Task<IEnumerable<PartDto>> GetCompatiblePartsAsync(string vehicleId);

        /// <summary>
        /// Reserve parts for a deal
        /// </summary>
        Task<PartsReservationDto> ReservePartsForDealAsync(string dealId, ReservePartsRequestDto request);

        /// <summary>
        /// Get service history for a customer
        /// </summary>
        Task<IEnumerable<ServiceHistoryDto>> GetCustomerServiceHistoryAsync(string customerId);

        /// <summary>
        /// Get financial quotes available for a deal
        /// </summary>
        Task<IEnumerable<FinancialQuoteDto>> GetFinancialQuotesForDealAsync(string dealId);

        /// <summary>
        /// Submit a deal for financing
        /// </summary>
        Task<FinancingApplicationResultDto> SubmitDealForFinancingAsync(string dealId, FinancingRequestDto request);
        
        /// <summary>
        /// Retrieve insurance quotes for a deal
        /// </summary>
        Task<IEnumerable<InsuranceQuoteDto>> GetInsuranceQuotesAsync(string dealId);
        
        /// <summary>
        /// Register a vehicle sale with DMV
        /// </summary>
        Task<DmvRegistrationResultDto> RegisterVehicleWithDmvAsync(string dealId);
        
        /// <summary>
        /// Create invoice for a completed deal
        /// </summary>
        Task<InvoiceDto> CreateDealInvoiceAsync(string dealId);
    }
}
