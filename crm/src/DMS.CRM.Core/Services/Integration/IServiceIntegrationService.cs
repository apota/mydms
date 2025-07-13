using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;

namespace DMS.CRM.Core.Services.Integration
{
    /// <summary>
    /// Integration service for connecting CRM with Service Management module
    /// </summary>
    public interface IServiceIntegrationService
    {
        /// <summary>
        /// Gets customer service history from Service module
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of service history items</returns>
        Task<IEnumerable<CustomerServiceHistoryDTO>> GetCustomerServiceHistoryAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customer's upcoming service appointments
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of upcoming service appointments</returns>
        Task<IEnumerable<ServiceAppointmentDTO>> GetUpcomingServiceAppointmentsAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customer's service reminders
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of service reminders</returns>
        Task<IEnumerable<ServiceReminderDTO>> GetServiceRemindersAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Books a service appointment for a customer
        /// </summary>
        /// <param name="appointmentDto">Service appointment details</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created appointment ID</returns>
        Task<Guid> BookServiceAppointmentAsync(ServiceAppointmentCreateDTO appointmentDto, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Gets customer's vehicle service status
        /// </summary>
        /// <param name="vehicleId">The vehicle ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Vehicle service status</returns>
        Task<VehicleServiceStatusDTO> GetVehicleServiceStatusAsync(Guid vehicleId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Synchronizes customer data with Service module
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if sync is successful</returns>
        Task<bool> SynchronizeCustomerWithServiceAsync(Guid customerId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Retrieves customer feedback on past service appointments
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of service feedback</returns>
        Task<IEnumerable<ServiceFeedbackDTO>> GetCustomerServiceFeedbackAsync(Guid customerId, CancellationToken cancellationToken = default);
    }
}
