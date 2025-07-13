using System;
using System.Collections.Generic;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.DTOs
{
    public record CustomerSegmentDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public SegmentType Type { get; init; }
        public Dictionary<string, object> Criteria { get; init; } = new();
        public int CustomerCount { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<Guid> RelatedCampaignIds { get; init; } = new();
    }

    public record CustomerSegmentCreateDto
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public SegmentType Type { get; init; }
        public Dictionary<string, object> Criteria { get; init; } = new();
        public bool IsActive { get; init; }
    }

    public record CustomerSegmentUpdateDto
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public Dictionary<string, object> Criteria { get; init; } = new();
        public bool IsActive { get; init; }
    }

    public record CustomerSegmentSummaryDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public SegmentType Type { get; init; }
        public int CustomerCount { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}