using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;

namespace DMS.CRM.Core.Services
{
    public interface ICustomerSurveyService
    {
        Task<IEnumerable<CustomerSurveyDto>> GetAllSurveysAsync(int skip = 0, int take = 50);
        Task<CustomerSurveyDto> GetSurveyByIdAsync(Guid id);
        Task<IEnumerable<CustomerSurveyDto>> GetSurveysByCustomerIdAsync(Guid customerId, int skip = 0, int take = 50);
        Task<IEnumerable<CustomerSurveyDto>> GetSurveysByCampaignIdAsync(Guid campaignId, int skip = 0, int take = 50);
        Task<CustomerSurveyDto> CreateSurveyAsync(CustomerSurveyCreateDto surveyDto);
        Task<CustomerSurveyDto> UpdateSurveyAsync(Guid id, CustomerSurveyUpdateDto surveyDto);
        Task<bool> DeleteSurveyAsync(Guid id);
        Task<bool> ActivateSurveyAsync(Guid id);
        Task<bool> DeactivateSurveyAsync(Guid id);
        
        Task<IEnumerable<CustomerSurveyResponseDto>> GetSurveyResponsesAsync(Guid surveyId, int skip = 0, int take = 50);
        Task<CustomerSurveyResponseDto> GetSurveyResponseByIdAsync(Guid id);
        Task<CustomerSurveyResponseDto> SubmitSurveyResponseAsync(CustomerSurveyResponseCreateDto responseDto);
        Task<Dictionary<string, object>> GetSurveyMetricsAsync(Guid surveyId);
        Task<Dictionary<string, object>> GetSurveyAnalyticsAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<DateTime, double>> GetSatisfactionTrendAsync(DateTime startDate, DateTime endDate, string timeInterval);
        Task<Dictionary<string, Dictionary<string, int>>> GetSurveyResponseDistributionAsync(Guid surveyId);
        Task<double> GetAverageSatisfactionScoreAsync(Guid surveyId);
    }
}
