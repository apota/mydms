using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.CRM.Core.Repositories;
using DMS.Shared.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DMS.CRM.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository implementation for CustomerSurvey entities
    /// </summary>
    public class CustomerSurveyRepository : EfRepository<CustomerSurvey>, ICustomerSurveyRepository
    {
        private readonly CrmDbContext _dbContext;

        public CustomerSurveyRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerSurvey>> GetAllSurveysAsync(int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerSurveys
                .OrderByDescending(s => s.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<CustomerSurvey> GetSurveyByIdAsync(Guid id)
        {
            var survey = await _dbContext.CustomerSurveys
                .Include(s => s.Customer)
                .Include(s => s.SurveyResponses)
                .FirstOrDefaultAsync(s => s.Id == id);
            return survey ?? throw new InvalidOperationException($"Survey with ID {id} not found");
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerSurvey>> GetSurveysByCampaignIdAsync(Guid campaignId, int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerSurveys
                .Where(s => s.RelatedToType == "Campaign" && s.RelatedToId == campaignId)
                .OrderByDescending(s => s.CreatedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerSurvey>> GetSurveysByDateRangeAsync(DateTime startDate, DateTime endDate, int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerSurveys
                .Where(s => s.SentDate >= startDate && s.SentDate <= endDate)
                .OrderByDescending(s => s.SentDate)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<CustomerSurvey> CreateSurveyAsync(CustomerSurvey survey)
        {
            _dbContext.CustomerSurveys.Add(survey);
            await _dbContext.SaveChangesAsync();
            return survey;
        }

        /// <inheritdoc />
        public async Task<CustomerSurvey> UpdateSurveyAsync(CustomerSurvey survey)
        {
            _dbContext.CustomerSurveys.Update(survey);
            await _dbContext.SaveChangesAsync();
            return survey;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteSurveyAsync(Guid id)
        {
            var survey = await _dbContext.CustomerSurveys.FindAsync(id);
            if (survey == null)
                return false;

            _dbContext.CustomerSurveys.Remove(survey);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc />
        public async Task<int> GetSurveyCountAsync(DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _dbContext.CustomerSurveys.AsQueryable();

            if (startDate.HasValue)
                query = query.Where(s => s.SentDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.SentDate <= endDate.Value);

            return await query.CountAsync();
        }

        /// <inheritdoc />
        public async Task<bool> SurveyExistsAsync(Guid id)
        {
            return await _dbContext.CustomerSurveys.AnyAsync(s => s.Id == id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerSurveyResponse>> GetSurveyResponsesAsync(Guid surveyId, int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerSurveyResponses
                .Where(r => r.SurveyId == surveyId)
                .OrderBy(r => r.SubmittedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerSurveyResponse>> GetResponsesByCustomerIdAsync(Guid customerId, int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerSurveyResponses
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.SubmittedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<CustomerSurveyResponse> GetSurveyResponseByIdAsync(Guid id)
        {
            var response = await _dbContext.CustomerSurveyResponses
                .Include(r => r.Survey)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
            return response ?? throw new InvalidOperationException($"Survey response with ID {id} not found");
        }

        /// <inheritdoc />
        public async Task<CustomerSurveyResponse> CreateSurveyResponseAsync(CustomerSurveyResponse response)
        {
            _dbContext.CustomerSurveyResponses.Add(response);
            await _dbContext.SaveChangesAsync();
            return response;
        }

        /// <inheritdoc />
        public async Task<CustomerSurveyResponse> UpdateSurveyResponseAsync(CustomerSurveyResponse response)
        {
            _dbContext.CustomerSurveyResponses.Update(response);
            await _dbContext.SaveChangesAsync();
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteSurveyResponseAsync(Guid id)
        {
            var response = await _dbContext.CustomerSurveyResponses.FindAsync(id);
            if (response == null)
                return false;

            _dbContext.CustomerSurveyResponses.Remove(response);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc />
        public async Task<int> GetCompletedResponsesCountAsync(Guid surveyId)
        {
            return await _dbContext.CustomerSurveyResponses
                .Where(r => r.SurveyId == surveyId)
                .CountAsync();
        }

        /// <inheritdoc />
        public async Task<double> GetAverageSatisfactionScoreAsync(Guid surveyId)
        {
            var responses = await _dbContext.CustomerSurveyResponses
                .Where(r => r.SurveyId == surveyId && r.Score.HasValue)
                .ToListAsync();

            if (!responses.Any())
                return 0.0;

            return responses.Average(r => r.Score!.Value);
        }
    }
}
