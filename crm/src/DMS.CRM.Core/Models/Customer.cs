using System;
using System.Collections.Generic;
using DMS.Shared.Core.Models;

namespace DMS.CRM.Core.Models
{
    /// <summary>
    /// Represents the type of contact
    /// </summary>
    public enum ContactType
    {
        Individual,
        Business
    }

    /// <summary>
    /// Represents the type of phone number
    /// </summary>
    public enum PhoneType
    {
        Mobile,
        Home,
        Work,
        Other
    }

    /// <summary>
    /// Represents the type of address
    /// </summary>
    public enum AddressType
    {
        Home,
        Work,
        Billing,
        Shipping
    }

    /// <summary>
    /// Represents the preferred communication method
    /// </summary>
    public enum CommunicationMethod
    {
        Email,
        Phone,
        SMS,
        Mail
    }

    /// <summary>
    /// Represents the source type of a customer
    /// </summary>
    public enum CustomerSourceType
    {
        WebLead,
        Showroom,
        ServiceCustomer,
        ReferralCustomer
    }

    /// <summary>
    /// Represents a phone number in the system
    /// </summary>
    public class PhoneNumber
    {
        public PhoneType Type { get; set; }
        public string? Number { get; set; }
        public bool Primary { get; set; }
        public bool ConsentToCall { get; set; }
        public DateTime? ConsentDate { get; set; }
    }

    /// <summary>
    /// Represents an address in the system
    /// </summary>
    public class Address
    {
        public AddressType Type { get; set; }
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public bool Primary { get; set; }
    }

    /// <summary>
    /// Represents communication preferences for a customer
    /// </summary>
    public class CommunicationPreferences
    {
        public CommunicationMethod PreferredMethod { get; set; }
        public bool OptInEmail { get; set; }
        public bool OptInSMS { get; set; }
        public bool OptInMail { get; set; }
        public bool DoNotContact { get; set; }
    }

    /// <summary>
    /// Represents demographic information for a customer
    /// </summary>
    public class DemographicInfo
    {
        public DateTime? BirthDate { get; set; }
        public string? Gender { get; set; }
        public string? Occupation { get; set; }
        public string? IncomeRange { get; set; }
        public string? EducationLevel { get; set; }
    }

    /// <summary>
    /// Represents a customer in the CRM system
    /// </summary>
    public class Customer : IAuditableEntity, ISoftDeleteEntity
    {
        public Guid Id { get; set; }
        public ContactType ContactType { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public List<PhoneNumber> PhoneNumbers { get; set; } = new();
        public List<Address> Addresses { get; set; } = new();
        public CommunicationPreferences CommunicationPreferences { get; set; } = new();
        public DemographicInfo DemographicInfo { get; set; } = new();
        public Guid? SourceId { get; set; }
        public CustomerSourceType SourceType { get; set; }
        public int LeadScore { get; set; }
        public LoyaltyTier LoyaltyTier { get; set; }
        public int LoyaltyPoints { get; set; }
        public decimal LifetimeValue { get; set; }
        public CustomerStatus Status { get; set; }
        public List<string> Tags { get; set; } = new();
        public string? Notes { get; set; }

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
        public virtual ICollection<CustomerVehicle> Vehicles { get; set; } = new List<CustomerVehicle>();
        public virtual ICollection<CustomerInteraction> Interactions { get; set; } = new List<CustomerInteraction>();
        public virtual CustomerJourney? Journey { get; set; }
        public virtual ICollection<CustomerSurvey> Surveys { get; set; } = new List<CustomerSurvey>();
    }
}
