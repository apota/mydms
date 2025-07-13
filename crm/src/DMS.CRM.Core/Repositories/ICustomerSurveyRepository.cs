using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.Repositories
{
    public interface ICustomerSurveyRepository
    {
        Task<IEnumerable<CustomerSurvey>> GetAllSurveysAsync(int skip = 0, int take = 50);
        Task<CustomerSurvey> GetSurveyByIdAsync(Guid id);
        Task<IEnumerable<CustomerSurvey>> GetSurveysByCampaignIdAsync(Guid campaignId, int skip = 0, int take = 50);
        Task<IEnumerable<CustomerSurvey>> GetSurveysByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50);
        Task<CustomerSurvey> CreateSurveyAsync(CustomerSurvey survey);
        Task<CustomerSurvey> UpdateSurveyAsync(CustomerSurvey survey);
        Task<bool> DeleteSurveyAsync(Guid id);
        Task<int> GetSurveyCountAsync(DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> SurveyExistsAsync(Guid id);
        
        // Survey Response methods
        Task<IEnumerable<CustomerSurveyResponse>> GetSurveyResponsesAsync(Guid surveyId, int skip = 0, int take = 50);
        Task<IEnumerable<CustomerSurveyResponse>> GetResponsesByCustomerIdAsync(Guid customerId, int skip = 0, int take = 50);
        Task<CustomerSurveyResponse> GetSurveyResponseByIdAsync(Guid id);
        Task<CustomerSurveyResponse> CreateSurveyResponseAsync(CustomerSurveyResponse response);
        Task<CustomerSurveyResponse> UpdateSurveyResponseAsync(CustomerSurveyResponse response);
        Task<bool> DeleteSurveyResponseAsync(Guid id);
        Task<int> GetCompletedResponsesCountAsync(Guid surveyId);
        Task<double> GetAverageSatisfactionScoreAsync(Guid surveyId);
    }
}
