using System;
using System.Collections.Generic;

namespace DMS.CRM.Core.DTOs
{
    // Inventory Integration DTOs
    public class VehicleDTO
    {
        public Guid Id { get; set; }
        public string VIN { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Trim { get; set; }
        public string ExteriorColor { get; set; }
        public string InteriorColor { get; set; }
        public int Mileage { get; set; }
        public decimal ListPrice { get; set; }
        public string InventoryStatus { get; set; } // Available, Pending, Sold, In Transit
        public DateTime ArrivalDate { get; set; }
        public int DaysInInventory { get; set; }
        public string Condition { get; set; } // New, Used, CPO
        public List<string> Features { get; set; }
        public List<string> Images { get; set; }
    }

    public class VehicleRecommendationDTO
    {
        public Guid VehicleId { get; set; }
        public string VIN { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string Trim { get; set; }
        public decimal ListPrice { get; set; }
        public string ExteriorColor { get; set; }
        public string Condition { get; set; }
        public float MatchScore { get; set; } // How well it matches the customer's preferences
        public List<string> MatchingPreferences { get; set; }
        public string PrimaryImage { get; set; }
        public string Status { get; set; }
    }

    public class VehicleInventoryStatusDTO
    {
        public Guid VehicleId { get; set; }
        public string Status { get; set; }
        public bool IsReserved { get; set; }
        public DateTime? ReservedUntil { get; set; }
        public bool HasPendingOffers { get; set; }
        public int OffersCount { get; set; }
        public bool IsAvailableForTestDrive { get; set; }
        public bool IsAvailableForDelivery { get; set; }
        public string LocationName { get; set; }
        public int EstimatedPreparationDays { get; set; }
    }

    public class VehicleReservationDTO
    {
        public Guid VehicleId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime ReservationDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string Notes { get; set; }
        public string SalesPersonId { get; set; }
        public bool RequiresDeposit { get; set; }
        public decimal DepositAmount { get; set; }
        public bool DepositPaid { get; set; }
    }

    public class TestDriveDTO
    {
        public Guid VehicleId { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public int DurationMinutes { get; set; }
        public string SalesPersonId { get; set; }
        public string Notes { get; set; }
        public bool CustomerIsPresent { get; set; }
        public string PickupLocation { get; set; }
    }

    // Financial Integration DTOs
    public class CustomerPaymentHistoryDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } // Credit Card, Cash, Check, ACH
        public string PaymentType { get; set; } // Vehicle Payment, Service Payment, Parts Payment
        public string ReferenceNumber { get; set; }
        public string Status { get; set; } // Processed, Pending, Failed, Refunded
        public string RelatedInvoiceNumber { get; set; }
        public Guid? RelatedVehicleId { get; set; }
    }

    public class CustomerFinancingDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid VehicleId { get; set; }
        public string FinancingType { get; set; } // Loan, Lease
        public string FinancingProvider { get; set; }
        public decimal OriginalAmount { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal MonthlyPayment { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int TermMonths { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime NextPaymentDue { get; set; }
        public int RemainingPayments { get; set; }
        public bool IsDelinquent { get; set; }
        public int DaysDelinquent { get; set; }
    }

    public class CustomerCreditStatusDTO
    {
        public Guid CustomerId { get; set; }
        public string CreditRating { get; set; } // Excellent, Good, Fair, Poor
        public int CreditScore { get; set; }
        public bool IsPreApproved { get; set; }
        public decimal PreApprovedAmount { get; set; }
        public bool HasPreviousFinancing { get; set; }
        public bool IsInGoodStanding { get; set; }
        public DateTime LastCreditCheck { get; set; }
        public bool CanFinance { get; set; }
        public string RecommendedFinancingType { get; set; }
    }

    public class FinancingPreapprovalDTO
    {
        public Guid CustomerId { get; set; }
        public Guid? VehicleId { get; set; }
        public decimal RequestedAmount { get; set; }
        public int RequestedTerm { get; set; }
        public decimal DownPayment { get; set; }
        public string FinancingType { get; set; } // Loan, Lease
        public bool AuthorizeCreditCheck { get; set; }
        public string EmploymentStatus { get; set; }
        public decimal MonthlyIncome { get; set; }
    }

    public class FinancingPreapprovalResultDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Status { get; set; } // Approved, Denied, Pending
        public decimal ApprovedAmount { get; set; }
        public decimal ApprovedRate { get; set; }
        public int ApprovedTerm { get; set; }
        public decimal EstimatedMonthlyPayment { get; set; }
        public string ApprovalCode { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<string> Conditions { get; set; }
        public string Lender { get; set; }
    }

    public class FinancingOfferDTO
    {
        public Guid Id { get; set; }
        public string OfferName { get; set; }
        public string FinancingType { get; set; } // Loan, Lease
        public string Lender { get; set; }
        public decimal InterestRate { get; set; }
        public int Term { get; set; }
        public decimal MonthlyPayment { get; set; }
        public decimal DownPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public decimal TotalCost { get; set; }
        public bool IncludesGap { get; set; }
        public bool IncludesWarranty { get; set; }
        public List<string> Incentives { get; set; }
        public DateTime OfferExpirationDate { get; set; }
    }

    public class CustomerInvoiceDTO
    {
        public Guid Id { get; set; }
        public string InvoiceNumber { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public string Status { get; set; } // Paid, Partial, Unpaid, Overdue
        public string InvoiceType { get; set; } // Vehicle, Service, Parts
        public Guid? RelatedVehicleId { get; set; }
        public Guid? RelatedServiceId { get; set; }
        public List<InvoiceLineItemDTO> LineItems { get; set; }
    }

    public class InvoiceLineItemDTO
    {
        public Guid Id { get; set; }
        public Guid InvoiceId { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
        public string ItemType { get; set; } // Part, Labor, Service, Fee, Tax
        public string ItemCode { get; set; }
    }

    public class CustomerOutstandingBalanceDTO
    {
        public Guid CustomerId { get; set; }
        public decimal TotalOutstanding { get; set; }
        public decimal Current { get; set; }
        public decimal Days30 { get; set; }
        public decimal Days60 { get; set; }
        public decimal Days90Plus { get; set; }
        public int OverdueInvoicesCount { get; set; }
        public DateTime? OldestOverdueDate { get; set; }
        public string CollectionStatus { get; set; }
    }
}
