using System;
using System.Collections.Generic;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.DTOs
{
    public record CustomerJourneyDto
    {
        public Guid Id { get; init; }
        public Guid CustomerId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public JourneyStage Stage { get; init; }
        public string? Substage { get; init; }
        public string? CurrentMilestone { get; init; }
        public string? NextMilestone { get; init; }
        public DateTime JourneyStartDate { get; init; }
        public DateTime? EstimatedCompletionDate { get; init; }
        public string? AssignedToId { get; init; }
        public string? AssignedToName { get; init; }
        public DateTime? LastActivityDate { get; init; }
        public ScheduledActivityDto? NextScheduledActivity { get; init; }
        public int JourneyScore { get; init; }
        public double PurchaseProbability { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public List<JourneyStepDto> JourneySteps { get; init; } = new();
    }

    public record CustomerJourneyCreateDto
    {
        public Guid CustomerId { get; init; }
        public JourneyStage Stage { get; init; }
        public string? Substage { get; init; }
        public string? CurrentMilestone { get; init; }
        public string? NextMilestone { get; init; }
        public DateTime JourneyStartDate { get; init; }
        public DateTime? EstimatedCompletionDate { get; init; }
        public string? AssignedToId { get; init; }
        public ScheduledActivityDto? NextScheduledActivity { get; init; }
        public int JourneyScore { get; init; }
    }

    public record CustomerJourneyUpdateDto
    {
        public JourneyStage NewStage { get; init; }
        public string? Substage { get; init; }
        public string? CurrentMilestone { get; init; }
        public string? NextMilestone { get; init; }
        public DateTime? EstimatedCompletionDate { get; init; }
        public string? AssignedToId { get; init; }
        public ScheduledActivityDto? NextScheduledActivity { get; init; }
        public int? JourneyScore { get; init; }
        public string? Notes { get; init; }
    }

    public record ScheduledActivityDto
    {
        public string? Type { get; init; }
        public DateTime? Date { get; init; }
        public string? Description { get; init; }
    }

    public record JourneyStepDto
    {
        public Guid Id { get; init; }
        public JourneyStage Stage { get; init; }
        public string StepName { get; init; } = string.Empty;
        public DateTime? CompletedDate { get; init; }
        public string? Notes { get; init; }
        public string? CompletedBy { get; init; }
        public int Order { get; init; }
    }
}
