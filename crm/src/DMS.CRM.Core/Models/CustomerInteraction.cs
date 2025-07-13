using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents the type of customer interaction
    /// </summary>
    public enum InteractionType
    {
        Call,
        Email,
        SMS,
        InPerson,
        Web,
        Social
    }

    /// <summary>
    /// Represents the direction of an interaction
    /// </summary>
    public enum InteractionDirection
    {
        Inbound,
        Outbound
    }

    /// <summary>
    /// Represents the sentiment of an interaction
    /// </summary>
    public enum InteractionSentiment
    {
        Positive,
        Neutral,
        Negative
    }

    /// <summary>
    /// Represents an interaction with a customer
    /// </summary>
    public class CustomerInteraction : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public InteractionType Type { get; set; }
        public InteractionDirection Direction { get; set; }
        public string? ChannelId { get; set; }
        public DateTime InteractionDate { get; set; }
        public int? Duration { get; set; } // Duration in seconds
        public string? UserId { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public string? Outcome { get; set; }
        public InteractionSentiment Sentiment { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? RelatedToType { get; set; }
        public Guid? RelatedToId { get; set; }
        
        // Follow-up properties
        public bool RequiresFollowUp { get; set; }
        public DateTime? FollowUpDate { get; set; }
        public string? FollowUpAssignedToId { get; set; }
        public InteractionStatus Status { get; set; } = InteractionStatus.Completed;
        public string? FollowUpNotes { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual Customer? Customer { get; set; }
    }
}
