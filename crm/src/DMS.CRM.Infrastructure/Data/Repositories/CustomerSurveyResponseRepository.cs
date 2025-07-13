using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DMS.CRM.Core.Models;
using DMS.CRM.Core.Repositories;
using DMS.Shared.Data.Postgres;
using Microsoft.EntityFrameworkCore;

namespace DMS.CRM.Infrastructure.Data.Repositories
{
    /// <summary>
    /// Repository implementation for CustomerSurveyResponse entities
    /// </summary>
    public class CustomerSurveyResponseRepository : EfRepository<CustomerSurveyResponse>, ICustomerSurveyResponseRepository
    {
        private readonly CrmDbContext _dbContext;

        public CustomerSurveyResponseRepository(CrmDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerSurveyResponse>> GetAllResponsesAsync(int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerSurveyResponses
                .OrderByDescending(r => r.SubmittedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<CustomerSurveyResponse> GetResponseByIdAsync(Guid id)
        {
            var response = await _dbContext.CustomerSurveyResponses
                .Include(r => r.Survey)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.Id == id);
            return response ?? throw new InvalidOperationException($"Survey response with ID {id} not found");
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerSurveyResponse>> GetResponsesBySurveyIdAsync(Guid surveyId, int skip = 0, int take = 50)
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
        public async Task<CustomerSurveyResponse> CreateResponseAsync(CustomerSurveyResponse response)
        {
            _dbContext.CustomerSurveyResponses.Add(response);
            await _dbContext.SaveChangesAsync();
            return response;
        }

        /// <inheritdoc />
        public async Task<CustomerSurveyResponse> UpdateResponseAsync(CustomerSurveyResponse response)
        {
            _dbContext.CustomerSurveyResponses.Update(response);
            await _dbContext.SaveChangesAsync();
            return response;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteResponseAsync(Guid id)
        {
            var response = await _dbContext.CustomerSurveyResponses.FindAsync(id);
            if (response == null)
                return false;

            _dbContext.CustomerSurveyResponses.Remove(response);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc />
        public async Task<int> GetResponseCountAsync(Guid? surveyId = null, Guid? customerId = null)
        {
            var query = _dbContext.CustomerSurveyResponses.AsQueryable();

            if (surveyId.HasValue)
                query = query.Where(r => r.SurveyId == surveyId.Value);

            if (customerId.HasValue)
                query = query.Where(r => r.CustomerId == customerId.Value);

            return await query.CountAsync();
        }

        /// <inheritdoc />
        public async Task<bool> ResponseExistsAsync(Guid id)
        {
            return await _dbContext.CustomerSurveyResponses.AnyAsync(r => r.Id == id);
        }

        /// <inheritdoc />
        public async Task<double> GetAverageScoreAsync(Guid surveyId)
        {
            var responses = await _dbContext.CustomerSurveyResponses
                .Where(r => r.SurveyId == surveyId && r.Score.HasValue)
                .ToListAsync();

            if (!responses.Any())
                return 0.0;

            return responses.Average(r => r.Score!.Value);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<CustomerSurveyResponse>> GetResponsesByQuestionIdAsync(Guid questionId, int skip = 0, int take = 50)
        {
            return await _dbContext.CustomerSurveyResponses
                .Where(r => r.QuestionId == questionId)
                .OrderByDescending(r => r.SubmittedAt)
                .Skip(skip)
                .Take(take)
                .ToListAsync();
        }
    }
}
