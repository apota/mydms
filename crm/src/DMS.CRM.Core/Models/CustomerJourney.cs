using System;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents a scheduled activity in the customer journey
    /// </summary>
    public class ScheduledActivity
    {
        public string? Type { get; set; }
        public DateTime? Date { get; set; }
        public string? Description { get; set; }
    }

    /// <summary>
    /// Represents a customer's journey through the dealership relationship lifecycle
    /// </summary>
    public class CustomerJourney : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public JourneyStage Stage { get; set; }
        public string? Substage { get; set; }
        public string? CurrentMilestone { get; set; }
        public string? NextMilestone { get; set; }
        public DateTime JourneyStartDate { get; set; }
        public DateTime? EstimatedCompletionDate { get; set; }
        public string? AssignedToId { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public ScheduledActivity? NextScheduledActivity { get; set; }
        public int JourneyScore { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
    }
}
