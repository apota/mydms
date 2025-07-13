using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;

namespace DMS.CRM.Core.Services.Integration
{
    /// <summary>
    /// Integration service for connecting CRM with Inventory Management module
    /// </summary>
    public interface IInventoryIntegrationService
    {
        /// <summary>
        /// Gets recommended vehicles for a customer based on their preferences
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="limit">Maximum number of results</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of recommended vehicles</returns>
        Task<IEnumerable<VehicleRecommendationDTO>> GetRecommendedVehiclesAsync(Guid customerId, int limit = 5, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets recent inventory updates that match customer preferences
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="days">Number of days to look back</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of matching vehicles</returns>
        Task<IEnumerable<VehicleDTO>> GetMatchingRecentInventoryAsync(Guid customerId, int days = 30, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets inventory status for specific vehicle
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Vehicle inventory status</returns>
        Task<VehicleInventoryStatusDTO> GetVehicleInventoryStatusAsync(Guid vehicleId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Reserves a vehicle for a customer
        /// </summary>
        /// <param name="reservationDto">Reservation details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created reservation ID</returns>
        Task<Guid> ReserveVehicleForCustomerAsync(VehicleReservationDTO reservationDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets vehicle availability for test drives
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="date">The date for test drive</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of available time slots</returns>
        Task<IEnumerable<DateTime>> GetTestDriveAvailabilityAsync(Guid vehicleId, DateTime date, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Schedules a test drive for a customer
        /// </summary>
        /// <param name="testDriveDto">Test drive details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created test drive ID</returns>
        Task<Guid> ScheduleTestDriveAsync(TestDriveDTO testDriveDto, CancellationToken cancellationToken = default);
    }
}
