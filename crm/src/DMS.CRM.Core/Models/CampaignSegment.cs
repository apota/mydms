using System;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents the relationship between a campaign and a customer segment
    /// </summary>
    public class CampaignSegment : IAuditableEntity
    {
        public Guid Id { get; set; }
        public Guid CampaignId { get; set; }
        public Guid SegmentId { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // Navigation properties
        public virtual Campaign? Campaign { get; set; }
        public virtual CustomerSegment? Segment { get; set; }
    }
}
