using System;
using System.Collections.Generic;

namespace DMS.SalesManagement.Core.DTOs
{
    // Accessories for vehicles
    public class AccessoryDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public bool IsInstalled { get; set; }
        public string ImageUrl { get; set; }
        public int InstallationTimeMinutes { get; set; }
    }

    // Parts compatible with vehicles
    public class PartDto
    {
        public string Id { get; set; }
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; }
        public string Category { get; set; }
        public string ManufacturerName { get; set; }
    }

    // Request to reserve parts for a deal
    public class ReservePartsRequestDto
    {
        public List<ReservePartItemDto> Parts { get; set; } = new List<ReservePartItemDto>();
        public DateTime RequiredDate { get; set; }
        public string Notes { get; set; }
    }

    public class ReservePartItemDto
    {
        public string PartId { get; set; }
        public int Quantity { get; set; }
    }

    // Result of parts reservation
    public class PartsReservationDto
    {
        public string ReservationId { get; set; }
        public string DealId { get; set; }
        public List<ReservedPartDto> ReservedParts { get; set; } = new List<ReservedPartDto>();
        public DateTime ReservationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal TotalPrice { get; set; }
        public bool AllPartsAvailable { get; set; }
    }

    public class ReservedPartDto
    {
        public string PartId { get; set; }
        public string PartNumber { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime? EstimatedAvailabilityDate { get; set; }
    }

    // Customer service history items
    public class ServiceHistoryDto
    {
        public string Id { get; set; }
        public string ServiceOrderNumber { get; set; }
        public DateTime ServiceDate { get; set; }
        public string VehicleDescription { get; set; }
        public string Vin { get; set; }
        public decimal TotalCost { get; set; }
        public string Status { get; set; }
        public List<ServiceLineItemDto> LineItems { get; set; } = new List<ServiceLineItemDto>();
    }

    public class ServiceLineItemDto
    {
        public string Description { get; set; }
        public decimal Cost { get; set; }
        public string Type { get; set; } // e.g., "Repair", "Maintenance", "Part"
    }

    // Financial quotes for deals
    public class FinancialQuoteDto
    {
        public string Id { get; set; }
        public string LenderName { get; set; }
        public string ProductType { get; set; } // e.g., "Loan", "Lease"
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal DownPayment { get; set; }
        public DateTime ExpirationDate { get; set; }
        public bool IsPromotional { get; set; }
        public string PromotionDescription { get; set; }
        public List<string> Requirements { get; set; } = new List<string>();
    }

    // Request for deal financing
    public class FinancingRequestDto
    {
        public string QuoteId { get; set; }
        public string LenderId { get; set; }
        public decimal RequestedAmount { get; set; }
        public int TermMonths { get; set; }
        public decimal DownPayment { get; set; }
        public string CoBuyerId { get; set; }
        public List<string> AdditionalDocumentIds { get; set; } = new List<string>();
    }

    // Result of financing application
    public class FinancingApplicationResultDto
    {
        public string ApplicationId { get; set; }
        public string Status { get; set; } // "Approved", "Rejected", "PendingReview", "AdditionalInfoRequired"
        public string LenderName { get; set; }
        public decimal ApprovedAmount { get; set; }
        public decimal InterestRate { get; set; }
        public int TermMonths { get; set; }
        public decimal MonthlyPayment { get; set; }
        public List<FinancingConditionDto> Conditions { get; set; } = new List<FinancingConditionDto>();
        public string Comments { get; set; }
        public List<string> RequiredDocuments { get; set; } = new List<string>();
    }

    public class FinancingConditionDto
    {
        public string Type { get; set; } // e.g., "DownPayment", "ProofOfIncome", "ProofOfResidence"
        public string Description { get; set; }
        public bool IsMandatory { get; set; }
    }

    // Insurance quotes for deals
    public class InsuranceQuoteDto
    {
        public string Id { get; set; }
        public string ProviderName { get; set; }
        public string PolicyType { get; set; } // e.g., "Comprehensive", "Liability"
        public decimal AnnualPremium { get; set; }
        public decimal MonthlyPremium { get; set; }
        public decimal Deductible { get; set; }
        public decimal Coverage { get; set; }
        public List<string> Inclusions { get; set; } = new List<string>();
        public List<string> Exclusions { get; set; } = new List<string>();
        public DateTime ExpirationDate { get; set; }
        public string ContactInfo { get; set; }
    }

    // DMV registration result
    public class DmvRegistrationResultDto
    {
        public string RegistrationId { get; set; }
        public string Status { get; set; } // "Complete", "Pending", "Rejected"
        public DateTime RegistrationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string PlateNumber { get; set; }
        public decimal RegistrationFee { get; set; }
        public decimal TaxesPaid { get; set; }
        public List<string> RequiredDocuments { get; set; } = new List<string>();
        public string Comments { get; set; }
    }

    // Invoice for a deal
    public class InvoiceDto
    {
        public string InvoiceNumber { get; set; }
        public DateTime InvoiceDate { get; set; }
        public string DealId { get; set; }
        public string CustomerName { get; set; }
        public string VehicleDescription { get; set; }
        public decimal Subtotal { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public List<InvoiceLineItemDto> LineItems { get; set; } = new List<InvoiceLineItemDto>();
        public List<InvoicePaymentDto> Payments { get; set; } = new List<InvoicePaymentDto>();
        public decimal Balance { get; set; }
        public string Status { get; set; } // "Paid", "PartiallyPaid", "Unpaid"
    }

    public class InvoiceLineItemDto
    {
        public string Description { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal ExtendedPrice { get; set; }
        public string ItemType { get; set; } // "Vehicle", "Accessory", "Service", "Fee", "Tax"
    }

    public class InvoicePaymentDto
    {
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public decimal Amount { get; set; }
        public string ReferenceNumber { get; set; }
    }
}
