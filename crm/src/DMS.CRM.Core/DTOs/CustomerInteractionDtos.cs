using System;
using System.Collections.Generic;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.DTOs
{
    public record CustomerInteractionDto
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public string CustomerName { get; init; }
        public InteractionType Type { get; init; }
        public CommunicationChannel Channel { get; init; }
        public string Subject { get; init; }
        public string Content { get; init; }
        public InteractionDirection Direction { get; init; }
        public InteractionStatus Status { get; init; }
        public DateTime TimeStamp { get; init; }
        public Guid? UserId { get; init; }
        public string UserName { get; init; }
        public bool RequiresFollowUp { get; init; }
        public DateTime? FollowUpDate { get; init; }
        public Guid? RelatedCampaignId { get; init; }
        public string RelatedCampaignName { get; init; }
        public Guid? RelatedVehicleId { get; init; }
        public string RelatedVehicleInfo { get; init; }
        public Guid? RelatedDealId { get; init; }
        public string RelatedDealInfo { get; init; }
        public Dictionary<string, string> AdditionalAttributes { get; init; }
        public int Sentiment { get; init; }
        public List<string> Tags { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public record CustomerInteractionCreateDto
    {
        public Guid CustomerId { get; init; }
        public InteractionType Type { get; init; }
        public CommunicationChannel Channel { get; init; }
        public string Subject { get; init; }
        public string Content { get; init; }
        public InteractionDirection Direction { get; init; }
        public DateTime TimeStamp { get; init; }
        public Guid? UserId { get; init; }
        public bool RequiresFollowUp { get; init; }
        public DateTime? FollowUpDate { get; init; }
        public Guid? RelatedCampaignId { get; init; }
        public Guid? RelatedVehicleId { get; init; }
        public Guid? RelatedDealId { get; init; }
        public Dictionary<string, string> AdditionalAttributes { get; init; }
        public List<string> Tags { get; init; }
    }

    public record CustomerInteractionUpdateDto
    {
        public string Subject { get; init; }
        public string Content { get; init; }
        public InteractionStatus Status { get; init; }
        public bool RequiresFollowUp { get; init; }
        public DateTime? FollowUpDate { get; init; }
        public Dictionary<string, string> AdditionalAttributes { get; init; }
        public List<string> Tags { get; init; }
    }

    public record CustomerInteractionSummaryDto
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public string CustomerName { get; init; }
        public InteractionType Type { get; init; }
        public CommunicationChannel Channel { get; init; }
        public string Subject { get; init; }
        public InteractionStatus Status { get; init; }
        public DateTime TimeStamp { get; init; }
        public bool RequiresFollowUp { get; init; }
        public DateTime? FollowUpDate { get; init; }
    }
}
