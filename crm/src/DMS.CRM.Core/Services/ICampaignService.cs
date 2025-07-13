using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.Services
{
    public interface ICampaignService
    {
        Task<IEnumerable<CampaignDto>> GetAllCampaignsAsync(int skip = 0, int take = 50);
        Task<CampaignDto> GetCampaignByIdAsync(Guid id);
        Task<IEnumerable<CampaignDto>> GetActiveCampaignsAsync(DateTime date, int skip = 0, int take = 50);
        Task<CampaignDto> CreateCampaignAsync(CampaignCreateDto campaignDto);
        Task<CampaignDto> UpdateCampaignAsync(Guid id, CampaignUpdateDto campaignDto);
        Task<bool> DeleteCampaignAsync(Guid id);
        Task<bool> ActivateCampaignAsync(Guid id);
        Task<bool> DeactivateCampaignAsync(Guid id);
        Task<Dictionary<string, object>> GetCampaignMetricsAsync(Guid id);
        Task<IEnumerable<CustomerDto>> GetCampaignTargetCustomersAsync(Guid campaignId, int skip = 0, int take = 50);
        Task<IEnumerable<CustomerInteractionDto>> GetCampaignInteractionsAsync(Guid campaignId, int skip = 0, int take = 50);
        
        // New methods
        Task<IEnumerable<CampaignDto>> GetCampaignsByTypeAsync(CampaignType type);
        Task<IEnumerable<CampaignDto>> GetCampaignsByStatusAsync(CampaignStatus status);
        Task<IEnumerable<CampaignDto>> GetCampaignsByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<CampaignDto>> GetCampaignsBySegmentAsync(Guid segmentId);
        Task<CampaignDto> UpdateCampaignMetricsAsync(Guid id, CampaignMetricsUpdateDto metricsDto);
    }
}
