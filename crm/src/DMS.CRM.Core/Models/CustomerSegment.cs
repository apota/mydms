using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents a segment of customers
    /// </summary>
    public class CustomerSegment : IAuditableEntity, ISoftDeleteEntity
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public SegmentType Type { get; set; }
        public string? Criteria { get; set; } // JSON object defining segment rules
        public int MemberCount { get; set; }
        public DateTime? LastRefreshed { get; set; }
        public List<string> Tags { get; set; } = new();

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }

        // ISoftDeleteEntity implementation
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }

        // Navigation properties
        public virtual ICollection<SegmentMember> Members { get; set; } = new List<SegmentMember>();
        public virtual ICollection<CampaignSegment> CampaignSegments { get; set; } = new List<CampaignSegment>();
    }

    /// <summary>
    /// Represents a member of a customer segment
    /// </summary>
    public class SegmentMember
    {
        public Guid Id { get; set; }
        public Guid SegmentId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime AddedOn { get; set; }

        // Navigation properties
        public virtual CustomerSegment? Segment { get; set; }
        public virtual Customer? Customer { get; set; }
    }
}
