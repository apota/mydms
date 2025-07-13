using System;
using System.Collections.Generic;

namespace DMS.CRM.Core.DTOs
{
    // Sales Integration DTOs
    public class CustomerPurchaseHistoryDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleDescription { get; set; }
        public string VIN { get; set; }
        public DateTime PurchaseDate { get; set; }
        public decimal PurchaseAmount { get; set; }
        public string PurchaseType { get; set; } // New, Used, CPO
        public string FinanceType { get; set; } // Cash, Finance, Lease
        public Guid SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public string DealNumber { get; set; }
    }

    public class CustomerDealDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; } // Quote, Pending, Approved, Closed, Lost
        public DateTime CreatedDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleDescription { get; set; }
        public decimal QuoteAmount { get; set; }
        public decimal? FinalAmount { get; set; }
        public Guid SalesPersonId { get; set; }
        public string SalesPersonName { get; set; }
        public List<string> Accessories { get; set; }
        public string Notes { get; set; }
    }

    public class SalesPersonDTO
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Department { get; set; }
        public string Title { get; set; }
        public string PhotoUrl { get; set; }
    }

    public class LeadCreateDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string LeadSource { get; set; }
        public string LeadStatus { get; set; }
        public string InterestType { get; set; } // New, Used, Service, Parts
        public string InterestVehicleModel { get; set; }
        public string Comments { get; set; }
        public bool OptInEmail { get; set; }
        public bool OptInSMS { get; set; }
        public bool OptInCall { get; set; }
    }

    public class CustomerPreferencesDTO
    {
        public Guid CustomerId { get; set; }
        public List<string> PreferredVehicleMakes { get; set; }
        public List<string> PreferredVehicleModels { get; set; }
        public string PreferredVehicleType { get; set; } // Sedan, SUV, Truck, etc.
        public string PreferredPriceRange { get; set; }
        public List<string> PreferredFeatures { get; set; }
        public string PreferredFinanceType { get; set; } // Cash, Finance, Lease
        public bool IsRepeatBuyer { get; set; }
        public DateTime LastPurchaseDate { get; set; }
        public int VehiclesPurchasedCount { get; set; }
    }

    // Service Integration DTOs
    public class CustomerServiceHistoryDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleDescription { get; set; }
        public string VIN { get; set; }
        public DateTime ServiceDate { get; set; }
        public string ServiceType { get; set; }
        public string Description { get; set; }
        public decimal TotalAmount { get; set; }
        public int Mileage { get; set; }
        public string TechnicianName { get; set; }
        public List<string> ServiceItems { get; set; }
        public string ServiceAdvisor { get; set; }
        public string Status { get; set; } // Completed, In Progress, Scheduled
        public string InvoiceNumber { get; set; }
    }

    public class ServiceAppointmentDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleDescription { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string ServiceType { get; set; }
        public string RequestedServices { get; set; }
        public string Status { get; set; } // Scheduled, Confirmed, In Progress, Completed, Cancelled
        public string ServiceAdvisor { get; set; }
        public string CustomerNotes { get; set; }
        public string DropOffMethod { get; set; } // Wait, Drop Off, Shuttle, Loaner
    }

    public class ServiceAppointmentCreateDTO
    {
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; }
        public string ServiceType { get; set; }
        public string RequestedServices { get; set; }
        public string CustomerNotes { get; set; }
        public string DropOffMethod { get; set; } // Wait, Drop Off, Shuttle, Loaner
        public bool NeedsTransportation { get; set; }
        public bool IsRecurring { get; set; }
    }

    public class ServiceReminderDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public string VehicleDescription { get; set; }
        public string ReminderType { get; set; } // Oil Change, Tire Rotation, State Inspection, etc.
        public DateTime DueDate { get; set; }
        public int DueMileage { get; set; }
        public bool IsSent { get; set; }
        public DateTime? LastSentDate { get; set; }
        public string Priority { get; set; } // High, Medium, Low
        public string Notes { get; set; }
    }

    public class VehicleServiceStatusDTO
    {
        public Guid VehicleId { get; set; }
        public string CurrentStatus { get; set; } // Active, In Service, Waiting for Parts, Completed, Delivered
        public DateTime LastServiceDate { get; set; }
        public DateTime? NextScheduledServiceDate { get; set; }
        public int CurrentMileage { get; set; }
        public bool HasOpenRecalls { get; set; }
        public List<string> PendingMaintenanceItems { get; set; }
        public bool IsUnderWarranty { get; set; }
        public DateTime? WarrantyExpiration { get; set; }
    }

    public class ServiceFeedbackDTO
    {
        public Guid Id { get; set; }
        public Guid ServiceAppointmentId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime ServiceDate { get; set; }
        public DateTime FeedbackDate { get; set; }
        public int SatisfactionRating { get; set; } // 1-5 scale
        public string Comments { get; set; }
        public bool WouldRecommend { get; set; }
        public string AreaOfImprovement { get; set; }
        public List<string> PositiveAspects { get; set; }
        public bool RequiresFollowUp { get; set; }
    }
}
