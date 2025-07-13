using System;
using System.Collections.Generic;
using DMS.SalesManagement.Core.Models;

namespace DMS.SalesManagement.Core.DTOs
{
    /// <summary>
    /// DTO for Lead entity
    /// </summary>
    public class LeadDto
    {
        public Guid Id { get; set; }
        public string? Source { get; set; }
        public string? SourceId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public AddressDto? Address { get; set; }
        public LeadStatus Status { get; set; }
        public InterestType InterestType { get; set; }
        public Guid? InterestVehicleId { get; set; }
        public string? AssignedSalesRepId { get; set; }
        public string? Comments { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public DateTime? FollowupDate { get; set; }
        public List<LeadActivityDto> Activities { get; set; } = new List<LeadActivityDto>();
    }

    /// <summary>
    /// DTO for Address value object
    /// </summary>
    public class AddressDto
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
    }

    /// <summary>
    /// DTO for LeadActivity entity
    /// </summary>
    public class LeadActivityDto
    {
        public Guid Id { get; set; }
        public LeadActivityType Type { get; set; }
        public DateTime Date { get; set; }
        public string? UserId { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for creating a new lead
    /// </summary>
    public class CreateLeadDto
    {
        public string? Source { get; set; }
        public string? SourceId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public AddressDto? Address { get; set; }
        public LeadStatus Status { get; set; } = LeadStatus.New;
        public InterestType InterestType { get; set; }
        public Guid? InterestVehicleId { get; set; }
        public string? AssignedSalesRepId { get; set; }
        public string? Comments { get; set; }
        public DateTime? FollowupDate { get; set; }
    }

    /// <summary>
    /// DTO for updating a lead
    /// </summary>
    public class UpdateLeadDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public AddressDto? Address { get; set; }
        public LeadStatus? Status { get; set; }
        public InterestType? InterestType { get; set; }
        public Guid? InterestVehicleId { get; set; }
        public string? AssignedSalesRepId { get; set; }
        public string? Comments { get; set; }
        public DateTime? FollowupDate { get; set; }
    }

    /// <summary>
    /// DTO for adding an activity to a lead
    /// </summary>
    public class AddLeadActivityDto
    {
        public LeadActivityType Type { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
    }
}
