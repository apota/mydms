using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DMS.CRM.Core.DTOs;

namespace DMS.CRM.Core.Services
{
    /// <summary>
    /// Service interface for AI-powered sentiment analysis and predictive analytics
    /// </summary>
    public interface IAIAnalyticsService
    {
        /// <summary>
        /// Analyzes customer feedback text for sentiment (positive, negative, neutral)
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>Sentiment analysis results including score and classification</returns>
        Task<SentimentAnalysisResultDto> AnalyzeSentimentAsync(string text);
        
        /// <summary>
        /// Analyzes customer feedback text for sentiment in bulk
        /// </summary>
        /// <param name="texts">Collection of texts to analyze</param>
        /// <returns>Collection of sentiment analysis results</returns>
        Task<IEnumerable<SentimentAnalysisResultDto>> AnalyzeSentimentBatchAsync(IEnumerable<string> texts);
        
        /// <summary>
        /// Extracts key topics and entities from customer feedback
        /// </summary>
        /// <param name="text">The text to analyze</param>
        /// <returns>Extracted topics and entities</returns>
        Task<TextAnalysisResultDto> ExtractTopicsAsync(string text);
        
        /// <summary>
        /// Predicts customer churn likelihood based on customer data and interaction history
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>Churn prediction result with likelihood score and contributing factors</returns>
        Task<ChurnPredictionResultDto> PredictCustomerChurnAsync(Guid customerId);
        
        /// <summary>
        /// Predicts next best action for customer engagement
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <returns>Next best action recommendations</returns>
        Task<NextBestActionDto> PredictNextBestActionAsync(Guid customerId);
        
        /// <summary>
        /// Generates personalized content recommendations for a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="context">Optional context of the content recommendation (email, sms, web)</param>
        /// <returns>Content recommendations</returns>
        Task<ContentRecommendationDto> GenerateContentRecommendationsAsync(Guid customerId, string context = "email");
        
        /// <summary>
        /// Predicts the optimal time to contact a customer
        /// </summary>
        /// <param name="customerId">The customer ID</param>
        /// <param name="channel">The communication channel (email, sms, call)</param>
        /// <returns>Optimal contact time recommendation</returns>
        Task<OptimalContactTimeDto> PredictOptimalContactTimeAsync(Guid customerId, string channel);
        
        /// <summary>
        /// Analyzes campaign performance and generates improvement recommendations
        /// </summary>
        /// <param name="campaignId">The campaign ID</param>
        /// <returns>Campaign optimization recommendations</returns>
        Task<CampaignOptimizationDto> GetCampaignOptimizationRecommendationsAsync(Guid campaignId);
        
        /// <summary>
        /// Segments customers based on behavior patterns using clustering algorithms
        /// </summary>
        /// <returns>Discovered customer segments</returns>
        Task<IEnumerable<CustomerSegmentDto>> DiscoverCustomerSegmentsAsync();
    }
}
