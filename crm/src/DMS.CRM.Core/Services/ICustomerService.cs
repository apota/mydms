using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerDto>> GetAllCustomersAsync(int skip = 0, int take = 50);
        Task<CustomerDto> GetCustomerByIdAsync(Guid id);
        Task<CustomerDto> GetCustomerByEmailAsync(string email);
        Task<IEnumerable<CustomerDto>> SearchCustomersAsync(string searchTerm, int skip = 0, int take = 50);
        Task<CustomerDto> CreateCustomerAsync(CustomerCreateDto customerDto);
        Task<CustomerDto> UpdateCustomerAsync(Guid id, CustomerUpdateDto customerDto);
        Task<bool> DeleteCustomerAsync(Guid id);
        Task<CustomerStatsDto> GetCustomerStatsAsync(Guid id);
        Task<IEnumerable<CustomerVehicleDto>> GetCustomerVehiclesAsync(Guid customerId);
        Task<CustomerVehicleDto> AddCustomerVehicleAsync(Guid customerId, CustomerVehicleCreateDto vehicleDto);
        Task<bool> RemoveCustomerVehicleAsync(Guid customerId, Guid vehicleId);
    }
}
