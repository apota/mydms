using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.Repositories
{
    /// <summary>
    /// Repository interface for CustomerSurveyResponse entities
    /// </summary>
    public interface ICustomerSurveyResponseRepository
    {
        Task<IEnumerable<CustomerSurveyResponse>> GetAllResponsesAsync(int skip = 0, int take = 50);
        Task<CustomerSurveyResponse> GetResponseByIdAsync(Guid id);
        Task<IEnumerable<CustomerSurveyResponse>> GetResponsesBySurveyIdAsync(Guid surveyId, int skip = 0, int take = 50);
        Task<IEnumerable<CustomerSurveyResponse>> GetResponsesByCustomerIdAsync(Guid customerId, int skip = 0, int take = 50);
        Task<CustomerSurveyResponse> CreateResponseAsync(CustomerSurveyResponse response);
        Task<CustomerSurveyResponse> UpdateResponseAsync(CustomerSurveyResponse response);
        Task<bool> DeleteResponseAsync(Guid id);
        Task<int> GetResponseCountAsync(Guid? surveyId = null, Guid? customerId = null);
        Task<bool> ResponseExistsAsync(Guid id);
        Task<double> GetAverageScoreAsync(Guid surveyId);
        Task<IEnumerable<CustomerSurveyResponse>> GetResponsesByQuestionIdAsync(Guid questionId, int skip = 0, int take = 50);
    }
}
