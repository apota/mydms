using System;
using System.Collections.Generic;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.DTOs
{
    public record CampaignDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Description { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public CampaignStatus Status { get; init; }
        public CampaignType Type { get; init; }
        public double Budget { get; init; }
        public string TargetAudience { get; init; }
        public Dictionary<string, string> CampaignParameters { get; init; }
        public List<CampaignChannelDto> CampaignChannels { get; init; }
        public List<Guid> TargetSegmentIds { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    public record CampaignChannelDto
    {
        public Guid Id { get; init; }
        public CommunicationChannel Channel { get; init; }
        public string Message { get; init; }
        public string Template { get; init; }
        public DateTime? ScheduledTime { get; init; }
        public Dictionary<string, string> ChannelSpecificParameters { get; init; }
    }

    public record CampaignCreateDto
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public CampaignType Type { get; init; }
        public double Budget { get; init; }
        public string TargetAudience { get; init; }
        public Dictionary<string, string> CampaignParameters { get; init; }
        public List<CampaignChannelCreateDto> CampaignChannels { get; init; }
        public List<Guid> TargetSegmentIds { get; init; }
    }

    public record CampaignChannelCreateDto
    {
        public CommunicationChannel Channel { get; init; }
        public string Message { get; init; }
        public string Template { get; init; }
        public DateTime? ScheduledTime { get; init; }
        public Dictionary<string, string> ChannelSpecificParameters { get; init; }
    }

    public record CampaignUpdateDto
    {
        public string Name { get; init; }
        public string Description { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public CampaignStatus Status { get; init; }
        public double Budget { get; init; }
        public string TargetAudience { get; init; }
        public Dictionary<string, string> CampaignParameters { get; init; }
        public List<CampaignChannelDto> CampaignChannels { get; init; }
        public List<Guid> TargetSegmentIds { get; init; }
    }

    public record CampaignSummaryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public CampaignStatus Status { get; init; }
        public CampaignType Type { get; init; }
        public DateTime StartDate { get; init; }
        public DateTime? EndDate { get; init; }
        public int TargetCustomerCount { get; init; }
        public double Budget { get; init; }
    }

    public record CampaignMetricsUpdateDto
    {
        public int Sent { get; init; }
        public int Delivered { get; init; }
        public int Opened { get; init; }
        public int Clicked { get; init; }
        public int Converted { get; init; }
        public decimal ROI { get; init; }
    }
}
