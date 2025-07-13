using System;
using System.Collections.Generic;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.DTOs
{
    public record CustomerSurveyDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Type { get; init; } = string.Empty;
        public SurveyStatus Status { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public List<SurveyQuestionDto> Questions { get; init; } = new();
        public Guid? RelatedCampaignId { get; init; }
        public string RelatedCampaignName { get; init; } = string.Empty;
        public int TotalResponses { get; init; }
        public Dictionary<string, object> SurveyMetrics { get; init; } = new();
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public record SurveyQuestionDto
    {
        public Guid Id { get; init; }
        public int Order { get; init; }
        public string Text { get; init; }
        public QuestionType Type { get; init; }
        public bool IsRequired { get; init; }
        public List<string> PossibleAnswers { get; init; }
    }

    public record CustomerSurveyCreateDto
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public SurveyType Type { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public List<SurveyQuestionCreateDto> Questions { get; init; }
        public Guid? RelatedCampaignId { get; init; }
    }

    public record SurveyQuestionCreateDto
    {
        public int Order { get; init; }
        public string Text { get; init; }
        public QuestionType Type { get; init; }
        public bool IsRequired { get; init; }
        public List<string> PossibleAnswers { get; init; }
    }

    public record CustomerSurveyUpdateDto
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public SurveyStatus Status { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public List<SurveyQuestionDto> Questions { get; init; }
    }

    public record CustomerSurveyResponseDto
    {
        public Guid Id { get; init; }
        public Guid SurveyId { get; init; }
        public Guid CustomerId { get; init; }
        public string CustomerName { get; init; }
        public DateTime ResponseDate { get; init; }
        public Dictionary<Guid, object> Answers { get; init; }
        public int SatisfactionScore { get; init; }
        public string Comments { get; init; }
        public Dictionary<string, string> AdditionalData { get; init; }
    }

    public record CustomerSurveyResponseCreateDto
    {
        public Guid SurveyId { get; init; }
        public Guid CustomerId { get; init; }
        public Dictionary<Guid, object> Answers { get; init; }
        public int? SatisfactionScore { get; init; }
        public string Comments { get; init; }
        public Dictionary<string, string> AdditionalData { get; init; }
    }
}
