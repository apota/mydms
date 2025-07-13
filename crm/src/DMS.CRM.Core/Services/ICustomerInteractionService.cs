using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.Services
{
    public interface ICustomerInteractionService
    {
        Task<IEnumerable<CustomerInteractionDto>> GetInteractionsByCustomerIdAsync(Guid customerId, int skip = 0, int take = 50);
        Task<CustomerInteractionDto> GetInteractionByIdAsync(Guid id);
        Task<IEnumerable<CustomerInteractionDto>> GetInteractionsByFilterAsync(
            DateTime? startDate, 
            DateTime? endDate, 
            InteractionType? type, 
            CommunicationChannel? channel, 
            InteractionStatus? status,
            bool? requiresFollowUp,
            string searchTerm,
            int skip = 0, 
            int take = 50);
        Task<CustomerInteractionDto> CreateInteractionAsync(CustomerInteractionCreateDto interactionDto);
        Task<CustomerInteractionDto> UpdateInteractionAsync(Guid id, CustomerInteractionUpdateDto interactionDto);
        Task<bool> DeleteInteractionAsync(Guid id);
        Task<IEnumerable<CustomerInteractionDto>> GetFollowUpsForDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetInteractionCountByTypeAsync(Guid customerId, InteractionType type);
        Task<Dictionary<string, int>> GetInteractionStatsByCustomerAsync(Guid customerId);
        Task<Dictionary<string, int>> GetInteractionStatsByChannelAsync(DateTime startDate, DateTime endDate);
    }
}
