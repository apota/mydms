using System;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents a customer's response to a survey
    /// </summary>
    public class CustomerSurveyResponse : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid SurveyId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid QuestionId { get; set; }
        public string? Question { get; set; }
        public ResponseType ResponseType { get; set; }
        public string? Response { get; set; }
        public int? Score { get; set; }
        public DateTime SubmittedAt { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual CustomerSurvey? Survey { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
