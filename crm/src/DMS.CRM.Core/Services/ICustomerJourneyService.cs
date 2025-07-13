using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.Services
{
    public interface ICustomerJourneyService
    {
        Task<CustomerJourneyDto> GetJourneyByCustomerIdAsync(Guid customerId);
        Task<CustomerJourneyDto> UpdateJourneyStageAsync(Guid customerId, CustomerJourneyUpdateDto journeyDto);
        Task<bool> AddJourneyStepAsync(Guid customerId, JourneyStage stage, string notes);
        Task<IEnumerable<CustomerJourneyDto>> GetCustomersByStageAsync(JourneyStage stage, int skip = 0, int take = 50);
        Task<Dictionary<JourneyStage, int>> GetJourneyDistributionAsync();
        Task<Dictionary<string, object>> GetJourneyStatsForCustomerAsync(Guid customerId);
        Task<double> CalculatePurchaseProbabilityAsync(Guid customerId);
        Task<DateTime?> EstimateNextStageTransitionAsync(Guid customerId);
    }
}
