using System;
using System.Collections.Generic;
using DMS.CRM.Core.Models;

namespace DMS.CRM.Core.DTOs
{
    // Basic Integration DTOs
    public class CustomerDTO
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public string? PrimaryPhone { get; set; }
        public CustomerStatus Status { get; set; }
        public LoyaltyTier LoyaltyTier { get; set; }
        public decimal LifetimeValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CustomerCreateDTO
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? BusinessName { get; set; }
        public string? Email { get; set; }
        public string? PrimaryPhone { get; set; }
        public string Source { get; set; } = "Integration";
    }

    public class CustomerVehicleDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string? VIN { get; set; }
        public string? Make { get; set; }
        public string? Model { get; set; }
        public int? Year { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public decimal? PurchasePrice { get; set; }
    }

    public class CustomerInteractionDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime InteractionDate { get; set; }
        public CommunicationChannel Channel { get; set; }
        public InteractionStatus Status { get; set; }
        public string? Subject { get; set; }
        public string? Description { get; set; }
    }

    public class CustomerJourneyDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public JourneyStage Stage { get; set; }
        public string? CurrentMilestone { get; set; }
        public DateTime JourneyStartDate { get; set; }
    }

    // Integration Orchestration DTOs
    public class Customer360DTO
    {
        public CustomerDTO Customer { get; set; }
        public List<CustomerVehicleDTO> Vehicles { get; set; }
        public List<CustomerPurchaseHistoryDTO> PurchaseHistory { get; set; }
        public List<CustomerServiceHistoryDTO> ServiceHistory { get; set; }
        public List<CustomerInteractionDTO> Interactions { get; set; }
        public List<CustomerFinancingDTO> FinancingDetails { get; set; }
        public List<ServiceAppointmentDTO> UpcomingAppointments { get; set; }
        public CustomerLoyaltyStatusDTO LoyaltyStatus { get; set; }
        public List<ServiceReminderDTO> ServiceReminders { get; set; }
        public CustomerJourneyDTO CustomerJourney { get; set; }
        public CustomerOutstandingBalanceDTO FinancialStatus { get; set; }
        public CustomerPreferencesDTO Preferences { get; set; }
        public List<VehicleRecommendationDTO> RecommendedVehicles { get; set; }
        public List<RecommendedActionDTO> RecommendedActions { get; set; }
        public CustomerMetricsDTO Metrics { get; set; }
    }
    
    public class IntegrationSyncResultDTO
    {
        public Guid CustomerId { get; set; }
        public bool Success { get; set; }
        public DateTime SyncTime { get; set; }
        public Dictionary<string, bool> ModuleResults { get; set; }
        public Dictionary<string, string> ModuleMessages { get; set; }
        public List<string> Conflicts { get; set; }
        public List<string> ResolvedConflicts { get; set; }
        public bool RequiresManualReview { get; set; }
    }
    
    public class CrossModuleInteractionDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime InteractionDate { get; set; }
        public string ModuleName { get; set; }
        public string InteractionType { get; set; }
        public string Description { get; set; }
        public string OutcomeStatus { get; set; }
        public string StaffMember { get; set; }
        public string Notes { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public string RelatedEntityType { get; set; }
    }
    
    public class RecommendedActionDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string ActionType { get; set; }
        public string ActionDescription { get; set; }
        public int Priority { get; set; } // 1-5 scale
        public DateTime RecommendedByDate { get; set; }
        public string Reason { get; set; }
        public string Module { get; set; }
        public string RelatedEntityType { get; set; }
        public Guid? RelatedEntityId { get; set; }
        public decimal? PotentialValue { get; set; }
        public float SuccessProbability { get; set; }
    }
    
    public class CustomerLifetimeValueDTO
    {
        public Guid CustomerId { get; set; }
        public decimal TotalLifetimeValue { get; set; }
        public decimal VehiclePurchases { get; set; }
        public decimal ServiceRevenue { get; set; }
        public decimal PartsRevenue { get; set; }
        public decimal FinancingRevenue { get; set; }
        public DateTime FirstTransactionDate { get; set; }
        public DateTime LastTransactionDate { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransactionValue { get; set; }
        public decimal ProjectedFutureValue { get; set; }
        public int RetentionScore { get; set; } // 1-100
        public string ValueSegment { get; set; } // Premium, High, Medium, Low
    }
    
    public class CustomerMetricsDTO
    {
        public Guid CustomerId { get; set; }
        public int DaysSinceLastInteraction { get; set; }
        public int DaysSinceLastPurchase { get; set; }
        public int DaysSinceLastService { get; set; }
        public float EngagementScore { get; set; } // 0-100
        public float SatisfactionScore { get; set; } // 0-100
        public int TotalVehiclesPurchased { get; set; }
        public int TotalServiceVisits { get; set; }
        public float RetentionProbability { get; set; } // 0-1
        public string CustomerSegment { get; set; }
        public float UpsellProbability { get; set; } // 0-1
        public string NextBestAction { get; set; }
    }
    
    // Loyalty DTOs
    public class CustomerLoyaltyStatusDTO
    {
        public Guid CustomerId { get; set; }
        public string LoyaltyTier { get; set; } // Bronze, Silver, Gold, Platinum
        public int CurrentPoints { get; set; }
        public int LifetimePoints { get; set; }
        public DateTime MemberSince { get; set; }
        public DateTime? TierExpirationDate { get; set; }
        public int PointsToNextTier { get; set; }
        public string NextTier { get; set; }
        public int PointsExpiringSoon { get; set; }
        public DateTime? PointsExpirationDate { get; set; }
        public List<LoyaltyBenefitDTO> CurrentBenefits { get; set; }
        public List<LoyaltyRewardDTO> AvailableRewards { get; set; }
    }
    
    public class LoyaltyTransactionDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } // Earn, Redeem, Adjust, Expire
        public int Points { get; set; }
        public string Source { get; set; }
        public string ReferenceId { get; set; }
        public string Description { get; set; }
        public Guid? RewardId { get; set; }
        public int PointsBalance { get; set; }
    }
    
    public class LoyaltyRewardDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PointsCost { get; set; }
        public string Category { get; set; } // Service, Parts, Discount, Experience
        public string RewardType { get; set; } // Discount, Free Item, Experience, Gift Card
        public decimal MonetaryValue { get; set; }
        public bool IsActive { get; set; }
        public List<string> EligibleTiers { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantityRedeemed { get; set; }
        public string ImageUrl { get; set; }
    }
    
    public class LoyaltyRedemptionResultDTO
    {
        public Guid RedemptionId { get; set; }
        public Guid CustomerId { get; set; }
        public Guid RewardId { get; set; }
        public string RewardName { get; set; }
        public DateTime RedemptionDate { get; set; }
        public int PointsRedeemed { get; set; }
        public int RemainingPoints { get; set; }
        public bool Success { get; set; }
        public string RedemptionCode { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string RedemptionInstructions { get; set; }
        public string Status { get; set; } // Active, Used, Expired, Cancelled
    }
    
    public class RedeemedRewardDTO
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public Guid RewardId { get; set; }
        public string RewardName { get; set; }
        public DateTime RedemptionDate { get; set; }
        public int PointsRedeemed { get; set; }
        public string RedemptionCode { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string Status { get; set; } // Active, Used, Expired, Cancelled
        public DateTime? UsedDate { get; set; }
    }
    
    public class LoyaltyBenefitDTO
    {
        public string BenefitName { get; set; }
        public string BenefitDescription { get; set; }
        public string Category { get; set; }
        public string TierLevel { get; set; }
        public bool IsActive { get; set; }
    }
}
