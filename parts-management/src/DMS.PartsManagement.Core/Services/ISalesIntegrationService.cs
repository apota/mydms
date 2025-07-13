using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.PartsManagement.Core.DTOs;

namespace DMS.PartsManagement.Core.Services
{
    public interface ISalesIntegrationService
    {
        /// <summary>
        /// Gets accessories for a specific vehicle
        /// </summary>
        Task<IEnumerable<AccessoryDto>> GetVehicleAccessoriesAsync(string vehicleId);

        /// <summary>
        /// Gets parts compatible with a specific vehicle
        /// </summary>
        Task<IEnumerable<PartDto>> GetCompatiblePartsAsync(string vehicleId);

        /// <summary>
        /// Reserves parts for a deal
        /// </summary>
        Task<PartsReservationDto> ReservePartsForDealAsync(string dealId, ReservePartsRequestDto request);

        /// <summary>
        /// Gets order status information for parts on a deal
        /// </summary>
        Task<IEnumerable<DealPartsOrderStatusDto>> GetDealPartsOrdersStatusAsync(string dealId);

        /// <summary>
        /// Gets installed parts and accessories for a vehicle
        /// </summary>
        Task<IEnumerable<InstalledPartDto>> GetInstalledVehiclePartsAsync(string vehicleId);

        /// <summary>
        /// Calculates installation time estimate for accessories
        /// </summary>
        Task<InstallationEstimateDto> CalculateAccessoryInstallationEstimateAsync(AccessoryInstallationRequestDto request);
    }
}
