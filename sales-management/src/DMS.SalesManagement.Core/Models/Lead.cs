using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.SalesManagement.Core.Models
{
    /// <summary>
    /// Represents the type of customer interest
    /// </summary>
    public enum InterestType
    {
        New,
        Used,
        Service
    }

    /// <summary>
    /// Represents the status of a sales lead
    /// </summary>
    public enum LeadStatus
    {
        New,
        Contacted,
        Qualified,
        Appointment,
        Sold,
        Lost
    }

    /// <summary>
    /// Represents a type of activity with a lead
    /// </summary>
    public enum LeadActivityType
    {
        Call,
        Email,
        Visit,
        TestDrive,
        Appointment,
        Followup,
        Other
    }

    /// <summary>
    /// Represents an activity performed with a lead
    /// </summary>
    public class LeadActivity
    {
        public Guid Id { get; set; }
        public LeadActivityType Type { get; set; }
        public DateTime Date { get; set; }
        public string? UserId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// Represents a customer address
    /// </summary>
    public class Address
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
    }

    /// <summary>
    /// Represents a sales lead in the system
    /// </summary>
    public class Lead : IAuditableEntity, ISoftDeleteEntity
    {
        public Guid Id { get; set; }
        public string? Source { get; set; }
        public string? SourceId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Address? Address { get; set; }
        public LeadStatus Status { get; set; }
        public InterestType InterestType { get; set; }
        public Guid? InterestVehicleId { get; set; }
        public string? AssignedSalesRepId { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime? FollowupDate { get; set; }
        public List<LeadActivity> Activities { get; set; } = new List<LeadActivity>();
        
        // ISoftDeleteEntity implementation
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletedBy { get; set; }
    }
}
