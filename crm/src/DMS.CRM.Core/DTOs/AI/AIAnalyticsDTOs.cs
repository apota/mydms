using System;
using System.Collections.Generic;

namespace DMS.CRM.Core.DTOs
{
    /// <summary>
    /// Results of sentiment analysis on text
    /// </summary>
    public class SentimentAnalysisResultDto
    {
        /// <summary>
        /// Original text that was analyzed
        /// </summary>
        public string OriginalText { get; set; }
        
        /// <summary>
        /// Sentiment score (-1.0 to 1.0 where -1 is very negative, 0 is neutral, 1 is very positive)
        /// </summary>
        public double SentimentScore { get; set; }
        
        /// <summary>
        /// Classification of the sentiment (Negative, Neutral, Positive)
        /// </summary>
        public SentimentClassification Classification { get; set; }
        
        /// <summary>
        /// Confidence level in the sentiment analysis (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Key phrases extracted from the text that contributed to sentiment
        /// </summary>
        public List<string> KeyPhrases { get; set; }
    }
    
    /// <summary>
    /// Classification of sentiment
    /// </summary>
    public enum SentimentClassification
    {
        Negative,
        Neutral,
        Positive
    }
    
    /// <summary>
    /// Results of text analysis for topic extraction
    /// </summary>
    public class TextAnalysisResultDto
    {
        /// <summary>
        /// Original text that was analyzed
        /// </summary>
        public string OriginalText { get; set; }
        
        /// <summary>
        /// Main topics identified in the text
        /// </summary>
        public List<string> Topics { get; set; }
        
        /// <summary>
        /// Entities extracted from the text
        /// </summary>
        public List<EntityDto> Entities { get; set; }
        
        /// <summary>
        /// Key phrases extracted from the text
        /// </summary>
        public List<string> KeyPhrases { get; set; }
    }
    
    /// <summary>
    /// Entity extracted from text analysis
    /// </summary>
    public class EntityDto
    {
        /// <summary>
        /// Name of the entity
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Type of the entity (Person, Location, Organization, Product, etc.)
        /// </summary>
        public string Type { get; set; }
        
        /// <summary>
        /// Confidence score for the entity extraction (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; set; }
    }
    
    /// <summary>
    /// Customer churn prediction result
    /// </summary>
    public class ChurnPredictionResultDto
    {
        /// <summary>
        /// ID of the customer
        /// </summary>
        public Guid CustomerId { get; set; }
        
        /// <summary>
        /// Probability of customer churn (0.0 to 1.0)
        /// </summary>
        public double ChurnProbability { get; set; }
        
        /// <summary>
        /// Risk level assessment (Low, Medium, High)
        /// </summary>
        public string RiskLevel { get; set; }
        
        /// <summary>
        /// Factors contributing to churn prediction with their relative importance
        /// </summary>
        public List<PredictionFactorDto> ContributingFactors { get; set; }
        
        /// <summary>
        /// Recommended actions to prevent churn
        /// </summary>
        public List<string> RecommendedActions { get; set; }
        
        /// <summary>
        /// When the prediction was generated
        /// </summary>
        public DateTime PredictionTimestamp { get; set; }
    }
    
    /// <summary>
    /// Factor contributing to a prediction
    /// </summary>
    public class PredictionFactorDto
    {
        /// <summary>
        /// Name of the factor
        /// </summary>
        public string FactorName { get; set; }
        
        /// <summary>
        /// Importance score of the factor (0.0 to 1.0)
        /// </summary>
        public double Importance { get; set; }
        
        /// <summary>
        /// Description of how this factor impacts the prediction
        /// </summary>
        public string Description { get; set; }
    }
    
    /// <summary>
    /// Next best action recommendation for customer engagement
    /// </summary>
    public class NextBestActionDto
    {
        /// <summary>
        /// ID of the customer
        /// </summary>
        public Guid CustomerId { get; set; }
        
        /// <summary>
        /// List of recommended actions in order of priority
        /// </summary>
        public List<ActionRecommendationDto> RecommendedActions { get; set; }
        
        /// <summary>
        /// When the recommendations were generated
        /// </summary>
        public DateTime RecommendationTimestamp { get; set; }
    }
    
    /// <summary>
    /// Recommended action for customer engagement
    /// </summary>
    public class ActionRecommendationDto
    {
        /// <summary>
        /// Type of the recommended action
        /// </summary>
        public string ActionType { get; set; }
        
        /// <summary>
        /// Description of the recommended action
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Confidence score for this recommendation (0.0 to 1.0)
        /// </summary>
        public double Confidence { get; set; }
        
        /// <summary>
        /// Expected outcome if the action is taken
        /// </summary>
        public string ExpectedOutcome { get; set; }
        
        /// <summary>
        /// Timeframe in which this action should be taken
        /// </summary>
        public string Timeframe { get; set; }
    }
    
    /// <summary>
    /// Content recommendation for personalized customer communications
    /// </summary>
    public class ContentRecommendationDto
    {
        /// <summary>
        /// ID of the customer
        /// </summary>
        public Guid CustomerId { get; set; }
        
        /// <summary>
        /// Recommended content items
        /// </summary>
        public List<ContentItemDto> RecommendedContent { get; set; }
        
        /// <summary>
        /// Recommended products to feature
        /// </summary>
        public List<Guid> RecommendedProductIds { get; set; }
        
        /// <summary>
        /// Recommended personalization variables
        /// </summary>
        public Dictionary<string, string> PersonalizationVars { get; set; }
        
        /// <summary>
        /// Tone and style recommendations
        /// </summary>
        public ToneStyleRecommendationDto ToneStyleRecommendation { get; set; }
    }
    
    /// <summary>
    /// Content item recommended for customer communications
    /// </summary>
    public class ContentItemDto
    {
        /// <summary>
        /// Type of content (offer, announcement, newsletter, etc.)
        /// </summary>
        public string ContentType { get; set; }
        
        /// <summary>
        /// Recommended subject line or title
        /// </summary>
        public string SubjectLine { get; set; }
        
        /// <summary>
        /// Main message content
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// Relevance score for this content item (0.0 to 1.0)
        /// </summary>
        public double RelevanceScore { get; set; }
    }
    
    /// <summary>
    /// Tone and style recommendations for customer communications
    /// </summary>
    public class ToneStyleRecommendationDto
    {
        /// <summary>
        /// Recommended tone of voice (formal, casual, professional, friendly, etc.)
        /// </summary>
        public string ToneOfVoice { get; set; }
        
        /// <summary>
        /// Recommended writing style
        /// </summary>
        public string WritingStyle { get; set; }
        
        /// <summary>
        /// Words and phrases to include for better engagement
        /// </summary>
        public List<string> RecommendedPhrases { get; set; }
        
        /// <summary>
        /// Words and phrases to avoid
        /// </summary>
        public List<string> PhrasesToAvoid { get; set; }
    }
    
    /// <summary>
    /// Optimal time to contact a customer
    /// </summary>
    public class OptimalContactTimeDto
    {
        /// <summary>
        /// ID of the customer
        /// </summary>
        public Guid CustomerId { get; set; }
        
        /// <summary>
        /// Communication channel (email, sms, call)
        /// </summary>
        public string Channel { get; set; }
        
        /// <summary>
        /// Best day of the week for contact
        /// </summary>
        public DayOfWeek BestDayOfWeek { get; set; }
        
        /// <summary>
        /// Best time of day for contact (24-hour format HH:MM - HH:MM)
        /// </summary>
        public string BestTimeOfDay { get; set; }
        
        /// <summary>
        /// Response likelihood score for the recommended time (0.0 to 1.0)
        /// </summary>
        public double ResponseLikelihood { get; set; }
        
        /// <summary>
        /// Alternative good times for contact
        /// </summary>
        public List<string> AlternativeTimes { get; set; }
        
        /// <summary>
        /// Times to avoid contact
        /// </summary>
        public List<string> TimesToAvoid { get; set; }
    }
    
    /// <summary>
    /// Campaign optimization recommendations
    /// </summary>
    public class CampaignOptimizationDto
    {
        /// <summary>
        /// ID of the campaign
        /// </summary>
        public Guid CampaignId { get; set; }
        
        /// <summary>
        /// Overall performance score of the campaign (0.0 to 1.0)
        /// </summary>
        public double PerformanceScore { get; set; }
        
        /// <summary>
        /// Optimization recommendations for the campaign
        /// </summary>
        public List<OptimizationRecommendationDto> Recommendations { get; set; }
        
        /// <summary>
        /// Content improvement suggestions
        /// </summary>
        public List<ContentImprovementDto> ContentImprovements { get; set; }
        
        /// <summary>
        /// Target audience refinement suggestions
        /// </summary>
        public List<AudienceRefinementDto> AudienceRefinements { get; set; }
        
        /// <summary>
        /// Campaign schedule adjustments
        /// </summary>
        public ScheduleAdjustmentDto ScheduleAdjustments { get; set; }
    }
    
    /// <summary>
    /// Optimization recommendation for a campaign
    /// </summary>
    public class OptimizationRecommendationDto
    {
        /// <summary>
        /// Category of the recommendation (content, audience, schedule, etc.)
        /// </summary>
        public string Category { get; set; }
        
        /// <summary>
        /// Title of the recommendation
        /// </summary>
        public string Title { get; set; }
        
        /// <summary>
        /// Detailed description of the recommendation
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Expected impact of implementing the recommendation (high, medium, low)
        /// </summary>
        public string ExpectedImpact { get; set; }
        
        /// <summary>
        /// Implementation difficulty (easy, moderate, complex)
        /// </summary>
        public string ImplementationDifficulty { get; set; }
    }
    
    /// <summary>
    /// Content improvement suggestion for a campaign
    /// </summary>
    public class ContentImprovementDto
    {
        /// <summary>
        /// Element to improve (subject line, CTA, body text, etc.)
        /// </summary>
        public string Element { get; set; }
        
        /// <summary>
        /// Current version of the content
        /// </summary>
        public string CurrentVersion { get; set; }
        
        /// <summary>
        /// Suggested improved version
        /// </summary>
        public string SuggestedVersion { get; set; }
        
        /// <summary>
        /// Reasoning behind the suggestion
        /// </summary>
        public string Reasoning { get; set; }
    }
    
    /// <summary>
    /// Audience refinement suggestion for a campaign
    /// </summary>
    public class AudienceRefinementDto
    {
        /// <summary>
        /// Segment to add or remove
        /// </summary>
        public string Segment { get; set; }
        
        /// <summary>
        /// Action to take (include, exclude, prioritize)
        /// </summary>
        public string Action { get; set; }
        
        /// <summary>
        /// Expected size of the segment
        /// </summary>
        public int EstimatedSize { get; set; }
        
        /// <summary>
        /// Expected impact on campaign performance (percentage)
        /// </summary>
        public double ExpectedImpact { get; set; }
        
        /// <summary>
        /// Reasoning behind the suggestion
        /// </summary>
        public string Reasoning { get; set; }
    }
    
    /// <summary>
    /// Campaign schedule adjustment suggestions
    /// </summary>
    public class ScheduleAdjustmentDto
    {
        /// <summary>
        /// Optimal send day of the week
        /// </summary>
        public DayOfWeek OptimalSendDay { get; set; }
        
        /// <summary>
        /// Optimal send time of day (24-hour format)
        /// </summary>
        public string OptimalSendTime { get; set; }
        
        /// <summary>
        /// Suggested frequency adjustments
        /// </summary>
        public string FrequencyAdjustment { get; set; }
        
        /// <summary>
        /// Whether to use send time optimization per user
        /// </summary>
        public bool UseSendTimeOptimization { get; set; }
        
        /// <summary>
        /// Reasoning behind the suggestions
        /// </summary>
        public string Reasoning { get; set; }
    }
}
