using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents the type of survey
    /// </summary>
    public enum SurveyType
    {
        SalesSatisfaction,
        ServiceSatisfaction,
        NPS,
        Custom
    }

    /// <summary>
    /// Represents the type of survey response
    /// </summary>
    public enum ResponseType
    {
        Text,
        Rating,
        Boolean,
        MultiChoice
    }

    /// <summary>
    /// Represents the status of a survey follow-up
    /// </summary>
    public enum FollowUpStatus
    {
        Pending,
        InProgress,
        Completed,
        NoAction
    }

    /// <summary>
    /// Represents a response to a survey question
    /// </summary>
    public class SurveyResponse
    {
        public Guid QuestionId { get; set; }
        public string? Question { get; set; }
        public ResponseType ResponseType { get; set; }
        public string? Response { get; set; }
        public int? Score { get; set; }
    }

    /// <summary>
    /// Represents a customer survey
    /// </summary>
    public class CustomerSurvey : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public SurveyType SurveyType { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? RelatedToType { get; set; }
        public Guid? RelatedToId { get; set; }
        public DateTime? SentDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int? OverallScore { get; set; }
        public string? Comments { get; set; }
        public bool FollowUpRequired { get; set; }
        public string? FollowUpAssignedToId { get; set; }
        public FollowUpStatus FollowUpStatus { get; set; }
        public SurveyStatus Status { get; set; } = SurveyStatus.Draft;

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
        public virtual ICollection<CustomerSurveyResponse> SurveyResponses { get; set; } = new List<CustomerSurveyResponse>();
    }
}
