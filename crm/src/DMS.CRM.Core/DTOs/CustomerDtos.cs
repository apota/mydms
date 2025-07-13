using System;
using System.Collections.Generic;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.DTOs
{
    /// <summary>
    /// DTO for Customer entity
    /// </summary>
    public class CustomerDto
    {
        public Guid Id { get; set; }
        public string ContactType { get; set; } = "Individual";
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public List<PhoneNumberDto> PhoneNumbers { get; set; } = new();
        public List<AddressDto> Addresses { get; set; } = new();
        public CommunicationPreferencesDto CommunicationPreferences { get; set; } = new();
        public DemographicInfoDto? DemographicInfo { get; set; }
        public string? Source { get; set; }
        public int LeadScore { get; set; }
        public string LoyaltyTier { get; set; } = "Bronze";
        public int LoyaltyPoints { get; set; }
        public decimal LifetimeValue { get; set; }
        public string Status { get; set; } = "Active";
        public List<string> Tags { get; set; } = new();
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for phone number
    /// </summary>
    public class PhoneNumberDto
    {
        public string Type { get; set; } = "Mobile";
        public string? Number { get; set; }
        public bool Primary { get; set; }
        public bool ConsentToCall { get; set; }
        public DateTime? ConsentDate { get; set; }
    }

    /// <summary>
    /// DTO for address
    /// </summary>
    public class AddressDto
    {
        public string Type { get; set; } = "Home";
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public bool Primary { get; set; }
    }

    /// <summary>
    /// DTO for communication preferences
    /// </summary>
    public class CommunicationPreferencesDto
    {
        public string PreferredMethod { get; set; } = "Email";
        public bool OptInEmail { get; set; }
        public bool OptInSMS { get; set; }
        public bool OptInMail { get; set; }
        public bool DoNotContact { get; set; }
    }

    /// <summary>
    /// DTO for demographic information
    /// </summary>
    public class DemographicInfoDto
    {
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? IncomeRange { get; set; }
        public string? EducationLevel { get; set; }
    }

    /// <summary>
    /// DTO for creating a new customer
    /// </summary>
    public class CreateCustomerDto
    {
        public string ContactType { get; set; } = "Individual";
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public List<PhoneNumberDto>? PhoneNumbers { get; set; }
        public List<AddressDto>? Addresses { get; set; }
        public CommunicationPreferencesDto? CommunicationPreferences { get; set; }
        public DemographicInfoDto? DemographicInfo { get; set; }
        public string Source { get; set; } = "Manual";
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing customer
    /// </summary>
    public class UpdateCustomerDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public List<PhoneNumberDto>? PhoneNumbers { get; set; }
        public List<AddressDto>? Addresses { get; set; }
        public CommunicationPreferencesDto? CommunicationPreferences { get; set; }
        public DemographicInfoDto? DemographicInfo { get; set; }
        public string? Status { get; set; }
        public List<string>? Tags { get; set; }
        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for customer search results
    /// </summary>
    public class CustomerSearchResultDto
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public string? PrimaryPhone { get; set; }
        public string Status { get; set; } = "Active";
        public decimal LifetimeValue { get; set; }
        public string LoyaltyTier { get; set; } = "Bronze";
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for customer statistics
    /// </summary>
    public class CustomerStatsDto
    {
        public int TotalCustomers { get; set; }
        public int ActiveCustomers { get; set; }
        public int NewCustomersThisMonth { get; set; }
        public decimal TotalLifetimeValue { get; set; }
        public decimal AverageLifetimeValue { get; set; }
        public Dictionary<string, int> CustomersByTier { get; set; } = new();
        public Dictionary<string, int> CustomersBySource { get; set; } = new();
    }

    /// <summary>
    /// DTO for customer vehicle information
    /// </summary>
    public class CustomerVehicleDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? VIN { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? Color { get; set; }
        public string? LicensePlate { get; set; }
        public string RelationshipType { get; set; } = "Owner";
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public string? PurchaseType { get; set; }
        public string? FinanceType { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime? LastServiceDate { get; set; }
        public int? Mileage { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO for creating a new customer vehicle
    /// </summary>
    public class CustomerVehicleCreateDto
    {
        public Guid CustomerId { get; set; }
        public string? VIN { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public string? Color { get; set; }
        public string? LicensePlate { get; set; }
        public string RelationshipType { get; set; } = "Owner";
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
        public string? PurchaseType { get; set; }
        public string? FinanceType { get; set; }
        public int? Mileage { get; set; }
        public string? Notes { get; set; }
    }

    // Type aliases for backward compatibility
    public class CustomerCreateDto : CreateCustomerDto { }
    public class CustomerUpdateDto : UpdateCustomerDto { }
}
