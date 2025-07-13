using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents the target audience for a campaign
    /// </summary>
    public class TargetAudience
    {
        public List<Guid> Segments { get; set; } = new();
        public int EstimatedReach { get; set; }
        public string? FilterCriteria { get; set; }
    }

    /// <summary>
    /// Represents the content of a marketing campaign
    /// </summary>
    public class CampaignContent
    {
        public Guid? TemplateId { get; set; }
        public string? Subject { get; set; }
        public string? Message { get; set; }
        public List<string> MediaUrls { get; set; } = new();
        public string? CallToAction { get; set; }
        public string? LandingPageUrl { get; set; }
    }

    /// <summary>
    /// Represents metrics for a marketing campaign
    /// </summary>
    public class CampaignMetrics
    {
        public int Sent { get; set; }
        public int Delivered { get; set; }
        public int Opened { get; set; }
        public int Clicked { get; set; }
        public int Converted { get; set; }
        public decimal ROI { get; set; }
    }

    /// <summary>
    /// Represents a marketing campaign in the system
    /// </summary>
    public class Campaign : IAuditableEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public CampaignType Type { get; set; }
        public CampaignStatus Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public decimal Budget { get; set; }
        public TargetAudience TargetAudience { get; set; } = new();
        public CampaignContent Content { get; set; } = new();
        public CampaignMetrics Metrics { get; set; } = new();

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual ICollection<CampaignSegment> CampaignSegments { get; set; } = new List<CampaignSegment>();
    }
}
