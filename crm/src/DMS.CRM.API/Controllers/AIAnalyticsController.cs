using DMS.CRM.Core.DTOs;
using DMS.CRM.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DMS.CRM.API.Controllers
{
    [ApiController]
    [Route("api/analytics")]
    [Authorize]
    public class AIAnalyticsController : ControllerBase
    {
        private readonly IAIAnalyticsService _analyticsService;
        private readonly ILogger<AIAnalyticsController> _logger;

        public AIAnalyticsController(
            IAIAnalyticsService analyticsService,
            ILogger<AIAnalyticsController> logger)
        {
            _analyticsService = analyticsService ?? throw new ArgumentNullException(nameof(analyticsService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpPost("sentiment")]
        [SwaggerOperation(Summary = "Analyze text sentiment", Description = "Analyzes the sentiment of the provided text")]
        [SwaggerResponse(200, "Sentiment analysis result", typeof(SentimentAnalysisResultDto))]
        public async Task<ActionResult<SentimentAnalysisResultDto>> AnalyzeSentiment([FromBody] TextAnalysisRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest("Text to analyze cannot be empty");
                }

                var result = await _analyticsService.AnalyzeSentimentAsync(request.Text);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing sentiment");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error analyzing text sentiment");
            }
        }

        [HttpPost("sentiment/batch")]
        [SwaggerOperation(Summary = "Analyze batch text sentiment", Description = "Analyzes the sentiment of multiple text inputs")]
        [SwaggerResponse(200, "Sentiment analysis results", typeof(IEnumerable<SentimentAnalysisResultDto>))]
        public async Task<ActionResult<IEnumerable<SentimentAnalysisResultDto>>> AnalyzeSentimentBatch([FromBody] BatchTextAnalysisRequestDto request)
        {
            try
            {
                if (request.Texts == null || request.Texts.Count == 0)
                {
                    return BadRequest("At least one text item must be provided");
                }

                var results = await _analyticsService.AnalyzeSentimentBatchAsync(request.Texts);
                return Ok(results);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing batch sentiments");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error analyzing batch texts sentiment");
            }
        }

        [HttpPost("topics")]
        [SwaggerOperation(Summary = "Extract topics from text", Description = "Extracts key topics and entities from the provided text")]
        [SwaggerResponse(200, "Text analysis result with topics", typeof(TextAnalysisResultDto))]
        public async Task<ActionResult<TextAnalysisResultDto>> ExtractTopics([FromBody] TextAnalysisRequestDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Text))
                {
                    return BadRequest("Text to analyze cannot be empty");
                }

                var result = await _analyticsService.ExtractTopicsAsync(request.Text);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extracting topics");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error extracting topics from text");
            }
        }

        [HttpGet("churn-prediction/{customerId:guid}")]
        [SwaggerOperation(Summary = "Predict customer churn", Description = "Predicts the likelihood of customer churn")]
        [SwaggerResponse(200, "Churn prediction result", typeof(ChurnPredictionResultDto))]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult<ChurnPredictionResultDto>> PredictChurn(Guid customerId)
        {
            try
            {
                var result = await _analyticsService.PredictCustomerChurnAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting churn for customer: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error predicting customer churn");
            }
        }

        [HttpGet("next-best-action/{customerId:guid}")]
        [SwaggerOperation(Summary = "Get next best action", Description = "Predicts the next best action for customer engagement")]
        [SwaggerResponse(200, "Next best action recommendations", typeof(NextBestActionDto))]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult<NextBestActionDto>> GetNextBestAction(Guid customerId)
        {
            try
            {
                var result = await _analyticsService.PredictNextBestActionAsync(customerId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting next best action for customer: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error predicting next best action");
            }
        }

        [HttpGet("content-recommendations/{customerId:guid}")]
        [SwaggerOperation(Summary = "Get content recommendations", Description = "Generates personalized content recommendations for a customer")]
        [SwaggerResponse(200, "Content recommendations", typeof(ContentRecommendationDto))]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult<ContentRecommendationDto>> GetContentRecommendations(
            Guid customerId, 
            [FromQuery] string context = "email")
        {
            try
            {
                var result = await _analyticsService.GenerateContentRecommendationsAsync(customerId, context);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating content recommendations for customer: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error generating content recommendations");
            }
        }

        [HttpGet("optimal-contact-time/{customerId:guid}")]
        [SwaggerOperation(Summary = "Get optimal contact time", Description = "Predicts the optimal time to contact a customer")]
        [SwaggerResponse(200, "Optimal contact time", typeof(OptimalContactTimeDto))]
        [SwaggerResponse(404, "Customer not found")]
        public async Task<ActionResult<OptimalContactTimeDto>> GetOptimalContactTime(
            Guid customerId, 
            [FromQuery] string channel)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(channel))
                {
                    return BadRequest("Communication channel must be specified");
                }

                var result = await _analyticsService.PredictOptimalContactTimeAsync(customerId, channel);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting optimal contact time for customer: {CustomerId}", customerId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error predicting optimal contact time");
            }
        }

        [HttpGet("campaign-optimization/{campaignId:guid}")]
        [SwaggerOperation(Summary = "Get campaign optimization recommendations", Description = "Analyzes campaign performance and provides optimization recommendations")]
        [SwaggerResponse(200, "Campaign optimization recommendations", typeof(CampaignOptimizationDto))]
        [SwaggerResponse(404, "Campaign not found")]
        public async Task<ActionResult<CampaignOptimizationDto>> GetCampaignOptimization(Guid campaignId)
        {
            try
            {
                var result = await _analyticsService.GetCampaignOptimizationRecommendationsAsync(campaignId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating optimization recommendations for campaign: {CampaignId}", campaignId);
                return StatusCode(StatusCodes.Status500InternalServerError, "Error generating campaign optimization recommendations");
            }
        }

        [HttpGet("discover-segments")]
        [SwaggerOperation(Summary = "Discover customer segments", Description = "Uses clustering algorithms to discover customer segments based on behavior patterns")]
        [SwaggerResponse(200, "Discovered customer segments", typeof(IEnumerable<CustomerSegmentDto>))]
        public async Task<ActionResult<IEnumerable<CustomerSegmentDto>>> DiscoverSegments()
        {
            try
            {
                var result = await _analyticsService.DiscoverCustomerSegmentsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering customer segments");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error discovering customer segments");
            }
        }
    }

    public class TextAnalysisRequestDto
    {
        public string Text { get; set; }
    }

    public class BatchTextAnalysisRequestDto
    {
        public List<string> Texts { get; set; }
    }
}
